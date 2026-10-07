namespace RustyPuzzle.Game.Board;

/// <summary>A room's terrain: one kind per cell, stored row by row.</summary>
internal sealed class BoardGrid
{
    private readonly TerrainKind[] _cells;

    internal BoardGrid(int width, int height, TerrainKind[] cells)
    {
        if (cells.Length != width * height)
        {
            throw new ArgumentException("A board needs exactly one terrain kind per cell.", nameof(cells));
        }

        Width = width;
        Height = height;
        _cells = cells;
    }

    internal int Width { get; }

    internal int Height { get; }

    internal bool Contains(Cell cell) => cell.Column >= 0 && cell.Column < Width && cell.Row >= 0 && cell.Row < Height;

    internal TerrainKind TerrainAt(Cell cell) => Contains(cell)
        ? _cells[(cell.Row * Width) + cell.Column]
        : throw new ArgumentOutOfRangeException(nameof(cell), cell, "The cell is outside the board.");

    /// <summary>Every cell, row by row.</summary>
    internal IEnumerable<Cell> Cells()
    {
        for (int row = 0; row < Height; row++)
        {
            for (int column = 0; column < Width; column++)
            {
                yield return new Cell(column, row);
            }
        }
    }
}
