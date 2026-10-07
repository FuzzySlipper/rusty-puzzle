using System.Numerics;
using Rusty.Engine;
using Rusty.Engine.Debugging;
using RustyPuzzle.Game.Board;
using RustyPuzzle.Game.Content;
using RustyPuzzle.Game.Interface;
using RustyPuzzle.Game.Presentation;
using RustyPuzzle.Game.Rooms;

namespace RustyPuzzle.Game;

/// <summary>
/// The Engine product: loads the authored content, plays the first room of the authored order, turns
/// admitted pointer presses and interface commands into board selections, and publishes the board scene
/// and the interface projection.
/// </summary>
public sealed class RustyPuzzleProduct : IEngineProduct, IDebugCommandModuleSource
{
    private readonly IEngineContext _engine;
    private readonly PuzzleContent _content;
    private readonly BoardLayout _layout;
    private readonly BoardCamera _camera;
    private readonly BoardScene _scene;
    private readonly PuzzleHud _hud;
    private bool _disposed;

    public RustyPuzzleProduct(ProductCreateContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        _engine = context.Engine;
        try
        {
            _content = PuzzleContent.Load(context.Content);
            _layout = new BoardLayout(_content.View);
            _camera = new BoardCamera(_engine, _content.View);
            _scene = new BoardScene(_engine.Graphics, _layout, _content.View);
            _hud = new PuzzleHud(_engine.Ui, _content.Text);
            Room = new RoomState(_content.Rooms[0]);
        }
        catch
        {
            Dispose();
            throw;
        }
    }

    internal RoomState Room { get; }

    internal BoardLayout Layout => _layout;

    internal BoardCamera Camera => _camera;

    internal PuzzleHud Hud => _hud;

    public void Start() => Publish();

    public ProductUpdateResult Update(ProductUpdate update)
    {
        Apply(update.Input);
        Publish();
        return ProductUpdateResult.None;
    }

    // Selecting is an interface fact, so the party list still answers while the runtime is paused.
    public void HandlePausedIntents(ReadOnlySpan<ProductInputEvent> intents)
    {
        Apply(intents);
        Publish();
    }

    public void Pause()
    {
    }

    public void Resume()
    {
    }

    public void Restart()
    {
        Room.Reset();
        Publish();
    }

    public void Shutdown()
    {
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        _scene?.Dispose();
        _camera?.Dispose();
        _hud?.Dispose();
    }

    public void RegisterDebugCommands(IDebugCommandModuleRegistrar registrar)
    {
        PuzzleDebugCommands commands = new(this);
        registrar.Register(new PlaytestDebugModule(commands.Inspect, commands.PlaytestAction, [], commands.Look));
        registrar.Register(commands);
    }

    /// <summary>Selects the cell drawn at a pointer position (normalized, bottom-left origin); off the board clears.</summary>
    internal void PickAt(Vector2 point)
    {
        _camera.Frame(Room, _layout);
        Cell? picked = _camera.Pick(Room, _layout, point);
        Room.Select(picked);
    }

    internal bool Apply(PuzzleCommand command) => command.ApplyTo(Room);

    /// <summary>Shows the current room state in the scene, the camera and the interface.</summary>
    internal void Publish()
    {
        _camera.Frame(Room, _layout);
        _scene.Publish(Room);
        _hud.Publish(Room);
    }

    private void Apply(ReadOnlySpan<ProductInputEvent> input)
    {
        foreach (ProductInputEvent item in input)
        {
            if (item is { Kind: InputEventKind.PointerButton, PointerButton: PointerButton.Primary, Edge: InputEdge.Pressed, HasPosition: true })
            {
                PickAt(new Vector2(item.X, item.Y));
            }
            else if (PuzzleCommand.From(item) is PuzzleCommand command)
            {
                Apply(command);
            }
        }
    }
}
