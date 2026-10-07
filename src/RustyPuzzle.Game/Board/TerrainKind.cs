using Rusty.Engine;
using RustyPuzzle.Game.Content;

namespace RustyPuzzle.Game.Board;

/// <summary>An authored terrain kind, <c>content/terrain/&lt;id&gt;.json</c>.</summary>
/// <param name="Name">The noun interface text uses for a cell of this kind.</param>
/// <param name="Symbol">The single character that marks this kind in a room's rows.</param>
/// <param name="Passable">Whether a party member may stand on it.</param>
/// <param name="Exit">Whether a party member standing on it has reached the room's exit.</param>
internal sealed record TerrainDefinition(string Name, string Symbol, bool Passable, bool Exit, TerrainLook Look);

/// <param name="Height">How tall the placeholder block stands, in cells.</param>
internal sealed record TerrainLook(float[] Colour, float Height);

/// <summary>A terrain kind as rooms and the board view use it.</summary>
internal sealed record TerrainKind(string Id, string Name, char Symbol, bool Passable, bool Exit, Color Colour, float Height)
{
    /// <summary>The content domain and bundle: <c>content/terrain/</c>.</summary>
    internal const string Domain = "terrain";
    private const float MaximumHeight = 4;

    /// <summary>Every authored terrain kind by ID; no two kinds share a symbol.</summary>
    internal static IReadOnlyDictionary<string, TerrainKind> Load(AuthoredContent content)
    {
        using AuthoredDomain domain = content.Open(Domain);
        Dictionary<string, TerrainKind> kinds = domain.ReadAll(ContentJson.Default.TerrainDefinition)
            .ToDictionary(pair => pair.Key, pair => Interpret(domain.PathOf($"{pair.Key}.json"), pair.Key, pair.Value), StringComparer.Ordinal);
        foreach (IGrouping<char, TerrainKind> shared in kinds.Values.GroupBy(kind => kind.Symbol).Where(group => group.Count() > 1))
        {
            Authored.Require(false, Domain, "symbol", $"'{shared.Key}' marks more than one kind: {string.Join(", ", shared.Select(kind => kind.Id))}.");
        }

        return kinds;
    }

    private static TerrainKind Interpret(string path, string id, TerrainDefinition definition)
    {
        Authored.Require(definition.Symbol.Length == 1 && !char.IsWhiteSpace(definition.Symbol[0]), path, "symbol",
            "must be one visible character.");
        Authored.Within(path, "look.height", definition.Look.Height, 0.01f, MaximumHeight);
        return new TerrainKind(id, definition.Name, definition.Symbol[0], definition.Passable, definition.Exit,
            Authored.Colour(path, "look.colour", definition.Look.Colour), definition.Look.Height);
    }
}
