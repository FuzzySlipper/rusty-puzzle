using System.Text.Json;
using System.Text.Json.Serialization;
using Rusty.Engine;
using Rusty.Engine.Persistence;

namespace RustyPuzzle.Game.Persistence;

/// <summary>A solved room's best result.</summary>
internal sealed record RoomProgress(int BestMoves);

/// <summary>The saved progress: which rooms are solved and in how few moves. <see cref="Version"/> names the format.</summary>
internal sealed record Progress(int Version, Dictionary<string, RoomProgress> Rooms)
{
    internal const int CurrentVersion = 1;

    internal static Progress Empty => new(CurrentVersion, new Dictionary<string, RoomProgress>(StringComparer.Ordinal));
}

/// <summary>
/// The one owner of saved progress, in Engine persistence. It loads once at start and saves only at the solve
/// boundary: a room just solved records its move count when that beats the best so far. Mid-room play is never
/// saved. A save that exists but cannot be read is an error naming the store, never a silent fresh start.
/// </summary>
internal sealed class ProgressStore : IDisposable
{
    private const string Scope = "rusty-puzzle";
    private const string Key = "progress";
    private const string Location = $"persistence {Scope}/{Key}";

    private readonly ProductStateStore<Progress> _store;
    private ulong _revision;

    internal ProgressStore(IEngineContext engine)
    {
        _store = new ProductStateStore<Progress>(engine, Scope, new JsonProductStateCodec<Progress>(ProgressJson.Default.Progress));
    }

    internal Progress Current { get; private set; } = Progress.Empty;

    /// <summary>Reads the saved progress, or starts empty when nothing is saved.</summary>
    internal void Load()
    {
        ProductStateLoad<Progress> load;
        try
        {
            load = _store.Load(Key);
        }
        catch (Exception error) when (error is JsonException or PersistenceStorageException)
        {
            throw new InvalidOperationException($"{Location}: the saved progress cannot be read: {error.Message}", error);
        }

        if (!load.Present)
        {
            Current = Progress.Empty;
            _revision = 0;
            return;
        }

        if (load.State is not Progress saved)
        {
            throw new InvalidOperationException($"{Location}: the saved progress holds no value.");
        }

        if (saved.Version != Progress.CurrentVersion)
        {
            throw new InvalidOperationException($"{Location}: saved progress is version {saved.Version}; this build reads version {Progress.CurrentVersion}.");
        }

        // The JSON options do not enforce nullability inside dictionary values, so each result is checked here.
        foreach ((string room, RoomProgress? result) in saved.Rooms)
        {
            if (result is null || result.BestMoves < 0)
            {
                throw new InvalidOperationException(
                    $"{Location}: room '{room}' has an invalid result ({(result is null ? "null" : $"bestMoves {result.BestMoves}")}).");
            }
        }

        Current = saved;
        _revision = load.Revision;
    }

    /// <summary>Records a solved room and saves when it is new or beats the best move count.</summary>
    internal void Solved(string room, int moves)
    {
        if (Current.Rooms.TryGetValue(room, out RoomProgress? best) && best.BestMoves <= moves)
        {
            return;
        }

        Dictionary<string, RoomProgress> rooms = new(Current.Rooms, StringComparer.Ordinal) { [room] = new RoomProgress(moves) };
        Progress next = Current with { Rooms = rooms };
        PersistenceSaveReceipt receipt = _store.Save(Key, next,
            _revision == 0 ? PersistenceRevisionGuard.Absent : PersistenceRevisionGuard.Exact, _revision);
        if (receipt.Outcome != PersistenceSaveOutcome.Saved)
        {
            throw new InvalidOperationException($"{Location}: the save changed underneath this session ({receipt.Outcome}).");
        }

        Current = next;
        _revision = receipt.Revision;
    }

    public void Dispose() => _store.Dispose();
}

// Missing constructor values, nulls in non-nullable fields and unknown members are errors, not defaults.
[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    RespectRequiredConstructorParameters = true, RespectNullableAnnotations = true,
    UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow)]
[JsonSerializable(typeof(Progress))]
internal sealed partial class ProgressJson : JsonSerializerContext;
