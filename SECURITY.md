# Security policy

## Supported use

The 0.1.x line supports a local Windows application on one trusted computer. Use the latest published patch. This is an initial release, not a security certification; automated checks cannot establish the absence of every vulnerability.

- The server rejects non-loopback listening addresses. The distributed executable uses `127.0.0.1:5000`; do not expose it through port forwarding, tunnels, or proxies.
- There is no client authentication. Other processes and users on the same computer may connect, read, or change the lists while the server is running. Loopback is not a boundary between Windows users or protection against local malware.
- SQLite databases and backups are unencrypted. Keep them in a private Windows profile or a directory with appropriate access permissions. Do not store passwords, financial records, or other sensitive information in list fields.
- Run without administrator privileges. The application does not require firewall exceptions, cloud credentials, or telemetry. Only open backups and server executables from trusted sources.
- The portable executables are unsigned. Download from this repository's Releases page. SHA-256 checksums detect changed downloads but are not a publisher signature.

## Controls and verification

The server limits admitted connections to 64, enforces a ten-second deadline for each exchange (including idle/partial reads), caps JSON frames at 4,096 bytes, validates inputs, and uses parameterized SQLite queries. These bounds reduce accidental resource exhaustion; they do not prevent a hostile local process from repeatedly connecting or filling the database.

PR and release workflows pin external Actions to commit IDs, do not persist checkout credentials, run a checksum-pinned Gitleaks scanner against available Git history, and fail on known direct/transitive NuGet vulnerability findings. Only the release job has repository write permission. Package checks reject database, private-key, environment, debug, and local IDE files. Scanner findings are redacted in logs. Audits depend on their rules and advisory feeds; the bundled runtime must also be updated in subsequent releases.

Run the checks after restoring/building:

Package verification also compares every bundled framework version against the latest runtime in [Microsoft's .NET 10 release metadata](https://github.com/dotnet/core/blob/main/release-notes/10.0/releases.json) and blocks outdated packages. Version 0.1.0 pins runtime 10.0.14 and requires SDK 10.0.403 or a newer patch of that feature band. If the feed is unavailable, verification fails rather than skipping this check.

```powershell
./scripts/Test-Security.ps1
```

Use a full Git checkout for history scanning. No user database is needed. The scanner is downloaded from its upstream GitHub release and verified before execution. Reports and tools remain under ignored `artifacts/`.

## Reporting a vulnerability

Use GitHub's **Security → Report a vulnerability** option if it is available for this repository. If it is unavailable, open an issue requesting a private contact channel, without exploit details, personal data, or credentials. Do not post a database or secrets in a public issue. Include affected versions, a minimal reproduction with synthetic data, and the expected impact when a private channel is established.

## Public repository privacy

Source code, issue contents, release notes, licenses, and Git author metadata are public. Existing commits retain their original author attribution, including email addresses. Secret scanning does not remove that metadata or make previously published information private. Review changes before committing and use a GitHub `noreply` address for new commits when personal email privacy is desired.
