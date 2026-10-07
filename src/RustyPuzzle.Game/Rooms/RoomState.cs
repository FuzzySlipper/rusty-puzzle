using RustyPuzzle.Game.Board;
using RustyPuzzle.Game.Party;

namespace RustyPuzzle.Game.Rooms;

/// <summary>
/// The live state of the room being played: where each party member stands and which cell the
/// player has selected. <see cref="Revision"/> grows with every change, so views republish only on change.
/// </summary>
internal sealed class RoomState
{
    private readonly List<Placement> _placements;

    internal RoomState(Room room)
    {
        Room = room;
        _placements = [.. room.Starts];
    }

    internal Room Room { get; }

    internal IReadOnlyList<Placement> Placements => _placements;

    internal Cell? Selected { get; private set; }

    internal ulong Revision { get; private set; }

    /// <summary>The member standing on the selected cell, if any.</summary>
    internal PartyMember? SelectedMember => Selected is Cell cell ? MemberAt(cell) : null;

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

    /// <summary>Selects a board cell, or clears the selection with null. A cell off the board is refused.</summary>
    internal bool Select(Cell? cell)
    {
        if (cell is Cell chosen && !Room.Grid.Contains(chosen))
        {
            return false;
        }

        if (Selected != cell)
        {
            Selected = cell;
            Revision++;
        }

        return true;
    }
}
