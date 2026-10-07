using RustyPuzzle.Game.Party;

namespace RustyPuzzle.Game.Board;

/// <summary>
/// One immutable moment of a room's board: its terrain and where each party member stands. Rules read it;
/// only <see cref="BoardEffects.Apply"/> makes the next state, so any state can be kept, compared or restored.
/// </summary>
internal sealed class BoardState
{
    private readonly Placement[] _placements;

    internal BoardState(BoardGrid grid, IEnumerable<Placement> placements)
    {
        Grid = grid;
        _placements = [.. placements];
    }

    internal BoardGrid Grid { get; }

    internal IReadOnlyList<Placement> Placements => _placements;

    internal PartyMember? MemberAt(Cell cell)
    {
        foreach (Placement placed in _placements)
        {
            if (placed.Cell == cell)
            {
                return placed.Member;
            }
        }

        return null;
    }

    internal Cell? CellOf(PartyMember member)
    {
        foreach (Placement placed in _placements)
        {
            if (placed.Member == member)
            {
                return placed.Cell;
            }
        }

        return null;
    }

    /// <summary>A cell a member may stand on: on the board, passable terrain, and nobody there.</summary>
    internal bool Open(Cell cell) => Grid.Contains(cell) && Grid.TerrainAt(cell).Passable && MemberAt(cell) is null;
}
