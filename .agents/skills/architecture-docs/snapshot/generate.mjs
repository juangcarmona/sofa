#!/usr/bin/env node
/**
 * generate.mjs — arc42 snapshot generator (portable, bundled with the architecture-docs skill).
 *
 * Usage:
 *   node generate.mjs <docsDir> [--out <file>] [--title <name>]
 *
 * Reads an arc42 documentation folder (README.md + NN-*.md sections with the
 * architecture-docs skill's frontmatter contract) and writes exactly one
 * self-contained HTML file: inline styles, inline pre-rendered Mermaid SVGs,
 * inert JSON data, one ES5 router. No external resources; opens from file://.
 * Generation is deterministic and never modifies authored files.
 *
 * Default output: <docsDir>/../arc42-snapshot.html  (sibling of the docs folder)
 */
import { mkdir, writeFile } from 'node:fs/promises';
import { execFile } from 'node:child_process';
import { promisify } from 'node:util';
import { resolve, join, dirname } from 'node:path';
import { loadDocs, Diagnostic } from './loadDocs.mjs';
import { renderMarkdown, extractMermaidSources, renderMermaid } from './render.mjs';
import { buildSnapshotHtml } from './shell.mjs';

const run = promisify(execFile);

function usage(code = 1) {
  process.stdout.write(
    [
      'Usage: node generate.mjs <docsDir> [--out <file>] [--title <name>]',
      '',
      '  docsDir   folder containing README.md and NN-*.md arc42 sections',
      '  --out     output HTML path (default: <docsDir>/../arc42-snapshot.html)',
      '  --title   page title override (default: derived from docsDir parent folder)',
      '',
    ].join('\n'),
  );
  process.exit(code);
}

function parseArgs(argv) {
  const positional = [];
  const flags = {};
  for (let i = 0; i < argv.length; i += 1) {
    const arg = argv[i];
    if (arg === '--out' || arg === '--title') {
      if (!argv[i + 1] || argv[i + 1].startsWith('--')) usage(1);
      flags[arg.slice(2)] = argv[i + 1];
      i += 1;
    } else if (arg === '--help' || arg === '-h') {
      usage(0);
    } else if (arg.startsWith('-')) {
      usage(1);
    } else {
      positional.push(arg);
    }
  }
  return { positional, flags };
}

/** Best-effort git revision of the repository containing docsDir. */
async function gitRevision(docsDir) {
  try {
    const { stdout } = await run('git', ['rev-parse', 'HEAD'], { cwd: docsDir });
    return stdout.trim();
  } catch {
    return undefined;
  }
}

async function main() {
  const { positional, flags } = parseArgs(process.argv.slice(2));
  if (positional.length !== 1) usage(1);
  const docsDir = resolve(positional[0]);

  let docs;
  try {
    docs = await loadDocs(docsDir);
  } catch (error) {
    if (error instanceof Diagnostic) {
      process.stderr.write(`arc42-snapshot: ${error.message}\n`);
      process.exit(2);
    }
    throw error;
  }

  const stackName = flags.title ?? docs.stackName;
  const outFile = flags.out
    ? resolve(flags.out)
    : join(dirname(docsDir), 'arc42-snapshot.html');
  if (!/\.html?$/i.test(outFile)) {
    throw new Diagnostic('--out must name an .html or .htm file; authored Markdown is never an output target');
  }

  // Route map: section file names and README.md -> hash routes.
  const fileToRoute = new Map(docs.sections.map((s) => [s.file, `#/section/${s.number}`]));
  fileToRoute.set(docs.readme.file, '#/');

  // Pre-render every mermaid diagram across all documents in one batch.
  const readmeMermaid = extractMermaidSources(docs.readme.body);
  const sectionMermaid = docs.sections.map((s) => extractMermaidSources(s.body));
  const allMermaid = [...readmeMermaid, ...sectionMermaid.flat()];
  process.stdout.write(
    `arc42-snapshot: ${docs.sections.length} sections, ${allMermaid.length} mermaid diagram(s)\n`,
  );
  const svgs = await renderMermaid(allMermaid);

  // Distribute the rendered SVGs back to their documents.
  let cursor = 0;
  const take = (count) => {
    const slice = svgs.slice(cursor, cursor + count);
    cursor += count;
    return slice;
  };

  const readmeRendered = await renderMarkdown(docs.readme.body, {
    fileToRoute,
    svgs: take(readmeMermaid.length),
    title: docs.readme.title,
  });
  const sectionsRendered = [];
  for (let i = 0; i < docs.sections.length; i += 1) {
    const section = docs.sections[i];
    const rendered = await renderMarkdown(section.body, {
      fileToRoute,
      svgs: take(sectionMermaid[i].length),
      currentRoute: `#/section/${section.number}`,
      title: section.title,
    });
    sectionsRendered.push({
      number: section.number,
      title: section.title,
      description: section.description,
      html: rendered.html,
    });
  }

  const revision = await gitRevision(docsDir);
  const html = buildSnapshotHtml({
    readme: {
      title: docs.readme.title,
      description: docs.readme.description,
      html: readmeRendered.html,
    },
    sections: sectionsRendered,
    stackName,
    revision,
  });

  await mkdir(dirname(outFile), { recursive: true });
  await writeFile(outFile, html, 'utf8');
  process.stdout.write(`arc42-snapshot: snapshot written to ${outFile}\n`);
}

main().catch((error) => {
  process.stderr.write(`arc42-snapshot: ${error.stack ?? error.message}\n`);
  process.exit(error instanceof Diagnostic ? 2 : 1);
});
