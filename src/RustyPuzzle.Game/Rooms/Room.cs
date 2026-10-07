using RustyPuzzle.Game.Board;
using RustyPuzzle.Game.Content;
using RustyPuzzle.Game.Party;

namespace RustyPuzzle.Game.Rooms;

/// <summary>
/// An authored room, <c>content/rooms/&lt;id&gt;.json</c>: equal-length rows of terrain symbols, where a
/// <paramref name="Party"/> marker places that member on <paramref name="StartTerrain"/>.
/// </summary>
/// <param name="Party">Room marker character to party member ID.</param>
/// <param name="StartTerrain">The terrain kind ID under each party marker.</param>
internal sealed record RoomDefinition(string Name, string[] Rows, Dictionary<string, string> Party, string StartTerrain);

/// <summary>The authored order rooms are played in, <c>content/campaign/rooms.json</c>.</summary>
internal sealed record RoomOrder(string[] Order);

/// <summary>Where one party member begins a room.</summary>
internal readonly record struct Placement(PartyMember Member, Cell Cell);

/// <summary>A room interpreted against the terrain and party vocabularies.</summary>
internal sealed record Room(string Id, string Name, BoardGrid Grid, IReadOnlyList<Placement> Starts)
{
    internal static Room Interpret(string id, RoomDefinition definition, IReadOnlyDictionary<string, TerrainKind> terrain,
        IReadOnlyDictionary<string, PartyMember> party)
    {
        string path = $"rooms/{id}.json";
        Authored.Require(definition.Rows.Length > 0 && definition.Rows[0].Length > 0, path, "rows", "must hold at least one cell.");
        int width = definition.Rows[0].Length;
        Authored.Require(terrain.TryGetValue(definition.StartTerrain, out TerrainKind? start), path, "startTerrain",
            $"names no terrain kind; the kinds are {string.Join(", ", terrain.Keys)}.");
        Authored.Require(start!.Passable, path, "startTerrain", "must be a passable kind.");

        Dictionary<char, PartyMember> markers = [];
        foreach ((string marker, string member) in definition.Party)
        {
            Authored.Require(marker.Length == 1, path, $"party.{marker}", "must be a single character.");
            Authored.Require(party.TryGetValue(member, out PartyMember? placed), path, $"party.{marker}",
                $"names no party member '{member}'; the members are {string.Join(", ", party.Keys)}.");
            Authored.Require(terrain.Values.All(kind => kind.Symbol != marker[0]), path, $"party.{marker}",
                "is already a terrain symbol.");
            markers.Add(marker[0], placed!);
        }

        Dictionary<char, TerrainKind> bySymbol = terrain.Values.ToDictionary(kind => kind.Symbol);
        TerrainKind[] cells = new TerrainKind[width * definition.Rows.Length];
        List<Placement> starts = [];
        for (int row = 0; row < definition.Rows.Length; row++)
        {
            string line = definition.Rows[row];
            Authored.Require(line.Length == width, path, $"rows[{row}]", $"must be {width} cells wide like the first row.");
            for (int column = 0; column < width; column++)
            {
                char symbol = line[column];
                if (markers.TryGetValue(symbol, out PartyMember? member))
                {
                    Authored.Require(starts.All(placed => placed.Member != member), path, $"rows[{row}]",
                        $"places '{symbol}' a second time.");
                    starts.Add(new Placement(member, new Cell(column, row)));
                    cells[(row * width) + column] = start;
                    continue;
                }

                Authored.Require(bySymbol.TryGetValue(symbol, out TerrainKind? kind), path, $"rows[{row}]",
                    $"column {column} holds '{symbol}', which is neither a terrain symbol nor a party marker.");
                cells[(row * width) + column] = kind!;
            }
        }

        Authored.Require(starts.Count == markers.Count, path, "party",
            $"marks {markers.Count} member(s) but the rows place {starts.Count}.");
        return new Room(id, definition.Name, new BoardGrid(width, definition.Rows.Length, cells), starts);
    }
}
