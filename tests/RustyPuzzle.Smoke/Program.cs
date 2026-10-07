using System.Numerics;
using System.Text;
using System.Text.Json.Nodes;
using Rusty.Engine;
using Rusty.Engine.Debugging;
using Rusty.Engine.Testing;
using RustyPuzzle.Game;
using RustyPuzzle.Game.Board;
using RustyPuzzle.Game.Interface;
using RustyPuzzle.Game.Presentation;

// Product callbacks over the real pinned Engine services, without a browser: content loading, the board
// owner, pointer picking through the fitted camera, interface commands and the projection. This checks
// callback policy, not host lifecycle admission or what the renderer shows.
string contentRoot = Path.Combine(AppContext.BaseDirectory, "content");
Dictionary<string, ReadOnlyMemory<byte>> files = Directory.GetFiles(contentRoot, "*", SearchOption.AllDirectories)
    .ToDictionary(path => Path.GetRelativePath(contentRoot, path).Replace('\\', '/'), path => (ReadOnlyMemory<byte>)File.ReadAllBytes(path));

using EngineTestHost host = EngineTestHost.Create(new EngineTestHostOptions { Content = files });
host.Call(engine =>
{
    using RustyPuzzleProduct product = Create(engine, files);
    CaptureCommands commands = new();
    product.RegisterDebugCommands(commands);
    product.Start();
    ulong step = 0;

    void Advance(params ProductInputEvent[] input) =>
        product.Update(new ProductUpdate(new ProductUpdateFacts(ProductUpdateMode.Demand, ProductLifecycleState.Running,
            1, 1, 0, step++, 0, 1, 0, 0), input));
    JsonObject Observe() => JsonNode.Parse(commands.Playtest!.Observe().Message)!.AsObject();
    JsonObject Hud() => product.Hud.Current!;
    (int, int, string?)? Selected()
    {
        JsonNode? selected = Observe()["selected"];
        return selected is null ? null : ((int)selected["column"]!, (int)selected["row"]!, (string?)selected["member"]);
    }
    // Presses the primary pointer where the camera draws a world point.
    void PressAt(Vector3 world)
    {
        Vector2 point = CameraQueries.Project(product.Camera.Descriptor, product.Camera.Aspect, world).NormalizedPoint;
        Advance(default(ProductInputEvent) with
        {
            Kind = InputEventKind.PointerButton, PointerButton = PointerButton.Primary, Edge = InputEdge.Pressed,
            HasPosition = true, X = point.X, Y = point.Y,
        });
    }
    Vector3 TopOf(CellBlock block) => block.Centre with { Y = block.Maximum.Y };
    BoardGrid grid = product.Room.Room.Grid;

    JsonObject board = Observe();
    Check((string?)board["room"] == "first-steps" && (int)board["width"]! == 8 && (int)board["height"]! == 8,
        "the first room of the authored order loads at its authored size");
    Check(board["rows"]!.AsArray().Select(row => (string?)row).SequenceEqual(
        ["########", "#..#..E#", "#..#...#", "#......#", "#...#..#", "#...#..#", "#......#", "########"]),
        "party markers stand on the start terrain and the rest keep their symbols");
    Check(board["members"]!.AsArray().Select(m => $"{m!["id"]}@{m["column"]},{m["row"]}").SequenceEqual(
        ["fighter@1,1", "ranger@1,3", "rogue@1,5", "mage@1,6"]), "each member starts on its marker");
    Check(Selected() is null && (string?)Hud()["room"] == "First Steps" && Hud()["party"]!.AsArray().Count == 4,
        "the projection names the room and lists the party with nothing selected");

    PressAt(TopOf(product.Layout.Terrain(grid, new Cell(5, 2))));
    Check(Selected() == (5, 2, null), "a press on a floor cell selects that cell");
    Check(((string?)Hud()["status"])!.Contains("column 6, row 3", StringComparison.Ordinal), "the projection describes the selected cell");

    PressAt(TopOf(product.Layout.Terrain(grid, new Cell(3, 1))));
    Check(Selected() == (3, 1, null), "a press on a wall's top selects the wall cell, not the floor behind it");

    PressAt(TopOf(product.Layout.Piece(grid, new Cell(1, 1))));
    Check(Selected() == (1, 1, "fighter") && (string?)Hud()["selected"] == "Fighter", "a press on a member's piece selects that member");

    PressAt(new Vector3(-6, 0, -6));
    Check(Selected() is null, "a press off the board clears the selection");

    JsonNode rangerCommand = Hud()["party"]!.AsArray().Single(m => (string?)m!["id"] == "ranger")!["command"]!;
    Advance(Claim(rangerCommand.ToJsonString()));
    Check(Selected() == (1, 3, "ranger"), "the party list's projected command selects that member");
    Advance(Claim("{\"action\":\"select\",\"column\":99,\"row\":0}"));
    Check(Selected() == (1, 3, "ranger"), "a select command off the board is refused");
    Advance(Claim("{\"action\":\"clear\"}"));
    Check(Selected() is null, "the clear command clears the selection");

    product.Pause();
    product.HandlePausedIntents([Claim(rangerCommand.ToJsonString())]);
    Check(Selected() == (1, 3, "ranger"), "a paused interface claim selects through the same command");
    product.Resume();
    product.Restart();
    Check(Selected() is null && (string?)Hud()["selected"] == "Nobody", "restart returns the room to its authored start");

    Check(commands.Puzzle!.Pick(0.5f, 0.5f).Succeeded && Selected() is not null, "the developer pick reaches the picking path");
    Check(JsonNode.Parse(commands.Playtest!.Action("anything").Message)!["available"]!.GetValue<bool>() == false, "playtest actions report that the board has no keyboard controls");

    Dictionary<string, ReadOnlyMemory<byte>> broken = new(files)
    {
        ["rooms/first-steps.json"] = Encoding.UTF8.GetBytes("{\"name\":\"Broken\",\"rows\":[\"#\"],\"party\":{}}"),
    };
    try
    {
        using RustyPuzzleProduct refused = Create(engine, broken);
        Check(false, "a room missing a required field is refused");
    }
    catch (InvalidOperationException error)
    {
        Check(error.Message.StartsWith("content/rooms/first-steps.json", StringComparison.Ordinal), "a content error names the failing file");
    }
});
Console.WriteLine("Rusty Puzzle smoke passed: room content, board, pointer picking, interface commands, restart and content errors.");

