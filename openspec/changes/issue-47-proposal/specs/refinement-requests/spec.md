# Spec Delta

## Purpose

Let an authorized owner explicitly request repository refinement and recover its exact source admission, run and published outcome without making SoFa an execution or policy authority.

## ADDED Requirements

### Requirement: Consume versioned source eligibility
The system SHALL consume an accepted compatible Lighthouse eligibility contract rather than reproduce admission policy. It SHALL present permission or denial reasons, required evidence, input revision or digest, actor authority and active reservations. Missing or incompatible contracts SHALL disable submission with an explicit limitation. Preflight SHALL NOT reserve work.

#### Scenario: Contract unavailable or denied
- **WHEN** a repository has only the currently installed request-ID-free entrypoint, an incompatible contract, unavailable evidence or source-denied eligibility
- **THEN** controls explain the limitation, link GitHub and do not silently fall back to heuristic attribution or local admission.

#### Scenario: Preflight races another client
- **WHEN** source preflight permits refine but evidence or a reservation changes before admission
- **THEN** repository admission rechecks the same semantics and returns the actual rejection or new-review requirement without treating preflight as a grant.

### Requirement: Confirm a current valid selection
The system SHALL allow selection only of configured repositories and open non-pull-request issues. It SHALL show repository, issue and stage refine before explicit confirmation, bind review to current source evidence and revalidate at confirmation. Changed selections or evidence SHALL require a new review.

#### Scenario: Reviewed refine request
- **WHEN** the authorized owner confirms an unchanged eligible issue within the review lifetime
- **THEN** only the reviewed refine intent is sent through that repository's supported Lighthouse entrypoint using the actual user's grant.

#### Scenario: Closed changed or invalid issue
- **WHEN** an issue closes, is a pull request, changes after review, belongs to an unconfigured repository or loses supported automation
- **THEN** no dispatch occurs and the UI explains the rejection or need to refresh review.

### Requirement: Bind durable requests to source identity
Each intentional request SHALL have an immutable client identity bound to its actor, repository, issue, stage and reviewed payload through source admission, run/attempt and published result. Repeated identical requests SHALL resolve to the original source admission or explicit rejection. Changed payload under the same identity SHALL conflict.

#### Scenario: Duplicate submissions across clients
- **WHEN** repeated clicks, delayed events, restarts or another client resubmit the same request identity and payload
- **THEN** the shared source contract resolves the original admission or explicit rejection without a second independent admission, while repository reservations remain authoritative.

#### Scenario: Conflicting identity or reservation
- **WHEN** a request identity is reused with changed payload or another request conflicts with an active repository reservation
- **THEN** source enforcement returns an explicit conflict or reservation denial and SoFa does not mint a replacement identity automatically.

### Requirement: Use exact dispatch run receipts
The system SHALL use an exact GitHub dispatch receipt when available and validate its source repository/workflow association. It SHALL distinguish transport submission, run discovery, source admission, running stage and published result. A run receipt SHALL NOT imply stage admission or completion.

#### Scenario: Documented GitHub receipt
- **WHEN** user-authorized dispatch with the selected API version returns HTTP 200 with workflow_run_id, run_url and html_url
- **THEN** the receipt identifies that exact run, the UI links it and later source evidence determines admission, stage progress and publication.

#### Scenario: Run exists but admission is rejected
- **WHEN** the exact dispatch run exists but the router rejects or skips stage admission
- **THEN** the UI shows run discovery separately from the source rejection, not a running or successful refinement.

### Requirement: Reconcile uncertain submissions by source request
Lost, malformed or ambiguous dispatch responses SHALL remain uncertain until resolved by source request identity and payload binding. Restart SHALL preserve reconciliation context. The system SHALL NOT automatically redispatch or use actor/title/time proximity, a manually selected candidate or the latest run as attribution.

#### Scenario: Lost response and restart
- **WHEN** GitHub may have accepted a dispatch but its response is lost and SoFa restarts
- **THEN** the existing durable identity is used to obtain the exact admission/run/attempt or explicit source rejection without creating a new request or dispatching automatically.

#### Scenario: Reconciliation unavailable or ambiguous
- **WHEN** source reconciliation is unavailable, incomplete or contradictory, even if one nearby run matches the issue
- **THEN** the receipt stays uncertain with dated evidence and source links; any diagnostic candidates are explicitly not correlation.

### Requirement: Validate published result attribution
The system SHALL display a refinement result only from source publication bound to the request, issue, stage, run and attempt under the accepted shared contract. Workflow success alone SHALL NOT establish publication. Missing, stale or contradictory evidence SHALL remain explicit.

#### Scenario: Exact published outcome
- **WHEN** source records bind a validated published refinement outcome to the request and run/attempt
- **THEN** the UI shows its actual status and gate with source issue, run and publication links.

#### Scenario: Workflow succeeds without a valid result
- **WHEN** a workflow succeeds without publication or the publication identity mismatches
- **THEN** the UI shows workflow completion separately and reports result pending, unavailable or contradictory rather than passed.

### Requirement: Provide accessible asynchronous phone interaction
Selection, review, confirmation, progress and recovery SHALL be keyboard accessible and usable in a real phone HTTPS session over LAN/VPN. Status SHALL use text rather than color alone. Submission SHALL return without waiting for stage completion and provide source navigation and safe refresh.

#### Scenario: Real phone session
- **WHEN** the owner signs in on a phone over the accepted network route, submits refine and later refreshes after leaving the page
- **THEN** controls remain readable and operable and recover the exact request receipt and source outcome without dispatching again.

#### Scenario: Keyboard and delayed evidence
- **WHEN** a keyboard user encounters delayed or stale evidence
- **THEN** labelled controls, visible focus, status announcements, evidence timestamps and refresh/source links remain available without claiming completion.

### Requirement: Preserve repository execution and human gates
The system SHALL submit only explicit refine intent and SHALL NOT create an execution queue, runtime, scheduler, admission policy or alternate approval path. Closing SoFa SHALL NOT stop admitted work. Direct GitHub entry and the triage to refine to propose to implement to integrate to review lifecycle SHALL remain source-authorized.

#### Scenario: SoFa stops after admission
- **WHEN** GitHub admits refine and SoFa stops
- **THEN** execution and publication continue independently and source evidence can be recovered after restart.

#### Scenario: Refinement passes
- **WHEN** source publishes a passed Ready outcome
- **THEN** SoFa displays it without requesting propose, implementation, integration or any new approval.

### Requirement: Keep comment continuation outside refine submission
Refine submission and recovery SHALL NOT post comments, edit labels or implicitly continue needs-info. Any future comment/continuation client SHALL consume the shared source intent and receipt contract in a separately authorized change.

#### Scenario: Needs-info issue is inspected
- **WHEN** an owner views or refreshes a needs-info issue in this refine-only control
- **THEN** no comment or continuation is sent and no routing labels are changed.
