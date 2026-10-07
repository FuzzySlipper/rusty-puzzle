using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;
using Rusty.Engine;
using RustyPuzzle.Game.Board;
using RustyPuzzle.Game.Party;
using RustyPuzzle.Game.Rooms;

namespace RustyPuzzle.Game.Interface;

/// <summary>
/// Publishes the board facts the DOM companion shows: the room, the selection, the party and the
/// commands each control sends. The DOM holds no board state of its own.
/// </summary>
internal sealed class PuzzleHud : IDisposable
{
    /// <summary>The projection stream and contract, as the product project declares them.</summary>
    private const string Stream = "rusty-puzzle";
    private const string Contract = "rusty.puzzle.board";

    private readonly IUiService _ui;
    private readonly InterfaceText _text;
    private readonly UiStream _stream;
    private ulong _sequence;
    private string? _published;

    internal PuzzleHud(IUiService ui, InterfaceText text)
    {
        _ui = ui;
        _text = text;
        _stream = ui.OpenStream(new UiStreamRequest(Stream, Contract));
    }

    /// <summary>The last projection published, for observation.</summary>
    internal JsonObject? Current { get; private set; }

    internal void Publish(RoomState room)
    {
        JsonObject projection = Build(room);
        string json = projection.ToJsonString();
        if (json == _published)
        {
            return;
        }

        _ui.PublishProjection(new UiProjection(_stream, ++_sequence, UiValues.FromJson(projection)));
        _published = json;
        Current = projection;
    }

    public void Dispose() => _stream.Dispose();

    private JsonObject Build(RoomState room)
    {
        PartyMember? selected = room.SelectedMember;
        JsonArray party = [];
        foreach (Placement placed in room.Placements)
        {
            party.Add(new JsonObject
            {
                ["id"] = placed.Member.Id,
                ["name"] = placed.Member.Name,
                ["place"] = Template.Fill(_text.MemberPlace, Member(placed.Member), Column(placed.Cell), Row(placed.Cell)),
                ["selected"] = placed.Member == selected,
                ["label"] = Template.Fill(_text.SelectMember, Member(placed.Member)),
                ["command"] = Command(PuzzleCommand.Select(placed.Cell.Column, placed.Cell.Row)),
            });
        }

        return new JsonObject
        {
            ["title"] = _text.Title,
            ["labels"] = new JsonObject
            {
                ["room"] = _text.RoomLabel,
                ["selected"] = _text.SelectedLabel,
                ["party"] = _text.PartyHeading,
            },
            ["room"] = room.Room.Name,
            ["selected"] = selected?.Name ?? _text.NobodySelected,
            ["status"] = Status(room, selected),
            ["party"] = party,
            ["intent"] = new JsonObject { ["id"] = PuzzleCommand.Intent, ["contract"] = PuzzleCommand.Contract },
        };
    }

    private string Status(RoomState room, PartyMember? member)
    {
        if (room.Selected is not Cell cell)
        {
            return _text.NothingSelected;
        }

        (string, string) terrain = ("terrain", room.Room.Grid.TerrainAt(cell).Name);
        return member is null
            ? Template.Fill(_text.CellSelected, terrain, Column(cell), Row(cell))
            : Template.Fill(_text.MemberSelected, Member(member), terrain, Column(cell), Row(cell));
    }

    private static JsonNode Command(PuzzleCommand command) =>
        JsonSerializer.SerializeToNode(command, InterfaceJson.Default.PuzzleCommand)!;

    private static (string, string) Member(PartyMember member) => ("member", member.Name);

    // People count cells from one.
    private static (string, string) Column(Cell cell) => ("column", (cell.Column + 1).ToString(CultureInfo.InvariantCulture));

    private static (string, string) Row(Cell cell) => ("row", (cell.Row + 1).ToString(CultureInfo.InvariantCulture));
}
