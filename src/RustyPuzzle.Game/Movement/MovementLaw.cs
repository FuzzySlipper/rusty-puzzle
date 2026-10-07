using RustyPuzzle.Game.Content;

namespace RustyPuzzle.Game.Movement;

/// <summary>An authored movement law, <c>content/laws/&lt;id&gt;.json</c>: its one-sentence rule and the ways it moves.</summary>
internal sealed record LawDefinition(string Rule, MovePart[] Moves);

/// <summary>A reusable movement law. Party members name the law they obey; several members may share one.</summary>
internal sealed record MovementLaw(string Id, string Rule, IReadOnlyList<MovePart> Moves)
{
    /// <summary>The content domain and bundle: <c>content/laws/</c>.</summary>
    internal const string Domain = "laws";

    // A square count past the largest board is an authoring slip, not a law.
    private const int MaximumDistance = 32;

    internal static IReadOnlyDictionary<string, MovementLaw> Load(AuthoredContent content)
    {
        using AuthoredDomain domain = content.Open(Domain);
        return domain.ReadAll(ContentJson.Default.LawDefinition)
            .ToDictionary(pair => pair.Key, pair => Interpret(domain.PathOf($"{pair.Key}.json"), pair.Key, pair.Value), StringComparer.Ordinal);
    }

    private static MovementLaw Interpret(string path, string id, LawDefinition definition)
    {
        Authored.Require(definition.Rule.Length > 0, path, "rule", "must state the law in one sentence.");
        Authored.Require(definition.Moves.Length > 0, path, "moves", "must give at least one way to move.");
        for (int index = 0; index < definition.Moves.Length; index++)
        {
            DistanceRange distance = definition.Moves[index].Distance;
            Authored.Require(distance.Min >= 1 && distance.Max >= distance.Min && distance.Max <= MaximumDistance, path,
                $"moves[{index}].distance", $"must have 1 <= min <= max <= {MaximumDistance}; found {distance.Min} to {distance.Max}.");
        }

        return new MovementLaw(id, definition.Rule, definition.Moves);
    }
}
