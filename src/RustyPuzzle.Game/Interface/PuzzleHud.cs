using System.Text.Json.Nodes;
using Rusty.Engine;
using RustyPuzzle.Game.Persistence;
using RustyPuzzle.Game.Session;

namespace RustyPuzzle.Game.Interface;

/// <summary>Publishes the session's <see cref="HudProjection"/> on the product's one UI stream, skipping unchanged values.</summary>
internal sealed class PuzzleHud : IDisposable
{
    /// <summary>The projection stream and contract, as the product project declares them.</summary>
    private const string Stream = "rusty-puzzle";
    private const string Contract = "rusty.puzzle.board";

    private readonly IUiService _ui;
    private readonly UiStream _stream;
    private ulong _sequence;
    private string? _published;

    internal PuzzleHud(IUiService ui)
    {
        _ui = ui;
        _stream = ui.OpenStream(new UiStreamRequest(Stream, Contract));
    }

    /// <summary>The last projection published, for observation.</summary>
    internal JsonObject? Current { get; private set; }

    internal void Publish(PuzzleSession session, HudText text, Progress progress)
    {
        JsonObject projection = HudProjection.Build(session, text, progress);
        string json = projection.ToJsonString();
        if (json == _published)
        {
            return;
        }

        _ui.PublishProjection(new UiProjection(_stream, ++_sequence, UiValues.FromJson(projection)));
        _published = json;
        Current = projection;
    }

    public void Dispose() => _stream.Dispose();
}
