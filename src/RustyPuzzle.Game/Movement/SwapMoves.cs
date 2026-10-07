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
            if (ally.Member != member && Distances.Within(from, ally.Cell, swap.Distance, swap.Measure))
            {
                yield return new Move(member, ally.Cell, swap,
                    [new Relocated(member, from, ally.Cell), new Relocated(ally.Member, ally.Cell, from)]);
            }
        }
    }
}
