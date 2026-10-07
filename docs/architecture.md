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
| `src/RustyPuzzle.Game/Board/` | `Cell`; terrain kinds (symbol, passable, exit, look) and their loader; `BoardGrid`, the room's terrain per cell |
| `src/RustyPuzzle.Game/Party/` | Party member definitions (name, look) and their loader |
| `src/RustyPuzzle.Game/Rooms/` | Authored room format and room order and their loader, interpretation of ASCII rows against the terrain and party vocabularies, and `RoomState`: the live placements and selection of the room being played, with a revision for republishing |
| `src/RustyPuzzle.Game/Presentation/` | Board view tuning and its loader; `BoardView`, the presentation built from one tuning load: `BoardLayout`, the one source of block geometry that drawing and picking share; `BoardCamera`, the orthographic camera fitted to the room from the surface aspect, and pointer picking through `CameraQueries.Ray`; `BoardScene`, the primitive appearances and published snapshot |
| `src/RustyPuzzle.Game/Interface/` | `HudText` (the HUD screen's words) and `Template`; `PuzzleCommand`, the one `{action, ...}` payload vocabulary; `PuzzleHud`, the `UiValues.FromJson` projection on one UI stream; `PuzzleDebugCommands` and the `PlaytestDebugModule` adapter |
| `src/RustyPuzzle.Game/RustyPuzzle.Game.csproj` | Product entry, content/UI roots, one content bundle per content directory, the `puzzle.command` payload intent, projection identity, `demand` lifecycle and unlocked cursor |
| `src/ui/main.js` | DOM HUD: room, selection, status text and party buttons from the projection; sends the projected commands; UI cleanup |
| `content/` | Product-authored data, one directory per domain ([content](content.md)) |
| `tests/RustyPuzzle.Smoke/` | Callback smoke test over `EngineTestHost`: a harness, the content packed into Engine containers, and one check file per domain |
| Engine SDK/runtime | Generated interop, admitted updates/input, retained UI transport, host, renderer, and browser shell |

## Lifecycle and data flow

The installed runtime loads the product assembly through its SDK-generated bind
entry point. The bind checks the SDK/runtime ABI identity and constructs the
product with `ProductCreateContext`. Construction opens each content domain's
declared bundle through `ProductContent.OpenBundle`, reads it through its
domain owner and fails, naming the file, on any invalid authored value; it
then opens the UI stream and builds the board view (camera, layout, scene),
disposing what it made if any step fails.

The product runs in `demand` lifecycle mode: the board is turn-based and has
nothing to animate, so the Engine admits an `Update` when input arrives
instead of at a fixed rate. Each update applies the admitted input in order:
a primary pointer press with a position is turned into a world ray by the
fitted camera and picks the nearest cell block (falling back to the board
plane under the gaps between blocks; off the board clears the selection), and
a `puzzle.command` payload is parsed into a typed command and applied to the
room state. A malformed payload is a first-party defect and faults the
runtime with the contract named. Paused interface claims reach
`HandlePausedIntents` and take the same path.

After applying input the product publishes: the camera refits when the room
or the surface size changed, the scene republishes its snapshot when the room
state's revision changed, and the HUD republishes when the projected value
changed. The projection carries every word the DOM shows (composed from
content templates), each party button's ready-made command, and the payload
intent identity, so the DOM keeps no vocabulary or state of its own.

Engine owns pause/resume/restart/shutdown admission. Restart reloads every
content domain as one set, so bundle edits restaged by `rusty dev` show
without replacing the runtime; it then replaces the board view and returns to
the first room's authored start. Content that fails to load leaves the room,
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
