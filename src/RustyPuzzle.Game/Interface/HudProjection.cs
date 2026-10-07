using System.Globalization;
using System.Text.Json.Nodes;
using RustyPuzzle.Game.Board;
using RustyPuzzle.Game.Movement;
using RustyPuzzle.Game.Party;
using RustyPuzzle.Game.Presentation;
using RustyPuzzle.Game.Rooms;
using RustyPuzzle.Game.Session;

namespace RustyPuzzle.Game.Interface;

/// <summary>
/// The board facts the DOM companion shows, in the authored words: the room, the selection and its law, the
/// party, the legal moves, the controls, and the command each button sends. The DOM holds no board state,
/// action names or fields of its own.
/// </summary>
internal static class HudProjection
{
    internal static JsonObject Build(PuzzleSession session, HudText text)
    {
        PartyMember? selected = session.SelectedMember;
        JsonArray party = [];
        foreach (Placement placed in session.Board.Placements)
        {
            party.Add(new JsonObject
            {
                ["id"] = placed.Member.Id,
                ["mark"] = placed.Member.Mark,
                ["place"] = Template.Fill(text.MemberPlace, Member(placed.Member), Column(placed.Cell), Row(placed.Cell)),
                ["selected"] = placed.Member == selected,
                ["label"] = Template.Fill(text.SelectMember, Member(placed.Member)),
                ["command"] = CommandPayload.ToJson(PuzzleCommand.Select(placed.Cell.Column, placed.Cell.Row)),
            });
        }

        JsonArray moves = [];
        foreach (Move move in session.Moves)
        {
            moves.Add(new JsonObject
            {
                ["label"] = MoveLabel(move, text),
                ["command"] = CommandPayload.ToJson(PuzzleCommand.Move(move.Member.Id, move.Target.Column, move.Target.Row, session.Revision)),
            });
        }

        JsonArray rooms = [];
        for (int index = 0; index < session.Rooms.Count; index++)
        {
            Room room = session.Rooms[index];
            rooms.Add(new JsonObject
            {
                ["label"] = Template.Fill(text.RoomOption, ("number", (index + 1).ToString(CultureInfo.InvariantCulture)), ("room", room.Name)),
                ["current"] = room == session.Room,
                ["command"] = CommandPayload.ToJson(PuzzleCommand.ChooseRoom(room.Id, session.Revision)),
            });
        }

        JsonArray controls =
        [
            Control("undo", text.Undo, PuzzleCommand.Undo(session.Revision), session.CanUndo),
            Control("reset", text.Reset, PuzzleCommand.Reset(session.Revision), session.MoveCount > 0),
        ];
        if (session.Solved && session.NextRoom is { } next)
        {
            controls.Add(Control("next-room", text.NextRoom, PuzzleCommand.ChooseRoom(next.Id, session.Revision), true));
        }

        return new JsonObject
        {
            ["title"] = text.Title,
            ["labels"] = new JsonObject
            {
                ["room"] = text.RoomLabel,
                ["rooms"] = text.RoomsLabel,
                ["selected"] = text.SelectedLabel,
                ["law"] = text.LawLabel,
                ["moveCount"] = text.MoveCountLabel,
                ["party"] = text.PartyHeading,
                ["moves"] = text.MovesHeading,
            },
            ["room"] = session.Room.Name,
            ["brief"] = session.Room.Brief,
            ["rooms"] = rooms,
            ["selected"] = selected?.Name ?? text.NobodySelected,
            ["law"] = selected is null ? null : LawText(session, selected),
            ["moveCount"] = session.MoveCount,
            ["solved"] = session.Solved,
            ["status"] = Status(session, selected, text),
            ["moves"] = moves,
            ["party"] = party,
            ["controls"] = controls,
            ["intent"] = new JsonObject { ["id"] = CommandPayload.Intent, ["contract"] = CommandPayload.Contract },
            ["view"] = new JsonObject { ["anchor"] = BoardCamera.ViewAnchor },
        };
    }

    /// <summary>
    /// What the member moves by: its own law, what its interactions do for others, and what other members'
    /// interactions lend it where it stands now.
    /// </summary>
    private static string LawText(PuzzleSession session, PartyMember member) => string.Join(" ",
        [member.Law.Rule, .. member.Interactions.Select(interaction => interaction.Rule),
            .. LegalMoves.Lends(session.Board, member).Select(lend => lend.Rule)]);

    /// <summary>A refusal first, then a solved room, then what is selected.</summary>
    private static string Status(PuzzleSession session, PartyMember? member, HudText text)
    {
        if (session.LastRefusal is Refusal refusal)
        {
            return text.Refused(refusal);
        }

        if (session.Solved)
        {
            string solved = Template.Fill(text.Solved, ("count", session.MoveCount.ToString(CultureInfo.InvariantCulture)));
            return $"{solved} {(session.NextRoom is null ? text.SolvedLast : text.SolvedNext)}";
        }

        if (session.Selected is not Cell cell)
        {
            return text.NothingSelected;
        }

        (string, string) terrain = ("terrain", session.Board.Grid.TerrainAt(cell).Name);
        if (member is null)
        {
            return Template.Fill(text.CellSelected, terrain, Column(cell), Row(cell));
        }

        string selected = Template.Fill(text.MemberSelected, Member(member), terrain, Column(cell), Row(cell));
        return session.Moves.Count > 0 ? selected : $"{selected} {Template.Fill(text.NoMoves, Member(member))}";
    }

    /// <summary>
    /// A move in words, read from its effects so every kind of move is described the same way: where the
    /// mover goes, then where each other member it moves goes.
    /// </summary>
    private static string MoveLabel(Move move, HudText text)
    {
        Cell lands = move.Effects.OfType<Relocated>().FirstOrDefault(relocated => relocated.Member == move.Member)?.To ?? move.Target;
        string label = Template.Fill(text.MoveTo, Column(lands), Row(lands));
        foreach (BoardEffect effect in move.Effects)
        {
            if (effect is Relocated relocated && relocated.Member != move.Member)
            {
                label = Template.Fill(text.AlsoMoves, ("move", label), Member(relocated.Member), Column(relocated.To), Row(relocated.To));
            }
        }

        return label;
    }

    private static JsonObject Control(string id, string label, PuzzleCommand command, bool enabled) => new()
    {
        ["id"] = id,
        ["label"] = label,
        ["command"] = CommandPayload.ToJson(command),
        ["enabled"] = enabled,
    };

    private static (string, string) Member(PartyMember member) => ("member", member.Name);

    // People count cells from one.
    private static (string, string) Column(Cell cell) => ("column", (cell.Column + 1).ToString(CultureInfo.InvariantCulture));

    private static (string, string) Row(Cell cell) => ("row", (cell.Row + 1).ToString(CultureInfo.InvariantCulture));
}
