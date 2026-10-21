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

The window can open now, but the connection button must be added before the diagnostic can be used through the interface.

## Visual Studio integration

Open `Homeryn.slnx` at the repository root. The original solution path under `src/Homeryn/` is no longer used; any remaining `.vs` folder there is an ignored local cache.

The interface is edited manually in Visual Studio:

1. Open `MainForm` in the Windows Forms Designer.
2. Add a `Button`, set its `Name` to `connectButton`, and set its `Text` to `Check connection`.
3. Select the button, open the Properties event list, and assign `ConnectButton_Click` to `Click`.
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

## Next implementation boundary

Complete the manual button wiring before marking the desktop connectivity scenario complete. Define the first household operation, its contract, and its persistence behavior before adding SQLite or business-service classes.
