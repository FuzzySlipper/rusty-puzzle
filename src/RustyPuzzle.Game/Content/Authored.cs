using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using Rusty.Engine;

namespace RustyPuzzle.Game.Content;

/// <summary>Reads authored content files into their typed records; every error names the file.</summary>
internal static class Authored
{
    private const string JsonExtension = ".json";

    internal static T Read<T>(ProductContent content, string path, JsonTypeInfo<T> type) where T : class =>
        Parse(content.ReadFile(path), type);

    /// <summary>Every JSON file directly in <paramref name="directory"/>, keyed by its file name without the extension.</summary>
    internal static IReadOnlyDictionary<string, T> ReadDirectory<T>(ProductContent content, string directory, JsonTypeInfo<T> type)
        where T : class
    {
        Dictionary<string, T> definitions = new(StringComparer.Ordinal);
        foreach (ProductContentFile file in content.ReadDirectory(directory))
        {
            if (file.Name.EndsWith(JsonExtension, StringComparison.Ordinal))
            {
                definitions.Add(file.Name[..^JsonExtension.Length], Parse(file, type));
            }
        }

        Require(definitions.Count > 0, directory, "", "holds no definitions.");
        return definitions;
    }

    /// <summary>Fails authored data that parses but contradicts itself or another definition.</summary>
    internal static void Require(bool condition, string path, string field, string problem)
    {
        if (!condition)
        {
            throw new InvalidOperationException($"content/{path} {field}: {problem}");
        }
    }

    /// <summary>A linear RGB colour: three finite components from zero to one.</summary>
    internal static Color Colour(string path, string field, float[] value)
    {
        Require(value.Length == 3 && value.All(c => float.IsFinite(c) && c is >= 0 and <= 1), path, field,
            "must be [r, g, b] with three numbers from 0 to 1.");
        return new Color(value[0], value[1], value[2], 1);
    }

    internal static void Within(string path, string field, float value, float minimum, float maximum) =>
        Require(float.IsFinite(value) && value >= minimum && value <= maximum, path, field,
            $"must be between {minimum} and {maximum}; found {value}.");

    private static T Parse<T>(ProductContentFile file, JsonTypeInfo<T> type) where T : class
    {
        try
        {
            return JsonSerializer.Deserialize(file.Bytes.Span, type) ?? throw new JsonException("The file holds no value.");
        }
        catch (JsonException error)
        {
            throw new InvalidOperationException($"content/{file.RelativePath}: {error.Message}", error);
        }
    }
}
