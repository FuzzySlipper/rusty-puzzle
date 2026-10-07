using Rusty.Engine;
using RustyPuzzle.Game.Content;

namespace RustyPuzzle.Game.Party;

/// <summary>An authored party member, <c>content/party/&lt;id&gt;.json</c>.</summary>
internal sealed record MemberDefinition(string Name, MemberLook Look);

internal sealed record MemberLook(float[] Colour);

/// <summary>A party member as rooms, the board view and the interface use it.</summary>
internal sealed record PartyMember(string Id, string Name, Color Colour)
{
    /// <summary>The content domain and bundle: <c>content/party/</c>.</summary>
    internal const string Domain = "party";

    /// <summary>Every authored party member by ID.</summary>
    internal static IReadOnlyDictionary<string, PartyMember> Load(AuthoredContent content)
    {
        using AuthoredDomain domain = content.Open(Domain);
        return domain.ReadAll(ContentJson.Default.MemberDefinition)
            .ToDictionary(pair => pair.Key, pair => Interpret(domain.PathOf($"{pair.Key}.json"), pair.Key, pair.Value), StringComparer.Ordinal);
    }

    private static PartyMember Interpret(string path, string id, MemberDefinition definition)
    {
        Authored.Require(definition.Name.Length > 0, path, "name", "must not be empty.");
        return new PartyMember(id, definition.Name, Authored.Colour(path, "look.colour", definition.Look.Colour));
    }
}
