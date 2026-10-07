using System.Text.Json.Nodes;
using Rusty.Engine;
using static Checks;

/// <summary>
/// The session: moves adopted from their effects, undo after each law, reset and its undo, the exit-zone win,
/// moving on to the next room, and refused commands that change nothing.
/// </summary>
static class SessionChecks
{
    internal static void Run(IEngineContext engine)
    {
        UndoEachLaw(engine);
        ResetAndPointer(engine);
        Win(engine);
        Refusals(engine);
    }

    private static void UndoEachLaw(IEngineContext engine)
    {
        Undoes(engine, "the fighter's step", ["#####", "#.R.#", "#.F##", "#...#", "#EEE#", "#####"],
            new() { ['F'] = "fighter", ['R'] = "ranger" }, 2, 2, 1, 2, "fighter@1,2");
        Undoes(engine, "the ranger's vault", ["#######", "#..#..#", "#..R.G#", "#..F..#", "#.....#", "#EEE..#", "#######"],
            new() { ['R'] = "ranger", ['G'] = "rogue", ['F'] = "fighter" }, 3, 2, 3, 4, "ranger@3,4");
        Undoes(engine, "the rogue's diagonal", ["#####", "#.#.#", "#.G.#", "#F..#", "#EEE#", "#####"],
            new() { ['G'] = "rogue", ['F'] = "fighter" }, 2, 2, 1, 1, "rogue@1,1");
        Undoes(engine, "the mage's swap", ["#######", "#M..F.#", "#.....#", "#..G..#", "#EEE..#", "#######"],
            new() { ['M'] = "mage", ['F'] = "fighter", ['G'] = "rogue" }, 1, 1, 4, 1, "mage@4,1", "fighter@1,1");
    }

    private static void Undoes(IEngineContext engine, string law, string[] rows, Dictionary<char, string> party,
        int column, int row, int targetColumn, int targetRow, params string[] after)
    {
        using PackedContent content = PackedContent.WithRoom(rows, party);
        using Harness puzzle = new(engine, content);
        string[] start = puzzle.Members();
        puzzle.MoveVia(column, row, targetColumn, targetRow);
        string[] moved = puzzle.Members();
        Check(after.All(moved.Contains) && (int)puzzle.Hud()["moveCount"]! == 1, $"{law} is adopted from its effects and counted");
        puzzle.Control("undo");
        Check(puzzle.Members().SequenceEqual(start) && (int)puzzle.Hud()["moveCount"]! == 0, $"undo takes back {law}");
    }

    private static void ResetAndPointer(IEngineContext engine)
    {
        using PackedContent shipped = PackedContent.Shipped();
        using Harness puzzle = new(engine, shipped);
        string[] start = puzzle.Members();
        var layout = puzzle.Product.View.Layout;
        var grid = puzzle.Product.Session.Board.Grid;
        puzzle.PressAt(Harness.TopOf(layout.Piece(grid, new(1, 1))));
        puzzle.PressAt(Harness.TopOf(layout.Terrain(grid, new(2, 1))));
        Check(puzzle.Members()[0] == "fighter@2,1" && puzzle.Selected() == (2, 1, "fighter"),
            "pressing a selected member's legal target makes the move, and the selection follows the member");
        puzzle.PressAt(Harness.TopOf(layout.Terrain(grid, new(3, 1))));
        string[] played = puzzle.Members();
        Check((int)puzzle.Hud()["moveCount"]! == 2, "each pointer move counts once");

        puzzle.Control("reset");
        Check(puzzle.Members().SequenceEqual(start) && (int)puzzle.Hud()["moveCount"]! == 0, "reset returns the room to its authored start");
        puzzle.Control("undo");
        Check(puzzle.Members().SequenceEqual(played) && (int)puzzle.Hud()["moveCount"]! == 2, "undo takes back a reset");
        puzzle.Control("undo");
        puzzle.Control("undo");
        Check(puzzle.Members().SequenceEqual(start) && !Enabled(puzzle, "undo") && !Enabled(puzzle, "reset"),
            "undo steps back to the start, where undo and reset are disabled");
    }

