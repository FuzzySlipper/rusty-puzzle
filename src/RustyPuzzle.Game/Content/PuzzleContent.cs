using System.Text.Json.Serialization;
using Rusty.Engine;
using RustyPuzzle.Game.Board;
using RustyPuzzle.Game.Interface;
using RustyPuzzle.Game.Party;
using RustyPuzzle.Game.Presentation;
using RustyPuzzle.Game.Rooms;

namespace RustyPuzzle.Game.Content;

/// <summary>Every authored definition the product plays with, each domain read and validated from its own files.</summary>
internal sealed record PuzzleContent(
    IReadOnlyDictionary<string, TerrainKind> Terrain,
    IReadOnlyDictionary<string, PartyMember> Party,
    IReadOnlyList<Room> Rooms,
    InterfaceText Text,
    BoardViewTuning View)
{
    private const string OrderPath = "campaign/rooms.json";

    internal static PuzzleContent Load(ProductContent content)
    {
        Dictionary<string, TerrainKind> terrain = Authored.ReadDirectory(content, "terrain", ContentJson.Default.TerrainDefinition)
            .ToDictionary(pair => pair.Key, pair => TerrainKind.Interpret(pair.Key, pair.Value), StringComparer.Ordinal);
        foreach (IGrouping<char, TerrainKind> shared in terrain.Values.GroupBy(kind => kind.Symbol).Where(group => group.Count() > 1))
        {
            Authored.Require(false, "terrain", "symbol", $"'{shared.Key}' marks more than one kind: {string.Join(", ", shared.Select(kind => kind.Id))}.");
        }

        Dictionary<string, PartyMember> party = Authored.ReadDirectory(content, "party", ContentJson.Default.MemberDefinition)
            .ToDictionary(pair => pair.Key, pair => PartyMember.Interpret(pair.Key, pair.Value), StringComparer.Ordinal);
        IReadOnlyDictionary<string, RoomDefinition> rooms = Authored.ReadDirectory(content, "rooms", ContentJson.Default.RoomDefinition);
        RoomOrder order = Authored.Read(content, OrderPath, ContentJson.Default.RoomOrder);
        Authored.Require(order.Order.Length > 0, OrderPath, "order", "must name at least one room.");
        List<Room> ordered = [];
        foreach (string id in order.Order)
        {
            Authored.Require(rooms.TryGetValue(id, out RoomDefinition? room), OrderPath, "order",
                $"names no room '{id}'; the rooms are {string.Join(", ", rooms.Keys)}.");
            Authored.Require(ordered.All(placed => placed.Id != id), OrderPath, "order", $"names '{id}' twice.");
            ordered.Add(Room.Interpret(id, room!, terrain, party));
        }

        return new PuzzleContent(terrain, party, ordered,
            Authored.Read(content, InterfaceText.Path, ContentJson.Default.InterfaceText),
            BoardViewTuning.Interpret(Authored.Read(content, BoardViewTuning.Path, ContentJson.Default.BoardViewDefinition)));
    }
}

// Missing constructor values, nulls in non-nullable fields and unknown members are errors, not defaults.
[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    RespectRequiredConstructorParameters = true, RespectNullableAnnotations = true,
    UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow)]
[JsonSerializable(typeof(TerrainDefinition))]
[JsonSerializable(typeof(MemberDefinition))]
[JsonSerializable(typeof(RoomDefinition))]
[JsonSerializable(typeof(RoomOrder))]
[JsonSerializable(typeof(InterfaceText))]
[JsonSerializable(typeof(BoardViewDefinition))]
internal sealed partial class ContentJson : JsonSerializerContext;
