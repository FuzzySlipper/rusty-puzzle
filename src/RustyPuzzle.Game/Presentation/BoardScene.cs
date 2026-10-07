using System.Numerics;
using Rusty.Engine;
using RustyPuzzle.Game.Board;
using RustyPuzzle.Game.Movement;
using RustyPuzzle.Game.Party;

namespace RustyPuzzle.Game.Presentation;

/// <summary>
/// Draws a <see cref="BoardPicture"/> in the Engine as placeholder primitives: a block per cell in its terrain
/// kind's colour (every other cell shaded, as a checkerboard), a ball per party member in the member's colour, a marker over the selected cell and over each
/// legal move's target, and a small token in each member's colour where the previewed move would put them.
/// Cells of one kind share an appearance. The scene republishes only when the picture changed.
/// </summary>
internal sealed class BoardScene : IDisposable
{
    private const ulong TerrainObjects = 1_000;
    private const ulong PieceObjects = 100_000;
    private const ulong SelectionObject = 200_000;
    private const ulong DestinationObjects = 300_000;
    private const ulong PreviewObjects = 400_000;

    private readonly IGraphicsService _graphics;
    private readonly BoardLayout _layout;
    private readonly BoardViewTuning _tuning;
    private readonly Dictionary<string, Appearance> _terrain = new(StringComparer.Ordinal);
    private readonly Dictionary<string, Appearance> _pieces = new(StringComparer.Ordinal);
    private readonly Appearance _selection;
    private readonly Appearance _destination;
    private BoardPicture? _published;

    internal BoardScene(IGraphicsService graphics, BoardLayout layout, BoardViewTuning tuning)
    {
        _graphics = graphics;
        _layout = layout;
        _tuning = tuning;
        _selection = Primitive(tuning.Selection.Colour, PrimitiveGeometry.Cube);
        _destination = Primitive(tuning.Destination.Colour, PrimitiveGeometry.Cube);
    }

    internal void Publish(BoardPicture picture)
    {
        if (picture == _published)
        {
            return;
        }

        BoardGrid grid = picture.Board.Grid;
        List<AppearanceFact> facts = [];
        ulong id = TerrainObjects;
        foreach (Cell cell in grid.Cells())
        {
            facts.Add(Fact(id++, Terrain(grid.TerrainAt(cell), cell), _layout.Terrain(grid, cell)));
        }

        id = PieceObjects;
        foreach (Placement placed in picture.Board.Placements)
        {
            facts.Add(Fact(id++, Piece(placed.Member), _layout.Piece(grid, placed.Cell)));
        }

        if (picture.Selected is Cell selected)
        {
            facts.Add(Fact(SelectionObject, _selection, BoardLayout.Marker(grid, selected, _tuning.Selection)));
        }

        id = DestinationObjects;
        foreach (Move move in picture.Moves)
        {
            facts.Add(Fact(id++, _destination, BoardLayout.Marker(grid, move.Target, _tuning.Destination)));
        }

        id = PreviewObjects;
        foreach (BoardEffect effect in picture.Preview?.Effects ?? [])
        {
            if (effect is Relocated relocated)
            {
                facts.Add(Fact(id++, Piece(relocated.Member), _layout.Preview(grid, relocated.To)));
            }
        }

        _graphics.PublishSnapshot(facts.ToArray());
        _published = picture;
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
        _destination.Dispose();
    }

    /// <summary>A cell's terrain block: every other cell is shaded so the checkerboard the laws keep to shows.</summary>
    private Appearance Terrain(TerrainKind kind, Cell cell)
    {
        if ((cell.Column + cell.Row) % 2 == 0)
        {
            return Appearance(_terrain, kind.Id, kind.Colour, PrimitiveGeometry.Cube);
        }

        float shade = _tuning.CheckerShade;
        return Appearance(_terrain, $"{kind.Id}:shaded", new Color(kind.Colour.R * shade, kind.Colour.G * shade, kind.Colour.B * shade, kind.Colour.A),
            PrimitiveGeometry.Cube);
    }

    private Appearance Piece(PartyMember member) => Appearance(_pieces, member.Id, member.Colour, PrimitiveGeometry.Sphere);

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
