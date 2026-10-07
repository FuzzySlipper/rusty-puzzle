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
    private readonly UiStream _stream;
    private ulong _sequence;
    private string? _published;

    internal PuzzleHud(IUiService ui)
    {
        _ui = ui;
        _stream = ui.OpenStream(new UiStreamRequest(Stream, Contract));
    }

    /// <summary>The last projection published, for observation.</summary>
    internal JsonObject? Current { get; private set; }

    /// <summary>Publishes the room in the authored words, unless the projection is unchanged.</summary>
    internal void Publish(RoomState room, HudText text)
    {
        JsonObject projection = Build(room, text);
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

    private static JsonObject Build(RoomState room, HudText text)
    {
        PartyMember? selected = room.SelectedMember;
        JsonArray party = [];
        foreach (Placement placed in room.Placements)
        {
            party.Add(new JsonObject
            {
                ["id"] = placed.Member.Id,
                ["name"] = placed.Member.Name,
                ["place"] = Template.Fill(text.MemberPlace, Member(placed.Member), Column(placed.Cell), Row(placed.Cell)),
                ["selected"] = placed.Member == selected,
                ["label"] = Template.Fill(text.SelectMember, Member(placed.Member)),
                ["command"] = Command(PuzzleCommand.Select(placed.Cell.Column, placed.Cell.Row)),
            });
        }

        return new JsonObject
        {
            ["title"] = text.Title,
            ["labels"] = new JsonObject
            {
                ["room"] = text.RoomLabel,
                ["selected"] = text.SelectedLabel,
                ["party"] = text.PartyHeading,
            },
            ["room"] = room.Room.Name,
            ["selected"] = selected?.Name ?? text.NobodySelected,
            ["status"] = Status(room, selected, text),
            ["party"] = party,
            ["intent"] = new JsonObject { ["id"] = PuzzleCommand.Intent, ["contract"] = PuzzleCommand.Contract },
        };
    }

    private static string Status(RoomState room, PartyMember? member, HudText text)
    {
        if (room.Selected is not Cell cell)
        {
            return text.NothingSelected;
        }

        (string, string) terrain = ("terrain", room.Room.Grid.TerrainAt(cell).Name);
        return member is null
            ? Template.Fill(text.CellSelected, terrain, Column(cell), Row(cell))
            : Template.Fill(text.MemberSelected, Member(member), terrain, Column(cell), Row(cell));
    }

    private static JsonNode Command(PuzzleCommand command) =>
        JsonSerializer.SerializeToNode(command, InterfaceJson.Default.PuzzleCommand)!;

    private static (string, string) Member(PartyMember member) => ("member", member.Name);

    // People count cells from one.
    private static (string, string) Column(Cell cell) => ("column", (cell.Column + 1).ToString(CultureInfo.InvariantCulture));

    private static (string, string) Row(Cell cell) => ("row", (cell.Row + 1).ToString(CultureInfo.InvariantCulture));
}
