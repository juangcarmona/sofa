# Spec Delta

## Purpose

Let the authorized owner explicitly request repository refinement and follow source evidence while preserving repository-owned execution, uncertainty and human decision gates.

## ADDED Requirements

### Requirement: Confirm a current valid selection
The system SHALL allow only configured repositories and open issues, excluding pull requests. It SHALL show repository, issue and stage `refine` before explicit confirmation, revalidate the selection and entrypoint before submission, and reject invalid, changed or unsupported selections without dispatch.

#### Scenario: Valid reviewed request
- **WHEN** an authorized owner confirms an unchanged open issue in a configured repository with a supported enabled entrypoint
- **THEN** the system submits only that issue and `stage=refine` through the repository's existing Lighthouse entrypoint.

#### Scenario: Selection changes after review
- **WHEN** the issue closes, becomes invalid, the reviewed issue version changes, repository configuration changes or the entrypoint cannot be verified before confirmation
- **THEN** the system performs no dispatch and requires refreshed evidence and a new review.

#### Scenario: Pull request or unsupported repository
- **WHEN** a selected record is a pull request or its repository lacks supported refinement automation
- **THEN** submission is unavailable with a specific explanation and a source link.

### Requirement: Retain uncertain and duplicate submissions
The system SHALL identify a request by bound user, repository, issue and stage, retain its submission receipt, and prevent repeated clicks or retries from silently dispatching duplicates. Ambiguous responses SHALL remain uncertain and SHALL NOT trigger automatic mutation retries. A deliberate new attempt SHALL disclose the possible existing request.

#### Scenario: Concurrent clicks and replay
- **WHEN** confirmation is clicked repeatedly, replayed with the same intent or submitted concurrently for an unresolved request key
- **THEN** the system returns the existing receipt and makes at most one outbound dispatch attempt.

#### Scenario: Lost response or process restart
- **WHEN** a dispatch times out or the control process stops after recording an attempt but before recording a definitive response
- **THEN** the recovered receipt reports an uncertain submission, links GitHub for inspection and does not automatically dispatch again.

#### Scenario: Explicit potentially duplicate attempt
- **WHEN** the owner reviews the unresolved receipt and explicitly confirms a new attempt despite uncertainty
- **THEN** both attempts remain visible as potentially duplicate requests and repository concurrency remains authoritative.

### Requirement: Separate submission from source evidence
The system SHALL distinguish submission acknowledgement, run discovery, stage activity and source-published result. It SHALL show evidence timestamps, bounds and issue/run links. Candidate matches SHALL NOT be asserted as exact request correlation without source proof; delays, stale evidence and ambiguity SHALL remain explicit.

#### Scenario: Acknowledged dispatch without a run
- **WHEN** GitHub acknowledges dispatch but no matching run is yet observable
- **THEN** the view shows submitted and discovery pending, not running or published, with an issue link and refresh path.

#### Scenario: Candidate runs without a correlation identifier
- **WHEN** one or more runs match repository, workflow, stage, issue, actor and time but no source request identifier proves the association
- **THEN** the view labels them candidates, links their source and does not claim exact dispatch-to-run correlation, even for a single candidate.

#### Scenario: Missing, stale or rejected evidence
- **WHEN** collection fails, exceeds its bounds, returns stale data or shows router rejection
- **THEN** the view reports the applicable limitation or rejection, preserves dated prior evidence where available and offers refresh and source links without claiming success.

### Requirement: Publish only evidenced stage results
The system SHALL display a published refinement result only from a source-published stage record tied to the selected issue, stage and source run. Workflow completion alone SHALL NOT establish publication. It SHALL keep result-to-run evidence separate from uncertain submission-to-run attribution.

#### Scenario: Successful workflow without a published result
- **WHEN** a candidate or source-linked run completes successfully but no valid source-published refinement result is available
- **THEN** the UI shows workflow completion and result unavailable or publication pending, not a passed refinement.

#### Scenario: Source result identifies its run
- **WHEN** a source-published refinement record identifies the selected issue, stage and workflow run
- **THEN** the UI links that result and run and displays its actual gate outcome, while retaining any uncertainty about which submission caused it.

### Requirement: Provide accessible asynchronous interaction
Selection, confirmation, errors and progress SHALL be keyboard accessible and usable on a phone. Status SHALL use text rather than color alone. Submission SHALL return without waiting for stage completion, and unavailable evidence SHALL provide a retry and source-navigation path.

#### Scenario: Keyboard and narrow-screen request
- **WHEN** the owner uses only a keyboard or a phone-width viewport
- **THEN** all selection, confirmation and recovery controls remain reachable, labelled and readable with visible focus and explicit status text.

#### Scenario: Long-running stage
- **WHEN** admitted repository work continues beyond the submission interaction
- **THEN** the receipt and source links remain available without keeping the submission request open or claiming completion.

### Requirement: Leave execution and human gates in the repository
The system SHALL submit only the explicitly confirmed refine action and SHALL NOT introduce an execution queue, scheduler, runtime, policy engine, automatic continuation or alternative approval. Stopping SoFa after GitHub admission SHALL NOT stop work. Product-semantic changes SHALL require a human-approved Product Change.

#### Scenario: Control service is stopped after admission
- **WHEN** GitHub admits refinement and SoFa is closed or stopped
- **THEN** repository execution and publication can continue independently and later observations recover source evidence.

#### Scenario: Refinement completes
- **WHEN** a published refinement passes its Ready gate
- **THEN** SoFa displays that result without submitting propose, implement or any later stage or granting a new approval.
