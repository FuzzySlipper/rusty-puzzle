using System.Numerics;
using Rusty.Engine;
using RustyPuzzle.Game.Board;
using RustyPuzzle.Game.Rooms;

namespace RustyPuzzle.Game.Presentation;

/// <summary>
/// Draws the room in the Engine as placeholder primitives: a block per cell in its terrain kind's colour, a
/// ball per party member in the member's colour, and a marker over the selected cell. Cells of one kind share an
/// appearance. The scene republishes only when the room state changed.
/// </summary>
internal sealed class BoardScene : IDisposable
{
    private const ulong TerrainObjects = 1_000;
    private const ulong PieceObjects = 100_000;
    private const ulong SelectionObject = 200_000;

    private readonly IGraphicsService _graphics;
    private readonly BoardLayout _layout;
    private readonly Dictionary<string, Appearance> _terrain = new(StringComparer.Ordinal);
    private readonly Dictionary<string, Appearance> _pieces = new(StringComparer.Ordinal);
    private readonly Appearance _selection;
    private (RoomState Room, ulong Revision)? _published;

    internal BoardScene(IGraphicsService graphics, BoardLayout layout, BoardViewTuning tuning)
    {
        _graphics = graphics;
        _layout = layout;
        _selection = Primitive(tuning.SelectionColour, PrimitiveGeometry.Cube);
    }

    internal void Publish(RoomState room)
    {
        if (_published is var (shown, revision) && ReferenceEquals(shown, room) && revision == room.Revision)
        {
            return;
        }

        BoardGrid grid = room.Room.Grid;
        List<AppearanceFact> facts = [];
        ulong id = TerrainObjects;
        foreach (Cell cell in grid.Cells())
        {
            TerrainKind kind = grid.TerrainAt(cell);
            facts.Add(Fact(id++, Appearance(_terrain, kind.Id, kind.Colour, PrimitiveGeometry.Cube), _layout.Terrain(grid, cell)));
        }

        id = PieceObjects;
        foreach (Placement placed in room.Placements)
        {
            facts.Add(Fact(id++, Appearance(_pieces, placed.Member.Id, placed.Member.Colour, PrimitiveGeometry.Sphere), _layout.Piece(grid, placed.Cell)));
        }

        if (room.Selected is Cell selected)
        {
            facts.Add(Fact(SelectionObject, _selection, _layout.Selection(grid, selected)));
        }

        _graphics.PublishSnapshot(facts.ToArray());
        _published = (room, room.Revision);
    }

    public void Dispose()
    {
        // Nothing may still be published when it is released.
        _graphics.PublishSnapshot(Array.Empty<AppearanceFact>());
        foreach (Appearance appearance in _terrain.Values.Concat(_pieces.Values))
        {
            appearance.Dispose();
        }

        _selection.Dispose();
    }

    private Appearance Appearance(Dictionary<string, Appearance> shared, string id, Color colour, PrimitiveGeometry geometry)
    {
        if (!shared.TryGetValue(id, out Appearance? appearance))
        {
            appearance = Primitive(colour, geometry);
            shared.Add(id, appearance);
        }

        return appearance;
    }

    private Appearance Primitive(Color colour, PrimitiveGeometry geometry) =>
        _graphics.CreatePrimitive(new PrimitiveAppearanceRequest(geometry, false, colour));

    // The cube and sphere primitives fill a unit box about their centre.
    private static AppearanceFact Fact(ulong id, Appearance appearance, CellBlock block) =>
        new(id, false, 0, new Transform(block.Centre, Quaternion.Identity, block.Size), appearance, true, RenderLayer.Scene);
}
