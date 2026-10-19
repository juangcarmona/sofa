---
name: architecture-docs
description: Maintain architecture documentation by checking implementation evidence, ADRs and product intent, and pruning legacy content. Use for architecture reviews, stale docs, changes to system boundaries, components, runtime, persistence, security, deployment, cross-cutting mechanisms, quality requirements, risks or technical debt, or publishing a self-contained arc42 HTML snapshot.
license: MIT
metadata:
  author: juangcarmona
---

# Maintain Architecture Documentation

Maintain a concise explanation of **how** the system is structurally shaped. Use the canonical twelve arc42 sections as the explicit structure under `docs/architecture/`; `README.md` is the navigation index, not a thirteenth section.

Keep these ownership boundaries intact:

| Source | Owns |
| --- | --- |
| The project's product-intent source, when one exists (for example `docs/product/model/` under ProductShape) | Product intent: what and why |
| `docs/architecture/` and, during migration, `docs/ARCHITECTURE.md` | Architecture: how the system realizes that intent |
| Source, configuration, tests and infrastructure | Implementation reality |
| `docs/adr/` | Rationale and consequences of significant decisions |

Architecture documentation may summarize a product concern only to explain its architectural realization. It never becomes another source of product requirements, terminology, drift state or traceability semantics. Never edit `docs/DESIGN.md` from this skill.

## arc42 references

Read only the sections affected by the task:

| Concern | Reference |
| --- | --- |
| Goals and architectural drivers | [Introduction and goals](references/arc42-01-introduction-and-goals.md) |
| Constraints | [Constraints](references/arc42-02-constraints.md) |
| System context and boundaries | [Context and scope](references/arc42-03-context-and-scope.md) |
| Solution strategy | [Solution strategy](references/arc42-04-solution-strategy.md) |
| Static decomposition | [Building block view](references/arc42-05-building-block-view.md) |
| Runtime interactions | [Runtime view](references/arc42-06-runtime-view.md) |
| Deployment | [Deployment view](references/arc42-07-deployment-view.md) |
| Crosscutting concepts | [Crosscutting concepts](references/arc42-08-crosscutting-concepts.md) |
| Architecture decisions | [Architecture decisions](references/arc42-09-architecture-decisions.md) |
| Quality realization | [Quality requirements](references/arc42-10-quality-requirements.md) |
| Risks and technical debt | [Risks and technical debt](references/arc42-11-risks-and-technical-debt.md) |
| Architecture terminology | [Glossary](references/arc42-12-glossary.md) |

When an architectural concern depends on product intent, also read [product-intent citations](references/product-intent-citations.md). When the project maintains no product-intent source, treat product-dependent claims as unverified assumptions and say so rather than inventing a source of truth.

To publish a shareable, self-contained HTML rendering of the arc42 documentation, read [arc42 snapshot](references/arc42-snapshot.md).

## Document contract

Every numbered architecture document starts with this minimal YAML frontmatter:

```yaml
---
title: <canonical arc42 section title>
arc42-section: "<zero-padded section number>"
description: <one sentence naming the architecture knowledge owned here>
---
```

Use only these three keys. `arc42-section` gives coding agents a stable section identity; `title` and `description` support discovery without reading the full document. Do not add status, owner, review-date, implementation inventory or traceability fields that can drift. `README.md` may use `title` and `description`, but never `arc42-section` because it is navigation rather than a thirteenth section.

Each section reference contains its canonical document scaffold. Use that scaffold when creating a document, replace bracketed labels with evidence-backed content, and remove unused optional headings. Never commit template placeholders. When a section has no substantive content, keep the document and state that concisely rather than inventing architecture.

## Workflow

### 1. Establish the evidence

Read `docs/README.md`, `docs/architecture/` when present, the relevant parts of legacy `docs/ARCHITECTURE.md`, `docs/adr/README.md`, applicable ADRs, and the project's product-intent source when one exists. Inspect the source, tests, configuration and infrastructure that actually implement the affected behavior. Treat generated files and legacy architecture prose as leads to verify, never as proof.

