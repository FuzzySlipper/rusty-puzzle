using RustyPuzzle.Game.Board;
using RustyPuzzle.Game.Party;

namespace RustyPuzzle.Game.Movement;

/// <summary>
/// The one owner of movement rules: every legal move a member's law allows on a board, then every move the
/// other members' interactions lend it there. Each part kind is evaluated by its own evaluator; the board state
/// is only read. When two parts reach the same target cell, the earlier one decides that cell: the member's
/// own law before lent laws.
/// </summary>
internal static class LegalMoves
{
    internal static IReadOnlyList<Move> For(BoardState state, PartyMember member)
    {
        if (state.CellOf(member) is not Cell from)
        {
            return [];
        }

        List<Move> moves = [];
        Add(moves, state, member, from, member.Law, lender: null);
        foreach ((MovementLaw law, PartyMember lender, _) in Lent(state, member, from))
        {
            Add(moves, state, member, from, law, lender);
        }

        return moves;
    }

    /// <summary>The lends of other members that reach this member where it stands.</summary>
    internal static IEnumerable<(MovementLaw Law, PartyMember Lender, string Rule)> Lends(BoardState state, PartyMember member) =>
        state.CellOf(member) is Cell at ? Lent(state, member, at) : [];

    private static IEnumerable<(MovementLaw Law, PartyMember Lender, string Rule)> Lent(BoardState state, PartyMember member, Cell at)
    {
        foreach (Placement other in state.Placements)
        {
            if (other.Member == member)
            {
                continue;
            }

            foreach (Interaction interaction in other.Member.Interactions)
            {
                switch (interaction)
                {
                    case Lend lend when Distances.Within(other.Cell, at, lend.Within, lend.Measure):
                        yield return (lend.Law, other.Member, lend.Rule);
                        break;
                    case Lend:
                        break;
                    default:
                        throw new InvalidOperationException($"No evaluator for {interaction.GetType().Name}.");
                }
            }
        }
    }

    private static void Add(List<Move> moves, BoardState state, PartyMember member, Cell from, MovementLaw law, PartyMember? lender)
    {
        foreach (MovePart part in law.Moves)
        {
            IEnumerable<Move> options = part switch
            {
                StepMove step => StepMoves.Options(state, member, from, step),
                SwapMove swap => SwapMoves.Options(state, member, from, swap),
                _ => throw new InvalidOperationException($"No evaluator for {part.GetType().Name}."),
            };
            moves.AddRange(options
                .Where(option => moves.All(taken => taken.Target != option.Target))
                .Select(option => option with { Lender = lender }));
        }
    }
}
