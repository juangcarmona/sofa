# Publish a shareable arc42 snapshot

The `arc42-snapshot` tool — bundled inside this skill at `snapshot/` — turns any arc42 documentation folder into one self-contained, navigable HTML page: inline styles, inline pre-rendered Mermaid SVGs, inert JSON data, a hash router, and full-text search. No external resources; the file opens from `file://` and can be shared as a single attachment.

## What it does

1. Reads `README.md` and every `NN-*.md` section file in the docs folder.
2. Validates the [document contract](../SKILL.md#document-contract): all twelve sections `01` through `12`, unique numbers matching filenames, and exactly three nonempty string metadata values. Section identity must be quoted and zero-padded. README frontmatter is optional; when present, only nonempty `title` and `description` strings are allowed. Unexpected Markdown files at the folder root are diagnosed rather than silently omitted; keep supporting material in subdirectories. Contract violations produce diagnostics and exit code 2.
3. Renders Markdown through `marked`, displaying raw HTML as escaped text. Section links, `./` paths and heading anchors become hash routes with stable GitHub-style heading IDs. Links to files outside the docs folder (evidence paths, `../adr/...`) and images become inert references; a single file cannot carry them. HTTP(S) and email links remain explicit navigation, not automatically loaded resources.
4. Uses Markdown tokens to identify Mermaid fences, including tilde fences and nested blocks, while leaving fenced examples untouched. Mermaid CLI/Puppeteer renders diagrams in strict mode. SVG output is sanitized and its identifiers and references are namespaced without changing authored labels.
5. Writes one HTML file with a section-card overview, category-coded section icons, collapsible desktop navigation, and a responsive mobile navigation drawer. Previous/next links show section titles. Keyboard navigation (`/` to search, `←`/`→` for prev/next, `?` for shortcuts), accessible control labels, light/dark appearance toggle, and a Git revision stamp are included.

## Prerequisites (one-time install)

The tool needs Node.js 24 or later. Install the exact runtime dependencies from the bundled integrity lockfile, then explicitly install the browser if the documents contain Mermaid diagrams:

```bash
npm ci --ignore-scripts --prefix <skill-install-dir>/snapshot
npm exec --offline --prefix <skill-install-dir>/snapshot -- puppeteer browsers install chrome
```

The first command disables dependency lifecycle scripts and downloads no browser. The second command uses the already installed Puppeteer CLI to download its pinned Chromium build. Review both the skill and its runtime dependencies before installation. Regression tests are repository tooling and are not part of the installed skill.

## How to run it

```bash
# Basic: generate from any arc42 docs folder
node <skill-install-dir>/snapshot/generate.mjs path/to/docs/architecture

# Custom output path
node <skill-install-dir>/snapshot/generate.mjs path/to/docs/architecture --out snapshot.html

# Custom title (overrides the auto-derived name)
node <skill-install-dir>/snapshot/generate.mjs path/to/docs/architecture --title "My Platform"
```

**Default output:** `<docsDir>/../arc42-snapshot.html` — a sibling of the docs folder. Override with `--out` naming an `.html` or `.htm` file; Markdown paths are rejected. An existing HTML output at that path is replaced only after validation and rendering succeed.

**Mermaid rendering** launches Puppeteer/Chromium; expect ~1–2 minutes for a folder with ~20 diagrams.

The page embeds script-safe JSON and a content-security policy that permits its hashed application script and inline styles while blocking remote subresources. The revision stamp identifies the checkout commit, not a guarantee that source documents were committed; review the source working tree before treating it as a release identifier.

## When to use it

- **After completing an architecture documentation update** — generate a snapshot so reviewers can navigate the full arc42 picture without cloning the repo.
- **Before a milestone review** — share the single HTML file with stakeholders who need read-only access to the current architecture state.
- **To validate the document contract** — the tool's diagnostics catch missing frontmatter, duplicate section numbers, and numbering gaps before they reach a reviewer.

## What the snapshot is not

The snapshot is a **point-in-time rendering**, not a live document. It carries no server, no forms, and no persistence. Relative links to files outside the docs folder are intentionally inert. The canonical source remains the Markdown files; the HTML is regenerable output.

## Relationship to the architecture-docs skill

The tool reads the exact frontmatter contract this skill defines (`title`, `arc42-section`, `description`). It is the publishing companion to the skill's authoring workflow: the skill governs what goes *into* the documents; the snapshot tool renders what comes *out*. The tool is bundled at `snapshot/` inside the skill so that installing the skill gives you both the authoring instructions and the publishing tool.
