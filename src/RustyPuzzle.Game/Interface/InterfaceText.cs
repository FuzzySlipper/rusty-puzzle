using System.Text;

namespace RustyPuzzle.Game.Interface;

/// <summary>
/// The interface's words, <c>content/interface/text.json</c>. Templates name their values in braces:
/// <c>{member}</c>, <c>{terrain}</c>, <c>{column}</c> and <c>{row}</c>.
/// </summary>
internal sealed record InterfaceText(
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
    internal const string Path = "interface/text.json";
}

/// <summary>Fills a template's <c>{name}</c> placeholders.</summary>
internal static class Template
{
    internal static string Fill(string template, params ReadOnlySpan<(string Name, string Value)> values)
    {
        StringBuilder text = new(template);
        foreach ((string name, string value) in values)
        {
            text.Replace("{" + name + "}", value);
        }

        return text.ToString();
    }
}
