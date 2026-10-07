using Rusty.Engine;
using RustyPuzzle.Game.Content;

namespace RustyPuzzle.Game.Presentation;

/// <summary>How the board is shown, <c>content/tuning/board-view.json</c>.</summary>
/// <param name="PitchDegrees">How far the camera looks down: 90 is straight down.</param>
/// <param name="Fill">How much of the view the board may fill, leaving a margin on every side.</param>
/// <param name="CellGap">The gap between neighbouring cells' blocks, in cells.</param>
/// <param name="Selection">The selected cell's marker.</param>
/// <param name="Destination">The marker on each cell the selected member may move to.</param>
/// <param name="Preview">Where the pointed-at move would put each member it moves.</param>
internal sealed record BoardViewDefinition(float PitchDegrees, float Fill, float CellGap, PieceLook Piece,
    MarkerDefinition Selection, MarkerDefinition Destination, PreviewLook Preview, float[] Background);

/// <summary>A party member's placeholder ball, in cells.</summary>
internal sealed record PieceLook(float Width, float Height);

/// <summary>A cell marker: a slab <paramref name="Height"/> tall on the cell's top, inset from its edges.</summary>
internal sealed record MarkerDefinition(float[] Colour, float Inset, float Height);

/// <summary>A small token in a member's colour, floating <paramref name="Lift"/> above a piece's top.</summary>
internal sealed record PreviewLook(float Width, float Height, float Lift);

/// <summary>A validated cell marker.</summary>
internal sealed record MarkerLook(Color Colour, float Inset, float Height);

/// <summary>The validated board view tuning.</summary>
internal sealed record BoardViewTuning(float PitchDegrees, float Fill, float CellGap, PieceLook Piece,
    MarkerLook Selection, MarkerLook Destination, PreviewLook Preview, Color Background)
{
    /// <summary>The content domain and bundle, <c>content/tuning/</c>, and this tuning's file in it.</summary>
    internal const string Domain = "tuning";
    private const string File = "board-view.json";
    private const string Path = $"{Domain}/{File}";

    internal static BoardViewTuning Load(AuthoredContent content)
    {
        using AuthoredDomain domain = content.Open(Domain);
        return Interpret(domain.Read(File, ContentJson.Default.BoardViewDefinition));
    }

    private static BoardViewTuning Interpret(BoardViewDefinition definition)
    {
        Authored.Within(Path, "pitchDegrees", definition.PitchDegrees, 10, 90);
        Authored.Within(Path, "fill", definition.Fill, 0.1f, 1);
        Authored.Within(Path, "cellGap", definition.CellGap, 0, 0.5f);
        Authored.Within(Path, "piece.width", definition.Piece.Width, 0.05f, 1);
        Authored.Within(Path, "piece.height", definition.Piece.Height, 0.05f, 4);
        Authored.Within(Path, "preview.width", definition.Preview.Width, 0.05f, 1);
        Authored.Within(Path, "preview.height", definition.Preview.Height, 0.01f, 1);
        Authored.Within(Path, "preview.lift", definition.Preview.Lift, 0, 2);
        return new BoardViewTuning(definition.PitchDegrees, definition.Fill, definition.CellGap, definition.Piece,
            Marker("selection", definition.Selection), Marker("destination", definition.Destination), definition.Preview,
            Authored.Colour(Path, "background", definition.Background));
    }

    private static MarkerLook Marker(string field, MarkerDefinition marker)
    {
        Authored.Within(Path, $"{field}.inset", marker.Inset, 0, 0.45f);
        Authored.Within(Path, $"{field}.height", marker.Height, 0.001f, 1);
        return new MarkerLook(Authored.Colour(Path, $"{field}.colour", marker.Colour), marker.Inset, marker.Height);
    }
}
