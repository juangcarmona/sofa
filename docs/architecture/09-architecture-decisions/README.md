---
title: Architecture decisions
arc42-section: "09"
description: Important architecture decisions and links to their records.
---

# 9. Architecture decisions

This directory is the canonical ADR collection for SoFa. It owns the decision records and their index; no parallel `docs/adr/` directory is used. The section remains part of the ArcShape twelve-section architecture.

| Decision | Status | Date | Record |
| --- | --- | --- | --- |
| Lighthouse owns delivery workflow and SoFa owns the Bastion execution runtime | Accepted | 2026-10-24 | [ADR-0001](0001-lighthouse-delivery-workflow-sofa-bastion-runtime.md) |
| One Bastion supports bounded parallel execution through repository-scoped runner registrations | Accepted | 2026-10-24 | [ADR-0002](0002-bounded-parallel-execution-on-bastion.md) |
| Bastion OpenCode uses the operator's OpenAI Codex account without a GitHub API secret | Accepted | 2026-10-24 | [ADR-0003](0003-bastion-opencode-codex-authentication.md) |

The decision is accepted for the architecture-alignment phase. It does not authorize implementation by itself; implementation remains a separate lifecycle item with its own validation and human gates.
