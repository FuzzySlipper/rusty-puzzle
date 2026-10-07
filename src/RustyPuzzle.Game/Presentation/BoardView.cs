using System.Numerics;
using Rusty.Engine;
using RustyPuzzle.Game.Board;
using RustyPuzzle.Game.Rooms;

namespace RustyPuzzle.Game.Presentation;

/// <summary>
/// The board's Engine presentation for one load of the view tuning: the shared block layout, the fitted
/// camera and the scene. A content reload replaces the whole view, so nothing drawn outlives its tuning.
/// </summary>
internal sealed class BoardView : IDisposable
{
    private readonly BoardScene _scene;

    internal BoardView(IEngineContext engine, BoardViewTuning tuning)
    {
        Layout = new BoardLayout(tuning);
        Camera = new BoardCamera(engine, tuning);
        try
        {
            _scene = new BoardScene(engine.Graphics, Layout, tuning);
        }
        catch
        {
            Camera.Dispose();
            throw;
        }
    }

    internal BoardLayout Layout { get; }

    internal BoardCamera Camera { get; }

    /// <summary>The cell drawn at a pointer position (normalized, bottom-left origin), or null off the board.</summary>
    internal Cell? Pick(RoomState room, Vector2 point)
    {
        Camera.Frame(room, Layout);
        return Camera.Pick(room, Layout, point);
    }

    /// <summary>Fits the camera and draws the room; each part republishes only when its input changed.</summary>
    internal void Publish(RoomState room)
    {
        Camera.Frame(room, Layout);
        _scene.Publish(room);
    }

    public void Dispose()
    {
        _scene.Dispose();
        Camera.Dispose();
    }
}
