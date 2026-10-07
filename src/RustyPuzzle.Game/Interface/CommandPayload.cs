using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Rusty.Engine;
using RustyPuzzle.Game.Session;

namespace RustyPuzzle.Game.Interface;

/// <summary>
/// The wire form of session commands: the <c>puzzle.command</c> payload intent carries one
/// <see cref="PuzzleCommand"/> as JSON. The projection hands the DOM each command ready to send, so the DOM
/// never spells an action or a field.
/// </summary>
internal static class CommandPayload
{
    /// <summary>The payload intent and contract, as the product project declares them.</summary>
    internal const string Intent = "puzzle.command";
    internal const string Contract = "puzzle.command.v1";
    private static readonly byte[] IntentBytes = Encoding.UTF8.GetBytes(Intent);
    private static readonly byte[] ContractBytes = Encoding.UTF8.GetBytes(Contract);

    /// <summary>The command a UI claim carries, or null for any other input. A malformed payload is a first-party defect.</summary>
    internal static PuzzleCommand? From(in ProductInputEvent input)
    {
        if (input.ValueKind != InputValueKind.ProductPayload
            || !input.Intent.Span.SequenceEqual(IntentBytes)
            || !input.PayloadContract.Span.SequenceEqual(ContractBytes))
        {
            return null;
        }

        try
        {
            return JsonSerializer.Deserialize(input.PayloadData.Span, InterfaceJson.Default.PuzzleCommand)
                ?? throw new JsonException("The payload holds no value.");
        }
        catch (JsonException error)
        {
            throw new InvalidOperationException($"{Contract}: {error.Message}", error);
        }
    }

    internal static JsonNode ToJson(PuzzleCommand command) => JsonSerializer.SerializeToNode(command, InterfaceJson.Default.PuzzleCommand)!;
}

// Missing constructor values, nulls in non-nullable fields and unknown members are errors, not defaults.
[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    RespectRequiredConstructorParameters = true, RespectNullableAnnotations = true,
    UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow)]
[JsonSerializable(typeof(PuzzleCommand))]
internal sealed partial class InterfaceJson : JsonSerializerContext;
