using RustyPuzzle.Game.Board;
using RustyPuzzle.Game.Interface;
using RustyPuzzle.Game.Party;
using RustyPuzzle.Game.Presentation;
using RustyPuzzle.Game.Rooms;

namespace RustyPuzzle.Game.Content;

/// <summary>
/// One consistent load of every content domain. Each domain's own owner reads and validates it; this record
/// only composes them in dependency order, so a reload either replaces all of them or none.
/// </summary>
internal sealed record PuzzleContent(
    IReadOnlyDictionary<string, TerrainKind> Terrain,
    IReadOnlyDictionary<string, PartyMember> Party,
    IReadOnlyList<Room> Rooms,
    HudText Hud,
    BoardViewTuning View)
{
    internal static PuzzleContent Load(AuthoredContent content)
    {
        IReadOnlyDictionary<string, TerrainKind> terrain = TerrainKind.Load(content);
        IReadOnlyDictionary<string, PartyMember> party = PartyMember.Load(content);
        return new PuzzleContent(terrain, party, Room.Load(content, terrain, party), HudText.Load(content), BoardViewTuning.Load(content));
    }
}