    private static void Win(IEngineContext engine)
    {
        using PackedContent content = PackedContent.WithRoom(["######", "#1.EE#", "######"], new() { ['1'] = "fighter" });
        content.Write("campaign/rooms.json", "{\"order\": [\"check\", \"first-steps\"]}");
        using Harness puzzle = new(engine, content);
        puzzle.MoveVia(1, 1, 2, 1);
        Check(!(bool)puzzle.Hud()["solved"]! && Controls(puzzle).All(id => id != "next-room"), "a party off the exits has not solved the room");
        puzzle.MoveVia(2, 1, 3, 1);
        Check((bool)puzzle.Hud()["solved"]! && ((string?)puzzle.Hud()["status"])!.Contains("solved in 2 moves", StringComparison.Ordinal),
            "the room is solved when the whole party stands on exit cells, and the projection says so with the move count");
        Check(puzzle.Hud()["moves"]!.AsArray().Count == 0, "a solved room offers no moves");
        Check(((string?)puzzle.Hud()["status"])!.Contains("next room", StringComparison.Ordinal), "a solved room with a next room points to it");
        ulong revision = puzzle.Revision();
        puzzle.Send($"{{\"action\":\"move\",\"member\":\"fighter\",\"column\":4,\"row\":1,\"revision\":{revision}}}");
        Check((string?)puzzle.Observe()["refused"] == "RoomSolved" && puzzle.Revision() == revision, "a move in a solved room is refused");

        puzzle.Control("next-room");
        Check((string?)puzzle.Observe()["room"] == "first-steps" && (int)puzzle.Hud()["moveCount"]! == 0 && !(bool)puzzle.Hud()["solved"]!,
            "the next-room control starts the next room of the authored order");
        puzzle.Control("undo");
        Check((string?)puzzle.Observe()["room"] == "check" && (bool)puzzle.Hud()["solved"]!, "undo returns to the solved room");

        using PackedContent last = PackedContent.WithRoom(["#####", "#1E.#", "#####"], new() { ['1'] = "fighter" });
        using Harness only = new(engine, last);
        only.MoveVia(1, 1, 2, 1);
        Check((bool)only.Hud()["solved"]! && ((string?)only.Hud()["status"])!.Contains("last room", StringComparison.Ordinal)
            && Controls(only).All(id => id != "next-room"), "the last room's solved line offers no next room");
    }

    private static void Refusals(IEngineContext engine)
    {
        using PackedContent content = PackedContent.WithRoom(["#####", "#.R.#", "#.F##", "#...#", "#EEE#", "#####"],
            new() { ['F'] = "fighter", ['R'] = "ranger" });
        using Harness puzzle = new(engine, content);
        string[] start = puzzle.Members();
        ulong revision = puzzle.Revision();

        void Refused(string json, string refusal, string what)
        {
            puzzle.Send(json);
            Check((string?)puzzle.Observe()["refused"] == refusal && puzzle.Members().SequenceEqual(start) && puzzle.Revision() == revision
                && !string.IsNullOrEmpty((string?)puzzle.Hud()["status"]), $"{what} is refused, changes nothing and is explained");
        }

        Refused($"{{\"action\":\"move\",\"member\":\"mage\",\"column\":1,\"row\":2,\"revision\":{revision}}}", "NoSuchMember",
            "moving a member who is not on the board");
        Refused($"{{\"action\":\"move\",\"member\":\"fighter\",\"column\":3,\"row\":3,\"revision\":{revision}}}", "IllegalMove",
            "a destination the member's law does not allow");
        Refused($"{{\"action\":\"move\",\"member\":\"fighter\",\"column\":1,\"row\":2,\"revision\":{revision + 1}}}", "StaleRevision",
            "a move made against another revision");
        Refused("{\"action\":\"move\",\"member\":\"fighter\",\"column\":1,\"row\":2}", "Incomplete", "a move without a revision");
        Refused($"{{\"action\":\"undo\",\"revision\":{revision}}}", "NothingToUndo", "undo at the start");
        Refused($"{{\"action\":\"room\",\"room\":\"nowhere\",\"revision\":{revision}}}", "NoSuchRoom", "an unknown room");

        puzzle.MoveVia(2, 2, 1, 2);
        JsonNode? stale = JsonNode.Parse($"{{\"action\":\"undo\",\"revision\":{revision}}}");
        puzzle.Send(stale!.ToJsonString());
        Check((string?)puzzle.Observe()["refused"] == "StaleRevision" && puzzle.Members().Contains("fighter@1,2"),
            "a control sent from an older projection is refused rather than undoing a move nobody saw");
    }

    private static bool Enabled(Harness puzzle, string id) =>
        (bool)puzzle.Hud()["controls"]!.AsArray().Single(control => (string?)control!["id"] == id)!["enabled"]!;

    private static IEnumerable<string?> Controls(Harness puzzle) =>
        puzzle.Hud()["controls"]!.AsArray().Select(control => (string?)control!["id"]);
}
