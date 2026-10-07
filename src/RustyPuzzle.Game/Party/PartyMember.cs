using Rusty.Engine;
using RustyPuzzle.Game.Content;
using RustyPuzzle.Game.Movement;

namespace RustyPuzzle.Game.Party;

/// <summary>
/// An authored party member, <c>content/party/&lt;id&gt;.json</c>: a composition of a movement law (by ID), zero
/// or more interactions with other members, and presentation. The name doubles as the archetype until
/// characters are given personal names.
/// </summary>
internal sealed record MemberDefinition(string Name, string Law, InteractionDefinition[] Interactions, MemberLook Look);

/// <param name="Mark">A short placeholder label (one or two letters) for lists and badges.</param>
internal sealed record MemberLook(float[] Colour, string Mark);

/// <summary>A party member as rooms, rules, the board view and the interface use it.</summary>
internal sealed record PartyMember(string Id, string Name, MovementLaw Law, IReadOnlyList<Interaction> Interactions, Color Colour, string Mark)
{
    /// <summary>The content domain and bundle: <c>content/party/</c>.</summary>
    internal const string Domain = "party";
    private const int MaximumMark = 2;

    /// <summary>Every authored party member by ID, each bound to the law it names.</summary>
    internal static IReadOnlyDictionary<string, PartyMember> Load(AuthoredContent content, IReadOnlyDictionary<string, MovementLaw> laws)
    {
        using AuthoredDomain domain = content.Open(Domain);
        return domain.ReadAll(ContentJson.Default.MemberDefinition)
            .ToDictionary(pair => pair.Key, pair => Interpret(domain.PathOf($"{pair.Key}.json"), pair.Key, pair.Value, laws), StringComparer.Ordinal);
    }

    private static PartyMember Interpret(string path, string id, MemberDefinition definition, IReadOnlyDictionary<string, MovementLaw> laws)
    {
        Authored.Require(definition.Name.Length > 0, path, "name", "must not be empty.");
        Authored.Require(laws.TryGetValue(definition.Law, out MovementLaw? law), path, "law",
            $"names no movement law '{definition.Law}'; the laws are {string.Join(", ", laws.Keys)}.");
        Authored.Require(definition.Look.Mark.Length is > 0 and <= MaximumMark, path, "look.mark", $"must be 1 to {MaximumMark} characters.");
        Interaction[] interactions = [.. definition.Interactions.Select((interaction, index) => Interpret(path, $"interactions[{index}]", interaction, laws))];
        return new PartyMember(id, definition.Name, law!, interactions, Authored.Colour(path, "look.colour", definition.Look.Colour), definition.Look.Mark);
    }

    private static Interaction Interpret(string path, string field, InteractionDefinition definition, IReadOnlyDictionary<string, MovementLaw> laws)
    {
        Authored.Require(definition.Rule.Length > 0, path, $"{field}.rule", "must state the interaction in one sentence.");
        switch (definition)
        {
            case LendDefinition lend:
                Authored.Require(laws.TryGetValue(lend.Law, out MovementLaw? lent), path, $"{field}.law",
                    $"names no movement law '{lend.Law}'; the laws are {string.Join(", ", laws.Keys)}.");
                Authored.Require(lend.Within.Min >= 1 && lend.Within.Max >= lend.Within.Min, path, $"{field}.within",
                    $"must have 1 <= min <= max; found {lend.Within.Min} to {lend.Within.Max}.");
                return new Lend(lend.Rule, lend.Within, lend.Measure, lent!);
            default:
                throw new InvalidOperationException($"content/{path} {field}: no interaction {definition.GetType().Name}.");
        }
    }
}
