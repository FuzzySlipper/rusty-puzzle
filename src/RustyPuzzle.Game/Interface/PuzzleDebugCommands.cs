using System.Numerics;
using System.Text.Json.Nodes;
using Rusty.Engine;
using Rusty.Engine.Debugging;
using RustyPuzzle.Game.Board;
using RustyPuzzle.Game.Movement;
using RustyPuzzle.Game.Session;

namespace RustyPuzzle.Game.Interface;

/// <summary>
/// Agent and developer commands over the same owners the player drives: reading the board and submitting
/// the session commands the pointer and the interface controls submit.
/// </summary>
internal sealed class PuzzleDebugCommands(RustyPuzzleProduct product) : IDebugCommandModule
{
    [DebugCommand("puzzle.inspect", Description = "Read the room, its terrain rows, where each party member stands, the selection, the selected member's legal moves with their effects, the move count, revision, solved state and last refusal, and the camera fit.")]
    public DebugCommandResult Inspect() => DebugCommandResult.Success(Observe().ToJsonString());

    [DebugCommand("puzzle.press", Description = "Press a cell at a column and row (from 0, top-left) as the pointer does: with a member selected, a legal target makes that move; otherwise it selects the cell.")]
    public DebugCommandResult Press(int column, int row) => Submitted(product.Session.Press(new Cell(column, row)));

    [DebugCommand("puzzle.select", Description = "Select the cell at a column and row (from 0, top-left), as the interface's select command does.")]
    public DebugCommandResult Select(int column, int row) => Submitted(product.Session.Submit(PuzzleCommand.Select(column, row)));

    [DebugCommand("puzzle.clear", Description = "Clear the selection, as the interface's clear command does.")]
    public DebugCommandResult Clear() => Submitted(product.Session.Submit(new PuzzleCommand(PuzzleAction.Clear)));

    [DebugCommand("puzzle.undo", Description = "Take back the last move or reset in this room, as the Undo control does.")]
    public DebugCommandResult Undo() => Submitted(product.Session.Submit(PuzzleCommand.Undo(product.Session.Revision)));

    [DebugCommand("puzzle.reset", Description = "Return the room to its authored start, as the Reset control does; undo brings the board back.")]
    public DebugCommandResult Reset() => Submitted(product.Session.Submit(PuzzleCommand.Reset(product.Session.Revision)));

    [DebugCommand("puzzle.room", Description = "Play a room of the authored order from its start, by room ID.")]
    public DebugCommandResult Room(string id) => Submitted(product.Session.Submit(PuzzleCommand.ChooseRoom(id)));

    [DebugCommand("puzzle.pick", Description = "Press the primary pointer at a view position (0 to 1, bottom-left origin) through the board's picking path.")]
    public DebugCommandResult Pick(float x, float y) => float.IsFinite(x) && float.IsFinite(y)
        ? Submitted(product.PressAt(new Vector2(x, y)))
        : Refused("The position must be finite.");

    /// <summary>The board has no keyboard controls yet: every playtest action is unavailable.</summary>
    internal PlaytestAction PlaytestAction(string id) =>
        new(id, string.Empty, 0, false, false, "The board is played with the pointer and the interface; use puzzle.press, puzzle.undo and puzzle.reset.", string.Empty, []);

    /// <summary>The board camera is fitted to the room, not steered.</summary>
    internal DebugCommandResult Look(double yawDegrees, double pitchDegrees) =>
        Refused("The board camera is fitted to the room and does not look around.");

    internal JsonObject Observe()
    {
        PuzzleSession session = product.Session;
        BoardGrid grid = session.Board.Grid;
        JsonArray rows = [];
        for (int row = 0; row < grid.Height; row++)
        {
            rows.Add(new string([.. Enumerable.Range(0, grid.Width).Select(column => grid.TerrainAt(new Cell(column, row)).Symbol)]));
        }

        JsonArray members = [];
        foreach (Placement placed in session.Board.Placements)
        {
            members.Add(new JsonObject { ["id"] = placed.Member.Id, ["column"] = placed.Cell.Column, ["row"] = placed.Cell.Row });
        }

        JsonArray moves = [];
        foreach (Move move in session.Moves)
        {
            JsonArray effects = [];
            foreach (BoardEffect effect in move.Effects)
            {
                effects.Add(effect switch
                {
                    Relocated relocated => new JsonObject
                    {
                        ["relocated"] = relocated.Member.Id,
                        ["from"] = new JsonArray(relocated.From.Column, relocated.From.Row),
                        ["to"] = new JsonArray(relocated.To.Column, relocated.To.Row),
                    },
                    _ => new JsonObject { ["effect"] = effect.GetType().Name },
                });
            }

            moves.Add(new JsonObject { ["column"] = move.Target.Column, ["row"] = move.Target.Row, ["effects"] = effects });
        }

        CameraDescriptor camera = product.View.Camera.Descriptor;
        return new JsonObject
        {
            ["room"] = session.Room.Id,
            ["name"] = session.Room.Name,
            ["width"] = grid.Width,
            ["height"] = grid.Height,
            ["rows"] = rows,
            ["members"] = members,
            ["moves"] = moves,
            ["moveCount"] = session.MoveCount,
            ["revision"] = session.Revision,
            ["solved"] = session.Solved,
            ["refused"] = session.LastRefusal?.ToString(),
            ["selected"] = session.Selected is Cell cell
                ? new JsonObject { ["column"] = cell.Column, ["row"] = cell.Row, ["member"] = session.SelectedMember?.Id }
                : null,
            ["camera"] = new JsonObject
            {
                ["aspect"] = product.View.Camera.Aspect,
                ["pitch"] = camera.Pose.PitchDegrees,
                ["verticalSize"] = camera.Projection.VerticalSize,
            },
        };
    }

    /// <summary>Publishes what the command changed and reports the board, or the refusal.</summary>
    private DebugCommandResult Submitted(Receipt receipt)
    {
        product.Publish();
        return receipt.Refused is Refusal refused ? Refused($"Refused: {refused}.") : Inspect();
    }

    private static DebugCommandResult Refused(string reason) => DebugCommandResult.Failure(DebugCommandStatus.InvalidArguments, reason);
}
