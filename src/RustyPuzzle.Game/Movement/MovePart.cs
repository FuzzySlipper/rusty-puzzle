using System.Text.Json.Serialization;
using RustyPuzzle.Game.Content;

namespace RustyPuzzle.Game.Movement;

/// <summary>
/// One way a member may move, authored as <c>{"kind": ..., fields}</c> in a movement law. The kinds are a
/// small closed vocabulary; each has exactly one evaluator (see <see cref="LegalMoves"/>). A law that needs a
/// genuinely new way of moving adds one part record here and one evaluator, never a rule per character.
/// </summary>
[JsonPolymorphic(TypeDiscriminatorPropertyName = "kind", UnknownDerivedTypeHandling = JsonUnknownDerivedTypeHandling.FailSerialization)]
[JsonDerivedType(typeof(StepMove), "step")]
[JsonDerivedType(typeof(SwapMove), "swap")]
/// <param name="Distance">How many squares away the move may reach, as the part measures it.</param>
internal abstract record MovePart(DistanceRange Distance);

/// <summary>
/// Moves in a straight line in one of <paramref name="Directions"/>, landing <paramref name="Distance"/> squares
/// away on an open cell. <paramref name="Path"/> says what the squares crossed on the way may hold.
/// </summary>
internal sealed record StepMove(Directions Directions, DistanceRange Distance, PathRule Path) : MovePart(Distance);

/// <summary>Trades places with another party member whose distance, by <paramref name="Measure"/>, is within <paramref name="Distance"/>.</summary>
internal sealed record SwapMove(DistanceRange Distance, Measure Measure) : MovePart(Distance);

/// <summary>An inclusive range of squares.</summary>
internal sealed record DistanceRange(int Min, int Max);

[JsonConverter(typeof(KebabCase<Directions>))]
internal enum Directions
{
    /// <summary>Up, down, left and right.</summary>
    Orthogonal,

    /// <summary>The four diagonals.</summary>
    Diagonal,

    /// <summary>All eight.</summary>
    Any,
}

/// <summary>What the squares a step crosses (not the one it lands on) may hold.</summary>
[JsonConverter(typeof(KebabCase<PathRule>))]
internal enum PathRule
{
    /// <summary>Every crossed square is open: passable and empty.</summary>
    Clear,

    /// <summary>Crossed squares are passable; party members on them are vaulted over.</summary>
    OverParty,
}

/// <summary>How far apart two cells are.</summary>
[JsonConverter(typeof(KebabCase<Measure>))]
internal enum Measure
{
    /// <summary>Squares counted along rows plus columns.</summary>
    Manhattan,

    /// <summary>The larger of the row and column difference: a diagonal square counts as one.</summary>
    Chebyshev,
}
