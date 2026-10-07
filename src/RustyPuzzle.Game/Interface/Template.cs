using System.Text;

namespace RustyPuzzle.Game.Interface;

/// <summary>Fills an authored template's <c>{name}</c> placeholders. Code composes text; content authors it.</summary>
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
