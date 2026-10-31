---
id: CHG-AUTHENTICATED-OPERATIONS-001
type: product-change
title: Operate and understand the software factory from one workspace
status: applied
base-revision: 'ae846c7faba9b92fcf3ce2bf7e152a3b803050d2'
operations:
  add:
    - ACT-IDENTITY-PROVIDER
    - UC-ACCESS-FACTORY
    - UC-OPERATE-ISSUE
    - TERM-WORK-COMMAND
    - BR-EXPLICIT-WORK-COMMANDS
    - FR-ISSUE-CONTEXT
    - FR-ISSUE-CONVERSATION
    - QR-MOBILE-OPERATIONS
    - UC-EXPLORE-FACTORY
    - UC-RESOLVE-INTERVENTION
    - UC-INSPECT-PROJECT-SIGNAL
    - JRN-DAILY-FACTORY-OPERATIONS
    - BR-EXPLAINABLE-SIGNALS
    - FR-FACTORY-DISCOVERY
    - FR-INTERVENTION-INBOX
    - FR-PROJECT-WORKSPACE
    - FR-PROJECT-HEALTH
    - FR-CONFIGURATION-DRIFT
    - FR-IDEA-PROMOTION
  modify:
    - CON-TRUSTED-LAN
    - BR-LAN-PRIVILEGE-BOUNDARY
    - FR-AUTHORIZED-ENTRY
    - FR-WORK-UNIT-SHAPING
    - QR-EXECUTION-SECURITY
    - FR-RUNTIME-HEALTH
    - FR-PROJECT-DISCOVERY
    - UC-TRIAGE-BACKLOG
    - QR-EXECUTION-RECOVERY
  remove: []
---

<!-- Infrastructure host names are redacted in this historical record. -->

## Problem

The factory definition permits future authenticated controls but does not define an end-to-end operational issue journey. The existing control constraint defaults to loopback only, which prevents the owner from operating from a phone on LAN or VPN. A source comment may currently resume refinement, while an ordinary conversation contribution must not unexpectedly request execution. Authentication, command intent, execution evidence and human decisions need explicit product boundaries.

## Intended Product Outcome

The full product supports a daily factory journey: discover repositories and issues, find requests for intervention, operate an issue, inspect project work through lists and boards, explain project health and configuration deviations, and explore ideas that can become GitHub issues. The actor is the existing personal factory owner; requester and engineer capabilities stay distinct. Repository evidence, Lighthouse rules and independent execution remain authoritative.

Juan signs in through an external identity provider to read and operate the factory. From an issue, he understands its conversation, lifecycle evidence, active work and required intervention, then explicitly requests an eligible Refine or Propose action or publishes a comment. He follows the source acceptance, exact execution and outcome inside SoFa, including duplicate and uncertain submissions. The same journey works on desktop and phone over LAN or VPN.

GitHub remains the source of repository work and verifies the actual acting user. Lighthouse owns shared capability, lifecycle and continuation contracts. Repository workflows and the independently operated runtime continue without SoFa. An identity-provider session does not grant GitHub permission or approve a proposal, implementation or integration.

## Rationale

The owner's original request identifies operation from a real issue as the first delivery and explicitly excludes making future dashboards or infrastructure prerequisites. This change preserves the valid observer, source-evidence and independent-execution model while making authenticated interaction a required product journey.

In the authentication interview on 2026-10-26, the owner answered the question about authentication for reading as well as writing: “Si, deberíamos autenticar... Y me planteo si debemos usar mi EntraID, el de la org de jgcarmona (que casi no lo usao para nada...) o un keycloak para todos mis servicios caseros (incluído este) u otro proveedor de auth...” This grounds authenticated access to the operational workspace; it did not select a provider.

After comparing DomusMind's local identity module with external providers, the owner said: “Vamos a hacer con SoFa lo del proveedor, vale? y vamos a hacerlo de forma profesional, si?” This grounds an external identity provider for SoFa, without authorizing extraction or migration of DomusMind authentication.

