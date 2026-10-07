using System.Text.Json;
using System.Text.Json.Serialization;

namespace RustyPuzzle.Game.Content;

/// <summary>
/// Reads and writes a closed vocabulary enum as its kebab-case name (<c>OverParty</c> is <c>"over-party"</c>).
/// Numbers and unknown names are refused. Attach it to the enum: <c>[JsonConverter(typeof(KebabCase&lt;T&gt;))]</c>.
/// </summary>
internal sealed class KebabCase<TEnum>() : JsonStringEnumConverter<TEnum>(JsonNamingPolicy.KebabCaseLower, allowIntegerValues: false)
    where TEnum : struct, Enum;
