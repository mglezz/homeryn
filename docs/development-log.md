# Development log

## 2026-10-29 — First portable local release

I prepared version 0.1.0 as a scoped Windows x64 release. I added adjacent-server discovery, shared assembly version metadata, and reproducible self-contained packaging. The archive contains the runtime, user guide, dependency inventory, and upstream notices; SHA-256 checksums accompany the download. A tag workflow validates the version and runs build, integration, and extracted-package checks before publication.

I added an online SQLite backup command that stages and integrity-checks a copy before publishing a new filename. It refuses overwrites and opens sources read-only. Regression tests recover shopping and chores from an active WAL database, preserve the original, and reject missing, corrupt, or unsafe destinations. The package smoke test also exercises command-line backup and recovery through the native executable.

I expanded editor coverage for stale updates made by a second client and refresh recovery. I refreshed the README with an original SVG banner, badges, downloads, and focused documentation, plus contribution and issue templates. I rendered and inspected the banner. The Windows Computer Use helper was unavailable after retries, so I did not claim a new complete visual review. Code signing, physical clean-machine testing, full DPI/accessibility coverage, and prolonged-load testing remain explicit release limitations.

## 2026-10-28 — Safer desktop editing

I aligned shopping creation/editing with chores by adding New and disabling Add for selected records. I tracked editable fields, disabled row changes while a draft exists, and added confirmation before New, Refresh, or an idle user close discards edits. Save is disabled when nothing changed, and text inputs use the server's length limits.

I checked both windows with isolated data, including purchase/date edits and cancelling discard prompts. Visual inspection exposed automatic first-row selection when a bound grid was re-enabled. I preserved the empty selection and added two real-control regression scenarios with injected clients, ephemeral ports, invisible forms, and STA message loops. The Release build passed without warnings and all 39 checks passed locally. Modal confirmations and visual review remain separate from those automated checks.

I documented the remaining local-release work instead of treating passing checks as a release guarantee. No release tag or package was created.

## 2026-10-28 — Response validation and repeatable verification

I reviewed and integrated the pending chores and layout branches after a clean Release build and 29 passing checks. I found that constructor-based JSON deserialization could silently supply false or null for omitted stored fields. I enabled required constructor fields while keeping optional request defaults, aligned shopping name validation with chores, and rejected impossible creation versions and continuation cursors.

I added one deadline for each complete list load and eight fake-server checks covering incomplete responses and slow pagination. I also added Windows CI using the actual console runner and a stable .NET 10 SDK selection. This protects the existing local scope; it does not establish release readiness or add household identity.

## 2026-10-25 — Consistent desktop forms

I applied the Chores visual layout to ShoppingListForm and MainForm: headings, hints, margins, native colors, button height, and footer spacing. Shopping now uses the same list/editor layout and opens centered over its parent. The main window groups its navigation into two balanced panels. I preserved the existing handlers and business behavior and kept the controls editable in the Windows Forms Designer.

I built Release with warnings treated as errors and ran all 29 integration checks successfully. I visually checked the main window at normal and maximized sizes, navigation into Shopping list, and the shopping layout with its connection-error message visible.

## 2026-10-25 — Persistent chores

I replaced the placeholder with a resizable Chores form and connected it to the existing TCP server. I kept controls and event subscriptions in the Designer file and added a separate client, service, repository, and shared contracts following the shopping implementation.

I chose optional free-text assignment because household identity does not exist yet. Due dates are optional calendar dates, and completion can be reversed. I retained version checks and refresh-after-conflict behavior. Three chores per page keep maximum-length escaped names and assignees within the existing frame limit.

I moved schema initialization into SqliteDatabase and added a transactional version-1-to-2 migration that preserves shopping data. I verified both features through the actual clients and TCP server with isolated databases, including concurrent changes, lock errors, paging, restart persistence, and migration. The Release build passed with warnings treated as errors, and all 29 integration checks passed.

I also checked the running interface with an isolated database: creating and assigning a chore, selecting it, setting a due date, marking it completed, saving, and recovering after the server stopped and restarted. Deletion and concurrent conflicts were verified by integration checks rather than the two-window manual workflow. Household accounts, member-based assignment, recurrence, and reminders remain future work.

## 2026-10-24 — Persistent shared shopping list

### Scope and decisions

I completed the first shopping-list increment on the existing feature branch. I kept one list per local server so I could verify a complete client/server/storage flow before introducing households, accounts, or permissions. I retained the main-window layout and navigation already added in Visual Studio, and completed the shopping controls in the designer.

I separated frame handling, command dispatch, application validation, and SQLite persistence. I used a small repository interface to keep SQL out of the shopping service. The desktop uses a shopping client over the existing framed TCP transport; form events only coordinate controls and requests.

I chose integer quantities from 1 to 999, trimmed names up to 80 UTF-16 code units, and reversible purchase state. I added a version to each item so a stale edit or delete cannot silently replace a newer change. I kept the 4 KiB frame limit and introduced five-item cursor pages with an initial maximum identifier.

### Problems resolved

I replaced the form's cached item-name field because clearing the textbox could leave an old name ready to submit. Inputs now come directly from the controls, and an empty name disables Add. During interactive verification, I also noticed that disabling the grid could trigger selection changes; I guarded selection handling while requests are pending to protect the editor.

I replaced the machine-specific server executable path with repository-relative discovery for the current build configuration and a file-selection fallback. I renamed the action Start server and check readiness with PING. Starting a process does not authenticate a user. I kept the Chores entry but added an explicit unavailable message instead of opening an empty form.

