## Purpose

Retain ideas outside the repository backlog until the operator explicitly creates or links a GitHub issue, preserving source identity and uncertain outcomes.
<!-- pdac-scope: cited -->

## ADDED Requirements

### Requirement: Retained idea draft

The workspace SHALL retain an admitted operator's idea with intended outcome, scope and selected configured repository before GitHub promotion, and allow revisiting or abandoning it without source writes.
<!-- pdac:cite id="FR-IDEA-PROMOTION" digest="sha256:02726ca033f3c323ee9b2898ea36086cdebe41aa270c6d785ececc5c1912b9e3" -->

#### Scenario: Revisit or abandon

- **WHEN** the operator saves an idea and later refreshes or abandons it
- **THEN** its content remains retained, no issue is created and it does not enter normal backlog execution

### Requirement: Explicit source promotion

The workspace SHALL show related issues and require explicit link or reviewed creation. Creation MUST verify current user, issue-write permission and duplicates; linking MUST NOT create.
<!-- pdac:cite id="FR-IDEA-PROMOTION" digest="sha256:02726ca033f3c323ee9b2898ea36086cdebe41aa270c6d785ececc5c1912b9e3" -->
<!-- pdac:cite id="UC-TRIAGE-BACKLOG" digest="sha256:0eaed6ca26d2e957f0eea2fbe207d6b201a0bb25708970f63d4f50c9703db9ac" -->

#### Scenario: Confirm creation

- **WHEN** the operator reviews exact content and candidates and confirms creation with current permission
- **THEN** the workspace sends once and retains the confirmed repository and issue identity

#### Scenario: Link instead of creating

- **WHEN** the operator selects an existing issue in the configured repository
- **THEN** the verified issue is retained as the source without any source write

#### Scenario: Permission or duplicate evidence blocks creation

- **WHEN** write permission is denied, a candidate has an equivalent normalized title or issue coverage is incomplete
- **THEN** creation is blocked with a reason and the draft remains available

### Requirement: Stable identity and uncertain outcome

Source identity SHALL persist; uncertain creation MUST reconcile without resend.
<!-- pdac:cite id="FR-IDEA-PROMOTION" digest="sha256:02726ca033f3c323ee9b2898ea36086cdebe41aa270c6d785ececc5c1912b9e3" -->
<!-- pdac:cite id="FR-BACKLOG-PROJECTION" digest="sha256:b98f45997965ba4d187781ca782921eb8c98cb8a9877de2081d26d1e6e5a4d55" -->
<!-- pdac:cite id="BR-GITHUB-SOURCE" digest="sha256:fd8141424d5fead38ed55d54689511fc445467133706e875371cc3199af8ab1a" -->

#### Scenario: Lost response and restart

- **WHEN** GitHub accepts creation but its response is lost, including across application restart
- **THEN** reconciliation recovers the correlated source issue without resending and repeated confirmation cannot duplicate it

### Requirement: Exploration does not execute work

Exploration SHALL need no terminal or automatic agent. Assistance MUST be explicit and supported. Backlog flow MUST await a confirmed issue; promotion MUST NOT start execution.
<!-- pdac:cite id="FR-IDEA-PROMOTION" digest="sha256:02726ca033f3c323ee9b2898ea36086cdebe41aa270c6d785ececc5c1912b9e3" -->
<!-- pdac:cite id="BR-GITHUB-SOURCE" digest="sha256:fd8141424d5fead38ed55d54689511fc445467133706e875371cc3199af8ab1a" -->

#### Scenario: Manual exploration and promoted work

- **WHEN** an operator drafts, reviews or promotes an idea
- **THEN** no agent, workflow, lifecycle transition or terminal starts, and only a confirmed issue offers entry into existing backlog controls
