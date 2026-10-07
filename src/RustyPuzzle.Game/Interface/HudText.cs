using RustyPuzzle.Game.Content;

namespace RustyPuzzle.Game.Interface;

/// <summary>
/// The board HUD's words, <c>content/interface/hud.json</c>. Templates name their values in braces:
/// <c>{member}</c>, <c>{terrain}</c>, <c>{column}</c>, <c>{row}</c>, and in <c>alsoMoves</c> the <c>{move}</c> text so
/// far. Each interface screen has its own file.
/// </summary>
internal sealed record HudText(
    string Title,
    string RoomLabel,
    string SelectedLabel,
    string LawLabel,
    string PartyHeading,
    string MovesHeading,
    string NobodySelected,
    string NothingSelected,
    string CellSelected,
    string MemberSelected,
    string NoMoves,
    string MemberPlace,
    string SelectMember,
    string MoveTo,
    string AlsoMoves)
{
    /// <summary>The content domain and bundle, <c>content/interface/</c>, and this screen's file in it.</summary>
    internal const string Domain = "interface";
    private const string File = "hud.json";

    internal static HudText Load(AuthoredContent content)
    {
        using AuthoredDomain domain = content.Open(Domain);
        return domain.Read(File, ContentJson.Default.HudText);
    }
}
