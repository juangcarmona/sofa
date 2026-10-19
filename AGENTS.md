# SoFa repository instructions

Read [README.md](README.md), [docs/README.md](docs/README.md), and [docs/engineering-lifecycle.md](docs/engineering-lifecycle.md) before making changes. Read the relevant ProductShape and ArcShape documents before changing product behavior or architecture.

## Sources of truth

- Product intent belongs to ProductShape under `docs/product/`. Change it through a Product Change; do not edit an accepted model directly. The current model is intentionally empty until SoFa's product is defined.
- Architecture belongs to `docs/architecture/` and explains how the implementation realizes accepted product intent. Use ArcShape's generated structure and the installed `architecture-docs` skill. Do not invent product requirements or architectural facts.
- Significant architecture decisions belong in `docs/adr/` through the shared ADR workflow.
- Implementation and tests belong under `src/` and `tests/`.
- The shared Agentic SDLC configuration belongs to `docs/engineering-lifecycle.md`. Follow it across work items; do not create a SoFa-specific lifecycle variant.

## Working agreements

- Use the repository's installed `agentic-sdlc` capabilities for refinement, proposal, implementation, integration, review, and status.
- Keep product definition changes distinct from implementation plans and delivery work.
- Preserve the established shared lifecycle labels, readiness and completion gates, retrospective practice, and human-only decisions.
- Keep generated ProductShape, ArcShape, and shared skill files under their installers' ownership. Update them through the relevant CLI or installer.
- Do not choose an application framework, runtime, domain model, or deployment shape until product intent and architecture have been established.

## Validation

Use `npm run verify` for the scaffold's deterministic checks. Update it and the CI workflow when new build, test, lint, or security tools are adopted. Report checks actually run and any known limitations.
