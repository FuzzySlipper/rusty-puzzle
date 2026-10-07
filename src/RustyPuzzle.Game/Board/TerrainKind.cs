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
    private const float MaximumHeight = 4;

    internal static TerrainKind Interpret(string id, TerrainDefinition definition)
    {
        string path = $"terrain/{id}.json";
        Authored.Require(definition.Symbol.Length == 1 && !char.IsWhiteSpace(definition.Symbol[0]), path, "symbol",
            "must be one visible character.");
        Authored.Within(path, "look.height", definition.Look.Height, 0.01f, MaximumHeight);
        return new TerrainKind(id, definition.Name, definition.Symbol[0], definition.Passable, definition.Exit,
            Authored.Colour(path, "look.colour", definition.Look.Colour), definition.Look.Height);
    }
}
