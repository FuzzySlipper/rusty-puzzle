using Rusty.Engine;
using RustyPuzzle.Game.Board;
using static Checks;

/// <summary>
/// Movement laws: each shipped law's legal and illegal destinations with walls, members and board edges in the
/// way, the swap's effects and their preview, and a fifth member composed from existing parts in content only.
/// </summary>
static class LawChecks
{
    internal static void Run(IEngineContext engine)
    {
        Laws(engine, "the fighter steps one square orthogonally; walls and members block",
            ["#####", "#.R.#", "#.F##", "#...#", "#EEE#", "#####"], new() { ['F'] = "fighter", ['R'] = "ranger" },
            select: new(2, 2), expected: [new(1, 2), new(2, 3)]);

        Laws(engine, "the ranger moves exactly two in a line, vaulting a member but not a wall, never onto a member",
            ["#######", "#..#..#", "#..R.G#", "#..F..#", "#.....#", "#EEE..#", "#######"],
            new() { ['R'] = "ranger", ['G'] = "rogue", ['F'] = "fighter" },
            select: new(3, 2), expected: [new(1, 2), new(3, 4)]);

        Laws(engine, "the rogue steps one square diagonally, never onto a member",
            ["#####", "#.#.#", "#.G.#", "#F..#", "#EEE#", "#####"], new() { ['G'] = "rogue", ['F'] = "fighter" },
            select: new(2, 2), expected: [new(1, 1), new(3, 1), new(3, 3)]);

        Laws(engine, "the mage swaps only with members within three squares in any direction",
            ["#######", "#M..F.#", "#.....#", "#..G..#", "#.....#", "#....R#", "#EEEE.#", "#######"],
            new() { ['M'] = "mage", ['F'] = "fighter", ['G'] = "rogue", ['R'] = "ranger" },
            select: new(1, 1), expected: [new(4, 1), new(3, 3)]);

        SwapPreview(engine);
        ContentOnlyMember(engine);
    }

    /// <summary>Selects a cell and checks the legal targets the board marks, the moves the HUD lists and the inspection agree.</summary>
    private static void Laws(IEngineContext engine, string law, string[] rows, Dictionary<char, string> party, Cell select, Cell[] expected)
    {
        using PackedContent content = PackedContent.WithRoom(rows, party);
        using Harness puzzle = new(engine, content);
        puzzle.Advance(Harness.Claim($"{{\"action\":\"select\",\"column\":{select.Column},\"row\":{select.Row}}}"));
        Cell[] inspected = [.. puzzle.Observe()["moves"]!.AsArray().Select(move => new Cell((int)move!["column"]!, (int)move["row"]!))];
        Cell[] marked = [.. puzzle.Product.Picture().Moves.Select(move => move.Target)];
        Check(Same(inspected, expected) && Same(marked, expected)
            && puzzle.Hud()["moves"]!.AsArray().Count == expected.Length, law);
    }

    /// <summary>The same cells, each once.</summary>
    private static bool Same(Cell[] actual, Cell[] expected) =>
        actual.Length == expected.Length && actual.Distinct().Count() == actual.Length && actual.ToHashSet().SetEquals(expected);

    private static void SwapPreview(IEngineContext engine)
    {
        using PackedContent content = PackedContent.WithRoom(
            ["#######", "#M..F.#", "#.....#", "#..G..#", "#EEE..#", "#######"],
            new() { ['M'] = "mage", ['F'] = "fighter", ['G'] = "rogue" });
        using Harness puzzle = new(engine, content);
        puzzle.Advance(Harness.Claim("{\"action\":\"select\",\"column\":1,\"row\":1}"));
        var swap = puzzle.Observe()["moves"]!.AsArray().Single(move => (int)move!["column"]! == 4)!["effects"]!.AsArray();
        Check(swap.Count == 2 && (string?)swap[0]!["relocated"] == "mage" && swap[0]!["to"]!.ToJsonString() == "[4,1]"
            && (string?)swap[1]!["relocated"] == "fighter" && swap[1]!["to"]!.ToJsonString() == "[1,1]",
            "a swap resolves to both relocations: the mage to the ally's cell and the ally to the mage's");
        Check(puzzle.Hud()["moves"]!.AsArray().Any(move => ((string?)move!["label"])!.Contains("the Fighter goes to column 2, row 2", StringComparison.Ordinal)),
            "the HUD states where the swapped ally goes");

        BoardGrid grid = puzzle.Product.Session.Board.Grid;
        puzzle.PointAt(Harness.TopOf(puzzle.Product.View.Layout.Piece(grid, new Cell(4, 1))));
        var preview = puzzle.Product.Picture().Preview;
        Check(preview is not null && preview.Target == new Cell(4, 1) && preview.Effects.Count == 2,
            "pointing at a swap target previews that swap's effects on the board");
        Check(puzzle.Observe()["members"]!.AsArray().Any(m => (string?)m!["id"] == "mage" && (int)m["column"]! == 1),
            "a preview changes nothing on the board");
        puzzle.PointAt(new System.Numerics.Vector3(-6, 0, -6));
        Check(puzzle.Product.Picture().Preview is null, "pointing off the board clears the preview");
    }

    private static void ContentOnlyMember(IEngineContext engine)
    {
        using PackedContent content = PackedContent.WithRoom(
            ["######", "#....#", "#.S#.#", "#.F..#", "#EEEE#", "######"], new() { ['S'] = "scout", ['F'] = "fighter" });
        content.Write("laws/dash.json",
            "{\"rule\": \"Dashes one or two squares in any direction along an open line.\", \"moves\": [{\"kind\": \"step\", \"directions\": \"any\", \"distance\": {\"min\": 1, \"max\": 2}, \"path\": \"clear\"}]}");
        content.Write("party/scout.json", "{\"name\": \"Scout\", \"law\": \"dash\", \"look\": {\"colour\": [0.6, 0.6, 0.6], \"mark\": \"Sc\"}}");
        using Harness puzzle = new(engine, content);
        puzzle.Advance(Harness.Claim("{\"action\":\"select\",\"column\":2,\"row\":2}"));
        Cell[] targets = [.. puzzle.Product.Picture().Moves.Select(move => move.Target)];
        Cell[] expected = [new(2, 1), new(1, 2), new(1, 1), new(3, 1), new(1, 3), new(3, 3), new(4, 4)];
        Check(Same(targets, expected),
            "a fifth member with a new law composed from existing parts is content only, and a clear path stops at walls and members");

        content.Write("laws/dash.json", "{\"rule\": \"Hops.\", \"moves\": [{\"kind\": \"hop\", \"distance\": {\"min\": 1, \"max\": 1}}]}");
        Check(Refusal(() => Harness.Create(engine, content).Dispose()).StartsWith("content/laws/dash.json", StringComparison.Ordinal),
            "a move part kind with no evaluator fails naming its file");
    }
}
