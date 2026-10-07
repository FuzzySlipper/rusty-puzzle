using System.Numerics;
using Rusty.Engine;
using Rusty.Engine.Debugging;
using RustyPuzzle.Game.Board;
using RustyPuzzle.Game.Content;
using RustyPuzzle.Game.Interface;
using RustyPuzzle.Game.Persistence;
using RustyPuzzle.Game.Presentation;
using RustyPuzzle.Game.Session;

namespace RustyPuzzle.Game;

/// <summary>
/// The Engine product: loads the authored content domains, plays the authored rooms through the session,
/// turns admitted pointer presses and interface commands into session commands, and publishes the board view
/// and the interface projection. Restart reloads the content, so edited content bundles show without a rebuild.
/// </summary>
public sealed class RustyPuzzleProduct : IEngineProduct, IDebugCommandModuleSource
{
    private readonly IEngineContext _engine;
    private readonly AuthoredContent _authored;
    private readonly PuzzleHud _hud;
    private readonly ProgressStore _progress;
    private PuzzleContent _content;
    private BoardView _view;
    // The cell under a free pointer: the legal move targeting it is previewed. Presentation only.
    private Cell? _pointed;
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
            _progress = new ProgressStore(_engine);
            _view = new BoardView(_engine, _content.View);
            Session = new PuzzleSession(_content.Rooms);
        }
        catch
        {
            Dispose();
            throw;
        }
    }

    internal PuzzleSession Session { get; private set; }

    internal BoardView View => _view;

    internal PuzzleContent Content => _content;

    internal PuzzleHud Hud => _hud;

    internal ProgressStore Progress => _progress;

    public void Start()
    {
        _progress.Load();
        Publish();
    }

    public ProductUpdateResult Update(ProductUpdate update)
    {
        if (update.Input.IsEmpty)
        {
            // Nothing was asked; the board view still follows layout changes.
            _view.Publish(Picture());
            return ProductUpdateResult.None;
        }

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
    /// Reloads every content domain and starts a new session at the first room's authored start. Content that
    /// fails to load leaves the current session, view and content in place and faults with the file named.
    /// </summary>
    public void Restart()
    {
        PuzzleContent content = PuzzleContent.Load(_authored);
        BoardView view = new(_engine, content.View);
        // The old view clears the published scene as it goes; the publish below draws the new one.
        _view.Dispose();
        _view = view;
        _content = content;
        Session = new PuzzleSession(content.Rooms);
        _pointed = null;
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
        _progress?.Dispose();
    }

    public void RegisterDebugCommands(IDebugCommandModuleRegistrar registrar)
    {
        PuzzleDebugCommands commands = new(this);
        registrar.Register(new PlaytestDebugModule(commands.Inspect, commands.PlaytestAction, [], commands.Look));
        registrar.Register(commands);
    }

    /// <summary>A primary press at a pointer position (normalized, bottom-left origin), as the session reads presses.</summary>
    internal Receipt PressAt(Vector2 point) => Press(_view.Pick(Session.Board, point));

    /// <summary>Submits a command and records progress when it solves the room.</summary>
    internal Receipt Submit(PuzzleCommand command) => Record(Session.Submit(command));

    /// <summary>A press on a cell, as the session reads presses, recording progress when it solves the room.</summary>
    internal Receipt Press(Cell? cell) => Record(Session.Press(cell));

    /// <summary>Notes the cell under a free pointer, whose legal move (if any) the board previews.</summary>
    internal void PointAt(Vector2 point) => _pointed = _view.Pick(Session.Board, point);

    /// <summary>What the board view shows now.</summary>
    internal BoardPicture Picture() =>
        new(Session.Board, Session.Selected, Session.Moves, Session.Moves.FirstOrDefault(move => move.Target == _pointed));

    /// <summary>Shows the current room state in the board view and the interface.</summary>
    internal void Publish()
    {
        _view.Publish(Picture());
        _hud.Publish(Session, _content.Hud, _progress.Current);
    }

    // The solve boundary: the only moment progress is saved.
    private Receipt Record(Receipt receipt)
    {
        if (receipt.Solved is var (room, moves))
        {
            _progress.Solved(room, moves);
        }

        return receipt;
    }

    private void Apply(ReadOnlySpan<ProductInputEvent> input)
    {
        foreach (ProductInputEvent item in input)
        {
            if (item is { Kind: InputEventKind.PointerButton, PointerButton: PointerButton.Primary, Edge: InputEdge.Pressed, HasPosition: true })
            {
                PressAt(new Vector2(item.X, item.Y));
            }
            else if (item.Kind == InputEventKind.PointerPosition)
            {
                PointAt(new Vector2(item.X, item.Y));
            }
            else if (CommandPayload.From(item) is PuzzleCommand command)
            {
                Submit(command);
            }
        }
    }
}
