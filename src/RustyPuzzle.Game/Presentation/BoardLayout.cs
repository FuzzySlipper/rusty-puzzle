using System.Numerics;
using RustyPuzzle.Game.Board;
using RustyPuzzle.Game.Rooms;

namespace RustyPuzzle.Game.Presentation;

/// <summary>An axis-aligned block standing for one board cell, or for what stands on it.</summary>
internal readonly record struct CellBlock(Cell Cell, Vector3 Centre, Vector3 Size)
{
    internal Vector3 Minimum => Centre - (Size / 2);

    internal Vector3 Maximum => Centre + (Size / 2);

    /// <summary>The distance along a ray to where it enters this block, or null when it misses.</summary>
    internal float? Entry(Vector3 origin, Vector3 direction)
    {
        float near = float.NegativeInfinity;
        float far = float.PositiveInfinity;
        for (int axis = 0; axis < 3; axis++)
        {
            float start = origin[axis];
            float step = direction[axis];
            float low = Minimum[axis];
            float high = Maximum[axis];
            if (step == 0)
            {
                if (start < low || start > high)
                {
                    return null;
                }

                continue;
            }

            float first = (low - start) / step;
            float second = (high - start) / step;
            near = Math.Max(near, Math.Min(first, second));
            far = Math.Min(far, Math.Max(first, second));
        }

        return near <= far && far >= 0 ? Math.Max(near, 0) : null;
    }
}

/// <summary>
/// Where the board's blocks stand in the world. Cell (column, row) covers x from column to column + 1 and
/// z from row to row + 1, with the ground at y = 0. The scene draws these blocks and picking tests them,
/// so what the player clicks is what was drawn.
/// </summary>
internal sealed class BoardLayout(BoardViewTuning tuning)
{
    internal CellBlock Terrain(BoardGrid grid, Cell cell)
    {
        float height = grid.TerrainAt(cell).Height;
        float side = 1 - tuning.CellGap;
        return new CellBlock(cell, Ground(cell) + new Vector3(0, height / 2, 0), new Vector3(side, height, side));
    }

    internal CellBlock Piece(BoardGrid grid, Cell cell)
    {
        PieceLook piece = tuning.Piece;
        return new CellBlock(cell, Top(grid, cell) + new Vector3(0, piece.Height / 2, 0), new Vector3(piece.Width, piece.Height, piece.Width));
    }

    internal CellBlock Selection(BoardGrid grid, Cell cell)
    {
        float side = 1 - (2 * tuning.SelectionInset);
        float height = tuning.SelectionHeight;
        return new CellBlock(cell, Top(grid, cell) + new Vector3(0, height / 2, 0), new Vector3(side, height, side));
    }

    /// <summary>Every block a pointer can pick in the room: each cell's terrain and each member's piece.</summary>
    internal IEnumerable<CellBlock> Pickable(RoomState room)
    {
        foreach (Cell cell in room.Room.Grid.Cells())
        {
            yield return Terrain(room.Room.Grid, cell);
        }

        foreach (Placement placed in room.Placements)
        {
            yield return Piece(room.Room.Grid, placed.Cell);
        }
    }

    /// <summary>
    /// The cell whose block a ray reaches first. A ray through the gaps between blocks picks the cell
    /// where it meets the ground; null when it misses the board.
    /// </summary>
    internal Cell? Pick(RoomState room, Vector3 origin, Vector3 direction)
    {
        Cell? nearest = null;
        float best = float.PositiveInfinity;
        foreach (CellBlock block in Pickable(room))
        {
            if (block.Entry(origin, direction) is float distance && distance < best)
            {
                best = distance;
                nearest = block.Cell;
            }
        }

        if (nearest is not null || direction.Y >= 0 || origin.Y <= 0)
        {
            return nearest;
        }

        Vector3 ground = origin + (direction * (-origin.Y / direction.Y));
        Cell under = new((int)MathF.Floor(ground.X), (int)MathF.Floor(ground.Z));
        return room.Room.Grid.Contains(under) ? under : null;
    }

    private static Vector3 Ground(Cell cell) => new(cell.Column + 0.5f, 0, cell.Row + 0.5f);

    private static Vector3 Top(BoardGrid grid, Cell cell) => Ground(cell) + new Vector3(0, grid.TerrainAt(cell).Height, 0);
}
