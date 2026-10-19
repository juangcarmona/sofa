/**
 * loadDocs.mjs — scan an arc42 documentation folder, validate the document contract
 * and parse every section. Mirrors productshape's "honest diagnostics" principle:
 * a broken model fails generation rather than producing a page that silently
 * omits part of the documentation.
 */
import { readdir, readFile } from 'node:fs/promises';
import { join, basename, dirname } from 'node:path';
import * as yaml from 'js-yaml';

const ALLOWED_KEYS = new Set(['title', 'arc42-section', 'description']);
const SECTION_FILE = /^(\d{2})-(.+)\.md$/;
const SECTION_NUMBERS = Array.from({ length: 12 }, (_, index) => String(index + 1).padStart(2, '0'));

export class Diagnostic extends Error {
  constructor(message) {
    super(message);
    this.name = 'Diagnostic';
  }
}

/**
 * Parse the `---\nkey: value\n---` frontmatter block of a Markdown file.
 * Returns { frontmatter, body } or throws a Diagnostic naming the file.
 */
export function parseFrontmatter(raw, fileName) {
  const match = /^---\r?\n([\s\S]*?)\r?\n---\r?\n?([\s\S]*)$/.exec(raw);
  if (!match) {
    throw new Diagnostic(`${fileName}: missing YAML frontmatter block`);
  }
  let frontmatter;
  try {
    frontmatter = yaml.load(match[1], { schema: yaml.JSON_SCHEMA }) ?? {};
  } catch (error) {
    throw new Diagnostic(`${fileName}: unparseable frontmatter: ${error.message}`);
  }
  if (typeof frontmatter !== 'object' || Array.isArray(frontmatter)) {
    throw new Diagnostic(`${fileName}: frontmatter must be a mapping`);
  }
  return { frontmatter, body: match[2] };
}

/**
 * Load and validate an arc42 docs folder.
 *
 * Contract (from the architecture-docs skill):
 *  - README.md exists, optionally has title/description, never `arc42-section`.
 *  - Section files are named NN-slug.md with zero-padded numbers.
 *  - Each has exactly the keys: title, arc42-section, description.
 *  - arc42-section values are unique and match the file number.
 *
 * @param {string} docsDir absolute path to the docs/architecture folder
 * @returns {{ readme: object, sections: object[], stackName: string }}
 */
export async function loadDocs(docsDir) {
  const entries = await readdir(docsDir, { withFileTypes: true });
  const mdFiles = entries
    .filter((entry) => entry.isFile() && /\.md$/i.test(entry.name))
    .map((e) => e.name)
    .sort();

  const readmeName = mdFiles.find((n) => n.toLowerCase() === 'readme.md');
  if (!readmeName) {
    throw new Diagnostic(`${docsDir}: no README.md found — the navigation index is required`);
  }

  const sectionFiles = mdFiles.filter((n) => SECTION_FILE.test(n));
  if (sectionFiles.length === 0) {
    throw new Diagnostic(`${docsDir}: no NN-*.md section files found`);
  }

  const problems = mdFiles
    .filter((name) => name !== readmeName && !SECTION_FILE.test(name))
    .map((name) => `${name}: unexpected Markdown file; name an arc42 section NN-slug.md or move supporting material into a subdirectory`);
  const sections = [];
  const seenNumbers = new Map();

  // --- README: navigation index, never a section itself.
  const readmeRaw = await readFile(join(docsDir, readmeName), 'utf8');
  const readmeParsed = /^---\r?\n/.test(readmeRaw)
    ? parseFrontmatter(readmeRaw, readmeName)
    : { frontmatter: {}, body: readmeRaw };
  for (const key of Object.keys(readmeParsed.frontmatter)) {
    if (key === 'arc42-section') {
      problems.push(`${readmeName}: README must not carry arc42-section (it is navigation, not a section)`);
    } else if (!['title', 'description'].includes(key)) {
      problems.push(`${readmeName}: unexpected frontmatter key "${key}"`);
    }
  }
  for (const key of ['title', 'description']) {
    if (key in readmeParsed.frontmatter &&
        (typeof readmeParsed.frontmatter[key] !== 'string' || !readmeParsed.frontmatter[key].trim())) {
      problems.push(`${readmeName}: ${key} must be a nonempty string when present`);
    }
  }

  // --- Sections.
  for (const name of sectionFiles) {
    const number = SECTION_FILE.exec(name)[1];
    const raw = await readFile(join(docsDir, name), 'utf8');
    let parsed;
    try {
      parsed = parseFrontmatter(raw, name);
    } catch (error) {
      problems.push(error.message);
      continue;
    }
    const fm = parsed.frontmatter;
    for (const key of Object.keys(fm)) {
      if (!ALLOWED_KEYS.has(key)) {
        problems.push(`${name}: unexpected frontmatter key "${key}"`);
      }
    }
    for (const key of ['title', 'arc42-section', 'description']) {
      if (!(key in fm)) {
        problems.push(`${name}: missing required frontmatter key "${key}"`);
      } else if (typeof fm[key] !== 'string' || !fm[key].trim()) {
        problems.push(`${name}: ${key} must be a nonempty string`);
      }
    }
    if (!SECTION_NUMBERS.includes(number)) {
      problems.push(`${name}: section number must be between 01 and 12`);
    }
    if (fm['arc42-section'] !== undefined && fm['arc42-section'] !== number) {
      problems.push(`${name}: arc42-section "${fm['arc42-section']}" does not match file number ${number}`);
    }
    if (seenNumbers.has(number)) {
      problems.push(`${name}: duplicate section number ${number} (also ${seenNumbers.get(number)})`);
    }
    seenNumbers.set(number, name);

    sections.push({
      number,
      slug: SECTION_FILE.exec(name)[2],
      file: name,
      title: String(fm.title ?? name),
      description: String(fm.description ?? ''),
      body: parsed.body,
    });
  }

  for (const number of SECTION_NUMBERS) {
    if (!seenNumbers.has(number)) problems.push(`required arc42 section ${number} is missing`);
  }

  if (problems.length > 0) {
    const error = new Diagnostic(`arc42 document contract violations:\n  - ${problems.join('\n  - ')}`);
    error.problems = problems;
    throw error;
  }

  sections.sort((a, b) => a.number.localeCompare(b.number));

  // Derive a name from the docsDir's parent (e.g. .../my-project/docs/architecture → "my-project").
  // Falls back to the docsDir folder name itself if there is no meaningful parent.
  const parent = basename(dirname(docsDir));
  const grandparent = basename(dirname(dirname(docsDir)));
  const stackName = parent === 'docs' ? grandparent : parent || basename(docsDir);

  return {
    readme: {
      file: readmeName,
      title: String(readmeParsed.frontmatter.title ?? 'Architecture Documentation'),
      description: String(readmeParsed.frontmatter.description ?? ''),
      body: readmeParsed.body,
    },
    sections,
    stackName,
  };
}
