# Runtime architecture navigation

The canonical target design is the [ArcShape architecture](../architecture/README.md). The current source remains a scaffold; no SoFa runtime is implemented.

| Concern | Owning section |
| --- | --- |
| Lighthouse, GitHub, engine and SoFa ownership | [Context](../architecture/03-context-and-scope/README.md) |
| .NET, Aspire, PostgreSQL and engine strategy | [Strategy](../architecture/04-solution-strategy/README.md) |
| Modules and execution interface | [Building blocks](../architecture/05-building-block-view/README.md) |
| Admission, execution, publication and recovery | [Runtime](../architecture/06-runtime-view/README.md) |
| Repository runners, identities and capacity | [Deployment](../architecture/07-deployment-view/README.md) |
| Persistence, authority and evidence | [Cross-cutting concepts](../architecture/08-crosscutting-concepts/README.md) |
| Trade-offs and acceptance status | [ADRs](../architecture/09-architecture-decisions/README.md) |

The [software factory roadmap](software-factory.md) owns sequencing. The [pilot](first-pilot.md) owns acceptance. This page contains no separate runtime design or database schema.
