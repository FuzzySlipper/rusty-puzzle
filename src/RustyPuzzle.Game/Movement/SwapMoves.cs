using RustyPuzzle.Game.Board;
using RustyPuzzle.Game.Party;

namespace RustyPuzzle.Game.Movement;

/// <summary>Evaluates <see cref="SwapMove"/>: the member and an ally in range trade cells.</summary>
internal static class SwapMoves
{
    internal static IEnumerable<Move> Options(BoardState state, PartyMember member, Cell from, SwapMove swap)
    {
        foreach (Placement ally in state.Placements)
        {
            int distance = Distance(from, ally.Cell, swap.Measure);
            if (ally.Member != member && distance >= swap.Distance.Min && distance <= swap.Distance.Max)
            {
                yield return new Move(member, ally.Cell, swap,
                    [new Relocated(member, from, ally.Cell), new Relocated(ally.Member, ally.Cell, from)]);
            }
        }
    }

    private static int Distance(Cell a, Cell b, Measure measure)
    {
        int columns = Math.Abs(a.Column - b.Column);
        int rows = Math.Abs(a.Row - b.Row);
        return measure switch
        {
            Measure.Manhattan => columns + rows,
            Measure.Chebyshev => Math.Max(columns, rows),
            _ => throw new InvalidOperationException($"No measure {measure}."),
        };
    }
}
