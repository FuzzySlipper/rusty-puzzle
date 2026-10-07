using System.Text.Json.Serialization;

namespace RustyPuzzle.Game.Movement;

/// <summary>
/// One authored way a member changes what other members may do, <c>{"kind": ..., fields}</c> in a party
/// member's <c>interactions</c>. A small closed vocabulary like move parts: each kind is one record here and
/// its evaluation in <see cref="LegalMoves"/>.
/// </summary>
/// <param name="Rule">The interaction in one sentence, shown with the laws it changes.</param>
[JsonPolymorphic(TypeDiscriminatorPropertyName = "kind", UnknownDerivedTypeHandling = JsonUnknownDerivedTypeHandling.FailSerialization)]
[JsonDerivedType(typeof(LendDefinition), "lend")]
internal abstract record InteractionDefinition(string Rule);

/// <summary>Other members within <paramref name="Within"/>, by <paramref name="Measure"/>, may also move by the law named <paramref name="Law"/>.</summary>
internal sealed record LendDefinition(string Rule, DistanceRange Within, Measure Measure, string Law) : InteractionDefinition(Rule);

/// <summary>An interaction bound to the definitions it names.</summary>
internal abstract record Interaction(string Rule);

/// <summary>Other members within <paramref name="Within"/> of the lender may also move by <paramref name="Law"/>.</summary>
internal sealed record Lend(string Rule, DistanceRange Within, Measure Measure, MovementLaw Law) : Interaction(Rule);
