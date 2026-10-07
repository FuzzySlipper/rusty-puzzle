using System.Numerics;
using Rusty.Engine;
using Rusty.Engine.Debugging;
using RustyPuzzle.Game.Content;
using RustyPuzzle.Game.Interface;
using RustyPuzzle.Game.Presentation;
using RustyPuzzle.Game.Rooms;

namespace RustyPuzzle.Game;

/// <summary>
/// The Engine product: loads the authored content domains, plays the first room of the authored order, turns
/// admitted pointer presses and interface commands into board selections, and publishes the board view and
/// the interface projection. Restart reloads the content, so edited content bundles show without a rebuild.
/// </summary>
public sealed class RustyPuzzleProduct : IEngineProduct, IDebugCommandModuleSource
{
    private readonly IEngineContext _engine;
    private readonly AuthoredContent _authored;
    private readonly PuzzleHud _hud;
    private PuzzleContent _content;
    private BoardView _view;
    private bool _disposed;

    public RustyPuzzleProduct(ProductCreateContext context)
        : this(context, new AuthoredContent(context.Content.OpenBundle))
    {
    }

    /// <param name="authored">Where the content domains are opened from.</param>
    internal RustyPuzzleProduct(ProductCreateContext context, AuthoredContent authored)
    {
        ArgumentNullException.ThrowIfNull(context);
        _engine = context.Engine;
        _authored = authored;
        try
        {
            _content = PuzzleContent.Load(authored);
            _hud = new PuzzleHud(_engine.Ui);
            _view = new BoardView(_engine, _content.View);
            Room = new RoomState(_content.Rooms[0]);
        }
        catch
        {
            Dispose();
            throw;
        }
    }

    internal RoomState Room { get; private set; }

    internal BoardView View => _view;

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

    /// <summary>
    /// Reloads every content domain and returns to the first room's authored start. Content that fails to
    /// load leaves the current room, view and content in place and faults with the file named.
    /// </summary>
    public void Restart()
    {
        PuzzleContent content = PuzzleContent.Load(_authored);
        BoardView view = new(_engine, content.View);
        // The old view clears the published scene as it goes; the publish below draws the new one.
        _view.Dispose();
        _view = view;
        _content = content;
        Room = new RoomState(content.Rooms[0]);
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
        _view?.Dispose();
        _hud?.Dispose();
    }

    public void RegisterDebugCommands(IDebugCommandModuleRegistrar registrar)
    {
        PuzzleDebugCommands commands = new(this);
        registrar.Register(new PlaytestDebugModule(commands.Inspect, commands.PlaytestAction, [], commands.Look));
        registrar.Register(commands);
    }

    /// <summary>Selects the cell drawn at a pointer position (normalized, bottom-left origin); off the board clears.</summary>
    internal void PickAt(Vector2 point) => Room.Select(_view.Pick(Room, point));

    internal bool Apply(PuzzleCommand command) => command.ApplyTo(Room);

    /// <summary>Shows the current room state in the board view and the interface.</summary>
    internal void Publish()
    {
        _view.Publish(Room);
        _hud.Publish(Room, _content.Hud);
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
