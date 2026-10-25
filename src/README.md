# Source

`observation/github.mjs` reads workflow runs and attention labels from configured GitHub repositories. It records source identities, timestamps and collection limits; it does not dispatch, cancel or publish work.

`SoFa.Runtime`, `bridge/` and `execution/` retain the superseded pilot implementation for historical rollback. Runtime startup requires an explicit legacy opt-in and `SoFa.AppHost` no longer launches it. Repository workflows use an independent executor. New execution functionality does not belong in SoFa.

See the [architecture](../docs/architecture/README.md) for ownership.
