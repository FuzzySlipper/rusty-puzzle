using Rusty.Engine;
using RustyPuzzle.Game.Content;

namespace RustyPuzzle.Game.Presentation;

/// <summary>How the board is shown, <c>content/tuning/board-view.json</c>.</summary>
/// <param name="PitchDegrees">How far the camera looks down: 90 is straight down.</param>
/// <param name="Fill">How much of the view the board may fill, leaving a margin on every side.</param>
/// <param name="CellGap">The gap between neighbouring cells' blocks, in cells.</param>
internal sealed record BoardViewDefinition(float PitchDegrees, float Fill, float CellGap, PieceLook Piece,
    SelectionLook Selection, float[] Background);

/// <summary>A party member's placeholder block, in cells.</summary>
internal sealed record PieceLook(float Width, float Height);

/// <summary>The selected cell's marker: a slab <paramref name="Height"/> tall over the cell's top, inset from its edges.</summary>
internal sealed record SelectionLook(float[] Colour, float Inset, float Height);

/// <summary>The validated board view tuning.</summary>
internal sealed record BoardViewTuning(float PitchDegrees, float Fill, float CellGap, PieceLook Piece,
    Color SelectionColour, float SelectionInset, float SelectionHeight, Color Background)
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
        Authored.Within(Path, "selection.inset", definition.Selection.Inset, 0, 0.45f);
        Authored.Within(Path, "selection.height", definition.Selection.Height, 0.001f, 1);
        return new BoardViewTuning(definition.PitchDegrees, definition.Fill, definition.CellGap, definition.Piece,
            Authored.Colour(Path, "selection.colour", definition.Selection.Colour),
            definition.Selection.Inset, definition.Selection.Height,
            Authored.Colour(Path, "background", definition.Background));
    }
}
