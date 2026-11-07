# Source

`observation/github.mjs` reads workflow runs and attention labels from configured GitHub repositories. It records source identities, timestamps and collection limits; it does not dispatch, cancel or publish work.

`SoFa.Runtime`, `bridge/` and `execution/` retain the superseded pilot implementation for historical rollback. Runtime startup requires an explicit legacy opt-in and `SoFa.AppHost` no longer launches it. Repository workflows use an independent executor. New execution functionality does not belong in SoFa.

`SoFa.Web/` and `SoFa.Modules.Workspace/` implement the authenticated workspace, owner/source sessions and source queries. `SoFa.AppHost/` composes that workspace plus the isolated development provider. `web/public/` retains the native browser presentation and Lighthouse UI tokens. `npm start` builds and launches the authenticated Release entrypoint using private configuration. `web/server.mjs` is a retained legacy fixture/tool, not the supported authenticated entrypoint.

See the [architecture](../docs/architecture/README.md) for ownership.
