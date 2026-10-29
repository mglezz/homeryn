# Release process

The first supported distribution is a portable, self-contained Windows x64 ZIP. It contains Desktop at the top level and Server under `server/`, plus runtime binaries, notices, a dependency inventory, and a user guide. Database files stay outside this package.

## Prepare and verify

1. Work on a release branch. Set the version in `Directory.Build.props` and add matching notes under `docs/releases/v<version>.md`. Update the package guide and README download filename for a version change.
2. Build with warnings treated as errors and run the console integration suite.
3. Run `./scripts/Package-Release.ps1` from PowerShell. It creates a new ZIP and `SHA256SUMS.txt` under `artifacts/release`. Existing ZIPs are refused; choose a different `-OutputDirectory` for another local build.
4. Run `./scripts/Test-Package.ps1 -ArchivePath <zip-path>`. Port 5000 must be free. It extracts outside the repository, uses isolated data, tests native executables and backup recovery, and stops only its own processes. Temporary logs remain available for diagnosis.
5. Review the diff and user-facing notes. Merge the PR only after `Verify` passes, including package checks.

Scripts require a PowerShell environment that permits repository scripts. GitHub-hosted Windows CI runs them with PowerShell 7. Do not change machine-wide execution settings just to build a release.

## Publish

After the reviewed commit is on `main`, create an annotated tag matching the version and push it:

```powershell
git switch main
git pull --ff-only
git tag -a v0.1.0 -m "Homeryn 0.1.0: first local Windows release"
git push origin v0.1.0
```

The `Release` workflow checks the tag against the project version, rebuilds, runs the suite, packages, and tests the extracted ZIP. Only then does it create a GitHub release with the ZIP and SHA-256 checksum. The workflow has contents-write permission for publication; ordinary PR verification has read-only permissions.

Check the published asset names, checksum, and release workflow conclusion. If verification fails, diagnose it before publishing. Never move a published tag or silently replace an existing release asset; issue a new patch version for fixes.

## Update and rollback

Back up first, stop the server and desktop, and extract updates into a new directory. Existing schema-1 shopping data migrates to schema 2. Recovery uses a copied backup with `--database`, preserving the original. See the [user guide](user-guide.md). There is no installer, automatic updater, or automatic database downgrade.

## Evidence and limits

The extracted package is exercised on Windows locally and in GitHub-hosted CI. Runtime configuration and bundled native runtime files are checked to verify self-contained deployment. This is not a claim that every Windows configuration or scaling setting was tested. Physical clean-machine testing, complete accessibility/DPI coverage, code signing, and prolonged-load qualification remain follow-up work.
