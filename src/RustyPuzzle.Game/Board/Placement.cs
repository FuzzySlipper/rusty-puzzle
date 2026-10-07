using RustyPuzzle.Game.Party;

namespace RustyPuzzle.Game.Board;

/// <summary>Where one party member stands.</summary>
internal readonly record struct Placement(PartyMember Member, Cell Cell);
