# Content

`content/` is the product-authored data, one directory per domain. Every
domain directory is its own Engine content bundle, named after the directory;
the product project declares them with one glob, so a new directory is a new
bundle without a build edit. A domain's C# owner opens its own bundle, reads
its own files into strict typed records and closes it. A missing field, an
unknown field, a null or an out-of-range value fails at load with
`content/<domain>/<file> <field>` named. There are no silent defaults.

A definition's ID is its file name without `.json`; files never repeat their
own ID. Colours are linear `[r, g, b]` with components from 0 to 1.

| Domain | Holds | Owner |
| --- | --- | --- |
| `terrain/<id>.json` | One terrain kind: `name` (the noun interface text uses), `symbol` (one character in room rows), `passable`, `exit`, `look.colour`, `look.height` (cells) | `Board/TerrainKind.cs` |
| `party/<id>.json` | One party member: `name`, `look.colour` | `Party/PartyMember.cs` |
| `rooms/<id>.json` | One room: `name`, `rows`, `party`, `startTerrain` (below) | `Rooms/Room.cs` |
| `campaign/rooms.json` | `order`: room IDs in play order; the first opens at start | `Rooms/Room.cs` (`RoomOrder`) |
| `interface/<screen>.json` | One interface screen's words and templates; `hud.json` is the board HUD | `Interface/HudText.cs` |
| `tuning/board-view.json` | Camera pitch and fill, cell gap, piece size, selection marker, background | `Presentation/BoardViewTuning.cs` |

Keep a reusable definition (a terrain kind, a party member) in its own file
and apart from where a room places it. Keep each file to one domain: a new
screen's words are a new file in `interface/`, a new kind of tuning a new file
in `tuning/`, never fields added to an existing file of another concern.

## Adding a domain

1. Create `content/<domain>/` with its files.
2. Add the authored record and the interpreted type in the C# folder that owns
   the domain. The type declares `internal const string Domain = "<domain>"`
   and a `static Load(AuthoredContent content)` that opens the domain, reads
   it and validates every value, naming the file and field on failure.
3. Add the authored record to `Content/ContentJson.cs`.
4. Call its `Load` from `Content/PuzzleContent.cs`, after the domains it
   refers to, and hand the result to the owner that uses it.
5. Add its checks to a check file of the same domain in `tests/RustyPuzzle.Smoke/`.

`PuzzleContent` only composes the domains; it holds no reading or validation
of its own.

## Rooms

`rows` are equal-length strings, the first row at the top of the board. Each
character is a terrain kind's `symbol` or a party marker. `party` maps each
marker character to a party member ID; that member starts there, standing on
the `startTerrain` kind, which must be passable. A marker must not also be a
terrain symbol, and each marker appears exactly once.

```json
{
  "name": "First Steps",
  "rows": ["#####", "#1.E#", "#####"],
  "party": { "1": "fighter" },
  "startTerrain": "floor"
}
```

Add a room by adding its file and naming it in `campaign/rooms.json`. A new
terrain kind is a new file in `terrain/` with an unused symbol.

## Interface text

Templates name their values in braces: `{member}`, `{terrain}`, `{column}`
and `{row}`. Columns and rows are shown counting from one. C# fills templates;
it never authors the words.

## Iterating under `rusty dev`

A content edit restages only the bundles; the running product keeps its
loaded content until the Engine restarts it. `Restart` reloads every domain
as one set: if any file is invalid, the restart fails naming it and the
content already playing stays. A C# or project edit replaces the runtime,
which loads the content fresh.

## Tests

The Engine test host cannot open declared build bundles (rusty-engine #9628).
`tests/RustyPuzzle.Smoke/PackedContent.cs` packs each domain directory into an
Engine content container and opens it through the same `ProductContentBundle`
reads, so the smoke test runs the product's own loaders. Checks that edit
content work on a private copy of the tree.
