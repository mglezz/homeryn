# Contributing to Homeryn

Start with a focused bug report or a small improvement. For larger features, describe the use case in an issue before adding architecture or dependencies. Household accounts and remote access need an authorization design first.

## Develop

Use Windows and the stable .NET 10 SDK selected by `global.json`:

```powershell
dotnet build Homeryn.slnx --configuration Release --warnaserror
dotnet run --project tests/Homeryn.IntegrationTests --configuration Release --no-build
```

Keep networking, validation, and SQLite access outside forms. Preserve Designer-owned controls, keyboard order, and readable C# conventions. See the [development guide](docs/development.md).

Use a focused branch, technical English commit messages, and a PR describing behavior and verification. Add regression coverage for bugs; keep test data isolated from the user's database. CI builds and smoke-tests the portable package before integration.

Never commit databases, backups, personal names, credentials, or build output. Include anonymized steps and error messages in public reports.
