# Development guide

## Prerequisites and build

Development requires Windows, the .NET 10 SDK, and Visual Studio with .NET desktop development support. Server and Shared target `net10.0`; Desktop and the integration runner target `net10.0-windows`.

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

The server creates schema version 1 on a new database and rejects a newer schema version. There is no automatic migration or backup facility yet. Stop the server before copying the database together with any remaining `-wal` and `-shm` files. Do not commit personal shopping data; database files are ignored.

Open **Shopping list** to load items. **Add** creates an item from the fields. Selecting a row fills the editor; **Save** updates the selected version, and **Remove** asks for confirmation. **Refresh** reloads the list and clears the editor. Other windows' changes appear on refresh, not automatically. After a conflict or an unconfirmed result, refresh is required before another change.

`ShoppingListForm.Designer.cs` owns the controls and event subscriptions. The form configures the three data-bound columns in `ConfigureItemsGrid`; transport and storage remain outside the form. See [verification](../tests/README.md) for manual checks.

## Next implementation boundary

Define household identity, membership, and authorization before separating data by household or accepting remote clients. Chores and assignment rules remain the next business capability; they are not implemented by this shopping increment.
