# Development log

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
