using Rusty.Engine;
using static Checks;

/// <summary>Content domains: the room interpretation, strict reads naming the file, and reload on restart.</summary>
static class ContentChecks
{
    internal static void Run(IEngineContext engine)
    {
        using (PackedContent shipped = PackedContent.Shipped())
        using (Harness puzzle = new(engine, shipped))
        {
            var board = puzzle.Observe();
            Check((string?)board["room"] == "first-steps" && (int)board["width"]! == 7 && (int)board["height"]! == 6,
                "the first room of the authored order loads at its authored size");
            Check(board["rows"]!.AsArray().Select(row => (string?)row).SequenceEqual(
                ["#######", "#...EE#", "#...EE#", "#.....#", "#...#.#", "#######"]),
                "party markers stand on the start terrain and the rest keep their symbols");
            Check(board["members"]!.AsArray().Select(m => $"{m!["id"]}@{m["column"]},{m["row"]}").SequenceEqual(
                ["fighter@1,1", "ranger@1,2", "rogue@1,3", "mage@1,4"]), "each member starts on its marker");
            Check((string?)puzzle.Hud()["room"] == "First Steps" && puzzle.Hud()["party"]!.AsArray().Count == 4,
                "the projection names the room and lists the party");
        }

        using (PackedContent edited = PackedContent.Copy())
        {
            edited.Write("rooms/first-steps.json", "{\"name\":\"Broken\",\"rows\":[\"#\"],\"party\":{}}");
            Check(Refusal(() => Harness.Create(engine, edited).Dispose()).StartsWith("content/rooms/first-steps.json", StringComparison.Ordinal),
                "a room missing a required field fails naming its file");
        }

        using (PackedContent edited = PackedContent.Copy())
        using (Harness puzzle = new(engine, edited))
        {
            string room = File.ReadAllText(Path.Combine(edited.Root, "rooms/first-steps.json"));
            edited.Write("rooms/first-steps.json", room.Replace("First Steps", "Second Look", StringComparison.Ordinal));
            puzzle.Product.Restart();
            Check((string?)puzzle.Hud()["room"] == "Second Look", "restart reloads edited content");

            edited.Write("terrain/wall.json", "{\"name\":\"wall\"}");
            Check(Refusal(puzzle.Product.Restart).StartsWith("content/terrain/wall.json", StringComparison.Ordinal),
                "restart with invalid content fails naming its file");
            puzzle.Advance();
            Check((string?)puzzle.Hud()["room"] == "Second Look", "a failed reload keeps the content that was playing");
        }
    }
}
