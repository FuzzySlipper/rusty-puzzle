# Content

`content/` is the product-authored data, one directory per domain. The host
admits it as the loose-content `ProductContent` snapshot; under `rusty dev` an
edit restages the product and replaces the runtime, so the next view shows it.
Each domain is read by its own C# owner into strict typed records: a missing
field, an unknown field, a null or an out-of-range value fails at load with
the file and field named. There are no silent defaults.

A definition's ID is its file name without `.json`; files never repeat their
own ID. Colours are linear `[r, g, b]` with components from 0 to 1.

| Path | Holds | Read by |
| --- | --- | --- |
| `terrain/<id>.json` | One terrain kind: `name` (the noun interface text uses), `symbol` (one character in room rows), `passable`, `exit`, `look.colour`, `look.height` (cells) | `Board/TerrainKind.cs` |
| `party/<id>.json` | One party member: `name`, `look.colour` | `Party/PartyMember.cs` |
| `rooms/<id>.json` | One room: `name`, `rows`, `party`, `startTerrain` (below) | `Rooms/Room.cs` |
| `campaign/rooms.json` | `order`: room IDs in play order; the first opens at start | `Content/PuzzleContent.cs` |
| `interface/text.json` | Every interface word and sentence template | `Interface/InterfaceText.cs` |
| `tuning/board-view.json` | Camera pitch and fill, cell gap, piece size, selection marker, background | `Presentation/BoardViewTuning.cs` |

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
