---
title: Glossary
arc42-section: "12"
description: Domain and architecture terminology used in this documentation.
---

# 12. Glossary

| Term | Definition |
| ---- | ---------- |
| Bastion | The one persistent machine hosting the SoFa process, self-hosted runner, model gateway, local model services, and execution resources. |
| Checkpoint | A durable, ordered recovery boundary for one execution run. |
| Delivery evidence | Observed GitHub commit, PR, checks, artifacts, workflow, integration, or deployment references; it is not human acceptance. |
| Fencing token | A monotonic lease authority value that prevents stale workers from publishing state. |
| Gate | A Lighthouse lifecycle result such as `triaged`, `ready`, `planned`, or `done`; it is not a stage. |
| GitHub Actions job | The workflow job that carries the Lighthouse stage execution onto a runner capability. |
| Lighthouse | The repository-owned lifecycle system defining routes, stages, gates, skills, workflows, stage results, and delivery evidence. |
| Model gateway | The Bastion/provider boundary that routes model calls and applies masking, metering, security, and health controls. |
| Operational state | SoFa's durable coordination and recovery records; it is not a Lighthouse stage result or product decision. |
| OpenCode | The persistent agentic execution engine supervised by SoFa on Bastion; initially authenticated through the execution account's OpenAI Codex/ChatGPT connection. |
| Recovery result | The bounded record of whether a run resumed, stopped, was rejected, or requires human action. |
| Runner host | The persistent Bastion machine on which one or more repository-scoped GitHub runner registrations are installed. |
| Runner registration | A GitHub Actions self-hosted runner installation registered for exactly one repository and identified by its own directory, service, and token. |
| Route | A Lighthouse composition policy selecting stages and prerequisites, for example `supervised` or `plan-only`. |
| SoFa runner capability | The GitHub Actions capability labels supplied by the repository-scoped runner registrations and the future local handoff adapter deployed as part of SoFa on Bastion. |
| Stage | One of Lighthouse's current `triage`, `refine`, `propose`, `implement`, `integrate`, or `review` operations. |
| Stage result | Lighthouse's validated contract describing one stage outcome, evidence, missing evidence, unavailable checks, review state, and next action. |
| Workspace / sandbox | The isolated per-run checkout and temporary execution environment. |
