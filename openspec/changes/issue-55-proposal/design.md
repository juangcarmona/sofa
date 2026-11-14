# Design

<!-- pdac-scope: cited -->

## Context

See proposal.md for motivation. The .NET Workspace module owns authenticated source access, CSRF enforcement and protected SQLite ControlJournal storage (ADR-0008/0009). The owner has accepted the connected-user checks in #52; this implementation does not repeat them.
<!-- pdac:cite id="FR-IDEA-PROMOTION" digest="sha256:02726ca033f3c323ee9b2898ea36086cdebe41aa270c6d785ececc5c1912b9e3" -->

## Goals / Non-Goals

**Goals:** durable drafts, explicit review and confirmation, exact source identities, one-shot transmission and read-only reconciliation.

**Non-Goals:** agent execution, lifecycle policy, permission administration, merge, deployment, or repeating #52 acceptance.

## Decisions

- Reuse the protected ControlJournal database and protector with an additive draft table, not another journal or browser storage. Owner-scoped GUIDs and revision comparison prevent stale tabs from mutating newer drafts.
- Store review actor/repository identity with the exact content. Saving invalidates review. Verify the acting GitHub user and current intended-App grants before review and again before confirmation. Linking requires source read authority; creation requires issue-write authority separately. Bound retained drafts to 25 records without automatic deletion.
- Enumerate bounded repository issues including closed issues before creation. Show related candidates; exact normalized-title equivalence blocks creation and requires linking. Missing pages or malformed identities block creation, never imply no duplicates. The operator explicitly confirms no candidate is equivalent.
- Persist an irreversible uncertain state before the single source POST. Embed a stable draft correlation marker in the reviewed issue body. A timeout, malformed receipt or process interruption reconciles the exact reviewed body and creating actor against repository-bound source identity; no automatic retry or replacement send is allowed. A subsequently edited source body remains uncertain rather than being guessed. This favours avoiding duplicates over automatic recovery when source evidence is unavailable.
- Issue creation/linking is a GitHub backlog operation, not an invented Lighthouse command. The existing shared issue controls own all subsequent stage eligibility and execution. Manual drafting needs no capability invocation; no new agent action is offered.
<!-- pdac:cite id="BR-GITHUB-SOURCE" digest="sha256:fd8141424d5fead38ed55d54689511fc445467133706e875371cc3199af8ab1a" -->

## Risks / Trade-offs

- [Concurrent equivalent ideas] → fresh source check and explicit operator confirmation reduce semantic duplicates; GitHub provides no atomic semantic-uniqueness constraint.
- [Lost response or crash before sending] → retain uncertain intent and reconcile; never resend automatically, including after restart.
- [Large repository] → fail closed after the bounded scan, with an explicit coverage limitation.
- Browser checks cover the visual journey; the connected-user checks in #52 are already accepted.

## Migration Plan

Create the additive idea table on first use in the existing protected journal. Old issue-control rows and metadata remain unchanged. Rollback removes access to the feature, not its retained data or source issues. Use existing private journal/key configuration; missing storage blocks saving and creation.
