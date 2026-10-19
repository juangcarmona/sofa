# SoFa

SoFa — my software factory.

This repository is being scaffolded around the shared engineering practices used across the related repositories. Product intent and application architecture will be defined after the product is introduced; this repository currently contains no product baseline or application behavior.

## Repository map

- [`docs/product/`](docs/product/README.md): ProductShape model and Product Changes.
- [`docs/architecture/`](docs/architecture/README.md): ArcShape-managed arc42 architecture documentation.
- [`docs/engineering-lifecycle.md`](docs/engineering-lifecycle.md): shared Agentic SDLC gates and working agreements.
- [`src/`](src/README.md): implementation source.
- [`tests/`](tests/README.md): automated tests.

## Tooling

Requires Node.js 22 or newer. Install dependencies and run the scaffold checks with:

```sh
npm ci
npm run verify
```

ProductShape and ArcShape are pinned as local development dependencies. Their managed files are refreshed through their CLIs; see [tooling notes](docs/tooling.md).
