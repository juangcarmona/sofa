# Product-Intent Citations in Architecture

A product-intent source is any canonical, tool-validated store of product requirements, actors, use cases, rules, constraints, quality requirements and domain definitions. [ProductShape](https://github.com/pdac-dev/productshape) under `docs/product/model/` is the worked example used throughout this reference: its CLI owns citation payloads, identity, digest and status. Architecture documents cite that intent only when explaining how the system realizes it.

When the project maintains a different product-intent source, apply the same contract: the source's tooling generates and validates citations, and architecture only places them. When the project maintains none, skip this reference and report product-dependent claims as unverified assumptions.

## Citation workflow

1. Run `npx prodshape validate`. Stop when the product baseline has errors.
2. Identify the governing accepted artifact and run `npx prodshape inspect <ID>`. Use `npx prodshape impact <ID>` when related intent may also govern the architecture.
3. Write an architecture statement that explains the realization rather than copying the product artifact.
4. Copy the digest reported by `inspect` into `npx prodshape cite --id <ID> --digest <digest>`. Add `--anchor <scenario-id>` only when grounding the statement in that accepted scenario.
5. Place the exact CLI-emitted payload inside the repository's Markdown comment carrier immediately below the statement.

Conceptually:

```markdown
Authorization is enforced at the server-side application boundary.

<!-- exact payload emitted by `prodshape cite`, never handwritten -->
```

The comment above is instructional, not a valid citation. Never construct, edit or infer an ID, digest or payload manually.

## Validation

Validate architecture citations directly by target path:

```text
npx prodshape citations verify docs/architecture --format json
```

While citations remain in legacy `docs/ARCHITECTURE.md`, or when that file is edited, pass that file as an additional direct target. Architecture Markdown does not use OpenSpec `pdac-scope` declarations.

Interpret citation status without relabeling it (field names shown as ProductShape reports them):

- `current`: the payload matches the accepted artifact.
- `stale`: inspect the changed artifact, the architecture statement and implementation; refresh the citation only after confirming or updating the realization.
- `unresolved`: identify the intended accepted artifact before repairing the citation, or remove the unsupported claim.
- `tampered`: regenerate the payload with the CLI and inspect the surrounding content for unauthorized manual changes.

A normal Markdown link can aid navigation but never replaces a product-intent citation. Do not add architecture-specific citation formats, traceability metadata, drift records, IDs, graphs, resolvers or verification tooling.
