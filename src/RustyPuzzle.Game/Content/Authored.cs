using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using Rusty.Engine;

namespace RustyPuzzle.Game.Content;

/// <summary>
/// Opens the authored content domains. Each directory under <c>content/</c> is one declared Engine bundle
/// named after the directory; a domain owner opens its own bundle, reads its own files and closes it.
/// </summary>
/// <param name="open">Opens a bundle by domain ID: the product's declared build bundles, or packed
/// containers of the same directories in tests.</param>
internal sealed class AuthoredContent(Func<string, ProductContentBundle> open)
{
    internal AuthoredDomain Open(string domain) => new(domain, open(domain));
}

/// <summary>One open content domain. Reads parse strict typed records, and every error names the file.</summary>
internal sealed class AuthoredDomain(string domain, ProductContentBundle bundle) : IDisposable
{
    private const string JsonExtension = ".json";

    /// <summary>The content-root path of a file in this domain, as errors name it.</summary>
    internal string PathOf(string file) => $"{domain}/{file}";

    internal T Read<T>(string file, JsonTypeInfo<T> type) where T : class
    {
        ProductContentFile read;
        try
        {
            read = bundle.ReadFile(file);
        }
        catch (FileNotFoundException error)
        {
            throw new InvalidOperationException($"content/{PathOf(file)}: the file is missing.", error);
        }

        return Parse(read, type);
    }

    /// <summary>Every JSON file directly in the domain, keyed by its file name without the extension.</summary>
    internal IReadOnlyDictionary<string, T> ReadAll<T>(JsonTypeInfo<T> type) where T : class
    {
        Dictionary<string, T> definitions = new(StringComparer.Ordinal);
        foreach (ProductContentFile file in bundle.ReadDirectory())
        {
            if (file.Name.EndsWith(JsonExtension, StringComparison.Ordinal))
            {
                definitions.Add(file.Name[..^JsonExtension.Length], Parse(file, type));
            }
        }

        Authored.Require(definitions.Count > 0, domain, "", "holds no definitions.");
        return definitions;
    }

    public void Dispose() => bundle.Dispose();

    private T Parse<T>(ProductContentFile file, JsonTypeInfo<T> type) where T : class
    {
        try
        {
            return JsonSerializer.Deserialize(file.Bytes.Span, type) ?? throw new JsonException("The file holds no value.");
        }
        catch (JsonException error)
        {
            throw new InvalidOperationException($"content/{PathOf(file.RelativePath)}: {error.Message}", error);
        }
    }
}

/// <summary>Checks for authored values that parse but contradict themselves or another definition.</summary>
internal static class Authored
{
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
}
