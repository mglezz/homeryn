# Engineering documentation

The technical documentation describes implemented behavior, design proposals, setup, and verification. Explanations, models, and instructions use an impersonal style. Investigation notes, decisions, and accounts of problems use first-person singular.

The root README and future release notes use clear, approachable language for people who will use Homeryn.

| Document | Purpose |
| --- | --- |
| [Development guide](development.md) | Build commands, Visual Studio integration, coding conventions, and Git workflow. |
| [Architecture](architecture.md) | Component responsibilities and dependency boundaries. |
| [Wire protocol](protocol.md) | Framing, messages, errors, and connection lifetime. |
| [Shopping list increment](shopping-list.md) | Scope, persistence model, concurrency rules, and acceptance criteria. |
| [Chores increment](chores.md) | Chore fields, assignment, completion, schema migration, and concurrency. |
| [Domain model](domain-model.md) | Proposed household concepts and relationships. |
| [Actors and use cases](use-cases.md) | User goals and the current technical verification scenario. |
| [Development log](development-log.md) | Investigation, problems, decisions, and verification evidence. |
| [Verification](../tests/README.md) | Executable integration checks and their limits. |

## Reference material

- [idsw1](https://github.com/mmasias/idsw1): progression from domain modeling to actors and use cases.
- [hdC domain model](https://github.com/mmasias/hdC/blob/main/docs/UMLdocs/mdd.md): separate class, object, and state views of a domain.
- [Distributed Systems and Parallel Programming coursework](https://github.com/mglezz/SistemasDistribuidos-ProgramacionParalelo): reference for straightforward C# classes, constructors, control flow, and socket examples.

These repositories provide reference material. Homeryn has its own requirements and retains the asynchronous communication needed for concurrent clients.
