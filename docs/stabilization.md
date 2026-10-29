# Local stabilization

Version 0.1.0 targets one shared shopping list and one chore list on Windows 11 x64. Multiple desktop processes can use the same loopback server; assignment remains free text. This initial local release has a deliberately limited scope. See the [release notes](releases/v0.1.0.md).

## Verified foundation

- Release builds treat warnings as errors. Windows CI runs the console integration suite on every pull request and push to `main`.
- Shopping and chores support persistent creation, editing, removal, and reversible purchase/completion state.
- Version checks reject stale updates and deletes. SQLite lock recovery, restart persistence, and version-1 migration have regression coverage.
- Malformed frames and incomplete records are rejected. Complete list loads share a bounded deadline and do not replace the displayed list with partial results.
- Editors separate creation from editing, track changes, protect drafts, and preserve refresh-required recovery after conflicts or uncertain responses.
- Real Windows Forms controls are exercised against an isolated TCP server for add, edit, save, refresh, New, and selection reset. Interactive checks supplement these tests; see [verification](../tests/README.md).
- Online SQLite backups are integrity-checked and never overwrite existing files. Regression checks restore both lists from an active WAL database and preserve the original.
- The portable self-contained ZIP includes the runtime and server. CI extracts it outside the repository and checks native desktop startup, TCP behavior, backups, and recovery before release publication.

## Follow-up qualification

- Complete the documented two-process conflict/removal checklist through the UI, including closing during a pending operation.
- Verify layouts and keyboard navigation at common Windows scaling settings and with long names, dates, and error messages. The latest changes have not been rechecked in the Visual Studio Designer.
- Test the portable package on a physical Windows machine without any .NET installation. Package checks already verify bundled runtimes and self-contained configuration, but hosted Windows runners also have development tools installed.
- Consider an installer, code signing, and an automatic update flow. These are not included in 0.1.0.
- Exercise prolonged use and larger lists beyond the integration fixtures. The five-second refresh limit is a responsiveness safeguard, not a performance guarantee.

## Separate future scope

Accounts, invitations, separate households, authentication, and access from other devices require a household authorization model. They are not enabled by this milestone. Recurring chores, reminders, automatic synchronization, and automatic mutation retries remain unimplemented.

The verification workflow retains a test package. The separate tag-triggered release workflow rebuilds and validates before publishing a ZIP and checksum. Release procedure: [releasing](releasing.md).
