# Start here — Homeryn 0.1.0

Shopping and chores for one Windows computer. Several copies of Homeryn can share the same local server.

## Start

1. Use Windows 11 x64. Download `Homeryn-0.1.0-win-x64.zip` from the official GitHub release.
2. Extract the entire ZIP to a writable folder. Keep the `server` folder beside `Homeryn.Desktop.exe`.
3. Open `Homeryn.Desktop.exe`, click **Start server**, then open **Shopping list** or **Chores**.

The .NET runtime is included; no SDK, Visual Studio, or separate runtime installation is needed. This first release is unsigned. A SHA-256 checksum is provided with the download; it checks file integrity, not publisher identity. Follow your organization's software policy if Windows prevents execution.

The server uses `127.0.0.1:5000`. It keeps running in its console after Desktop closes; press Ctrl+C in that console to stop it. If port 5000 is occupied, close your existing Homeryn server before starting a different version. Do not stop unrelated applications to free the port.

## Use the lists

- Enter a name and click **Add**. Purchases also have a quantity; chores can have a responsible person and an optional date.
- Select a row, change its fields, and **Save**. Purchased and Completed can both be reversed.
- **New** starts another entry. **Remove** asks before deleting a saved record.
- **Refresh** loads changes from other windows. Changes do not appear automatically.
- While the editor contains unsaved changes, row selection pauses. New, Refresh, and closing an idle window ask before discarding your work; **No** keeps it.
- After a conflict, timeout, or lost connection, refresh before trying another change. Do not repeat an uncertain Add blindly: it may already have been saved.

## Your data

The default database is `%LOCALAPPDATA%\Homeryn\server\homeryn.db`, separate from the extracted app. Updating the app does not delete it. Names are stored locally without encryption. There are no accounts or permissions between local clients; use this only on a trusted computer.

To upgrade, back up first, close Desktop, stop the server with Ctrl+C, extract the new version into a separate folder, and start it. Version-1 databases migrate automatically while preserving shopping data; newer unsupported schemas are rejected. Keep the old app and backup until the update is verified.

## Back up

From PowerShell in the extracted app folder:

```powershell
.\server\Homeryn.Server.exe --backup "$env:USERPROFILE\Documents\homeryn-backup.db"
```

Choose a new filename each time. Existing files are never overwritten. The command makes a consistent SQLite snapshot, verifies it, and exits. It also works while the server is running, though writes can briefly wait. A backup is one `.db` file; do not copy a live database manually because recent changes may be in its WAL journal.

For a custom source:

```powershell
.\server\Homeryn.Server.exe --database "D:\Homeryn data\home.db" --backup "D:\Homeryn backups\home-2026-10-29.db"
```

## Recover a backup without overwriting the original

1. Close Desktop and stop the current server with Ctrl+C.
2. Copy the backup to a **new recovery file** in a writable folder. Keep the backup unchanged.
3. Start the server with that recovery file:

```powershell
.\server\Homeryn.Server.exe --database "D:\Homeryn recovery\recovered.db"
```

4. Open Desktop, use **Check connection**, then refresh both lists and confirm your records.

Continue starting the server with this custom path while using the recovered data. **Start server** uses the default database when no server is already running. Restoring a snapshot does not include changes made after the snapshot. This workflow leaves the original database intact for comparison; automatic in-place restore is not included.

## Scope and help

This release includes one shopping list and one chore list per server, free-text assignees, explicit refresh, persistence, and backups. It has no household accounts, invitations, recurring tasks, reminders, LAN/cloud access, or automatic updates.

Report reproducible problems at https://github.com/mglezz/homeryn/issues. Include the app version, Windows version, steps, and error text. Remove personal names and never attach your database to a public issue.
