---
pdac-scope: cited
---

# Tasks

The owner authorized the bounded, read-only configuration inspection now implemented in draft PR #197. Merge, release, deployment, and acceptance of separate parent work remain distinct. Checkboxes below record evidence actually present in this candidate; the remaining tasks identify what still needs review. VG-1–VG-8 refer only to the proposal verification groups mapped in [proposal.md](proposal.md#proposal-verification-matrix); they are not numbered acceptance criteria from the original issue.

## 1. Confirm delivery prerequisites before coding

- [x] 1.1 Verify PR #100 is merged to `main` and its discovery/source interfaces are present. #48 and #50 remain separate open work; this endpoint uses only configured-repository, connected-user content reads and does not claim their issue/action acceptance.
- [x] 1.2 Record the owner's authorization for this bounded read-only implementation in the PR and issue conversation; merge, release and deployment are still pending.
- [x] 1.3 Recheck the existing repository access, connected-user identity and bounded source-read contracts. The new route is additive and does not duplicate discovery or issue readers.

## 2. Project source facts and supported interpretations

- [x] 2.1 Add pinned CLI 0.12.65 manifest/upgrade schemas and owner-supplied SDLC 0.22.4 compatibility fixtures with integrity/source receipts; verify offline schema tests reject the SDLC forbidden asset keys without relaxing the schema (VG-7). The released SDLC contract/schema are pinned at `evidence/schemas/sdlc-0.22.4/`; `ReleasedSdlcContractFixtureIsUnsupportedByItsClosedDeclaredSchema` asserts the declared extra asset keys against the unchanged closed schema.
- [x] 2.2 Implement the bounded source support table and provenance; focused projector tests cover manifest, upgrade, npm declaration/lock dimensions, duplicate keys and unknown schemas (VG-1, VG-7).
- [x] 2.3 Preserve separate facts and unsupported states; tests cover denied/missing content and bounded applicability without inferring runtime installation or non-adoption (VG-4).
- [x] 2.4 Document supported paths, versions, unsupported semantics and fixture lineage in [repository-configuration.md](../../../docs/repository-configuration.md) (VG-1, VG-4, VG-7).
<!-- pdac:cite id="FR-CONFIGURATION-DRIFT" digest="sha256:abb59ea4de5de304e41806960f9d7cc593cd9086082a2c04ce7fe1b4dc67eff7" -->
<!-- pdac:cite id="FR-PROJECT-DISCOVERY" digest="sha256:fb93a1c32232be6a679aa7aa074e9b6c0efd871cd48592b6b8d56996c36a2382" -->

## 3. Deliver the authorized bounded read query

- [x] 3.1 Add the configured-repository GET query using the connected-user reader and immutable commit; focused tests verify the single branch resolution, content grant, route bounds and absence of issue/dispatch writes (VG-1, VG-6).
- [x] 3.2 Enforce the content-read, file, aggregate and response limits; boundary tests verify partial/unknown results without truncation (VG-4).
- [x] 3.3 Scope the short cache to session, account, repository and authority, check permissions before reuse, and clear results on authority changes; tests cover stale reads, permission loss and late responses (VG-4, VG-8).
- [x] 3.4 Document route, authority, read limits, refresh and content-grant constraints in [repository-configuration.md](../../../docs/repository-configuration.md) (VG-1, VG-6).
<!-- pdac:cite id="QR-EXECUTION-SECURITY" digest="sha256:314e97415b3211a4f4e6187bb16495661d31129062e6bda38ceb8a2a4d4e9790" -->
<!-- pdac:cite id="BR-EXPLAINABLE-SIGNALS" digest="sha256:8b54c80507f22f6883aaad91a53f479da5892bcc8def4f0e836076d1cf2063d5" -->

## 4. Explain comparisons and navigation in project detail

- [x] 4.1 Add explicit second-repository selection and only compare facts with compatible scope and contract identity; preserve both observed origins (VG-2).
- [x] 4.2 Keep declared and lock-resolved facts separate; latest, installed and required-policy values remain unknown (VG-3).
- [x] 4.3 Show exception declarations as unassessed, with source provenance; no validity contract is invented (VG-5).
- [x] 4.4 Add immutable source links and exact, same/configured-repository issue links; unsafe strings remain inert and no write is sent (VG-6).
- [x] 4.5 Bound browser memory to two repository projections and 4 MiB, clear on scope/account changes and avoid persistent storage (VG-4, VG-8).
- [x] 4.6 Document comparison limitations, exception status and refresh semantics in [repository-configuration.md](../../../docs/repository-configuration.md) (VG-2, VG-3, VG-5, VG-6).
<!-- pdac:cite id="FR-CONFIGURATION-DRIFT" digest="sha256:abb59ea4de5de304e41806960f9d7cc593cd9086082a2c04ce7fe1b4dc67eff7" -->
<!-- pdac:cite id="BR-EXPLAINABLE-SIGNALS" digest="sha256:8b54c80507f22f6883aaad91a53f479da5892bcc8def4f0e836076d1cf2063d5" -->
<!-- pdac:cite id="QR-EXECUTION-SECURITY" digest="sha256:314e97415b3211a4f4e6187bb16495661d31129062e6bda38ceb8a2a4d4e9790" -->

## 5. Prove accessible inspection as it lands

- [ ] 5.1 Add keyboard-operable controls, visible focus, textual state explanations, stable refresh focus and wrapping source details; verify browser tests at desktop and 390px in light/dark themes, including removed focused sources and unrelated control focus (VG-8).
- [ ] 5.2 Capture browser evidence for matching, exception-unassessed, outdated, stale, conflicting, no applicable package, inaccessible and unsupported fixtures; verify screenshots, accessibility results and read-only request traces identify fixture provenance, viewport and candidate head (VG-8).
- [ ] 5.3 Update browser-test and user documentation with evidence locations and the physical-device limitation; verify review can reproduce the eight-fixture journey without a live credential or misleading real-fleet demonstration (VG-8).
<!-- pdac:cite id="QR-MOBILE-OPERATIONS" digest="sha256:2485b5877f0872af3dbdaa0a7ac00ac7651b28173adf6e3cf8d9ed643029bb35" -->
<!-- pdac:cite id="BR-EXPLAINABLE-SIGNALS" digest="sha256:8b54c80507f22f6883aaad91a53f479da5892bcc8def4f0e836076d1cf2063d5" -->

## 6. Verify the integrated candidate

- [ ] 6.1 Refresh citations and check the proposal verification-group-to-scenario/task matrix against the accepted product baseline; verify `npm run spec:validate` and `npm run product:citations` pass with no scope drift.
- [ ] 6.2 Run `npm run verify` and the configured required CI on the exact implementation head using the supported host lanes; verify linked results include projection, .NET, browser, security and documentation checks without claiming unperformed real-provider or physical-device checks.
- [ ] 6.3 Obtain independent source-bound review and developer review of every lifecycle DoD dimension; verify the PR links VG-1–VG-8 evidence, known limitations and explicit protected decisions before requesting integration.
<!-- pdac:cite id="FR-CONFIGURATION-DRIFT" digest="sha256:abb59ea4de5de304e41806960f9d7cc593cd9086082a2c04ce7fe1b4dc67eff7" -->

## Workflow follow-up

- Owner acceptance, required forge approvals, merge and deployment remain separate from this proposal and its implementation checks.
- Archive through the existing OpenSpec lifecycle only after review requirements are satisfied; verify the archived capability and citations.
