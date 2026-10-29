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

The runner currently contains 43 checks. It creates unique databases under the temporary directory and removes its test directory after shutdown. It does not use the user's default database.

Four backup checks restore both lists from an active WAL database, verify independence from later source changes, refuse existing/self destinations, reject missing sources without creating empty databases, and reject invalid sources without publishing a partial copy.

Eight client-response checks use local fake servers to reject missing stored fields, incomplete pages, null records, control characters, invalid identifiers/cursors, and unexpected initial versions. Both list clients must cancel a slow multi-page read within the shared five-second deadline rather than restarting the timeout for every page.

Two editor checks run real Windows Forms controls on STA message loops against an isolated server. They cover Add/Save separation, dirty fields, draft selection protection, persisted quantity/completion changes, reverting edits, New, and clearing automatic row selection after add, save, and refresh. These forms are invisible and use ephemeral ports; they never use the normal server database. Modal confirmations and visual layout still require interactive verification.

The GitHub Actions `Verify` workflow runs the Release build and this console runner on Windows for every pull request and push to `main`. It also packages and tests the extracted self-contained applications outside the repository, covering native startup, TCP CRUD, stale deletion, command-line backup and recovery. It supports manual runs and retains the ZIP as a workflow artifact. It does not run interactive visual checks. See the [release process](../docs/releasing.md).

Seven chore checks also cover CRUD, trimmed Unicode names and assignees, due dates, completion and reopening, clearing optional fields, validation, stale edits/deletes, concurrent additions, bounded pagination, lock recovery, and restart persistence. Two migration checks verify version-1 shopping preservation, independent chore storage, repeated initialization, and rejection of a newer schema.

## Interactive verification

The integration runner exercises editor controls but does not assess visual layout or modal confirmations. To check the interface with isolated data:

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

## Chores interface checks

1. Open Chores with an isolated server database. Verify Add and Save are disabled for a blank name.
2. Add a chore with a responsible person and no date. Select it and verify the editor fields.
3. Check the due date and Completed, then Save. Verify the row, counts, and cleared editor.
4. Select the row, uncheck Completed and the due date, clear the responsible person, and Save.
5. Select a row and click New; verify the fields and selection clear without modifying storage.
6. Load the same version in two desktop windows, save in one, then save in the other. Verify a conflict requires Refresh.
7. Cancel a removal, then confirm removal and verify the resulting list.
8. Stop the server and Refresh. Verify readable feedback and disabled mutations. Restart with the same database and Refresh to recover.
9. Resize the form and open its Designer in Visual Studio to inspect the layout.

On 2026-10-25, interactive checks with an isolated database covered the empty list, disabled Add/Save, adding a named and assigned chore without a date, selecting it, saving a due date and completed state, pending/completed counts, editor reset, server-unavailable feedback, and successful Refresh after restart with saved fields preserved. The Chores form also opened successfully in the Visual Studio Designer. Concurrent conflicts, deletion, reopening, and clearing optional fields were covered by the integration runner.

## Draft protection checks

In both list windows, edit a selected row and verify that Add is disabled, Save is enabled, and row selection is paused. Click New, Refresh, or Close and choose No: the fields and selection must stay intact. Choose Yes to discard; New clears the editor, Refresh replaces it only after a successful load, and Close closes the window. Reverting all fields to their loaded values should enable the grid and disable Save without prompting. Repeat using only quantity/purchase changes and only assignee/date/completion changes.

On 2026-10-28, interactive checks with an isolated database covered creating and selecting shopping and chore records, disabled Add during editing, purchase/date changes activating Save, cancellation of shopping Refresh and chore Close preserving drafts, saved fields, and unavailable-server feedback preserving displayed rows. Visual inspection caught automatic grid selection after re-enabling; the corrected behavior is covered by both editor regression checks. The full modal matrix, two-process manual checklist, scaling checks, and Designer recheck remain pending.