Asked to choose the managed jgcarmona tenant or a home-hosted provider, the owner initially answered: “Entra ID de jgcarmona (recomendado)”. The owner subsequently reopened that choice: “Vamos a dar una vuelta, no tomemos esta decisión así a la ligera, ¿vale?” The complete response is preserved in proposal.md. This reopening is historical; the subsequent explicit direction below resolves provider selection. Provider registration and protocol realization belong in architecture and deployment configuration; no credentials are part of the product model.

The owner subsequently selected: “Para SoFa, concretaría la dirección en Keycloak compartido en [development host] para iniciar sesión, más una conexión autorizada con GitHub para actuar en tu nombre. La integración con GitHub sigue siendo independiente del login.” This initially chose shared Keycloak on [development host] and preserves independent GitHub authority; the later placement direction below replaces the host choice. The original LAN/VPN scope and configured owner access remain; shared identity does not expand SoFa to household administration.

The owner answered “OK...” to allowing ordinary comments during active work with a warning about possible source-side invalidation, then requested a concrete infrastructure and secret-recovery proposal. The full response is preserved in proposal.md. This resolves the comment choice; it does not accept the forthcoming infrastructure proposal.

The owner then refined placement: “Keycloak lo podemos poner en otra máquina, en [production host], por ejemplo... Digamos que [production host] es producción y [development host] es desarrollo”. This directs production identity to [production host] and development to [development host]; the deployment realization is proposed in ADR-0007. Shared identity and source authority semantics remain unchanged.

These answers resolve specific decisions. They do not approve the complete Product Change or accept an implementation by implication.

The owner explicitly closed identity direction and requested completion of the whole product on 2026-10-26: production Keycloak on [production host] and a development container inside Aspire. The complete answer is preserved in proposal.md. The original request grounds the daily operations, project, health and idea capabilities; these are now proposed rather than deferred as an undefined future product.

## Affected Product Areas

Factory access, issue context and conversation, explicit work commands, human intervention, repository search and project views, source authorization, execution correlation, uncertain outcomes, explainable health and configuration signals, idea promotion and mobile usability. Existing recommendations, delivery verification, capacity observations and independent runtime ownership remain applicable.

## Open Questions

- Which sign-in and session reauthentication policy should apply on a personal phone, including required authentication strength and the period before an unattended session must authenticate again? This needs a concrete security proposal before implementation; no timeout or assurance level is inferred from the word professional.
- Which source definitions, retention windows and comparable samples will support each later health/trend signal? Resolve within #54 from Lighthouse/source evidence; absent definitions produce unavailable signals and do not block the first delivery.

## Product Acceptance

For the full product, the owner can find a configured issue without attention labels, identify and resolve a source-backed intervention, switch between project views without changing work, explain a signal from its inputs and calculation, and explicitly promote or link an idea without duplicates. Each outcome is checked with real source evidence or honestly declared gaps; no invented score or notification volume is a success measure.

An owner can explain who is signed in, which GitHub account will act, why a command is available or unavailable, whether a request was accepted, which execution belongs to it, and what outcome or human decision remains. The journey can be completed on a phone without routinely setting labels or leaving SoFa to find results. A repeated or uncertain request never implies another authorized execution. Ordinary comments and replies that request continuation have explicit, distinct effects. Identity failure does not disclose protected content or grant writes, and closing SoFa does not stop admitted work.

The canonical six-stage lifecycle remains triage → refine → propose → implement → integrate → review. Command submission, issue state, execution state, active reservation and human approval remain distinct facts. Provider sign-in does not replace repository authority.

## Out of Scope

Accepting this draft, applying the model, implementation plans, deployment, provider credentials, changes to DomusMind, a new identity service, a SoFa scheduler or runtime, automatic approval or merge, a new lifecycle stage, public hosting and multi-user administration. Extensive dashboards, terminals, file browsers, health scoring formulas and copied screenshot thresholds are not prerequisites. The existing anonymous read surface receives no new privileges; this change defines its authenticated operational successor.
