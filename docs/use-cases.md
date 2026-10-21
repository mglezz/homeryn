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
| UC-03 | Household member | Maintain a shared shopping list. | Core | Proposed |
| UC-04 | Household member | Record a purchased item. | Core | Proposed |
| UC-05 | Household member | Create and assign a chore. | Core | Proposed |
| UC-06 | Household member | Record a completed chore. | Core | Proposed |
| UC-07 | Household member | View current household information. | Core | Proposed |
| TECH-01 | Developer | Verify local client/server connectivity. | Current milestone | Implemented and checked through the desktop window |

Detailed household flows, permission rules, and acceptance criteria depend on the open questions in the [domain model](domain-model.md).

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
