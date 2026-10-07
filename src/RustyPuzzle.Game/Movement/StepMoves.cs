using RustyPuzzle.Game.Board;
using RustyPuzzle.Game.Party;

namespace RustyPuzzle.Game.Movement;

/// <summary>Evaluates <see cref="StepMove"/>: straight lines, no turns.</summary>
internal static class StepMoves
{
    private static readonly Cell[] OrthogonalSteps = [new(0, -1), new(1, 0), new(0, 1), new(-1, 0)];
    private static readonly Cell[] DiagonalSteps = [new(-1, -1), new(1, -1), new(1, 1), new(-1, 1)];

    internal static IEnumerable<Move> Options(BoardState state, PartyMember member, Cell from, StepMove step)
    {
        foreach (Cell direction in Steps(step.Directions))
        {
            for (int distance = 1; distance <= step.Distance.Max; distance++)
            {
                Cell cell = new(from.Column + (direction.Column * distance), from.Row + (direction.Row * distance));
                if (distance >= step.Distance.Min && state.Open(cell))
                {
                    yield return new Move(member, cell, step, [new Relocated(member, from, cell)]);
                }

                // The cell is crossed on the way to a farther landing only if the path rule lets it be.
                if (!Crossable(state, cell, step.Path))
                {
                    break;
                }
            }
        }
    }

    private static bool Crossable(BoardState state, Cell cell, PathRule path) => path switch
    {
        PathRule.Clear => state.Open(cell),
        PathRule.OverParty => state.Grid.Contains(cell) && state.Grid.TerrainAt(cell).Passable,
        _ => throw new InvalidOperationException($"No path rule {path}."),
    };

    private static Cell[] Steps(Directions directions) => directions switch
    {
        Directions.Orthogonal => OrthogonalSteps,
        Directions.Diagonal => DiagonalSteps,
        Directions.Any => [.. OrthogonalSteps, .. DiagonalSteps],
        _ => throw new InvalidOperationException($"No directions {directions}."),
    };
}
