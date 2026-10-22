# SoFa

SoFa — my software factory.

SoFa is Juan’s personal software factory. The ProductShape baseline describes the complete factory from backlog intake through verified delivery, while application implementation is intentionally empty until the architecture is established.

## Repository map

- [`docs/product/`](docs/product/README.md): ProductShape model and Product Changes.
- [`docs/architecture/`](docs/architecture/README.md): ArcShape-managed arc42 architecture documentation.
- [`docs/engineering-lifecycle.md`](docs/engineering-lifecycle.md): shared Agentic SDLC gates and working agreements.
- [`src/`](src/README.md): reserved implementation boundary; currently empty apart from repository tooling.
- [`tests/`](tests/README.md): repository and tooling tests.

## Tooling

Requires Node.js 22 or newer. Install dependencies and run the scaffold checks with:

```sh
npm ci
npm run verify
```

ProductShape and ArcShape are pinned as local development dependencies. Their managed files are refreshed through their CLIs; see [tooling notes](docs/tooling.md).
