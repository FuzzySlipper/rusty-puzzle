using System.Text.Json.Serialization;
using RustyPuzzle.Game.Content;

namespace RustyPuzzle.Game.Session;

/// <summary>What a <see cref="PuzzleCommand"/> asks the session to do. The one declaration of the actions.</summary>
[JsonConverter(typeof(KebabCase<PuzzleAction>))]
internal enum PuzzleAction
{
    /// <summary>Select the cell at <see cref="PuzzleCommand.Column"/>, <see cref="PuzzleCommand.Row"/>.</summary>
    Select,

    /// <summary>Clear the selection.</summary>
    Clear,

    /// <summary><see cref="PuzzleCommand.Member"/> makes its legal move to the target cell.</summary>
    Move,

    /// <summary>Take back the last move, reset or room change.</summary>
    Undo,

    /// <summary>Return the room to its authored start; undo brings the board back.</summary>
    Reset,

    /// <summary>Play <see cref="PuzzleCommand.Room"/> from its start.</summary>
    Room,
}

/// <summary>
/// One request to the session, as the interface and the pointer make it: <c>{"action": ..., fields}</c>.
/// Board-changing actions carry the <see cref="Revision"/> the requester saw, so a request made against an
/// older board is refused rather than applied to a board nobody looked at.
/// </summary>
internal sealed record PuzzleCommand(PuzzleAction Action, int? Column = null, int? Row = null, string? Member = null,
    string? Room = null, ulong? Revision = null)
{
    internal static PuzzleCommand Select(int column, int row) => new(PuzzleAction.Select, column, row);

    internal static PuzzleCommand Move(string member, int column, int row, ulong revision) =>
        new(PuzzleAction.Move, column, row, Member: member, Revision: revision);

    internal static PuzzleCommand Undo(ulong revision) => new(PuzzleAction.Undo, Revision: revision);

    internal static PuzzleCommand Reset(ulong revision) => new(PuzzleAction.Reset, Revision: revision);

    internal static PuzzleCommand ChooseRoom(string room, ulong revision) => new(PuzzleAction.Room, Room: room, Revision: revision);
}