static RustyPuzzleProduct Create(IEngineContext engine, IReadOnlyDictionary<string, ReadOnlyMemory<byte>> files)
{
    ProductContentFile[] content = [.. files.Select(file => new ProductContentFile(Encoding.UTF8.GetBytes(file.Key), file.Value))];
    return new RustyPuzzleProduct(new ProductCreateContext(engine, new ProductContent(content),
        new ProductInputConfiguration(default, default, default, default, InputCursorMode.Unlocked), default!));
}

static ProductInputEvent Claim(string json) => default(ProductInputEvent) with
{
    Kind = InputEventKind.DirectProductPayload,
    ValueKind = InputValueKind.ProductPayload,
    Provenance = InputProvenance.DirectUi,
    Intent = Encoding.UTF8.GetBytes(PuzzleCommand.Intent),
    PayloadContract = Encoding.UTF8.GetBytes(PuzzleCommand.Contract),
    PayloadData = Encoding.UTF8.GetBytes(json),
};

static void Check(bool condition, string message)
{
    if (!condition)
    {
        throw new InvalidOperationException(message);
    }

    Console.WriteLine($"ok - {message}");
}

sealed class CaptureCommands : IDebugCommandModuleRegistrar
{
    internal PlaytestDebugModule? Playtest { get; private set; }
    internal PuzzleDebugCommands? Puzzle { get; private set; }

    DebugCommandRegistrationResult IDebugCommandModuleRegistrar.Register<TModule>(TModule module)
    {
        if (module is PlaytestDebugModule playtest) Playtest = playtest;
        if (module is PuzzleDebugCommands puzzle) Puzzle = puzzle;
        return new(DebugCommandRegistrationStatus.Registered, "Captured for product callback test");
    }
}
