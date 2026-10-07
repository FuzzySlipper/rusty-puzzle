# Reusing sibling product code

Sibling Rusty projects are one-time code and pattern donors. Rusty Puzzle
remains an ordinary safe C# product with the packaged Engine as its runtime
dependency. No build, restore, content load, or gameplay path may require
another checkout.

## What the Engine already gives this product

Verified by name against the installed pin in `Directory.Build.props`
(`Rusty.Engine.dll` in the pair's SDK feed): orthographic and perspective
`CameraProjection` with `CameraQueries.Ray`/`Project` for pointer picking;
`PointerButton`/`PointerPosition` input with normalized positions in
unlocked cursor mode; `CreatePrimitive`, mesh resources, `CreateSprite` and
sprite atlases with billboard modes; content bundles (`ListBundles`,
`OpenBundle`, `ReadBytes`); `UiValues.FromJson` for projections; payload
intents; `ProductStateStore<T>` with `JsonProductStateCodec`;
`DebugCommandAttribute` modules and `PlaytestDebugModule`; and
`EngineTestHost` for semantic tests. The renderer has no picking: tile
selection is product maths (ray against the board plane, then floor).

The Engine forbids rendering gameplay in the DOM companion. The board,
pieces and legal-move highlights are Engine-rendered from C#; `src/ui/` shows
the HUD, turn/undo/reset controls, room selection and accessible text only.

## Where to look

Paths are relative to the named sibling repository, not build references.
No sibling draws an overhead tile board; the overhead/isometric board and the
party movement grammar are original work here.

| Donor | Useful starting points | Puzzle adaptation |
| --- | --- | --- |
| `rusty-goldbox` | `src/RustyGoldbox.Core/Combat/CombatField.cs`, `src/RustyGoldbox.Game/Presentation/CombatScene.cs`, `SessionProjection.cs` (`ToUiValue`), `GameCommands.cs` (payload intent parse), `src/RustyGoldbox.Core/Campaigns/SaveSlots.cs`, `tests/RustyGoldbox.Tests/GameTests.cs`, `src/ui/dom.js`, `src/ui/panels/controls.js` | Board cells from ASCII rows with distance/neighbour queries; primitive figures on cells with a camera fitted to the board; one `{action, ...}` payload intent; small save slots; xunit over `EngineTestHost`; selected-move button pattern. Strip rulesets, modules, expressions, multiplayer seats and the Gold Box vocabulary. |
| `rusty-d20` | `src/RustyD20.Product/RustyD20Product.cs` (`D20Surface`: `TacticalCamera`, board layers, highlight materials), `Core/Tactical/TacticalEncounter.cs` | The only orthographic overhead camera among siblings; per-cell highlight by material. Strip d20 rules, exploration/camp modes and adventure content. Its pin is older: verify every call here. |
| `rusty-roguelike` | `src/RustyRoguelike.Product/Session/GameSession.cs`, `Saves/RoguelikeSaveStore.cs`, `RoguelikeProduct.cs`, `RustyRoguelike.Product.csproj` | Command/receipt session with expected revision, execute on a detached candidate then adopt (the undo stack falls out of its capture/restore), deterministic opposition step after each party action, `demand` lifecycle for a turn-based product. Strip procgen admission/provenance ceremony. |
| `rusty-hotel` | `src/Hotel.Game/HotelProduct.cs`, `Content/Authored.cs`, `Content/ContentJson.cs`, `Interface/HotelDebugCommands.cs`, `Interface/HotelHud.cs`, `tests/Hotel.Smoke/`, `src/ui/pause.js`, `src/ui/developer.js`, `tests/ui/*.test.mjs`, `docs/reuse.md` | Product entry composition with try/catch disposal, strict source-generated JSON content reads that name the failing file, debug-command module and playtest module registration, smoke test that drives the product lifecycle under `EngineTestHostOptions`, pause and developer-panel UI modules. Strip every hotel domain. |
| `rusty-craftsurvive` | `src/CraftSurvive.Game/Modules/World/MapCameraRig.cs`, `WorldMapVoxelView.cs` (pointer press handling and aspect from `CameraView.ReadSurface()`), `.den-playwright.json`, `.github/workflows/verify.yml` | Overhead camera rig with `CameraQueries.Ray` picking; replace its relief march with a plane intersection and switch to orthographic. Strip terrain, voxels, survival. |
| `rusty-rifles` | `src/Rifles.Game/Dungeon/MovementGrid.cs`, `Dungeon/WorldArt.cs` (billboard sprite), `Presentation/SessionCommand.cs` | Occupancy grid with blocked edges and an admit predicate; billboard sprite for a piece; JSON UI command parsing. Strip crowd/footprint/reservation logic and musket vocabulary. |
| `rusty-dungeon` | `src/DelveRpg.Kit/World/DungeonLevel.cs`, `LevelShaping.cs` (`RoomTemplate(Id, Rows)`) | Flat tile array with a revision counter; ASCII marker rows as the handcrafted room format. Strip Delver-derived rules and content. |
| `rusty-crawler` | `src/PartyRpg.Host/ProductPlaytest.cs`, `Kit/Party/PartyRoster.cs` | `PlaytestDebugModule` observe/action/look for agent playtests; ordered roster with a selected member. Nothing else; the rulesets and imports are donor-derived. |

Not donors for this product: `rusty-underworld`, `rusty-space`, `rusty-riders`,
`rusty-doom` and `rusty-fptester` (first-person or flight products; Hotel
already holds the cleaner copies of FpTester's flows). Never copy licensed or
derived material: goldbox rule modules (OGL, CC-BY, ORC), Might and Magic,
Daggerfall, Ultima Underworld, Delver and Doom imports, Unity-converted riders
art, and generated hotel assets. Code reuse never authorizes importing art.

These are consultation candidates, not compatibility promises or a record of
code already copied. The initial source survey with exact revisions lives in
Den at `[doc: rusty-puzzle/bootstrap-donor-survey]`. Refresh the source before
an implementation decision; do not mistake an uncommitted working tree for
its HEAD.

## Copying procedure

1. Find the smallest relevant flow: entry point, state owner, implementation,
   and meaningful caller. Read it rather than copying a class by name.
2. Check the installed package selected by `Directory.Build.props`. Prefer its
   safe helpers where they already express the mechanism. Different donor pins
   do not establish API compatibility with this product.
3. Record the source revision and paths for code actually copied. For
   uncommitted source, preserve exact source hashes and identify the
   working-tree snapshot. Retain applicable attribution and license notices.
4. Adapt into the existing product domain owner. Keep one owner for board
   state, party state, turn history and room definitions. Use Engine-admitted
   time and input; do not import a separate clock, input authority, renderer,
   transport or persistence layer.
5. Verify compilation against this product's pin and the ordinary changed
   behavior. Build success alone does not establish visible interaction.

Keep settled provenance for incorporated code in this file beside the copied
flow. Keep investigation logs, build results, captures and task status in Den.
Do not change donor repositories to make them easier to copy.

## Incorporated flows

The board bootstrap adapted these small flows from committed donor revisions;
none of the consulted files had uncommitted changes. Rusty Puzzle owns the
adapted copies; no sibling is a build dependency.

| Donor and revision | Donor file | Rusty Puzzle owner and retained behavior | SHA-256 of source |
| --- | --- | --- | --- |
| `rusty-hotel` `16ddf045854b7384a132b12e9ed87fe802946e7c` | `src/Hotel.Game/Content/Authored.cs` | `Content/Authored.cs`: typed read with errors prefixed `content/<file>`, `Require`/`Within`/`Colour` checks; reads `ProductContent` instead of content references | `f7d0d1c5c68a53cb09e3d556f0cf47c2430dc3b782898889a3e828ad9b1b7ca4` |
| `rusty-hotel` `16ddf04…` | `src/Hotel.Game/Content/ContentJson.cs` | `Content/PuzzleContent.cs` `ContentJson`, `Interface/PuzzleCommand.cs` `InterfaceJson`: strict source-generation options (required, nullable, unmapped members disallowed) | `8376eb762e44dd4fa78c87c8309800adabd8a0c0bff93f5ae1f1304c760afa88` |
| `rusty-hotel` `16ddf04…` | `src/Hotel.Game/HotelProduct.cs` | `RustyPuzzleProduct.cs`: construction with dispose-on-failure, `IDebugCommandModuleSource` registering `PlaytestDebugModule` and a product command module | `f62cb04fe1c3a59d417fafc87c1d71273a9cc4636e6b0230ca8cbb5e5f12b22b` |
| `rusty-hotel` `16ddf04…` | `tests/Hotel.Smoke/Program.cs`, `Hotel.Smoke.csproj` | `tests/RustyPuzzle.Smoke/`: Exe over `EngineTestHost` with content linked into the output, `ProductCreateContext` construction, synthetic `ProductUpdate`s, capturing registrar | `676df76fb684ea2e2c7cfe60cb5f2057cbfdf3b725408a7782dc790e1abc1b19`, `90f761f177117d2e3d39a06bb25997c0d48bd99c29f0ef66f40d9c32d9780b52` |
| `rusty-goldbox` `418d15192201120814c6325b8df299988e4abba3` | `src/RustyGoldbox.Game/Presentation/CombatScene.cs` | `Presentation/BoardScene.cs`, `BoardLayout.cs`: cells at `(x + 0.5, z + 0.5)`, unit-cube primitives scaled and raised to stand on a cell, empty snapshot before release | `d800a9efdf4abf11c6ee919a2b0cb8f2d169ad229cf6144bee8b7b4703ad69c3` |
| `rusty-goldbox` `418d151…` | `src/RustyGoldbox.Game/GameCommands.cs` | `Interface/PuzzleCommand.cs`: one payload intent filtered by intent and contract, `{action, ...}` body | `077ab633ba32cb56d6335dcb414dfcd54ebf1d71cdeac3e5318092ad7033f64e` |
| `rusty-craftsurvive` `6d635625ecce950a3c9d3708174489c1ebdebba7` | `src/CraftSurvive.Game/Modules/World/WorldMapVoxelView.cs` | `RustyPuzzleProduct.cs` press filter (primary, pressed, with position) and `Presentation/BoardCamera.cs` aspect from `CameraView.ReadSurface()` with a fallback before the page reports | `546af0d896f4aff7c0671d8cf244000cbb8e0d4b9f0267ad197519359d45769a` |

Consulted for direction only, with nothing copied: `rusty-d20`
`047934240040ac889ac787239bfec5f9fa8c7a2a` `RustyD20Product.cs`
(`TacticalCamera`, an orthographic overhead camera), `rusty-crawler`
`8c3996ea4cff5002f02cd04dc10287a1aca30260` `ProductPlaytest.cs`
(`PlaytestDebugModule` wiring) and `rusty-dungeon`
`fb8c6ec8a9662035082c389dc43ef27d9e93f2b8` `LevelShaping.cs` (ASCII room rows).
The camera fit (measuring block corners through `CameraQueries.Project` and
recentring along `CameraQueries.Ray`), block picking with the board-plane
fallback, the room format and every content value are original here. No donor
vocabulary, rules, content or art was incorporated.
