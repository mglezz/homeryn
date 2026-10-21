# Verification

I run the integration checks from the repository root:

```powershell
dotnet build Homeryn.slnx --configuration Release --warnaserror
dotnet run --project tests/Homeryn.IntegrationTests --configuration Release --no-build
```

I use a standalone console runner without third-party test packages. I print one PASS line per completed check and return a nonzero exit code on failure. I do not use `dotnet test` for this runner.

I start the actual server on an ephemeral loopback port, so I can run verification without claiming the application's normal port 5000. I reference the actual desktop connection service and independently construct raw frames for wire-level checks.

I cover:

- Successful desktop PING/PONG.
- A header and payload written one byte at a time.
- Two frames sent together on one connection.
- Invalid JSON, invalid commands, and recovery on the same connection.
- Zero, negative, and oversized frame lengths.
- Truncated headers and payloads.
- Thirty-two connected clients making three requests each while another client stays idle.
- Caller cancellation and connection refusal.
- Oversized and semantically invalid server responses.
- The desktop operation's five-second timeout.
- Shutdown while clients are idle or partway through a frame, followed by port reuse.

I bound the suite with a 30-second cancellation deadline and a separate shutdown timeout. I verify 96 successful exchanges in the concurrency scenario; I do not interpret that small local test as a throughput benchmark or a scalability guarantee.

I do not automate Windows Forms interaction in this milestone. I document the remaining manual button check in the [development guide](../docs/development.md). I also do not test SQLite or business rules before they exist.