After checking Microsoft's provider documentation, I treated SQLite operations as synchronous. I offloaded one storage operation at a time behind an asynchronous semaphore, used a separate connection per operation, and bounded SQLite lock waits. The test with an external write lock confirms that PING still progresses and storage recovers after the lock is released.

I made uncertain write results explicit: a response may be lost after commit, so the desktop requires Refresh before another mutation instead of automatically retrying. The current refresh is not a snapshot across pages, and there are no server-pushed updates.

### Verification

I built the solution in Release with warnings treated as errors and ran all 20 integration checks. The checks cover the original TCP scenarios, shopping CRUD and validation, concurrent updates and additions, stale deletion, pagination, lock failure and recovery, and persistence across restart.

I used an isolated temporary database for the desktop checks. I verified adding Milk, selecting it, saving quantity 3 and purchased state, clearing the editor after success, and displaying a readable error with further mutations disabled when the server stopped. I closed the verification instance afterwards. The full two-window manual checklist remains documented separately; concurrent-write correctness and restart behavior have automated evidence.

I rebuilt after adding the selection guard and checked the final diff for whitespace errors. Household identity, authorization, chores, remote access, and CPU-bound parallel work remain outside this increment.

## 2026-10-21 — Local TCP foundation

### Starting point

I started from three Visual Studio project directories under `src/Homeryn/`, a solution that included only the server, and an existing Visual Studio ignore file. I found that the desktop entry point instantiated `MainForm`, but the form's partial declarations still used `Form1`. I also found missing references to Shared and empty model, service, and persistence classes.

### Decisions and changes

I moved the solution to the repository root and the projects directly under `src/`. I added the missing solution entries and references, aligned the main form name, and established `docs/` and `tests/`. I recorded the original scaffold in a dedicated build commit before replacing its networking placeholders.

I removed empty domain, service, and persistence classes until I can define meaningful behavior for them. I preserved the existing forms for manual interface development.

I chose length-prefixed UTF-8 JSON because TCP does not preserve application message boundaries. I limited payload allocation to 4,096 bytes and isolated malformed traffic to the relevant connection.

I implemented cancellable asynchronous accepts and per-client request loops. I used a five-second deadline for the desktop diagnostic and kept socket operations out of the form. I prepared `ConnectButton_Click` while leaving button creation and event wiring for Visual Studio.

### Problems encountered

I corrected the incomplete solution and the main form naming mismatch before adding networking.

During integration-test development, I found that retrieving `TcpClient.GetStream()` after a socket half-close can report a disconnected socket on Windows. I changed the truncated-frame test to retain its stream before half-closing the sending direction. I then verified that the server closes the incomplete connection and continues serving other clients.

I also handled `InvalidDataException` explicitly for invalid frame lengths, so the server isolates that failure and the form can report an invalid server response without terminating the application.

### Verification and limits

I built the solution in Release with warnings treated as errors on .NET SDK 10.0.401. I ran the 13 integration checks described in [Verification](../tests/README.md), including 32 simultaneous active clients, malformed traffic, cancellation, timeout, and shutdown.

I also launched the compiled console executable separately and verified PING/PONG on its fixed endpoint, `127.0.0.1:5000`. I confirmed that a second instance exits with code 1 when that port is already occupied. I tested graceful cancellation through the integration runner; I terminated the separate smoke-test process after checking the executable.

I have not performed the interactive button check because I am adding that control separately in Visual Studio. I do not mark that manual step complete.

I have not implemented household behavior, SQLite persistence, authentication, cross-device access, or CPU-bound parallel work. I will use the proposed domain model and use-case inventory to define the next increment.

## 2026-10-21 — Documentation voice and code readability

### Review

I found that applying first-person language to every document made instructions repetitive and obscured the actors in use-case flows. The distinction should be between describing the system and recording my own work.

After reviewing the TCP examples from class 7, the Windows Forms example from class 5, and the class and constructor examples in the [coursework repository](https://github.com/mglezz/SistemasDistribuidos-ProgramacionParalelo), I adopted a more explicit coding style.

### Changes

I rewrote technical descriptions, models, instructions, and project status in an impersonal style. Investigation, decisions, and problems remain in first-person singular. In the use cases, I replaced ambiguous personal actions with the responsible actor or component.

To make the code easier to follow, I replaced positional records and the server's primary constructor with ordinary classes and constructors. I expanded nested conditional expressions and property patterns into explicit conditions, used explicit object creation, and replaced the concurrency test's LINQ expressions with loops and a named method.

I retained asynchronous I/O, cancellation, payload limits, and complete-frame reads. These mechanisms are required by Homeryn's communication behavior even though the initial classroom socket example is synchronous.

### Verification

I built the updated solution in Release with warnings treated as errors and ran all 13 integration checks successfully. The checks still cover 32 concurrent active clients, malformed traffic, cancellation, timeout, and shutdown. I left the designer files and manual button wiring unchanged.

## 2026-10-21 — Desktop connection button

### Implementation

I added the connection button through the Windows Forms Designer in Visual Studio. I set its name to `connectButton`, its text to `Check connection`, its location to `24, 24`, and its size to `160, 36`. I assigned the existing `ConnectButton_Click` handler through the event properties.

### Verification

I built the solution in Release with warnings treated as errors, then launched the desktop application from Visual Studio. With the local server running, clicking the button displayed "The connection is working." After stopping the server, the same button displayed the connection-error message and became available again after dismissal.

These checks complete the previously pending desktop interaction for TECH-01. I updated the setup instructions and project status to reflect the working button.

I also reran all 13 integration checks successfully after adding the control.
