using System.Text.Json.Serialization;
using RustyPuzzle.Game.Content;

namespace RustyPuzzle.Game.Session;

/// <summary>Why the session refused a command. Each has its words in the HUD text.</summary>
[JsonConverter(typeof(KebabCase<Refusal>))]
internal enum Refusal
{
    /// <summary>The command lacks a field its action needs.</summary>
    Incomplete,

    /// <summary>The command was made against an older board.</summary>
    StaleRevision,

    /// <summary>The cell is off the board.</summary>
    OffBoard,

    /// <summary>No party member of that name stands on this board.</summary>
    NoSuchMember,

    /// <summary>The member's law does not allow that move.</summary>
    IllegalMove,

    /// <summary>The room is solved; undo, reset or move on.</summary>
    RoomSolved,

    /// <summary>There is nothing to take back.</summary>
    NothingToUndo,

    /// <summary>No room of that name is in the authored order.</summary>
    NoSuchRoom,
}

/// <summary>
/// What became of a command: accepted, or refused and why, with the session's revision afterwards.
/// <paramref name="Solved"/> is the room this command solved, with its move count: the solve boundary.
/// </summary>
internal sealed record Receipt(Refusal? Refused, ulong Revision, (string Room, int Moves)? Solved = null)
{
    internal bool Accepted => Refused is null;
}
