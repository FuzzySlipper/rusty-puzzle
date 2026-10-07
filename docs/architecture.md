# Product architecture

> The product decides. The Engine guarantees.

```text
Authored content (content/)
  -> typed domain records (C#: board, party, rooms, presentation, interface)
  -> Rusty.Engine safe SDK
  -> SDK-generated bind entry point and ABI
  -> packaged Rust host, input, renderer, UI transport, and browser shell
  -> DOM companion
```

## Owners

| Path or service | Responsibility |
| --- | --- |
| `src/RustyPuzzle.Game/RustyPuzzleProduct.cs` | Explicit composition and lifecycle: loads content, plays the first room of the authored order, routes admitted pointer presses and interface commands, publishes the board view and projection, reloads content on restart, registers debug modules |
| `src/RustyPuzzle.Game/Content/` | `AuthoredContent`/`AuthoredDomain`: opening one domain's content bundle and strict source-generated JSON reads that name the file and field. `ContentJson`: the authored record types. `PuzzleContent`: one consistent load composed from each domain owner's `Load` |
| `src/RustyPuzzle.Game/Board/` | `Cell`; terrain kinds (symbol, passable, exit, look) and their loader; `BoardGrid`, the room's terrain per cell; `BoardState`, one immutable moment of the board (terrain and placements); `BoardEffect` records and `BoardEffects.Apply`, the one applier that makes the next state |
| `src/RustyPuzzle.Game/Movement/` | Reusable movement laws and their loader; `MovePart`, the closed vocabulary of ways to move (`step`, `swap`) with their typed directions, distances, path rules and measures; `Interaction`, the closed vocabulary of ways a member changes other members' moves (`lend`); `LegalMoves`, the one owner that gathers a member's own law and the laws lent to it where it stands, dispatches each part to its evaluator (`StepMoves`, `SwapMoves`) and returns `Move`s carrying their board effects and lender, without changing the board |
| `src/RustyPuzzle.Game/Party/` | Party member definitions (name, law, interactions, look) and their loader |
| `src/RustyPuzzle.Game/Rooms/` | Authored room format and room order and their loader, and interpretation of ASCII rows against the terrain and party vocabularies (a room needs at least as many exit cells as party members) |
| `src/RustyPuzzle.Game/Session/` | `PuzzleCommand` and `PuzzleAction`, the one command vocabulary; `Receipt` and `Refusal`; `PuzzleSession`, the one owner of play: the room, the history of adopted `Turn`s (board and move count), the selection, the selected member's legal moves, the exit-zone win, and what a pointer press means |
| `src/RustyPuzzle.Game/Presentation/` | Board view tuning and its loader; `BoardView`, the presentation built from one tuning load: `BoardLayout`, the one source of block geometry that drawing and picking share; `BoardCamera`, the orthographic camera that follows the UI element anchored as its view, fitted to the room from that view's aspect, and pointer picking from canvas to view through `CameraQueries.Ray`; `BoardScene`, the primitive appearances and published snapshot of a `BoardPicture` (board, selection, legal-move markers and the previewed move's effects) |
| `src/RustyPuzzle.Game/Persistence/` | `ProgressStore`, the one owner of saved progress: a small versioned record of solved rooms and best move counts in Engine persistence (`ProductStateStore` with a source-generated JSON codec), loaded at start and saved only at the solve boundary |
| `src/RustyPuzzle.Game/Interface/` | `HudText` (the HUD screen's words, including one sentence per refusal) and `Template`; `CommandPayload`, the `puzzle.command` wire form of session commands; `HudProjection`, the projection built from the session; `PuzzleHud`, its `UiValues.FromJson` publication on one UI stream; `PuzzleDebugCommands` and the `PlaytestDebugModule` adapter |
| `src/RustyPuzzle.Game/RustyPuzzle.Game.csproj` | Product entry, content/UI roots, one content bundle per content directory, the `puzzle.command` payload intent, projection identity, `realtime` lifecycle and unlocked cursor |
| `src/ui/main.js` | DOM HUD: room, move count, selection and law, status, controls, party and legal-move buttons from the projection; the anchored board view element; sends the projected commands; UI cleanup |
| `content/` | Product-authored data, one directory per domain ([content](content.md)) |
| `tests/RustyPuzzle.Smoke/` | Callback smoke test over `EngineTestHost`: a harness, the content packed into Engine containers, one check file per domain, and `SolverChecks`, the breadth-first solvability and intent check of every shipped room |
| Engine SDK/runtime | Generated interop, admitted updates/input, retained UI transport, host, renderer, and browser shell |

## Lifecycle and data flow

The installed runtime loads the product assembly through its SDK-generated bind
entry point. The bind checks the SDK/runtime ABI identity and constructs the
product with `ProductCreateContext`. Construction opens each content domain's
declared bundle through `ProductContent.OpenBundle`, reads it through its
domain owner and fails, naming the file, on any invalid authored value; it
then opens the UI stream and builds the board view (camera, layout, scene),
disposing what it made if any step fails.

The product runs in `realtime` lifecycle mode. The board is turn-based, but its
camera must refit when the page lays out or resizes the board view, and a
`demand` product receives no update for a layout change (rusty-engine #9629).
An update without input only refits the camera and redraws an unchanged
picture; an update with input applies it in order.
A primary pointer press with a position is converted from canvas to the
board's view, turned into a world ray by the fitted camera, and picks the
nearest cell block (falling back to the board plane under the gaps between
blocks); the session reads the press: with a member selected, pressing one of
its legal targets makes that move, otherwise the press selects the cell or,
off the board, clears the selection. A pointer position event picks the cell
under the free cursor; when it is a legal move's target, the board previews
that move by drawing a token where each member it relocates would stand. A
`puzzle.command` payload is parsed into a typed command and submitted to the
session. A malformed payload is a first-party defect and faults the runtime
with the contract named. Paused interface claims reach `HandlePausedIntents`
and take the same path.

The session executes a move by resolving it to its board effects, applying
them to a new board (the current one is never changed) and adopting that board
onto its history. Reset and room changes are adopted the same way, so undo,
which steps back one history entry, takes them back too. Board-changing
commands name the revision they were made against and are refused when it is
stale; every refusal changes nothing and has authored words. The room is
solved when every party member stands on an exit cell at once; a solved room
refuses moves until undone or reset. A receipt names the room a command just
solved; the product saves that room's move count when it beats the best so
far. That solve boundary is the only save: pausing, quitting or leaving a room
mid-play saves nothing. Progress loads at `Start`; a save that cannot be read,
or is of another format version, faults naming the store instead of starting
afresh. The picker marks solved rooms with their best move count.

After applying input the product publishes: the camera refits when the room
or the view's size or place changed, the scene republishes when the board
picture changed, and the HUD republishes when the projected value changed. The
projection carries every word the DOM shows (composed from content
templates), each button's ready-made command, the payload intent identity and
the board view's anchor name, so the DOM keeps no vocabulary or state of its
own.

Engine owns pause/resume/restart/shutdown admission. Restart reloads every
content domain as one set, so bundle edits restaged by `rusty dev` show
without replacing the runtime; it then replaces the board view and starts a
new session at the first room's authored start. Content that fails to load leaves the room,
view and content in place and faults with the file named. The UI stream lives
for the product's lifetime. Disposal publishes an empty snapshot before
releasing appearances, then releases the camera and UI stream.

## Build and host

`Directory.Build.props` pins one immutable SDK/runtime pair. The Engine `rusty`
command installs it into its shared cache (`rusty install`), supplies its
package source to restores, and runs the product on its runtime (`rusty dev`,
which owns staging, watching, worker replacement and serving). `rusty build`
stages CoreCLR through `StageRustyEngineCoreClrProduct`; `rusty build --aot`
runs `VerifyRustyEngineAot` for explicit fidelity/release checks. Generated
bindings and the bind entry point are ignored output, never edited sources.
The product supplies only its C#, DOM UI and content; browser assets and host
binaries stay in the Engine runtime. The `engine-pair` workflow advances the
pin only after the product builds and serves on the new pair.

Before adding a mechanism, check both the installed safe SDK and the owners
above. Product meaning stays downstream. A missing Engine capability is an
upstream request, not another local host, transport, scheduler, or renderer.
