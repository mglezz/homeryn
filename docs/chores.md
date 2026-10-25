# Persistent chores increment

## Scope and rules

One local server owns one chore list alongside its shopping list. Every local client has the same access. Chores can be added, edited, assigned a free-text responsible person, completed, reopened, and permanently removed after confirmation.

| Field | Rule |
| --- | --- |
| Name | Trimmed; 1–80 UTF-16 code units; no remaining control characters. |
| Assignee | Optional free-text label; trimmed; up to 60 UTF-16 code units; no remaining control characters. Empty or whitespace-only values become null. |
| Due date | Optional calendar date, from 1753-01-01 through 9998-12-31, matching the desktop picker. No time or timezone conversion. Past dates are allowed. |
| Completed | Boolean; both completion and reopening are allowed. |
| Identifier | Positive server-generated identifier, never reused after deletion. |
| Version | Starts at 1 and increments on each successful update; stale updates and deletes are rejected. |

Duplicate names are allowed. Assignment does not establish identity, membership, or permissions. Recurrence, reminders, automatic rotation, and completion history remain outside this increment.

## Storage and migration

`SqliteDatabase.Initialize` owns schema creation and migration. Schema version 2 adds `Chores` with `Id`, `Name`, nullable `Assignee`, nullable ISO `DueDate`, `IsCompleted`, and `Version`. The database path is shared with shopping. A single transaction creates both tables on a new file or adds Chores to a version-1 database while preserving existing shopping rows and identifiers. Initialization is repeatable. Versions newer than 2 are rejected.

SQL parameters, per-operation connections, WAL mode, and the two-second SQLite lock timeout follow shopping. Both features share the dispatcher's storage gate. Updates and deletes check the expected version within the same transaction as the write and distinguish missing records from conflicts.

## Reads and failures

The client reads ordered pages of at most three chores so maximum-length escaped Unicode names and assignees fit within the 4,096-byte frame. The first page fixes an inclusive upper identifier; following pages retain that boundary. Later additions appear on the next refresh. Each page is internally consistent, but a complete refresh is not a snapshot across pages.

There is no automatic refresh or mutation retry. Conflicts, missing records, and unconfirmed outcomes disable changes until a successful Refresh. A failed request preserves the editor, and closing the form cancels pending client work. A timeout may occur after the server committed a change.

## Desktop workflow

Open Chores to load saved tasks. Add creates a chore from the editor when no row is selected. Select a row to populate the editor and enable Save and Remove. New clears the selection and fields without modifying storage. A checked date picker includes a due date; unchecked means no date. Completed takes effect when Save is clicked. Remove asks for confirmation. Successful changes clear the editor and update the pending/completed counts.

Controls and event subscriptions remain in the Designer file. Form code manages data binding, selection, feedback, and cancellation. `ChoresClient` validates responses and uses `TcpClientService`; `ChoresService` validates requests and delegates to `IChoreRepository`.

See [protocol](protocol.md) for wire fields and [verification](../tests/README.md) for automated and interactive evidence.
