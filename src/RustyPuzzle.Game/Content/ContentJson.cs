using System.Text.Json.Serialization;
using RustyPuzzle.Game.Board;
using RustyPuzzle.Game.Interface;
using RustyPuzzle.Game.Party;
using RustyPuzzle.Game.Presentation;
using RustyPuzzle.Game.Rooms;

namespace RustyPuzzle.Game.Content;

// Every authored record type, read strictly: missing constructor values, nulls in non-nullable fields and
// unknown members are errors, not defaults. A new authored record adds its line here.
[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    RespectRequiredConstructorParameters = true, RespectNullableAnnotations = true,
    UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow)]
[JsonSerializable(typeof(TerrainDefinition))]
[JsonSerializable(typeof(MemberDefinition))]
[JsonSerializable(typeof(RoomDefinition))]
[JsonSerializable(typeof(RoomOrder))]
[JsonSerializable(typeof(HudText))]
[JsonSerializable(typeof(BoardViewDefinition))]
internal sealed partial class ContentJson : JsonSerializerContext;
