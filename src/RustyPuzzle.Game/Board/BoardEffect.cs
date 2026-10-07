using RustyPuzzle.Game.Party;

namespace RustyPuzzle.Game.Board;

/// <summary>
/// One consequence of a move on the board. A move resolves to a list of effects that legal-move display,
/// previews, undo and tests all read; <see cref="BoardEffects.Apply"/> is the only code that carries them out.
/// A new kind of consequence is a new effect record and its case in the applier.
/// </summary>
internal abstract record BoardEffect;

/// <summary>A member goes from one cell to another. Relocations in one move happen together, so two members can trade places.</summary>
internal sealed record Relocated(PartyMember Member, Cell From, Cell To) : BoardEffect;

/// <summary>The one applier of board effects.</summary>
internal static class BoardEffects
{
    /// <summary>The state after a move's effects, all applied at once. The given state is left unchanged.</summary>
    internal static BoardState Apply(BoardState state, IReadOnlyList<BoardEffect> effects)
    {
        Dictionary<PartyMember, Cell> cells = state.Placements.ToDictionary(placed => placed.Member, placed => placed.Cell);
        foreach (BoardEffect effect in effects)
        {
            switch (effect)
            {
                case Relocated relocated:
                    if (cells[relocated.Member] != relocated.From)
                    {
                        throw new InvalidOperationException($"The {relocated.Member.Id} is not at {relocated.From} to relocate.");
                    }

                    cells[relocated.Member] = relocated.To;
                    break;
                default:
                    throw new InvalidOperationException($"No applier for {effect.GetType().Name}.");
            }
        }

        BoardState next = new(state.Grid, state.Placements.Select(placed => placed with { Cell = cells[placed.Member] }));
        if (next.Placements.Select(placed => placed.Cell).Distinct().Count() != next.Placements.Count)
        {
            throw new InvalidOperationException("A move's effects put two members on one cell.");
        }

        return next;
    }
}
