using Rusty.Engine;
using RustyPuzzle.Game.Content;

/// <summary>
/// A content tree the product reads through the Engine bundle API. The test host cannot open the product's
/// declared build bundles, so each domain directory is packed into an Engine content container when the
/// product opens it, and read through the same <see cref="ProductContentBundle"/> calls. Packing on every open
/// means an edit to a copied tree shows on the product's next reload.
/// </summary>
sealed class PackedContent : IDisposable
{
    private readonly string _scratch = Path.Combine(Path.GetTempPath(), "rusty-puzzle-smoke-" + Guid.NewGuid());
    private readonly bool _owned;

    private PackedContent(string root, bool owned)
    {
        Root = root;
        _owned = owned;
        Directory.CreateDirectory(_scratch);
    }

    internal string Root { get; }

    /// <summary>The authored content copied beside the smoke executable.</summary>
    internal static PackedContent Shipped() => new(Path.Combine(AppContext.BaseDirectory, "content"), owned: false);

    /// <summary>A private copy of the shipped content, for checks that edit files.</summary>
    internal static PackedContent Copy()
    {
        string source = Path.Combine(AppContext.BaseDirectory, "content");
        string root = Path.Combine(Path.GetTempPath(), "rusty-puzzle-content-" + Guid.NewGuid());
        foreach (string file in Directory.GetFiles(source, "*", SearchOption.AllDirectories))
        {
            string target = Path.Combine(root, Path.GetRelativePath(source, file));
            Directory.CreateDirectory(Path.GetDirectoryName(target)!);
            File.Copy(file, target);
        }

        return new PackedContent(root, owned: true);
    }

    internal void Write(string path, string text) => File.WriteAllText(Path.Combine(Root, path), text);

    /// <summary>A copy of the shipped content whose authored order plays only the given room.</summary>
    internal static PackedContent WithRoom(string[] rows, Dictionary<char, string> party)
    {
        PackedContent content = Copy();
        string rowList = string.Join(", ", rows.Select(row => $"\"{row}\""));
        string partyMap = string.Join(", ", party.Select(pair => $"\"{pair.Key}\": \"{pair.Value}\""));
        content.Write("rooms/check.json", $"{{\"name\": \"Check\", \"rows\": [{rowList}], \"party\": {{{partyMap}}}, \"startTerrain\": \"floor\"}}");
        content.Write("campaign/rooms.json", "{\"order\": [\"check\"]}");
        return content;
    }

    internal AuthoredContent Authored(IEngineContext engine) => new(domain =>
    {
        string container = Path.Combine(_scratch, $"{domain}-{Guid.NewGuid():N}.rpk");
        ProductContentBundle.PackContainer(engine.Content, Path.Combine(Root, domain), container, compress: false);
        return ProductContentBundle.OpenContainer(engine.Content, container);
    });

    public void Dispose()
    {
        Directory.Delete(_scratch, recursive: true);
        if (_owned)
        {
            Directory.Delete(Root, recursive: true);
        }
    }
}
