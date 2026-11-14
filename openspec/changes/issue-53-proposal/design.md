---
pdac-scope: cited
---

# Design

## Context

See proposal.md for motivation and authority. PR #100 is merged to `main` (included in `48c0b1b`); its configured-repository discovery, issue reader and connected-user transport are available for reuse. `GitHubObservations.ReadRepositoryAsync` checks permission before a 15-second scoped cache and checks connection generation before returning results. Its issue/action permissions are not a substitute for content-read authority.

The merged PR #100 interfaces are the existing implementation to reuse, not a separate proposal to rebuild. The bounded #53 query uses only configured repository discovery, connected-user identity and content-read authority. Issues #48 and #50 remain separate open work; this query does not claim their issue/action acceptance or perform their writes. ADR-0008 is accepted on `main`; preserve ADR-0006 observation independence, ADR-0007 identity separation and ADR-0009 .NET/Aspire ownership. No dependency on proposed ADR-0010 or a new ADR-0008 decision.

## Goals / Non-Goals

**Goals:** deterministic source projections, explicit interpretation support, comparable evidence and safe project-level drill-down with bounded reads.

**Non-Goals:** registry collection, runtime installation detection, new policy/exception schema, automatic maintenance, durable history, recursive monorepo discovery or implementing pending prerequisites.

## Decisions

### 1. Extend the existing workspace query boundary

Add an internal read query in the workspace module, exposed as `GET /api/repositories/{owner}/{repository}/configuration`. Input is a configured repository identity; the server resolves its configured branch once to an immutable commit, then reads every file at that commit through the existing connected-user transport. Use existing route availability conventions (400 invalid, 403 denied, 409 disconnected/changed, 503 unavailable). Partial supported collection returns 200 with explicit coverage/reasons. No caller-supplied URL, arbitrary path, executable schema reference or mutable ref is fetched. No mutating inspection method is added.

Response contract: repository, configuredBranch, observedRevision, dataScope, observedAt, availability, coverage, freshness, limits, sources[], facts[] and reasons[]. A source records path, revision, observedAt, availability and interpretation diagnostics. A fact records a stable ID derived from repository/revision/path/field pointer, component, factKind, rawValue, contract identity/version, interpretation status and safe immutable source URL. Comparison records reference both fact IDs, expected origin, comparable scope, outcome (match/different/unassessed), calculation and reason. Coverage and freshness are separate fields; conflict is a relation between facts, not destructive merging.

Alternatives: a new adapter/service duplicates authority and discovery; extending historical Node/gh collection would use the wrong identity. Reuse transport with a bounded content-read operation, not a second provider. If existing app grants cannot read contents, expose that unavailable capability rather than introduce credentials or broaden grants in this story.
<!-- pdac:cite id="FR-PROJECT-DISCOVERY" digest="sha256:fb93a1c32232be6a679aa7aa074e9b6c0efd871cd48592b6b8d56996c36a2382" -->
<!-- pdac:cite id="QR-EXECUTION-SECURITY" digest="sha256:314e97415b3211a4f4e6187bb16495661d31129062e6bda38ceb8a2a4d4e9790" -->

### 2. Support facts without inventing shared semantics

Initial support is deliberately narrow. Only Lighthouse CLI/SDLC/UI, ProductShape and ArcShape package identities are projected from npm sections. Source contracts are kept distinct:

| Source / version | Projection | Supported interpretation / comparison |
| --- | --- | --- |
| `lighthouse.json`, schemaVersion 1, baseline 0.1.0 | baseline, profiles, integration pins, SDLC/UI pins | Validate against released CLI 0.12.65 manifest schema supplied by owner. Compare only corresponding component pins or equal baseline identities. |
| `.lighthouse/upgrade.json`, schemaVersion 1 | CLI package declaration, baseline, capability/projection versions and source adoption verdicts | Validate against released CLI 0.12.65 upgrade schema. Adoption verdict remains source-owned; it does not establish runtime installation or exception validity. |
| `.lighthouse/sdlc.json`, observed schemaVersion 1 | raw package, routes and gates declarations | No complete released control-plane schema supplied: interpretation unsupported; no validated policy or cross-contract comparison. |
| `.lighthouse/commands.json`, observed schemaVersion 1 / contractVersion 1 | raw package and command-contract declarations | Alias schema not supplied: interpretation unsupported. Preserve contractVersion separately from package version. |
| Root `package.json`, npm dependency/devDependency/optionalDependency/peerDependency maps | package declaration strings | Exact `x.y.z` declarations compare to same package and kind; ranges, tags and other forms remain source facts with ordering unassessed. No dependency execution. |
| Root `package-lock.json`, lockfileVersion 3 | exact matching package resolutions under packages entries | Resolution facts only; no proof of installed/runtime state. Preserve multiple package locations and versions. Other lock versions/ecosystems unsupported. |
| At most two managed `.github/workflows/*.yml` paths explicitly named in a validated manifest | source path/revision and inert source evidence | Workflow syntax/version semantics unsupported in this increment; no YAML execution or inferred harness/CLI version. Other paths are not followed. |
| Owner-supplied SDLC 0.22.4 `contract.json` / declared schema | package provenance and raw contract version 0.5.0 | Incompatible under the supplied closed schema; do not activate it as a validated contract. |

