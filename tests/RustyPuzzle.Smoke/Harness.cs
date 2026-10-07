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
        Intent = Encoding.UTF8.GetBytes(PuzzleCommand.Intent),
        PayloadContract = Encoding.UTF8.GetBytes(PuzzleCommand.Contract),
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
