using RustyPuzzle.Game.Board;
using RustyPuzzle.Game.Party;

namespace RustyPuzzle.Game.Movement;

/// <summary>
/// One legal move: the member, the cell the player picks to make it, the law part that allows it, and the
/// board effects it would have. Nothing changes until a session applies <see cref="Effects"/>.
/// </summary>
internal sealed record Move(PartyMember Member, Cell Target, MovePart Part, IReadOnlyList<BoardEffect> Effects);
