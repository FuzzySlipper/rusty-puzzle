using System.Numerics;
using System.Text;
using System.Text.Json.Nodes;
using Rusty.Engine;
using Rusty.Engine.Debugging;
using RustyPuzzle.Game;
using RustyPuzzle.Game.Interface;
using RustyPuzzle.Game.Presentation;

/// <summary>
/// One product under the Engine test host, driven through its ordinary callbacks: demand updates carrying
/// pointer presses or interface claims, and observation through the playtest module and the projection.
/// </summary>
sealed class Harness : IDisposable
{
    private readonly CaptureCommands _commands = new();
    private ulong _step;

    internal Harness(IEngineContext engine, PackedContent content)
    {
        Product = Create(engine, content);
        Product.RegisterDebugCommands(_commands);
        Product.Start();
    }

    internal RustyPuzzleProduct Product { get; }

    internal PlaytestDebugModule Playtest => _commands.Playtest!;

    internal PuzzleDebugCommands Puzzle => _commands.Puzzle!;

    internal static RustyPuzzleProduct Create(IEngineContext engine, PackedContent content) =>
        new(new ProductCreateContext(engine, new ProductContent(default, engine.Content),
            new ProductInputConfiguration(default, default, default, default, InputCursorMode.Unlocked), default!),
            content.Authored(engine));

    internal void Advance(params ProductInputEvent[] input) =>
        Product.Update(new ProductUpdate(new ProductUpdateFacts(ProductUpdateMode.Demand, ProductLifecycleState.Running,
            1, 1, 0, _step++, 0, 1, 0, 0), input));

    internal JsonObject Observe() => JsonNode.Parse(Playtest.Observe().Message)!.AsObject();

    internal JsonObject Hud() => Product.Hud.Current!;

    /// <summary>Where each member stands, as "id@column,row" in authored order.</summary>
    internal string[] Members() =>
        [.. Observe()["members"]!.AsArray().Select(m => $"{m!["id"]}@{m["column"]},{m["row"]}")];

    internal ulong Revision() => (ulong)Observe()["revision"]!;

    /// <summary>Sends a command as the interface does.</summary>
    internal void Send(string json) => Advance(Claim(json));

    /// <summary>Sends the command the projection hands one of its controls ("undo", "reset", "next-room").</summary>
    internal void Control(string id) =>
        Send(Hud()["controls"]!.AsArray().Single(control => (string?)control!["id"] == id)!["command"]!.ToJsonString());

    /// <summary>Selects a member's cell, then sends the projected command of its legal move to the target, as the DOM does.</summary>
    internal void MoveVia(int column, int row, int targetColumn, int targetRow)
    {
        Send($"{{\"action\":\"select\",\"column\":{column},\"row\":{row}}}");
        Send(Hud()["moves"]!.AsArray().Single(move => (int)move!["command"]!["column"]! == targetColumn
            && (int)move["command"]!["row"]! == targetRow)!["command"]!.ToJsonString());
    }

    /// <summary>The selected cell and the member on it, or null.</summary>
    internal (int Column, int Row, string? Member)? Selected()
    {
        JsonNode? selected = Observe()["selected"];
        return selected is null ? null : ((int)selected["column"]!, (int)selected["row"]!, (string?)selected["member"]);
    }

    /// <summary>Presses the primary pointer where the board camera draws a world point.</summary>
    internal void PressAt(Vector3 world)
    {
        BoardCamera camera = Product.View.Camera;
        Vector2 point = CameraQueries.Project(camera.Descriptor, camera.Aspect, world).NormalizedPoint;
        Advance(default(ProductInputEvent) with
        {
            Kind = InputEventKind.PointerButton, PointerButton = PointerButton.Primary, Edge = InputEdge.Pressed,
            HasPosition = true, X = point.X, Y = point.Y,
        });
    }

    /// <summary>Moves a free pointer over where the board camera draws a world point.</summary>
    internal void PointAt(Vector3 world)
    {
        BoardCamera camera = Product.View.Camera;
        Vector2 point = CameraQueries.Project(camera.Descriptor, camera.Aspect, world).NormalizedPoint;
        Advance(default(ProductInputEvent) with { Kind = InputEventKind.PointerPosition, HasPosition = true, X = point.X, Y = point.Y });
    }

    internal static Vector3 TopOf(CellBlock block) => block.Centre with { Y = block.Maximum.Y };

    /// <summary>An interface claim of the product's payload intent.</summary>
    internal static ProductInputEvent Claim(string json) => default(ProductInputEvent) with
    {
        Kind = InputEventKind.DirectProductPayload,
        ValueKind = InputValueKind.ProductPayload,
        Provenance = InputProvenance.DirectUi,
        Intent = Encoding.UTF8.GetBytes(CommandPayload.Intent),
        PayloadContract = Encoding.UTF8.GetBytes(CommandPayload.Contract),
        PayloadData = Encoding.UTF8.GetBytes(json),
    };

    public void Dispose() => Product.Dispose();

    private sealed class CaptureCommands : IDebugCommandModuleRegistrar
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
}

static class Checks
{
    internal static void Check(bool condition, string message)
    {
        if (!condition)
        {
            throw new InvalidOperationException(message);
        }

        Console.WriteLine($"ok - {message}");
    }

    /// <summary>Runs an action that must fail, returning its error message.</summary>
    internal static string Refusal(Action action)
    {
        try
        {
            action();
        }
        catch (InvalidOperationException error)
        {
            return error.Message;
        }

        throw new InvalidOperationException("the action was expected to fail");
    }
}