Unknown schema versions, invalid JSON, conflicting duplicate keys, nullable pins and unknown combinations carry diagnostics; raw facts from invalid sources never masquerade as validated facts. Released definitions are immutable versioned test/consumer resources with the owner's integrity/source receipts, not locally rewritten policy. Schema evaluation is offline with allowlisted bundled references; never dereference a repository `$schema` URL. Source objects outside this table remain unsupported, not silently coerced.

The supplied SDLC contract has `assets.browserE2eSchema`, `capabilityCatalog`, `capabilityCatalogSchema`, `commandAliases` and `commandAliasesSchema`, which are absent from the supplied `assets.properties` under `additionalProperties: false`. This static incompatibility is sufficient to choose unsupported consumption; implementation will assert it with a pinned fixture. Do not patch upstream schemas or claim a runtime validation was run here. Missing control-plane and alias schemas do not block honest raw-source inspection; adding validated semantics later requires released evidence.

Alternative: parse everything as one version or relax schemas to accept the current files. Rejected because it creates false adoption and comparison claims. Unsupported interpretation preserves useful evidence at lower semantic coverage.
<!-- pdac:cite id="FR-CONFIGURATION-DRIFT" digest="sha256:abb59ea4de5de304e41806960f9d7cc593cd9086082a2c04ce7fe1b4dc67eff7" -->
<!-- pdac:cite id="BR-EXPLAINABLE-SIGNALS" digest="sha256:8b54c80507f22f6883aaad91a53f479da5892bcc8def4f0e836076d1cf2063d5" -->

### 3. Explicit comparison selection, not implicit policy

The operator selects an observed supported fact as an expected baseline for the same component, fact kind and contract identity. The browser computes deterministic comparison over returned facts and displays both sources and the algorithm. It can select an accessible configured repository's fact in the same live source scope; selecting another repository triggers only an explicit bounded inspection, never fleet crawling. There is no default latest baseline or persisted policy. Comparison selection is local view state and is discarded when scope changes.

Compare exact numeric `x.y.z` package pins within the same source contract using integer tuple ordering (not floating point); this reports lower/equal/higher declaration only, not compatibility. Manifest/upgrade pins can be 40-character source identities; these support identity equality only, not version ordering. Cross-file comparisons require matching component and supported semantic kind; differing contract identities remain unassessed. Lock resolutions are compared only with lock resolutions; declaration-versus-resolution is labelled a cross-kind difference, not a supported baseline verdict. Workflow/alias/capability version strings have no inferred ordering or compatibility.

Latest available, installed and required-by-policy slots display unknown with reasons in this increment: no registry, runtime or shared policy reader is added. An operator-selected expected declaration is labelled selected baseline, never required policy. Exceptions are inert declarations opened through the existing source/evidence reader or explicitly linked source text; no exception file format, automatic extraction policy or validity validator is introduced. Show the declaration alongside the difference and validity unsupported/unassessed. Root npm not-applicable is established only by a complete supported package declaration with no relevant entries; it says nothing about nested projects or other ecosystems.

Alternatives: manual universal version input loses provenance; a registry integration expands scope without supplying required-policy meaning; an exception validator invents policy. All are excluded.
<!-- pdac:cite id="FR-CONFIGURATION-DRIFT" digest="sha256:abb59ea4de5de304e41806960f9d7cc593cd9086082a2c04ce7fe1b4dc67eff7" -->
<!-- pdac:cite id="BR-EXPLAINABLE-SIGNALS" digest="sha256:8b54c80507f22f6883aaad91a53f479da5892bcc8def4f0e836076d1cf2063d5" -->

### 4. Bounded on-demand collection and ephemeral retention

Per repository inspection: one immutable branch-resolution read and at most eight content reads (six fixed files plus two validated managed workflow paths), at most two concurrent content reads, 256 KiB decoded content per file, 1 MiB decoded aggregate and 2 MiB projected response. Use a 30-second collection deadline, cancellation, no pagination or automatic retry. Resolve/read source failures explicitly; count read attempts even when a source is missing. Reject symlink/submodule/path traversal targets rather than following them. Oversized/remaining sources are incomplete with reasons; do not silently truncate values into valid facts.

Coalesce identical reads for 15 seconds within the existing live session/connection generation/repository/revision scope, after fresh authority checks. Keep at most two repository projections and 4 MiB in browser memory for inspection/comparison, no localStorage, durable table or history. Only the open project is inspected on demand; no background fleet collection. A refresh within the coalescing window can return the same observation time, visibly unchanged. After 15 seconds label retained evidence stale/as-of; refresh is explicit, not a claim that repository configuration expires. These are engineering budgets, not product health thresholds.

