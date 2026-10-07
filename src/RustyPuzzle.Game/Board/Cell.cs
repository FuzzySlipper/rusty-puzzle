namespace RustyPuzzle.Game.Board;

/// <summary>One board square: <see cref="Column"/> grows to the right, <see cref="Row"/> grows down the authored rows.</summary>
internal readonly record struct Cell(int Column, int Row);
