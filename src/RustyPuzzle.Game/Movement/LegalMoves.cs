using RustyPuzzle.Game.Board;
using RustyPuzzle.Game.Party;

namespace RustyPuzzle.Game.Movement;

/// <summary>
/// The one owner of movement rules: every legal move a member's law allows on a board. Each part kind is
/// evaluated by its own evaluator; the board state is only read. When two parts reach the same target cell,
/// the earlier part in the law decides that cell.
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
        foreach (MovePart part in member.Law.Moves)
        {
            IEnumerable<Move> options = part switch
            {
                StepMove step => StepMoves.Options(state, member, from, step),
                SwapMove swap => SwapMoves.Options(state, member, from, swap),
                _ => throw new InvalidOperationException($"No evaluator for {part.GetType().Name}."),
            };
            moves.AddRange(options.Where(option => moves.All(taken => taken.Target != option.Target)));
        }

        return moves;
    }
}
