# Engineering documentation

I maintain this documentation in English and write from my perspective as the developer. I separate implemented behavior from design proposals and use plain language for the root README and future release notes.

| Document | Purpose |
| --- | --- |
| [Development guide](development.md) | I record build commands, Visual Studio integration, and my Git workflow. |
| [Architecture](architecture.md) | I define component responsibilities and dependency boundaries. |
| [Wire protocol](protocol.md) | I specify framing, messages, errors, and connection lifetime. |
| [Domain model](domain-model.md) | I model the intended household concepts before implementing them. |
| [Actors and use cases](use-cases.md) | I distinguish user goals from the current technical verification scenario. |
| [Development log](development-log.md) | I record problems, decisions, evidence, and remaining work. |
| [Verification](../tests/README.md) | I describe the executable integration checks and their limits. |

## Reference material

I use the progression from domain modeling to actors and use cases in [idsw1](https://github.com/mmasias/idsw1) as a reference for organizing my engineering work.

I also use the separation of class, object, and state models in the [hdC domain model](https://github.com/mmasias/hdC/blob/main/docs/UMLdocs/mdd.md) as a reference for keeping different views of a domain explicit. I develop Homeryn's model independently; those examples do not define its requirements.