If the request concerns a change, inspect its diff or planned touch points first. If it is a general audit, sample every architecture surface represented by the twelve arc42 sections. This step is complete when every claim under review has an identified owner and at least one current evidence source.

### 2. Apply the significance gate

Update architecture documentation when the change alters or reveals at least one of these:

- system context, trust boundary or external system;
- major component responsibility, dependency direction or interface;
- architecturally important runtime interaction or failure path;
- persistence strategy, data ownership or consistency boundary;
- security, identity or authorization mechanism;
- deployment topology or mapping from software to infrastructure;
- major technology choice or cross-cutting mechanism;
- realization of a quality attribute;
- significant architectural risk or technical debt.

Normally classify class-level details, endpoint internals, local validation, routine dependency updates, isolated refactors and ordinary feature code as **no architecture impact**. A change is not significant merely because it changes many lines. If no item above changes, report `No architecture documentation impact` with one sentence of evidence and stop without editing architecture files.

This step is complete when the outcome is either a justified no-impact decision or a list of affected architecture concerns.

### 3. Map the impact to arc42

Map each architectural fact to one canonical owning section. A change may affect multiple sections; update every affected view, but do not duplicate the same authoritative explanation across them. Keep the sections distinct even when one is intentionally small; never merge distinct concerns into a generic document. `README.md` only identifies the documentation boundary, links the twelve sections and points to supporting diagrams.

For initial generation or an explicitly approved full migration, establish `README.md` and all twelve numbered documents with the document contract and section-specific scaffolds, then fill them with concise, evidence-backed content. For normal maintenance, preserve valid frontmatter and update only affected documents. Add no other architecture document unless a concrete volume, audience or maintenance need makes the canonical section impractical; keep supporting diagram sources under `docs/architecture/diagrams/`.

This step is complete when every affected fact has one arc42 owner and every affected view is identified.

### 4. Update only the affected documents

Write stable architectural structures, interactions, mechanisms, consequences and evidence at the abstraction defined by the owning arc42 section. Keep one fact in one authoritative location and link to it from other sections. Put operational procedures in runbooks, detailed API behavior in specifications or generated API documentation, and volatile inventories in source-controlled manifests or infrastructure.

Use Mermaid when a diagram communicates relationships better than prose. Keep diagram source beside the architecture document or under `docs/architecture/diagrams/`; every diagram needs explanatory text and names consistent with the prose. Link to implementation evidence instead of copying package, class, route, table or resource inventories.

Remove claims disproved by current evidence and consolidate duplicates into their canonical section. This step is complete when each changed document is concise, current, evidence-backed and limited to its arc42 responsibility.

### 5. Bind product-dependent claims to the product-intent source

When the project maintains a product-intent source with a citation mechanism, follow the repository-specific workflow in the [product-intent citations](references/product-intent-citations.md) reference. Architecture explains the realization of product intent and places the citation immediately below the grounded statement or coherent block of statements; it never copies the requirement, actor, use case, rule, constraint, quality requirement or domain definition as architecture truth.

Do not add citation metadata, drift markers, IDs, resolvers or verification tooling of your own; the product-intent source owns citation generation, validation, identity, status and drift. A single citation may ground a coherent paragraph, table row, architectural rule, mechanism or other meaningful unit when that unit derives from the same canonical product intent. This step is complete when every architecture statement or coherent block of statements that materially depends on product intent is grounded by the relevant current citation, or the absence of a product-intent source is reported, and architecture defines no product intent.

### 6. Use ADRs for significant decisions

Create or reference an ADR through the repository's existing ADR process when a significant choice needs rationale, alternatives or consequences. `09-architecture-decisions.md` is the architecture-facing index of those decisions; other arc42 views describe the resulting shape and link to the ADR where relevant.

Keep rationale, rejected alternatives and consequences in the ADR rather than repeating them in arc42 sections. This step is complete when every significant decision has one authoritative ADR and all affected architecture views link to it without duplicating it.

### 7. Migrate legacy content incrementally

Treat `docs/ARCHITECTURE.md` as untrusted migration input. Classify each touched passage before moving it:

