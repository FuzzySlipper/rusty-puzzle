using System.Buffers;
using System.Text;
using Rusty.Engine;
using Rusty.Engine.Persistence;
using static Checks;

/// <summary>
/// Saved progress through the test host's temporary persistence root: saved only at the solve boundary,
/// restored by a relaunched product, best results kept, and an unreadable save refused with the store named.
/// </summary>
static class ProgressChecks
{
    private const string Scope = "rusty-puzzle";
    private const string Key = "progress";

    internal static void Run(IEngineContext engine)
    {
        Clear(engine);
        using PackedContent shipped = PackedContent.Shipped();
        using (Harness first = new(engine, shipped))
        {
            Check(!Solved(first, 0), "a first launch has no solved rooms");
            SolveFirstSteps(first);
            Check(Solved(first, 0) && Label(first, 0).Contains("solved in 5", StringComparison.Ordinal),
                "solving a room saves it with its move count and the picker marks it");
            first.Send(first.Hud()["rooms"]![1]!["command"]!.ToJsonString());
            first.MoveVia(1, 1, 3, 1);
        }

        using (Harness relaunched = new(engine, shipped))
        {
            Check(Solved(relaunched, 0) && Label(relaunched, 0).Contains("solved in 5", StringComparison.Ordinal),
                "a relaunched product restores solved rooms at start");
            Check(!Solved(relaunched, 1) && (string?)relaunched.Observe()["room"] == "first-steps",
                "a room left mid-play is not saved, and play starts at the first room again");
            relaunched.MoveVia(1, 1, 2, 1);
            relaunched.MoveVia(2, 1, 1, 1);
            SolveFirstSteps(relaunched);
            Check(Label(relaunched, 0).Contains("solved in 5", StringComparison.Ordinal), "a slower solve keeps the best move count");
        }

        Write(engine, "not json at all");
        Check(Refusal(() => { using var product = Harness.Create(engine, shipped); product.Start(); })
                .StartsWith("persistence rusty-puzzle/progress", StringComparison.Ordinal),
            "an unreadable save is reported naming the store, not replaced by a fresh start");
        Write(engine, "{\"version\": 2, \"rooms\": {}}");
        Check(Refusal(() => { using var product = Harness.Create(engine, shipped); product.Start(); }).Contains("version 2", StringComparison.Ordinal),
            "a save of another format version is reported, not misread");
        Clear(engine);
    }

    // First Steps: the fighter walks four squares right and one down onto the exit.
    private static void SolveFirstSteps(Harness puzzle)
    {
        for (int column = 1; column < 5; column++)
        {
            puzzle.MoveVia(column, 1, column + 1, 1);
        }

        puzzle.MoveVia(5, 1, 5, 2);
    }

    private static bool Solved(Harness puzzle, int index) => (bool)puzzle.Hud()["rooms"]![index]!["solved"]!;

    private static string Label(Harness puzzle, int index) => (string)puzzle.Hud()["rooms"]![index]!["label"]!;

    private static void Write(IEngineContext engine, string text)
    {
        using ProductStateStore<byte[]> raw = new(engine, Scope, new Bytes());
        raw.Save(Key, Encoding.UTF8.GetBytes(text), PersistenceRevisionGuard.Any, 0);
    }

    private static void Clear(IEngineContext engine)
    {
        using ProductStateStore<byte[]> raw = new(engine, Scope, new Bytes());
        raw.Delete(Key, PersistenceRevisionGuard.Any, 0);
    }

    private sealed class Bytes : IProductStateCodec<byte[]>
    {
        public void Encode(in byte[] state, IBufferWriter<byte> destination) => destination.Write(state);

        public byte[] Decode(ReadOnlySpan<byte> payload) => payload.ToArray();
    }
}
