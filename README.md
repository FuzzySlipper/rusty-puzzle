# Rusty Puzzle

A turn-based, grid-based fantasy party puzzle game on Rusty Engine. Each party
member obeys a distinctive movement law, and the player solves rooms by
choosing, sequencing and combining those rules. Read the
[game design](docs/game-design.md) for product direction and the
[sibling reuse guidance](docs/reuse.md) before borrowing code from another
Rusty product. Tasks and implementation progress live in Den project
`rusty-puzzle`.

C# loads the authored rooms, terrain and party from `content/`, draws the
board in the Engine as placeholder primitives under a fitted overhead camera,
and turns pointer presses and interface commands into board selections. The
DOM companion shows the projected room, selection and party. The packaged
Engine owns the host, input, update admission, renderer and browser shell.

## Setup

Engine pairs target Linux x64 and Windows x64. Install the .NET 10 SDK, `curl`
and `tar`. NativeAOT also needs the platform compiler/linker prerequisites
(Clang and zlib development headers on Linux). Get the Engine's `rusty`
command once:

```bash
curl -fsSL https://raw.githubusercontent.com/FuzzySlipper/rusty-engine/main/scripts/install-rusty.sh | bash
```

On Windows, in PowerShell:

```powershell
irm https://raw.githubusercontent.com/FuzzySlipper/rusty-engine/main/scripts/install-rusty.ps1 | iex
```

Then, from this repository:

```bash
rusty status
rusty install
rusty dev --port 8787
```

Open the URL printed by the host. The first room of the authored order opens;
click a board cell or a party member to select it.
`rusty dev` runs the pinned pair's runtime: CoreCLR loads the product, and
changes to declared C#, UI, or content inputs rebuild and reload it. See
`rusty dev --help` for `--bind-host`, `--live-debug`, and `--debugger`.

`Directory.Build.props` pins the exact SDK/runtime pair and names the product
project (`<RustyEngineProject>`) that `rusty dev` and `rusty build` use without
`--project`. `rusty install`
downloads it once into the shared Engine cache, and later builds and runs work
offline. No Engine source checkout is required.

To adopt the newest published pair deliberately:

```bash
rusty update
rusty build
```

`rusty update` lists the release notes to read; include the changed
`Directory.Build.props` in the resulting source change. This repository's
`engine-pair` workflow does the same every six hours: it moves the pin only
after the product builds and serves on the new pair, and otherwise opens an
`engine-pair-update` issue with the build output and the notes to read. For an explicit
NativeAOT fidelity/release check:

```bash
rusty build --aot
```

The smoke test drives the product callbacks over the pinned Engine's real
services, without a browser: content loading and reload, the board, pointer
picking, interface commands and debug commands. Each domain's checks live in
their own file beside the shared harness.

```bash
dotnet run --project tests/RustyPuzzle.Smoke
```

## Repository shape

| Path | Responsibility |
| --- | --- |
| `src/RustyPuzzle.Game/` | Ordinary safe C# product: board, party, rooms, presentation, interface and product metadata |
| `src/ui/main.js` | DOM presentation and semantic input |
| `content/` | Product-authored content, one directory per domain ([authoring](docs/content.md)) |
| `tests/RustyPuzzle.Smoke/` | Product callback smoke test over the Engine test host |
| `Directory.Build.props` | Matched Engine SDK/runtime pin and default product project |
| `docs/architecture.md` | Current ownership and data flow |
| `docs/game-design.md` | Product direction: concept, party movement grammar, prototype scope |
| `docs/reuse.md` | One-time sibling donors, copying procedure and incorporated provenance |
| `.den-serve.json` | Playtest host configuration for the Den playtest service |
| `docs/ui.md` | DOM companion contract |
| `docs/content.md` | Content layout and authoring formats |
| `docs/agent-review/` | Reusable review workflow and lane packets |

The SDK generates the product's bind entry point inside its ordinary build;
there is no composition project. The Engine runtime supplies the host and
browser shell. Product metadata, input intents, content/UI
roots, and projection identity live in the ordinary `.csproj`.

Read [AGENTS.md](AGENTS.md) before extending the product. Keep instructions
about current behavior and ownership; exact dependency identities belong in
configuration, and task status belongs in the task system.
