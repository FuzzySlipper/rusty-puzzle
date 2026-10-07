using Rusty.Engine.Testing;

// Product callbacks over the real pinned Engine services, without a browser. Each domain's checks live in its
// own file and run here in their own callback. This checks callback policy, not host lifecycle admission or
// what the renderer shows.
using EngineTestHost host = EngineTestHost.Create(new EngineTestHostOptions());
host.Call(ContentChecks.Run);
host.Call(SelectionChecks.Run);
host.Call(LawChecks.Run);
host.Call(DebugChecks.Run);
Console.WriteLine("Rusty Puzzle smoke passed.");
