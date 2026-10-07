using System.Numerics;
using Rusty.Engine;
using RustyPuzzle.Game.Board;
using static Checks;

/// <summary>Selection: pointer presses through the fitted camera, interface commands, pause and restart.</summary>
static class SelectionChecks
{
    internal static void Run(IEngineContext engine)
    {
        using PackedContent shipped = PackedContent.Shipped();
        using Harness puzzle = new(engine, shipped);
        BoardGrid grid = puzzle.Product.Room.Room.Grid;
        var layout = puzzle.Product.View.Layout;
        Check(puzzle.Selected() is null, "nothing is selected at start");

        puzzle.PressAt(Harness.TopOf(layout.Terrain(grid, new Cell(5, 2))));
        Check(puzzle.Selected() == (5, 2, null), "a press on a floor cell selects that cell");
        Check(((string?)puzzle.Hud()["status"])!.Contains("column 6, row 3", StringComparison.Ordinal), "the projection describes the selected cell");

        puzzle.PressAt(Harness.TopOf(layout.Terrain(grid, new Cell(3, 1))));
        Check(puzzle.Selected() == (3, 1, null), "a press on a wall's top selects the wall cell, not the floor behind it");

        puzzle.PressAt(Harness.TopOf(layout.Piece(grid, new Cell(1, 1))));
        Check(puzzle.Selected() == (1, 1, "fighter") && (string?)puzzle.Hud()["selected"] == "Fighter", "a press on a member's piece selects that member");

        puzzle.PressAt(new Vector3(-6, 0, -6));
        Check(puzzle.Selected() is null, "a press off the board clears the selection");

        string ranger = puzzle.Hud()["party"]!.AsArray().Single(m => (string?)m!["id"] == "ranger")!["command"]!.ToJsonString();
        puzzle.Advance(Harness.Claim(ranger));
        Check(puzzle.Selected() == (1, 3, "ranger"), "the party list's projected command selects that member");
        puzzle.Advance(Harness.Claim("{\"action\":\"select\",\"column\":99,\"row\":0}"));
        Check(puzzle.Selected() == (1, 3, "ranger"), "a select command off the board is refused");
        puzzle.Advance(Harness.Claim("{\"action\":\"clear\"}"));
        Check(puzzle.Selected() is null, "the clear command clears the selection");

        puzzle.Product.Pause();
        puzzle.Product.HandlePausedIntents([Harness.Claim(ranger)]);
        Check(puzzle.Selected() == (1, 3, "ranger"), "a paused interface claim selects through the same command");
        puzzle.Product.Resume();
        puzzle.Product.Restart();
        Check(puzzle.Selected() is null && (string?)puzzle.Hud()["selected"] == "Nobody", "restart returns the room to its authored start");
    }
}
