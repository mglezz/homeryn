# Local stabilization

The current target is a reliable Windows application for one shared shopping list and one chore list on a single computer. This is a stabilization milestone, not a release. Multiple desktop processes can use the same loopback server; assignment remains free text.

## Verified foundation

- Release builds treat warnings as errors. Windows CI runs the console integration suite on every pull request and push to `main`.
- Shopping and chores support persistent creation, editing, removal, and reversible purchase/completion state.
- Version checks reject stale updates and deletes. SQLite lock recovery, restart persistence, and version-1 migration have regression coverage.
- Malformed frames and incomplete records are rejected. Complete list loads share a bounded deadline and do not replace the displayed list with partial results.
- Editors separate creation from editing, track changes, protect drafts, and preserve refresh-required recovery after conflicts or uncertain responses.
- Real Windows Forms controls are exercised against an isolated TCP server for add, edit, save, refresh, New, and selection reset. Interactive checks supplement these tests; see [verification](../tests/README.md).

## Before a local release

- Complete the documented two-process conflict/removal checklist through the UI, including closing during a pending operation.
- Verify layouts and keyboard navigation at common Windows scaling settings and with long names, dates, and error messages. The latest changes have not been rechecked in the Visual Studio Designer.
- Define and test a backup/restore workflow. Currently, stop the server before copying the database and any remaining journal files as described in the development guide.
- Validate installation or portable packaging on a clean Windows machine, including server discovery and runtime requirements. The current workflow assumes a development build.
- Exercise prolonged use and larger lists beyond the integration fixtures. The five-second refresh limit is a responsiveness safeguard, not a performance guarantee.

## Separate future scope

Accounts, invitations, separate households, authentication, and access from other devices require a household authorization model. They are not enabled by this milestone. Recurring chores, reminders, automatic synchronization, and automatic mutation retries remain unimplemented.

No release tag or distributable is produced by the verification workflow.
