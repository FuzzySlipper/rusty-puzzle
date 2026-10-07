using RustyPuzzle.Game.Board;
using RustyPuzzle.Game.Movement;
using RustyPuzzle.Game.Party;

namespace RustyPuzzle.Game.Rooms;

/// <summary>
/// The live state of the room being played: the current board and which cell the player has selected, with
/// the selected member's legal moves. <see cref="Revision"/> grows with every change, so views republish only
/// on change.
/// </summary>
internal sealed class RoomState
{
    internal RoomState(Room room)
    {
        Room = room;
        Board = new BoardState(room.Grid, room.Starts);
    }

    internal Room Room { get; }

    internal BoardState Board { get; private set; }

    internal Cell? Selected { get; private set; }

    internal ulong Revision { get; private set; }

    /// <summary>The member standing on the selected cell, if any.</summary>
    internal PartyMember? SelectedMember => Selected is Cell cell ? Board.MemberAt(cell) : null;

    /// <summary>Every legal move of the selected member; empty when no member is selected.</summary>
    internal IReadOnlyList<Move> Moves { get; private set; } = [];

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
            Moves = SelectedMember is PartyMember member ? LegalMoves.For(Board, member) : [];
            Revision++;
        }

        return true;
    }
}
