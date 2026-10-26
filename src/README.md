# Source

`observation/github.mjs` reads workflow runs and attention labels from configured GitHub repositories. It records source identities, timestamps and collection limits; it does not dispatch, cancel or publish work.

`SoFa.Runtime`, `bridge/` and `execution/` retain the superseded pilot implementation for historical rollback. Runtime startup requires an explicit legacy opt-in and `SoFa.AppHost` no longer launches it. Repository workflows use an independent executor. New execution functionality does not belong in SoFa.

`web/` serves the native HTML/CSS/JS observation interface and its fixed-repository, read-only HTTP endpoint. Lighthouse UI supplies semantic theme tokens. Start it with `npm start`; no build pipeline or application framework is required.

See the [architecture](../docs/architecture/README.md) for ownership.
