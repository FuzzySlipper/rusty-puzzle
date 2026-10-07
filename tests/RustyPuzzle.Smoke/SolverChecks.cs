using Rusty.Engine;
using RustyPuzzle.Game.Board;
using RustyPuzzle.Game.Movement;
using RustyPuzzle.Game.Rooms;
using static Checks;

/// <summary>
/// Every shipped room, in the authored order, solved breadth-first with the product's own laws and effect
/// applier: each must be solvable, and its authored intent must hold. A combination room has no solution
/// without a swap or a vault over a member; an independent room does. Prints each room's shortest solution.
/// </summary>
static class SolverChecks
{
    // A room whose search passes this many boards is too open to check here; tighten its walls or its party.
    private const int MaximumBoards = 1_000_000;

    internal static void Run(IEngineContext engine)
    {
        using PackedContent shipped = PackedContent.Shipped();
        using Harness puzzle = new(engine, shipped);
        foreach (Room room in puzzle.Product.Content.Rooms)
        {
            (int? shortest, int boards) = Solve(room, interactions: true);
            (int? alone, _) = Solve(room, interactions: false);
            Console.WriteLine($"   {room.Id,-16} {room.Intent,-12} shortest {shortest?.ToString() ?? "none",4}  without help {alone?.ToString() ?? "none",4}  boards {boards}");
            Check(shortest is not null, $"{room.Id} is solvable");
            Check(room.Intent == RoomIntent.Combination ? alone is null : alone is not null,
                room.Intent == RoomIntent.Combination
                    ? $"{room.Id} needs the members to use one another"
                    : $"{room.Id} can be solved by each member alone");
        }
    }

    /// <summary>The fewest moves to put the whole party on exit cells, and how many boards the search reached.</summary>
    private static (int? Shortest, int Boards) Solve(Room room, bool interactions)
    {
        BoardState start = new(room.Grid, room.Starts);
        Dictionary<string, int> reached = new(StringComparer.Ordinal) { [Key(start)] = 0 };
        Queue<BoardState> frontier = new([start]);
        while (frontier.TryDequeue(out BoardState? board))
        {
            int moves = reached[Key(board)];
            if (board.Placements.All(placed => board.Grid.TerrainAt(placed.Cell).Exit))
            {
                return (moves, reached.Count);
            }

            if (reached.Count > MaximumBoards)
            {
                throw new InvalidOperationException($"{room.Id}: the search passed {MaximumBoards} boards.");
            }

            foreach (Placement placed in board.Placements)
            {
                foreach (Move move in LegalMoves.For(board, placed.Member))
                {
                    if (!interactions && Helped(board, move))
                    {
                        continue;
                    }

                    BoardState next = BoardEffects.Apply(board, move.Effects);
                    if (reached.TryAdd(Key(next), moves + 1))
                    {
                        frontier.Enqueue(next);
                    }
                }
            }
        }

        return (null, reached.Count);
    }

    /// <summary>A move that uses another member: a swap, or a step that vaults over a member.</summary>
    private static bool Helped(BoardState board, Move move)
    {
        if (move.Part is SwapMove)
        {
            return true;
        }

        Relocated step = move.Effects.OfType<Relocated>().Single(relocated => relocated.Member == move.Member);
        int distance = Math.Max(Math.Abs(step.To.Column - step.From.Column), Math.Abs(step.To.Row - step.From.Row));
        for (int crossed = 1; crossed < distance; crossed++)
        {
            Cell cell = new(step.From.Column + ((step.To.Column - step.From.Column) / distance * crossed),
                step.From.Row + ((step.To.Row - step.From.Row) / distance * crossed));
            if (board.MemberAt(cell) is not null)
            {
                return true;
            }
        }

        return false;
    }

    private static string Key(BoardState board) => string.Join(";", board.Placements.Select(placed => $"{placed.Cell.Column},{placed.Cell.Row}"));
}
