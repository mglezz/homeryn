# Development guide

## Prerequisites and build

Development requires Windows, the .NET 10 SDK, and Visual Studio with .NET desktop development support. Server and Shared target `net10.0`; Desktop and the integration runner target `net10.0-windows`.

`global.json` selects the latest installed stable .NET 10 SDK feature band, starting at 10.0.100. GitHub Actions installs the matching SDK and runs the same Release build and integration runner for each pull request and push to `main`. Check the `Verify` workflow before merging; `dotnet test` alone is not a substitute.

Run these commands from the repository root:

```powershell
dotnet build Homeryn.slnx --configuration Release --warnaserror
dotnet run --project tests/Homeryn.IntegrationTests --configuration Release --no-build
```

The integration runner is a console application without third-party test packages. Execute it with `dotnet run`; `dotnet test` does not execute these checks.

## Run locally

Start the server in one terminal:

```powershell
dotnet run --project src/Homeryn.Server
```

The startup message should identify `127.0.0.1:5000`. Press Ctrl+C to stop the server. If another process owns that port, identify it before deciding whether to stop it.

Start the desktop application in another terminal:

```powershell
dotnet run --project src/Homeryn.Desktop
```

Click **Check connection** to run the diagnostic. A successful exchange displays "The connection is working." If the server is unavailable, the window displays a connection error.

## Visual Studio integration

Open `Homeryn.slnx` at the repository root. The original solution path under `src/Homeryn/` is no longer used; any remaining `.vs` folder there is an ignored local cache.

The controls are configured in the Windows Forms Designer. To inspect the connection diagnostic:

1. Open `MainForm` in the Windows Forms Designer.
2. Select `connectButton`, with the text `Check connection`. Preserve the current main-window layout when changing behavior.
3. Open the Properties event list and verify that `Click` is assigned to `ConnectButton_Click`.
4. Start the server, then the desktop project, and click the button.
5. Check that a success message appears. Repeat with the server stopped to verify error feedback.

Use the existing event handler. Keep networking code in `TcpClientService`. Server and Desktop can be configured as multiple startup projects; retry the check if the desktop starts before the server begins listening.

## Form layout

MainForm, ShoppingListForm, and ChoresForm use the same visual conventions: a bold 16-point Segoe UI heading, a short hint below it, 24-pixel outer margins, native controls, 32-pixel-high action buttons, and feedback below the actions. The two list windows share the same initial size, white grid area, and editor placement. The main window groups navigation into two equal-width panels.

Keep these settings and event subscriptions in each Designer file. Preserve anchors, minimum sizes, and keyboard tab order when moving controls. List windows open centered over the main window.

## Coding conventions

Use straightforward C# that can be followed one step at a time. The [coursework repository](https://github.com/mglezz/SistemasDistribuidos-ProgramacionParalelo) provides the reference style.

- Use descriptive English names for classes, methods, variables, and comments.
- Prefer ordinary classes, explicit constructors, and explicit object creation.
- Use namespace blocks and conventional entry-point methods in application code.
- Prefer `if/else` branches and loops when compact expressions obscure the steps.
- Keep methods focused and avoid abstractions without a concrete purpose.
- Explain non-obvious networking behavior with short comments.
- Keep `async/await`, cancellation, resource disposal, and complete-frame reads where required for correctness.

The shared framing methods use a type parameter, `T`, so the same read/write logic can handle both request and response classes. This avoids duplicating the protocol implementation.

## Documentation conventions

Use impersonal descriptions for architecture, protocols, domain models, use cases, and project status. Write instructions as direct actions, such as "Open the solution" or "Run the server."

Use first-person singular for investigation, decisions, errors encountered, and lessons learned. Keep those accounts primarily in the development log. Avoid repeating "I" in technical explanations.

Keep technical documentation in precise English. Use natural, accessible language in the README and release notes. Distinguish proposed functionality from implemented behavior.

## Git workflow

Use focused branches and technical English for branch names, commit messages, and pull-request descriptions.

Examples of commit subjects:

- `build: establish the .NET 10 solution and project references`
- `feat: implement framed JSON ping over asynchronous TCP`
- `test: verify concurrent TCP clients and connection failures`

Before integration, inspect the diff, build with warnings treated as errors, and execute the integration runner. The pull request should describe the resulting behavior, validation evidence, and incomplete work. Merge validated changes into `main` while preserving focused commits.

## Shopping list and local storage

Build the whole solution before using **Start server**. The launcher finds `Homeryn.Server.exe` under the repository's matching build configuration. If it cannot find the executable, select the built server in the file dialog. It checks PING before starting another process and waits for a response before reporting readiness. This is process startup, not authentication. The server has its own console and continues running after the desktop closes; stop it with Ctrl+C.

The default database is `%LOCALAPPDATA%\Homeryn\server\homeryn.db`. The startup log prints the resolved path. Changing the working directory does not select a different default database. To use isolated test data:

```powershell
dotnet run --project src/Homeryn.Server -- --database "$env:TEMP\homeryn-demo\shopping.db"
```

The server creates schema version 2 on a new database. Existing version-1 databases are migrated transactionally by adding Chores; shopping data is preserved. Schemas newer than version 2 are rejected. There is no automatic backup facility. Stop the server before copying the database together with any remaining `-wal` and `-shm` files. Do not commit personal shopping data; database files are ignored.

Open **Shopping list** to load items. **Add** creates an item from the fields. Selecting a row fills the editor; **Save** updates the selected version, and **Remove** asks for confirmation. **Refresh** reloads the list and clears the editor. Other windows' changes appear on refresh, not automatically. After a conflict or an unconfirmed result, refresh is required before another change.

`ShoppingListForm.Designer.cs` owns the controls and event subscriptions. The form configures the three data-bound columns in `ConfigureItemsGrid`; transport and storage remain outside the form. See [verification](../tests/README.md) for manual checks.

Both editors use **New** to clear the current selection and start an entry. Add is disabled while a stored row is selected, and Save is enabled only when its fields change. Unsaved drafts disable row changes; New, Refresh, and a normal idle-window close ask before discarding them, with No selected by default. Failed refreshes preserve the draft. Closing during a pending request still cancels client work; a mutation may already have committed, so reload before repeating it. Name inputs are limited to 80 characters and assignees to 60.

## Chores

Open **Chores** from the main window. Enter a name, optionally enter a responsible person and check the date picker to set a due date, then click **Add**. Select a row to edit it and click **Save**. Check **Completed** to finish a chore or uncheck it to reopen it. **New** clears the selection and fields. **Remove** asks for confirmation, and **Refresh** loads other clients' changes and clears the editor.

The controls and event subscriptions belong to `ChoresForm.Designer.cs`; its four data-bound columns are configured in `ConfigureItemsGrid`. The resizable layout remains editable in the Windows Forms Designer. Networking belongs to `ChoresClient` and `TcpClientService`; server validation and SQLite access remain outside the form.

Restart a previously running server after rebuilding so the new chore commands are available. Use an isolated database for manual testing as described above. See [Chores](chores.md) for data rules and [verification](../tests/README.md) for checks.

## Next implementation boundary

Define household identity, membership, and authorization before separating data by household or accepting remote clients. Chores currently use optional free-text assignees. Membership-based assignment, recurring chores, and reminders remain future work.
