using RustyPuzzle.Game.Board;
using RustyPuzzle.Game.Movement;
using RustyPuzzle.Game.Party;
using RustyPuzzle.Game.Rooms;

namespace RustyPuzzle.Game.Session;

/// <summary>One adopted moment of play: the board and how many moves led to it since the room's start.</summary>
internal sealed record Turn(Room Room, BoardState Board, int MoveCount);

/// <summary>
/// The one owner of play: which room, the history of adopted boards, the selection and the commands that
/// change them. A move is resolved to its effects, applied to a new board (the current one is never
/// touched) and adopted onto the history; undo steps back through the history, and a reset is adopted like a
/// move, so undo takes it back too. Choosing a room starts that room's own history. Nothing costs anything and
/// the history is unbounded.
/// </summary>
internal sealed class PuzzleSession
{
    private readonly IReadOnlyList<Room> _rooms;
    private readonly List<Turn> _history = [];
    private Cell? _selected;

    internal PuzzleSession(IReadOnlyList<Room> rooms)
    {
        _rooms = rooms;
        _history.Add(Start(rooms[0]));
        Refresh();
    }

    internal Room Room => Current.Room;

    internal BoardState Board => Current.Board;

    internal int MoveCount => Current.MoveCount;

    /// <summary>Grows with every adopted board change; board-changing commands must name the revision they saw.</summary>
    internal ulong Revision { get; private set; }

    internal Cell? Selected => _selected;

    internal PartyMember? SelectedMember => _selected is Cell cell ? Board.MemberAt(cell) : null;

    /// <summary>The selected member's legal moves; none when no member is selected or the room is solved.</summary>
    internal IReadOnlyList<Move> Moves { get; private set; } = [];

    /// <summary>Every party member stands on an exit cell.</summary>
    internal bool Solved { get; private set; }

    internal bool CanUndo => _history.Count > 1;

    /// <summary>Every room of the authored order, in order.</summary>
    internal IReadOnlyList<Room> Rooms => _rooms;

    /// <summary>The room after this one in the authored order, if any.</summary>
    internal Room? NextRoom => _rooms.SkipWhile(room => room != Room).Skip(1).FirstOrDefault();

    /// <summary>The last command's refusal, until a command is accepted.</summary>
    internal Refusal? LastRefusal { get; private set; }

    private Turn Current => _history[^1];

    internal Receipt Submit(PuzzleCommand command)
    {
        (Room room, bool solved) before = (Room, Solved);
        Refusal? refused = Execute(command);
        LastRefusal = refused;
        Refresh();
        bool solvedNow = Solved && !(before.solved && before.room == Room);
        return new Receipt(refused, Revision, solvedNow ? (Room.Id, MoveCount) : null);
    }

    /// <summary>
    /// What a pointer press on a cell means: with a member selected, pressing one of its legal targets makes
    /// that move; otherwise the press selects the cell, and a press off the board clears the selection.
    /// </summary>
    internal Receipt Press(Cell? cell)
    {
        if (SelectedMember is PartyMember member && Moves.Any(move => move.Target == cell))
        {
            Cell target = cell!.Value;
            return Submit(PuzzleCommand.Move(member.Id, target.Column, target.Row, Revision));
        }

        return Submit(cell is Cell chosen ? PuzzleCommand.Select(chosen.Column, chosen.Row) : new PuzzleCommand(PuzzleAction.Clear));
    }

    private Refusal? Execute(PuzzleCommand command)
    {
        // Selection and choosing a room change no board the requester was looking at, so they carry no revision.
        if (command.Action is not (PuzzleAction.Select or PuzzleAction.Clear or PuzzleAction.Room) && command.Revision != Revision)
        {
            return command.Revision is null ? Refusal.Incomplete : Refusal.StaleRevision;
        }

        return command.Action switch
        {
            PuzzleAction.Select => SelectAt(command),
            PuzzleAction.Clear => Select(null),
            PuzzleAction.Move => MoveMember(command),
            PuzzleAction.Undo => Undo(),
            PuzzleAction.Reset => Adopt(Start(Room)),
            PuzzleAction.Room => ChooseRoom(command),
            _ => throw new InvalidOperationException($"No action {command.Action}."),
        };
    }

    private Refusal? SelectAt(PuzzleCommand command) => command is { Column: int column, Row: int row }
        ? Select(new Cell(column, row))
        : Refusal.Incomplete;

    private Refusal? Select(Cell? cell)
    {
        if (cell is Cell chosen && !Board.Grid.Contains(chosen))
        {
            return Refusal.OffBoard;
        }

        _selected = cell;
        return null;
    }

    private Refusal? MoveMember(PuzzleCommand command)
    {
        if (command is not { Member: string id, Column: int column, Row: int row })
        {
            return Refusal.Incomplete;
        }

        if (Solved)
        {
            return Refusal.RoomSolved;
        }

        if (Board.Placements.FirstOrDefault(placed => placed.Member.Id == id) is not { Member: PartyMember member })
        {
            return Refusal.NoSuchMember;
        }

        if (LegalMoves.For(Board, member).FirstOrDefault(move => move.Target == new Cell(column, row)) is not Move move)
        {
            return Refusal.IllegalMove;
        }

        // The candidate board is a new state; the current one is untouched until the candidate is adopted.
        BoardState candidate = BoardEffects.Apply(Board, move.Effects);
        Adopt(new Turn(Room, candidate, MoveCount + 1));
        _selected = candidate.CellOf(member);
        return null;
    }

    private Refusal? Undo()
    {
        if (!CanUndo)
        {
            return Refusal.NothingToUndo;
        }

        PartyMember? selected = SelectedMember;
        _history.RemoveAt(_history.Count - 1);
        Revision++;
        Follow(selected);
        return null;
    }

    private Refusal? ChooseRoom(PuzzleCommand command)
    {
        if (command.Room is not string id)
        {
            return Refusal.Incomplete;
        }

        if (_rooms.FirstOrDefault(room => room.Id == id) is not Room room)
        {
            return Refusal.NoSuchRoom;
        }

        _history.Clear();
        _history.Add(Start(room));
        Revision++;
        _selected = null;
        return null;
    }

    private Refusal? Adopt(Turn turn)
    {
        PartyMember? selected = SelectedMember;
        _history.Add(turn);
        Revision++;
        Follow(selected);
        return null;
    }

    /// <summary>Keeps a selected member selected wherever the new board puts it; a selected cell stays when it exists.</summary>
    private void Follow(PartyMember? member)
    {
        _selected = member is not null ? Board.CellOf(member)
            : _selected is Cell cell && Board.Grid.Contains(cell) ? cell
            : null;
    }

    private void Refresh()
    {
        Solved = Board.Placements.All(placed => Board.Grid.TerrainAt(placed.Cell).Exit);
        Moves = !Solved && SelectedMember is PartyMember member ? LegalMoves.For(Board, member) : [];
    }

    private static Turn Start(Room room) => new(room, new BoardState(room.Grid, room.Starts), 0);
}
