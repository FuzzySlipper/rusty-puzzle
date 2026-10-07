using System.Text.Json.Nodes;
using Rusty.Engine;
using static Checks;

/// <summary>The agent and developer debug modules reach the same owners the player drives.</summary>
static class DebugChecks
{
    internal static void Run(IEngineContext engine)
    {
        using PackedContent shipped = PackedContent.Shipped();
        using Harness puzzle = new(engine, shipped);
        Check(puzzle.Puzzle.Pick(0.5f, 0.5f).Succeeded && puzzle.Selected() is not null, "the developer pick reaches the picking path");
        Check(puzzle.Puzzle.Select(2, 2).Succeeded && puzzle.Selected() == (2, 2, null), "the developer select applies the interface command");
        Check(!puzzle.Puzzle.Select(-1, 0).Succeeded, "the developer select refuses a cell off the board");
        Check(JsonNode.Parse(puzzle.Playtest.Action("anything").Message)!["available"]!.GetValue<bool>() == false,
            "playtest actions report that the board has no keyboard controls");
    }
}
