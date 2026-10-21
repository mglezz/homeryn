# Development guide

## Prerequisites and build

I use Windows, the .NET 10 SDK, and Visual Studio with .NET desktop development support. I target `net10.0` for Server and Shared, and `net10.0-windows` for Desktop and the integration runner.

I run these commands from the repository root:

```powershell
dotnet build Homeryn.slnx --configuration Release --warnaserror
dotnet run --project tests/Homeryn.IntegrationTests --configuration Release --no-build
```

I use a package-free executable integration runner at this stage. I run it with `dotnet run`; I do not use `dotnet test` as evidence that these checks executed. I reserve migration to a test framework for a later testing milestone.

## Run locally

I start the server in one terminal:

```powershell
dotnet run --project src/Homeryn.Server
```

I expect the startup message to identify `127.0.0.1:5000`. I stop the server with Ctrl+C. If another process owns that port, I inspect it before deciding which process to stop.

I start the desktop application in another terminal:

```powershell
dotnet run --project src/Homeryn.Desktop
```

I can open the window now, but I must add the connection button before using the diagnostic through the interface.

## Visual Studio integration

I open `Homeryn.slnx` at the repository root. I no longer open the initial solution from `src/Homeryn/`; any remaining `.vs` folder there is an ignored local cache.

I keep the interface layout under manual control in Visual Studio:

1. I open `MainForm` in the Windows Forms Designer.
2. I add a `Button`, set its `Name` to `connectButton`, and set its `Text` to `Check connection`.
3. I select the button, open the Properties event list, and assign `ConnectButton_Click` to `Click`.
4. I start the server, then the desktop project, and click the button.
5. I expect a readable success message. I repeat with the server stopped to verify error feedback.

I use the existing event handler rather than generate a second implementation. I do not edit the networking service through the designer. I can configure Server and Desktop as multiple startup projects, but I retry the check if the desktop starts before the server begins listening.

## Git workflow

I work on focused branches and keep commit messages, branch names, and pull-request descriptions in technical English.

I use commit subjects such as `build: establish the .NET 10 solution and project references`, `feat: implement framed JSON ping over asynchronous TCP`, and `test: verify concurrent TCP clients and connection failures`.

Before integration, I inspect the diff, build with warnings treated as errors, and execute the integration runner. I describe the resulting behavior, validation evidence, and incomplete work in the pull request. I merge a validated branch into `main` while preserving its focused commits.

I keep README and release notes approachable and distinguish working capabilities from planned features. I write engineering explanations and development records in first-person singular.

## Next implementation boundary

I will finish the manual button wiring before marking the desktop connectivity scenario complete. I will then define the first household operation, its contract, and its persistence behavior before adding SQLite or business-service classes.
