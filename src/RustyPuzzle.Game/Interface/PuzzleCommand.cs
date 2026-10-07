using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Rusty.Engine;
using RustyPuzzle.Game.Board;
using RustyPuzzle.Game.Rooms;

namespace RustyPuzzle.Game.Interface;

/// <summary>What a <see cref="PuzzleCommand"/> asks for.</summary>
[JsonConverter(typeof(PuzzleActionConverter))]
internal enum PuzzleAction
{
    /// <summary>Select the cell at <see cref="PuzzleCommand.Column"/>, <see cref="PuzzleCommand.Row"/>.</summary>
    Select,

    /// <summary>Clear the selection.</summary>
    Clear,
}

/// <summary>
/// One <c>puzzle.command.v1</c> payload from the interface: <c>{"action": ..., fields}</c>. The interface
/// sends the commands the projection hands it, so the actions and their fields are declared only here.
/// </summary>
internal sealed record PuzzleCommand(PuzzleAction Action, int? Column = null, int? Row = null)
{
    /// <summary>The payload intent and contract, as the product project declares them.</summary>
    internal const string Intent = "puzzle.command";
    internal const string Contract = "puzzle.command.v1";
    private static readonly byte[] IntentBytes = Encoding.UTF8.GetBytes(Intent);
    private static readonly byte[] ContractBytes = Encoding.UTF8.GetBytes(Contract);

    internal static PuzzleCommand Select(int column, int row) => new(PuzzleAction.Select, column, row);

    /// <summary>Applies the command to the room; false when the room refuses it.</summary>
    internal bool ApplyTo(RoomState room) => Action switch
    {
        PuzzleAction.Select => Column is int column && Row is int row && room.Select(new Cell(column, row)),
        PuzzleAction.Clear => room.Select(null),
        _ => throw new InvalidOperationException($"{Contract}: unknown action {Action}."),
    };

    /// <summary>The command a UI claim carries, or null for any other input.</summary>
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
}

internal sealed class PuzzleActionConverter() : JsonStringEnumConverter<PuzzleAction>(JsonNamingPolicy.KebabCaseLower, allowIntegerValues: false);

// Missing constructor values, nulls in non-nullable fields and unknown members are errors, not defaults.
[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    RespectRequiredConstructorParameters = true, RespectNullableAnnotations = true,
    UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow)]
[JsonSerializable(typeof(PuzzleCommand))]
internal sealed partial class InterfaceJson : JsonSerializerContext;