| Classification | Action |
| --- | --- |
| Product intent | Remove the duplicate; cite the product-intent source from any retained architectural realization |
| Actual architecture | Verify against implementation, then map it to one of the twelve arc42 documents |
| Architecture decision | Link an existing ADR or create a proposed ADR; keep only the resulting shape in the view |
| Implementation detail | Remove it or replace it with a link to its owning source |
| Stale information | Delete it |
| Duplicate information | Consolidate it in one owner document and link from elsewhere |

Do not redistribute the legacy file section by section. Migrate only the area required by the current task unless the user explicitly approves a full migration, and delete or consolidate material that has no canonical owner. When `docs/architecture/README.md` first becomes the entry point, keep `docs/ARCHITECTURE.md` as a short migration pointer until all inbound references are updated. Search for those references and update affected commands, agents, documentation indexes and guardrail-generation inputs in the same migration change.

This step is complete when each touched legacy passage has one classification and no content was preserved merely because it already existed.

### 8. Validate and report

Run the narrowest checks that cover the edited documents:

```text
npx markdownlint-cli2 <changed-markdown-files>
```

When the project's product-intent source provides citation verification, run it over the edited architecture documents (for example `npx prodshape citations verify docs/architecture --format json` under ProductShape). While the legacy file contains citations or is edited directly, validate it by passing `docs/ARCHITECTURE.md` as the target. Treat every stale, unresolved or tampered citation as a review item; do not silence it by deleting the citation or copying the product text. Check links and Mermaid syntax with repository tooling when available, and compare every changed diagram and statement with the implementation evidence gathered in step 1.

Report the significance decision, files changed, evidence checked, product-intent citation status, ADRs created or referenced, legacy content removed or deferred, checks run and unresolved risks. This step is complete when validation passes or every remaining failure is reported without being hidden.

### 9. Publish a shareable snapshot (optional)

When the documentation update is ready for review or reaches a milestone, generate a self-contained, navigable HTML snapshot so stakeholders can browse the full arc42 picture without cloning the repository. The snapshot tool is bundled with this skill at `snapshot/` and reads the same document contract defined above. It renders a section-card overview, category-coded section navigation, a collapsible desktop sidebar, a responsive mobile drawer, full-text search, pre-rendered Mermaid diagrams, title-led previous/next links, and a Git revision stamp.

```bash
# One-time: install locked dependencies without lifecycle scripts
npm ci --ignore-scripts --prefix <skill-install-dir>/snapshot

# Explicitly install the pinned browser for Mermaid rendering
npm exec --offline --prefix <skill-install-dir>/snapshot -- puppeteer browsers install chrome

# Generate from any arc42 docs folder
node <skill-install-dir>/snapshot/generate.mjs path/to/docs/architecture
```

The tool requires Node.js 24 or later. The default output is `arc42-snapshot.html` written as a sibling of the docs folder; override with `--out <file.html>`. Contract violations, including missing or malformed section frontmatter, missing sections among `01` through `12`, duplicate section numbers, and unexpected Markdown filenames, fail generation with exit code 2. See [arc42 snapshot](references/arc42-snapshot.md) for full usage details. This step is optional; skip it when no shareable rendering is needed.

## Final challenge

Before finishing, answer each question with evidence:

- Did architecture prose explain **how** without becoming another product-intent source?
- Did every product dependency use a citation from the project's product-intent source rather than a substitute, or was the absence of one reported?
- Did the change avoid custom citation, drift, ID, graph, DSL and resolver machinery?
- Was ordinary implementation detail left out?
- Are all twelve arc42 concerns kept in their canonical documents rather than merged into generic files?
- Is `README.md` only the navigation index rather than another architecture section?
- Was legacy text verified and pruned rather than preserved by default?
- Are all architecture claims evidenced rather than invented?
- Are decisions linked to ADRs without duplicating their rationale?
- Is each fact owned once, with summaries linking to it, so documentation cannot grow by accumulation alone?
- If a snapshot was generated, does it reflect the final state of the edited documents and pass the tool's contract validation?

Any `no` reopens the relevant workflow step.
