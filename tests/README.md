# Verification

Run the integration checks from the repository root:

```powershell
dotnet build Homeryn.slnx --configuration Release --warnaserror
dotnet run --project tests/Homeryn.IntegrationTests --configuration Release --no-build
```

The standalone console runner requires no third-party test packages. It prints one PASS line per completed check and returns a nonzero exit code on failure. Use `dotnet run`, not `dotnet test`, to execute it.

The runner starts the actual server on an ephemeral loopback port, leaving the application's normal port 5000 available. It uses the actual desktop connection service and independently constructs frames for wire-level checks.

Coverage includes:

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

A 30-second cancellation deadline bounds the suite, with a separate shutdown timeout. The concurrency scenario verifies 96 successful exchanges; it is not a throughput benchmark or scalability guarantee.

Windows Forms interaction is not automated in this milestone. The remaining manual button check is described in the [development guide](../docs/development.md). SQLite and business-rule tests will be added alongside those implementations.
