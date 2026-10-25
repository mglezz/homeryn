# Actors and use cases

Household goals are distinct from the connectivity experiment. The TCP server and SQLite are implementation components inside the intended system boundary, not external business actors.

## Actors

| Actor | Responsibility | Status |
| --- | --- | --- |
| Household member | Organize shared purchases and chores. | Proposed |
| Household organizer | Establish a household and manage participation; precise permissions remain open. | Proposed |
| Developer | Start the local server and verify client/server connectivity. | Current technical scenario |

## Initial use-case inventory

| ID | Actor | Goal | Priority | Status |
| --- | --- | --- | --- | --- |
| UC-01 | Household organizer | Create a household. | Core | Proposed |
| UC-02 | Household organizer | Invite a person to the household. | Core | Proposed |
| UC-03 | Household member | Maintain a shared shopping list. | Core | Implemented locally without membership or permissions |
| UC-04 | Household member | Record a purchased item. | Core | Implemented locally without membership or permissions |
| UC-05 | Household member | Create and assign a chore. | Core | Implemented locally with free-text assignment |
| UC-06 | Household member | Record a completed chore. | Core | Implemented locally without membership or permissions |
| UC-07 | Household member | View current household information. | Core | Proposed |
| TECH-01 | Developer | Verify local client/server connectivity. | Current milestone | Implemented and checked through the desktop window |

Household identity and permission rules still depend on the open questions in the [domain model](domain-model.md). In the current prototype, any local client acts as a list user without authentication.

## UC-03: Maintain the local shared shopping list

**Preconditions:** The server is running and its database is available.

1. The user opens the shopping window; the application loads the list.
2. The user enters a name and quantity and clicks Add.
3. The server validates the input, stores the item, and returns its identifier and version.
4. The window displays the committed item and clears the editor.
5. The user selects a row, changes its fields, and clicks Save, or clicks Remove and confirms deletion.
6. The server checks the selected version and commits the change.

**Alternatives:** Invalid input leaves storage unchanged. A stale edit or deleted item requires refresh. A timeout or communication failure leaves the result uncertain and requires refresh before another mutation. Refresh clears the editor and loads current records. Closing the window cancels pending client work.

**Postcondition:** Successful changes are persisted in SQLite and can be loaded by other clients. Membership and authorization are outside this increment.

## UC-04: Record or reverse a purchase

Select an item, change Purchased, and click Save. The same validation, version check, persistence, and failure behavior as UC-03 apply. Other windows observe the result on Refresh.

## UC-05: Maintain local chores

With the server running, open Chores. Enter a chore name, optionally a responsible person and due date, and click Add. Select a row to edit its fields and Save, or Remove and confirm deletion. New clears the selection and editor; Refresh loads current records. Assignment is a free-text label, not an authenticated household member. Validation, version conflicts, uncertain outcomes, and cancellation follow UC-03.

## UC-06: Complete or reopen a chore

Select a chore, check Completed, and Save. Uncheck Completed and Save to reopen it. The server persists the new version. Other clients observe it on Refresh. Completed chores stay visible until explicitly removed.

## TECH-01: Verify local connectivity

**Goal:** Verify communication between the desktop connection service and the local TCP server.

**Preconditions:** The solution has been built, the server is listening on `127.0.0.1:5000`, and a button's `Click` event is connected to `ConnectButton_Click` for the manual UI check.

**Main flow:**

1. The developer clicks the connection button.
2. The form disables repeated activation while the operation is pending.
3. The connection service opens a TCP connection and sends a framed JSON `PING`.
4. The server returns a framed JSON `PONG`.
5. The connection service validates the response and closes the connection.
6. The form displays the result and enables the button for another check.

**Alternative flows:**

- The form displays a readable connection error if the server is unavailable or its response is invalid.
- The form displays a timeout message if the operation exceeds five seconds.
- Closing the form cancels the operation.

**Postcondition:** One exchange has been verified. No household member is authenticated, and no persistent session is established.

**Acceptance evidence:** The integration runner exercises the actual desktop service against the server, including malformed traffic, client failures, concurrent clients, and shutdown. The desktop window has also been checked with the server running and stopped: the button displays the appropriate result and becomes available for another check.
