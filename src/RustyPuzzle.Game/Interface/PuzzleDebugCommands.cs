using System.Numerics;
using System.Text.Json.Nodes;
using Rusty.Engine;
using Rusty.Engine.Debugging;
using RustyPuzzle.Game.Board;
using RustyPuzzle.Game.Rooms;

namespace RustyPuzzle.Game.Interface;

/// <summary>
/// Agent and developer commands over the same owners the player drives: reading the board, selecting
/// through the interface command, and selecting through the pointer picking path.
/// </summary>
internal sealed class PuzzleDebugCommands(RustyPuzzleProduct product) : IDebugCommandModule
{
    [DebugCommand("puzzle.inspect", Description = "Read the room, its terrain rows, where each party member stands, the selection and the camera fit.")]
    public DebugCommandResult Inspect() => DebugCommandResult.Success(Observe().ToJsonString());

    [DebugCommand("puzzle.select", Description = "Select the cell at a column and row (from 0, top-left), as the interface's select command does.")]
    public DebugCommandResult Select(int column, int row) =>
        product.Apply(PuzzleCommand.Select(column, row)) ? Published() : Refused($"Cell ({column}, {row}) is off the board.");

    [DebugCommand("puzzle.clear", Description = "Clear the selection, as the interface's clear command does.")]
    public DebugCommandResult Clear()
    {
        product.Apply(new PuzzleCommand(PuzzleAction.Clear));
        return Published();
    }

    [DebugCommand("puzzle.pick", Description = "Press the primary pointer at a view position (0 to 1, bottom-left origin) through the board's picking path.")]
    public DebugCommandResult Pick(float x, float y)
    {
        if (!float.IsFinite(x) || !float.IsFinite(y))
        {
            return Refused("The position must be finite.");
        }

        product.PickAt(new Vector2(x, y));
        return Published();
    }

    /// <summary>The board has no keyboard controls yet: every playtest action is unavailable.</summary>
    internal PlaytestAction PlaytestAction(string id) =>
        new(id, string.Empty, 0, false, false, "The board is played with the pointer and the interface; use puzzle.pick or puzzle.select.", string.Empty, []);

    /// <summary>The board camera is fitted to the room, not steered.</summary>
    internal DebugCommandResult Look(double yawDegrees, double pitchDegrees) =>
        Refused("The board camera is fitted to the room and does not look around.");

    internal JsonObject Observe()
    {
        RoomState room = product.Room;
        BoardGrid grid = room.Room.Grid;
        JsonArray rows = [];
        for (int row = 0; row < grid.Height; row++)
        {
            rows.Add(new string([.. Enumerable.Range(0, grid.Width).Select(column => grid.TerrainAt(new Cell(column, row)).Symbol)]));
        }

        JsonArray members = [];
        foreach (Placement placed in room.Placements)
        {
            members.Add(new JsonObject { ["id"] = placed.Member.Id, ["column"] = placed.Cell.Column, ["row"] = placed.Cell.Row });
        }

        CameraDescriptor camera = product.Camera.Descriptor;
        return new JsonObject
        {
            ["room"] = room.Room.Id,
            ["name"] = room.Room.Name,
            ["width"] = grid.Width,
            ["height"] = grid.Height,
            ["rows"] = rows,
            ["members"] = members,
            ["selected"] = room.Selected is Cell cell
                ? new JsonObject { ["column"] = cell.Column, ["row"] = cell.Row, ["member"] = room.SelectedMember?.Id }
                : null,
            ["camera"] = new JsonObject
            {
                ["aspect"] = product.Camera.Aspect,
                ["pitch"] = camera.Pose.PitchDegrees,
                ["verticalSize"] = camera.Projection.VerticalSize,
            },
        };
    }

    private DebugCommandResult Published()
    {
        product.Publish();
        return Inspect();
    }

    private static DebugCommandResult Refused(string reason) => DebugCommandResult.Failure(DebugCommandStatus.InvalidArguments, reason);
}
