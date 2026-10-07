using RustyPuzzle.Game.Content;

namespace RustyPuzzle.Game.Interface;

/// <summary>
/// The board HUD's words, <c>content/interface/hud.json</c>. Templates name their values in braces:
/// <c>{member}</c>, <c>{terrain}</c>, <c>{column}</c> and <c>{row}</c>. Each interface screen has its own file.
/// </summary>
internal sealed record HudText(
    string Title,
    string RoomLabel,
    string SelectedLabel,
    string PartyHeading,
    string NobodySelected,
    string NothingSelected,
    string CellSelected,
    string MemberSelected,
    string MemberPlace,
    string SelectMember)
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
