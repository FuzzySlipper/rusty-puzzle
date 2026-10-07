using System.Text.Json;
using RustyPuzzle.Game.Content;
using RustyPuzzle.Game.Session;

namespace RustyPuzzle.Game.Interface;

/// <summary>
/// The board HUD's words, <c>content/interface/hud.json</c>. Templates name their values in braces:
/// <c>{member}</c>, <c>{terrain}</c>, <c>{column}</c>, <c>{row}</c>, <c>{count}</c>, and in <c>alsoMoves</c> the
/// <c>{move}</c> text so far. <see cref="Refusals"/> holds one sentence per <see cref="Refusal"/>, keyed by its
/// kebab-case name. Each interface screen has its own file.
/// </summary>
internal sealed record HudText(
    string Title,
    string RoomLabel,
    string SelectedLabel,
    string LawLabel,
    string MoveCountLabel,
    string PartyHeading,
    string MovesHeading,
    string NobodySelected,
    string NothingSelected,
    string CellSelected,
    string MemberSelected,
    string NoMoves,
    string Solved,
    string SolvedNext,
    string SolvedLast,
    string MemberPlace,
    string SelectMember,
    string MoveTo,
    string AlsoMoves,
    string Undo,
    string Reset,
    string NextRoom,
    Dictionary<string, string> Refusals)
{
    /// <summary>The content domain and bundle, <c>content/interface/</c>, and this screen's file in it.</summary>
    internal const string Domain = "interface";
    private const string File = "hud.json";

    internal static HudText Load(AuthoredContent content)
    {
        using AuthoredDomain domain = content.Open(Domain);
        HudText text = domain.Read(File, ContentJson.Default.HudText);
        string path = domain.PathOf(File);
        HashSet<string> refusals = [.. Enum.GetValues<Refusal>().Select(Key)];
        foreach (string missing in refusals.Except(text.Refusals.Keys))
        {
            Authored.Require(false, path, $"refusals.{missing}", "has no sentence.");
        }

        foreach (string unknown in text.Refusals.Keys.Except(refusals))
        {
            Authored.Require(false, path, $"refusals.{unknown}", $"is not a refusal; the refusals are {string.Join(", ", refusals)}.");
        }

        return text;
    }

    internal string Refused(Refusal refusal) => Refusals[Key(refusal)];

    private static string Key(Refusal refusal) => JsonNamingPolicy.KebabCaseLower.ConvertName(refusal.ToString());
}