A transient content-read failure can retain prior evidence as stale only after authority is still verified. Failed authority verification, 401/403, connection generation change, logout or repository removal clears affected projections, baselines and outstanding browser epochs. The response must match requested repository and current dataScope before rendering. Never retain data from a former account under a stale banner.

Alternative: polling all repositories or durable snapshots adds load/history outside this story. On-demand reads reduce that load but leave older facts visibly stale until refreshed.
<!-- pdac:cite id="BR-EXPLAINABLE-SIGNALS" digest="sha256:8b54c80507f22f6883aaad91a53f479da5892bcc8def4f0e836076d1cf2063d5" -->
<!-- pdac:cite id="QR-EXECUTION-SECURITY" digest="sha256:314e97415b3211a4f4e6187bb16495661d31129062e6bda38ceb8a2a4d4e9790" -->

### 5. Source-first navigation and accessible presentation

Use the existing project detail extension point, not a separate fleet dashboard. Display fact kind, declared/raw interpretation, coverage, freshness and comparison reason in text, not color alone. Keep source identity stable across refresh; preserve focused evidence or move to a persistent project control when it disappears. Support light/dark and 390px layouts with wrapping paths and revisions.

Construct exact evidence links from validated configured repository identity, immutable commit and allowlisted path. Reuse safe HTTPS GitHub repository-bound link validation; source text is inert. Maintenance work is shown only when an existing source explicitly links it, using accepted issue/evidence readers or safe links. Do not search the whole issue backlog or infer no work from missing linkage. Cross-repository links outside current authorized scope are not opened by the internal reader.

Alternative: upgrade buttons or automatic maintenance issue creation change authority and are excluded.
<!-- pdac:cite id="FR-CONFIGURATION-DRIFT" digest="sha256:abb59ea4de5de304e41806960f9d7cc593cd9086082a2c04ce7fe1b4dc67eff7" -->
<!-- pdac:cite id="QR-MOBILE-OPERATIONS" digest="sha256:2485b5877f0872af3dbdaa0a7ac00ac7651b28173adf6e3cf8d9ed643029bb35" -->

## Risks / Trade-offs

- [Prerequisites are assembled in an unmerged candidate] → Maintainer must integrate PR #100 into `main`, obtain the existing #52/#48 acceptance and refresh exact interfaces before implementation; reuse the component work rather than replacing it here.
- [Limited contract support] → Keep raw evidence available, unsupported status explicit and comparison disabled where semantics are missing. Shared contract expansion is separate work.
- [Repository size exceeds budgets] → Report bounded partial coverage. Do not raise limits silently or recursively crawl monorepos.
- [Access loss races with a read] → Permission-before-reuse, generation/epoch checks and clearing tests cover server and browser paths.
- [Selected baseline is mistaken for policy] → Label its origin on every verdict; policy-required remains unknown.
- [Browser screenshots mistaken for device acceptance] → Record viewport evidence only; physical-device acceptance is separate owner work.

## Migration Plan

No persisted data or repository configuration changes. After prerequisite acceptance and explicit implementation release, land projection and tests, then the read-only route, then project presentation and browser checks. Update support documentation with each slice. Deploy through the existing application process only after normal acceptance; this proposal authorizes no deployment. Roll back the additive route/presentation together; source delivery and configuration remain untouched. Clear ephemeral projections on application restart or scope change.

## Validation and Visual Evidence Plan

Use provenance-bearing fixtures from current checkout files and owner-packet artifacts with exact revision/path receipts. Synthetic version/state variants must be labelled synthetic, derived from a named original and never presented as real fleet observations. Cover matching, exception-unassessed, lower declaration, stale, conflict, no applicable npm package, inaccessible source and unsupported contract, plus unknown/ranges, incompatible baselines, immutable reads, limits and races.

Projection tests exercise released schemas without remote schema fetches; integration tests use the existing injected clock/transport to prove content grants, scope isolation, stale handling and absence of mutation requests. Browser tests exercise text/state rendering, safe links, keyboard focus and comparisons at desktop/390px in both themes. Retain screenshots, accessibility results and request traces with test names, fixture source, viewport and candidate revision. Capture only controlled fixtures and safe post-auth pages, not credentials.

Later commands: `npm test`, `npm run test:browser`, `npm run verify:dotnet`, `npm run lint:docs`, `npm run spec:validate`, `npm run product:citations`, then required `npm run verify` and CI on the exact candidate. Docker-backed and real-provider checks require their existing supported host lanes, not Docker-in-Docker. No implementation tests or visual evidence are claimed by this proposal.

## Definition of Done

All unchecked tasks complete or explicitly dropped without silent acceptance loss; evidence is linked for each proposal verification group in the [proposal matrix](proposal.md#proposal-verification-matrix); support limits documented; citations current; required exact-head checks pass; independent source-bound review and lifecycle DoD dimensions reviewed by the developer. These proposal-local groups are not original issue acceptance criteria. Owner acceptance, protected decisions and deployment remain distinct.
