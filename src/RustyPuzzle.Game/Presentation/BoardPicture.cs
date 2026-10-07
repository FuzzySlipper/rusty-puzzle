using RustyPuzzle.Game.Board;
using RustyPuzzle.Game.Movement;

namespace RustyPuzzle.Game.Presentation;

/// <summary>
/// Everything the board view shows at one moment: the board, the selected cell, the selected member's legal
/// moves and the move the pointer rests on, whose effects are previewed. Equal pictures need no republish.
/// </summary>
internal sealed record BoardPicture(BoardState Board, Cell? Selected, IReadOnlyList<Move> Moves, Move? Preview);
