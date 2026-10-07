using System.Text.Json.Serialization;
using RustyPuzzle.Game.Board;
using RustyPuzzle.Game.Content;
using RustyPuzzle.Game.Party;

namespace RustyPuzzle.Game.Rooms;

/// <summary>
/// An authored room, <c>content/rooms/&lt;id&gt;.json</c>: equal-length rows of terrain symbols, where a
/// <paramref name="Party"/> marker places that member on <paramref name="StartTerrain"/>.
/// </summary>
/// <param name="Brief">The room's one new idea, in a sentence or two the interface shows.</param>
/// <param name="Intent">What the room is designed to ask of the party; the solver check holds it to that.</param>
/// <param name="Party">Room marker character to party member ID.</param>
/// <param name="StartTerrain">The terrain kind ID under each party marker.</param>
internal sealed record RoomDefinition(string Name, string Brief, RoomIntent Intent, string[] Rows, Dictionary<string, string> Party,
    string StartTerrain);

/// <summary>What a room is designed to ask of the party.</summary>
[JsonConverter(typeof(KebabCase<RoomIntent>))]
internal enum RoomIntent
{
    /// <summary>The members need one another: no solution exists without a swap or a vault over a member.</summary>
    Combination,

    /// <summary>A control room: each member can do its own job without help from another.</summary>
    Independent,
}

/// <summary>The authored order rooms are played in, <c>content/campaign/rooms.json</c>.</summary>
internal sealed record RoomOrder(string[] Order)
{
    /// <summary>The content domain and bundle: <c>content/campaign/</c>.</summary>
    internal const string Domain = "campaign";
    internal const string File = "rooms.json";
}

/// <summary>A room interpreted against the terrain and party vocabularies.</summary>
internal sealed record Room(string Id, string Name, string Brief, RoomIntent Intent, BoardGrid Grid, IReadOnlyList<Placement> Starts)
{
    /// <summary>The content domain and bundle: <c>content/rooms/</c>.</summary>
    internal const string Domain = "rooms";

    /// <summary>The rooms of the authored order, in play order. Every room the order names must exist and interpret.</summary>
    internal static IReadOnlyList<Room> Load(AuthoredContent content, IReadOnlyDictionary<string, TerrainKind> terrain,
        IReadOnlyDictionary<string, PartyMember> party)
    {
        RoomOrder order;
        using (AuthoredDomain campaign = content.Open(RoomOrder.Domain))
        {
            order = campaign.Read(RoomOrder.File, ContentJson.Default.RoomOrder);
        }

        string orderPath = $"{RoomOrder.Domain}/{RoomOrder.File}";
        Authored.Require(order.Order.Length > 0, orderPath, "order", "must name at least one room.");
        using AuthoredDomain domain = content.Open(Domain);
        IReadOnlyDictionary<string, RoomDefinition> rooms = domain.ReadAll(ContentJson.Default.RoomDefinition);
        List<Room> ordered = [];
        foreach (string id in order.Order)
        {
            Authored.Require(rooms.TryGetValue(id, out RoomDefinition? room), orderPath, "order",
                $"names no room '{id}'; the rooms are {string.Join(", ", rooms.Keys)}.");
            Authored.Require(ordered.All(placed => placed.Id != id), orderPath, "order", $"names '{id}' twice.");
            ordered.Add(Interpret(domain.PathOf($"{id}.json"), id, room!, terrain, party));
        }

        return ordered;
    }

    private static Room Interpret(string path, string id, RoomDefinition definition, IReadOnlyDictionary<string, TerrainKind> terrain,
        IReadOnlyDictionary<string, PartyMember> party)
    {
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
        // The room is solved when the whole party stands on exit cells at once.
        int exits = cells.Count(cell => cell.Exit);
        Authored.Require(exits >= starts.Count, path, "rows",
            $"has {exits} exit cell(s) for a party of {starts.Count}; the whole party must fit on the exits.");
        return new Room(id, definition.Name, definition.Brief, definition.Intent, new BoardGrid(width, definition.Rows.Length, cells), starts);
    }
}
