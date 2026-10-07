using Rusty.Engine;
using RustyPuzzle.Game.Content;

namespace RustyPuzzle.Game.Party;

/// <summary>An authored party member, <c>content/party/&lt;id&gt;.json</c>.</summary>
internal sealed record MemberDefinition(string Name, MemberLook Look);

internal sealed record MemberLook(float[] Colour);

/// <summary>A party member as rooms, the board view and the interface use it.</summary>
internal sealed record PartyMember(string Id, string Name, Color Colour)
{
    internal static PartyMember Interpret(string id, MemberDefinition definition)
    {
        string path = $"party/{id}.json";
        Authored.Require(definition.Name.Length > 0, path, "name", "must not be empty.");
        return new PartyMember(id, definition.Name, Authored.Colour(path, "look.colour", definition.Look.Colour));
    }
}
