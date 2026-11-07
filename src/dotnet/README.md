# Workspace tooling snapshot

The CQRS interfaces and LH1001/LH1002 analyzer source were copied from published Lighthouse CLI 0.12.53 (source `dd1694735391f54241604a1686ccd627de7b2036`). Their Apache-2.0 LICENSE and NOTICE are retained here. The compiler diagnostic ledgers use the matching published source rules; local table formatting preserves their diagnostic content.

These copies are adopter-owned. Project files retain explicit package versions and the existing repository layout instead of introducing central package management. The new workspace module enables the analyzer. The workspace, AppHost and related tests enable nullable/SDK analysis, warnings as errors and all-dependency NuGet audit. Retained runtime projects keep their existing policy during migration. Existing host/CI SDK selection and the separately pinned isolated verification image remain unchanged; this partial migration does not claim a new universal SDK pin.

`npm run verify:dotnet` adapts the installed profile to SoFa's existing solution and provisioned runtime-test lanes. It performs locked restore, Release build, format checks for the migrated projects, module activation checks, real compiler rejection for both LH1001 and LH1002, and solution tests. Container verification uses the existing host-provisioned PostgreSQL connection and excludes host Docker-engine tests; full host CI still runs those tests. Runtime formatting/policy migration remains outside this slice.

The implemented CQRS queries project account authority and observations. Actual provider verification runs separately on a host through `npm run verify:identity:host`; no Docker socket or Keycloak service is added to the credential-free agent capsule. Production identity and persistence remain separate release work.
