using Rusty.Engine.Testing;

// Product callbacks over the real pinned Engine services, without a browser. Each domain's checks live in its
// own file and run here in their own callback. This checks callback policy, not host lifecycle admission or
// what the renderer shows.
string persistence = Path.Combine(Path.GetTempPath(), "rusty-puzzle-smoke-persistence-" + Guid.NewGuid());
using EngineTestHost host = EngineTestHost.Create(new EngineTestHostOptions { PersistenceRoot = persistence });
host.Call(ContentChecks.Run);
host.Call(SelectionChecks.Run);
host.Call(LawChecks.Run);
host.Call(SessionChecks.Run);
host.Call(SolverChecks.Run);
host.Call(ProgressChecks.Run);
host.Call(DebugChecks.Run);
Console.WriteLine("Rusty Puzzle smoke passed.");
Directory.Delete(persistence, recursive: true);
