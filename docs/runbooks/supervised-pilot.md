# Supervised pilot operations

This runbook covers the first triage/refine adapter from issue 16. It requires Linux, Docker, Node 24, .NET 10 and an exact released Lighthouse SDLC host contract. The maintained contract is documented in the installed package's `host-contract.md`.

## Prepare the host

Use a private supervisor directory outside repository workspaces. Install the pinned SDLC package there with a lockfile. Keep the policy and GitHub publisher credential readable only by the supervisor account. Configure `Runtime__Socket`, `Runtime__ClientUid`, `Runtime__PolicyFile`, `Runtime__ContractRoot`, `Runtime__WorkRoot`, `Runtime__GithubTokenFile`, `Runtime__EngineImage`, `Runtime__EngineAuthVolume` and `Runtime__Model`; set `ConnectionStrings__sofa` to the operational PostgreSQL database. `SoFa.AppHost` supplies the database connection when running through Aspire.

The trusted policy declares the repository, owner, enabled stages, exact package version, trusted workflow commit and maximum stage duration. The request validator rejects mismatches. Updating the workflow commit or package requires an explicit operator policy update; do not derive trusted policy from a job or model response.

Build the isolated image with `bash scripts/build-engine.sh /absolute/path/to/opencode`. The script requires OpenCode 2.1.0. Provision a dedicated Docker authentication volume owned by UID 20003 and import the authorized OpenAI connection over a pipe. Never put credentials in a Docker image, repository, issue, workflow artifact or command argument. Check provider metadata and perform a bounded model request from the exact engine context before enabling admission.

The supervisor account can operate Docker and holds the GitHub mutation credential. The runner and engine cannot access either. Run the repository runner under a distinct UID, bind only the socket directory and provisioned bridge into it, and grant socket group access. Configure `ClientUid` to that kernel UID. Register it only to the selected repository with the `lighthouse-host` capability. Keep existing runner services and checkouts untouched.

## Start and verify

Run `dotnet run --project src/SoFa.AppHost` for local composition with the runtime configuration above, or run the published `SoFa.Runtime.dll` under a persistent supervisor with the same PostgreSQL connection. The HTTP transport binds only the Unix socket. `/health` returns ready after leadership and recovery complete. A second supervisor cannot take the PostgreSQL leadership lock. Remove a stale socket only after confirming its owning service has stopped.

Install the compatible Lighthouse package in the consumer, project `lighthouse-host.yml` from its host-contract command, and commit it. Register the resulting main commit in host policy before dispatch. The bridge does not execute repository scripts. Owner issue events and `/lh:triage` comments request triage; an exact `/lh:refine <runId>` owner comment resumes a published passing triage result. The source, policy and predecessor digest must still match. These commands do not approve integration.

The bridge writes a receipt to a unique `RUNNER_TEMP` file. Check the live GitHub Actions job, supervisor row, engine termination and published comment together. A `waiting-human` operational state can contain a blocked Lighthouse report: inspect its `result.status` and evidence before deciding the next stage.

## Failure and retention

Use `lighthouse-host-bridge status <runId>` and `cancel <runId>` through the authorized socket identity. Admission limits active capacity to one and attempts to three per issue/stage/source. Cancellation racing with claim is serialized; publication already in progress must be reconciled. A source change or workflow cancellation invalidates publication authority.

After restart the supervisor stops old containers before fencing interrupted runs. For an uncertain publication it searches for the exact marker, content and publisher identity. No confirmed receipt means capacity stays blocked for operator reconciliation. Inspect the remote issue and stored intent before any repair; never reset a row merely to make the queue move.

Source snapshots and output workspaces are deleted after termination. The supervisor deletes the run session after stopping its engine, including recovery after interruption. Cleanup failure keeps the operational slot occupied and requires intervention. PostgreSQL retains bounded requests, results, publication intents and receipts. Database backup, operational record expiry and disaster recovery remain deployment work beyond this first slice.

## Verification evidence

`npm run verify` includes PostgreSQL contention/fencing tests, kernel peer-identity tests, publisher authority/receipt checks, process cancellation and bridge artifact cleanup. `npm run test:runtime` provisions an isolated PostgreSQL test container unless `SOFA_TEST_DATABASE` is supplied.

The OpenAI provider smoke request on 2026-10-24 returned `SOFA_PROVIDER_OK` from the dedicated engine image and authentication volume. This proves provider access. Consumer deployment, a live stage receipt, refinement approval and the eventual product PR need their own recorded evidence; local fixtures do not prove those outcomes.
