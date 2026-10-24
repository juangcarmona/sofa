# Source

`SoFa.Runtime` implements supervised stage admission, PostgreSQL coordination, isolated execution and fenced GitHub publication. `SoFa.AppHost` composes the runtime and database for local operation. `bridge/` contains the repository runner adapter; `execution/` defines the bounded OpenCode image and policy.

Repository tooling remains under `tooling/`. See the [architecture](../docs/architecture/README.md) for ownership and the [pilot runbook](../docs/runbooks/supervised-pilot.md) for deployment and acceptance limits.
