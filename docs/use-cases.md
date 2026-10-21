# Actors and use cases

I separate household goals from the connectivity experiment. I do not classify the TCP server or SQLite as external business actors: I treat them as implementation components inside the intended system boundary.

## Actors

| Actor | Responsibility I model | Status |
| --- | --- | --- |
| Household member | Organize shared purchases and chores. | Proposed |
| Household organizer | Establish a household and manage participation; precise permissions remain open. | Proposed |
| Developer | Start the local server and verify client/server connectivity. | Current technical scenario |

## Initial use-case inventory

| ID | Actor | Goal | Priority I assign | Status |
| --- | --- | --- | --- | --- |
| UC-01 | Household organizer | Create a household. | Core | Proposed |
| UC-02 | Household organizer | Invite a person to the household. | Core | Proposed |
| UC-03 | Household member | Maintain a shared shopping list. | Core | Proposed |
| UC-04 | Household member | Record a purchased item. | Core | Proposed |
| UC-05 | Household member | Create and assign a chore. | Core | Proposed |
| UC-06 | Household member | Record a completed chore. | Core | Proposed |
| UC-07 | Household member | View current household information. | Core | Proposed |
| TECH-01 | Developer | Verify local client/server connectivity. | Current milestone | Implemented below the UI; manual button wiring pending |

I defer detailed household flows, permission rules, and acceptance criteria until I resolve the open questions in the [domain model](domain-model.md).

## TECH-01: Verify local connectivity

**Goal:** I verify that the desktop connection service can communicate with the local TCP server.

**Preconditions:** I have built the solution, started the server on `127.0.0.1:5000`, and, for a manual UI check, connected a button's `Click` event to `ConnectButton_Click`.

**Main flow:**

1. I click the connection button.
2. I disable repeated activation while the operation is pending.
3. I establish a TCP connection and send a framed JSON `PING`.
4. I receive and validate a framed JSON `PONG`.
5. I close the diagnostic connection and display the result.
6. I enable the button for another check.

**Alternative flows:**

- I display a readable connection error if the server is unavailable or its response is invalid.
- I display a timeout message if the operation exceeds five seconds.
- I cancel the operation when I close the form.

**Postcondition:** I have verified one exchange; I have not authenticated a household member or established a persistent session.

**Acceptance evidence:** I exercise the actual desktop service against the server in the integration runner, including malformed traffic, client failures, concurrent clients, and shutdown. I still need to perform the manual button check after I create the control in Visual Studio.
