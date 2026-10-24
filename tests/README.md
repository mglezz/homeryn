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

A 90-second cancellation deadline bounds the suite, with separate shutdown timeouts. The concurrency scenario verifies 96 successful exchanges; it is not a throughput benchmark or scalability guarantee.

Seven additional checks use isolated SQLite files and the actual shopping client and server:

- Add, read, update, and delete, including trimmed Unicode names and purchase state.
- Invalid names, quantities, missing payloads, identifiers, versions, and cursors.
- Concurrent updates of the same version, stale deletion, and missing items.
- Twenty-four concurrent additions, unique identifiers, and multiple pages of maximum-length escaped Unicode names.
- Pagination across deletion and a subsequent addition beyond the first page's upper bound.
- Database lock timeout while PING remains responsive, followed by recovery.
- Restart persistence for fields, versions, and deletions.

The runner currently contains 20 checks. It creates unique databases under the temporary directory and removes its test directory after shutdown. It does not use the user's default database.

## Interactive verification

The integration runner does not automate Windows Forms. To check the interface with isolated data:

1. Start the server with `--database` pointing to a temporary file, then launch Desktop.
2. Open Shopping list. Verify that an empty or whitespace-only name disables Add.
3. Add an item, select it, change its quantity and purchase state, and Save.
4. Open a second desktop process and load the list. Edit the same version in both windows; save one, then verify that the second reports a conflict and requires Refresh.
5. Confirm Remove deletes a selected item; cancelling confirmation leaves it unchanged.
6. Restart the server with the same file and reload to verify persistence.
7. Stop the server, attempt Refresh, and verify readable feedback and disabled mutations. Restart and Refresh to recover.
8. Close the shopping window while a request is pending; the application should remain usable.

The connection-button procedure remains in the [development guide](../docs/development.md). Record completed manual checks separately from this repeatable checklist.

On 2026-10-24, interactive checks with an isolated database covered an empty list, disabled Add for an empty name, adding an item, selecting it, saving quantity 3 and purchased state, clearing the editor after success, and feedback with mutations disabled after the server stopped. Deletion, concurrent conflicts, and restart persistence were verified by the integration runner; the full two-window manual checklist has not been completed.
