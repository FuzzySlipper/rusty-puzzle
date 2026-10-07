using System.Numerics;
using Rusty.Engine;
using RustyPuzzle.Game.Board;
using RustyPuzzle.Game.Rooms;

namespace RustyPuzzle.Game.Presentation;

/// <summary>
/// The board's one camera: orthographic, looking down the rows at the authored pitch, fitted so every
/// block of the room fills the view up to the authored margin. It also turns a pointer position on the
/// view into the cell drawn there.
/// </summary>
internal sealed class BoardCamera : IDisposable
{
    // Before the page reports its surface, the camera frames a widescreen view.
    private const double FallbackAspect = 16.0 / 9;
    private const double Near = 0.1;
    // Room in front of the nearest block and behind the farthest, in cells.
    private const double DepthMargin = 1;

    private readonly IEngineContext _engine;
    private readonly BoardViewTuning _tuning;
    private readonly Camera _camera;
    private ulong? _surfaceRevision;
    private RoomState? _framed;

    internal BoardCamera(IEngineContext engine, BoardViewTuning tuning)
    {
        _engine = engine;
        _tuning = tuning;
        Descriptor = Orthographic(new CameraPose(Vector3.Zero, -tuning.PitchDegrees, 0), 1, Near + DepthMargin);
        _camera = engine.CameraView.CreateCamera(Descriptor);
        engine.CameraView.SetActiveCamera(_camera);
        engine.CameraView.SetBackgroundColor(new SetBackgroundColorRequest(tuning.Background));
    }

    internal CameraDescriptor Descriptor { get; private set; }

    /// <summary>The width-to-height ratio of the view the camera was last fitted to.</summary>
    internal double Aspect { get; private set; } = FallbackAspect;

    /// <summary>Refits the camera when the room or the view's size changed since the last fit.</summary>
    internal void Frame(RoomState room, BoardLayout layout)
    {
        CameraSurfaceReadout surface = _engine.CameraView.ReadSurface();
        if (ReferenceEquals(room, _framed) && _surfaceRevision == surface.Revision)
        {
            return;
        }

        _framed = room;
        _surfaceRevision = surface.Revision;
        Aspect = surface.Reported && surface.CssWidth > 0 && surface.CssHeight > 0 ? surface.CssWidth / surface.CssHeight : FallbackAspect;
        Descriptor = Fit(room, layout);
        _engine.CameraView.UpdateCamera(new CameraUpdateRequest(_camera, Descriptor));
    }

    /// <summary>The cell drawn at a pointer position (normalized, bottom-left origin), or null off the board.</summary>
    internal Cell? Pick(RoomState room, BoardLayout layout, Vector2 point)
    {
        CameraRay ray = CameraQueries.Ray(Descriptor, Aspect, point);
        return layout.Pick(room, ray.Origin, ray.Direction);
    }

    public void Dispose() => _camera.Dispose();

    /// <summary>
    /// Measures every block corner from a trial camera over the board's centre, then moves the camera across
    /// the view to centre them, back along its view far enough to have them all in front, and sizes the view
    /// to hold them. The Engine's own projection supplies the view axes, so the fit matches what is drawn.
    /// </summary>
    private CameraDescriptor Fit(RoomState room, BoardLayout layout)
    {
        BoardGrid grid = room.Room.Grid;
        CameraPose pose = new(new Vector3(grid.Width / 2f, 0, grid.Height / 2f), -_tuning.PitchDegrees, 0);
        // A unit view at aspect 1 projects a point to 0.5 plus its offset across and up the view.
        CameraDescriptor trial = Orthographic(pose, 1, Near + DepthMargin);
        double left = double.MaxValue, right = double.MinValue, bottom = double.MaxValue, top = double.MinValue;
        double nearest = double.MaxValue, farthest = double.MinValue;
        foreach (CellBlock block in layout.Pickable(room))
        {
            foreach (Vector3 corner in Corners(block))
            {
                CameraProjectionResult seen = CameraQueries.Project(trial, 1, corner);
                left = Math.Min(left, seen.NormalizedPoint.X);
                right = Math.Max(right, seen.NormalizedPoint.X);
                bottom = Math.Min(bottom, seen.NormalizedPoint.Y);
                top = Math.Max(top, seen.NormalizedPoint.Y);
                nearest = Math.Min(nearest, seen.Depth);
                farthest = Math.Max(farthest, seen.Depth);
            }
        }

        // An orthographic ray starts on the near plane at its point and runs along the view.
        CameraRay across = CameraQueries.Ray(trial, 1, new Vector2((float)((left + right) / 2), (float)((bottom + top) / 2)));
        double back = Near + DepthMargin - nearest;
        Vector3 position = across.Origin - (across.Direction * (float)(Near + back));
        double size = Math.Max(top - bottom, (right - left) / Aspect) / _tuning.Fill;
        return Orthographic(pose with { Position = position }, size, back + farthest + DepthMargin);
    }

    private static CameraDescriptor Orthographic(CameraPose pose, double verticalSize, double far) =>
        new(pose, CameraBasisMode.Derived, default,
            new CameraProjection(CameraProjectionKind.Orthographic, 0, verticalSize, Near, far), CameraViewports.Full);

    private static IEnumerable<Vector3> Corners(CellBlock block)
    {
        Vector3 low = block.Minimum;
        Vector3 high = block.Maximum;
        for (int corner = 0; corner < 8; corner++)
        {
            yield return new Vector3((corner & 1) == 0 ? low.X : high.X, (corner & 2) == 0 ? low.Y : high.Y, (corner & 4) == 0 ? low.Z : high.Z);
        }
    }
}
