import { mkdtemp, readFile, writeFile, rm } from 'node:fs/promises';
import { tmpdir } from 'node:os';
import { join, posix } from 'node:path';
import { Marked } from 'marked';
import GithubSlugger from 'github-slugger';
import createDOMPurify from 'dompurify';
import { JSDOM } from 'jsdom';

const window = new JSDOM('').window;
const purifier = createDOMPurify(window);

/** Escape text for safe interpolation into HTML. */
export function escapeHtml(text) {
  return String(text)
    .replace(/&/g, '&amp;')
    .replace(/</g, '&lt;')
    .replace(/>/g, '&gt;')
    .replace(/"/g, '&quot;')
    .replace(/'/g, '&#39;');
}

export function normalizeMermaidSvg(svg, index) {
  const clean = purifier.sanitize(svg, {
    USE_PROFILES: { html: true, svg: true, svgFilters: true },
    ADD_TAGS: ['foreignObject'],
    FORBID_TAGS: ['script', 'iframe', 'object', 'embed', 'image', 'img', 'audio', 'video', 'link', 'use', 'animate', 'set', 'animateMotion', 'animateTransform'],
  });
  const document = new window.DOMParser().parseFromString(clean, 'image/svg+xml');
  const root = document.documentElement;
  if (root.localName !== 'svg' || document.querySelector('parsererror')) {
    throw new Error('Mermaid produced invalid SVG');
  }
  const rootId = `mmd-${index}`;
  const idMap = new Map();
  for (const element of [root, ...root.querySelectorAll('[id]')]) {
    const oldId = element.getAttribute('id');
    const newId = element === root ? rootId : `${rootId}-x${idMap.size}`;
    if (oldId) idMap.set(oldId, newId);
    element.setAttribute('id', newId);
  }
  const rewriteReferences = (value) => value.replace(/url\(\s*["']?#([^\s)"']+)["']?\s*\)/g,
    (match, id) => idMap.has(id) ? `url(#${idMap.get(id)})` : match);
  const unsafeCss = (value) => /@import|(?:https?:|data:|file:|\/\/)|url\(\s*["']?(?!#)/i.test(value);
  for (const element of [root, ...root.querySelectorAll('*')]) {
    for (const attribute of [...element.attributes]) {
      const { name, value } = attribute;
      if (/^(?:href|xlink:href|src)$/i.test(name)) {
        if (value.startsWith('#') && idMap.has(value.slice(1))) {
          element.setAttribute(name, `#${idMap.get(value.slice(1))}`);
        } else {
          element.removeAttribute(name);
        }
      } else if (name === 'aria-labelledby' || name === 'aria-describedby') {
        element.setAttribute(name, value.split(/\s+/).map((id) => idMap.get(id) ?? id).join(' '));
      } else if (unsafeCss(value)) {
        element.removeAttribute(name);
      } else {
        element.setAttribute(name, rewriteReferences(value));
      }
    }
  }
  for (const style of root.querySelectorAll('style')) {
    if (unsafeCss(style.textContent)) {
      style.remove();
    } else {
      style.textContent = rewriteReferences(style.textContent)
        .replace(/#([a-zA-Z_][\w:.-]*)/g, (match, id) => idMap.has(id) ? `#${idMap.get(id)}` : match);
    }
  }
  const width = /max-width:\s*([\d.]+)px/.exec(root.getAttribute('style') ?? '');
  root.removeAttribute('style');
  if (width) root.setAttribute('style', `max-width: ${Math.round(Number(width[1]))}px`);
  return new window.XMLSerializer().serializeToString(root);
}

/**
 * Pre-render mermaid diagram sources to normalized inline SVGs.
 * @param {string[]} sources mermaid diagram sources, in document order
 * @returns {Promise<string[]>} normalized SVG strings
 */
export async function renderMermaid(sources) {
  if (sources.length === 0) return [];
  const { run } = await import('@mermaid-js/mermaid-cli');
  const staging = await mkdtemp(join(tmpdir(), 'arc42-mermaid-'));
  try {
    const svgFiles = [];
    for (let i = 0; i < sources.length; i += 1) {
      const inFile = join(staging, `d${i}.mmd`);
      const outFile = join(staging, `d${i}.svg`);
      await writeFile(inFile, sources[i], 'utf8');
      await run(inFile, outFile, {
        quiet: true,
        outputFormat: 'svg',
        parseMMDOptions: {
          backgroundColor: 'transparent',
          mermaidConfig: { securityLevel: 'strict', htmlLabels: false },
        },
      });
      const raw = await readFile(outFile, 'utf8');
      svgFiles.push(normalizeMermaidSvg(raw, i));
    }
    return svgFiles;
  } finally {
    await rm(staging, { recursive: true, force: true });
  }
}

function isMermaid(lang) {
  return String(lang ?? '').trim().split(/\s+/)[0] === 'mermaid';
}

function normalizedHeading(text) {
  return String(text ?? '')
    .replace(/^\s*\d{1,2}[.)]\s*/, '')
    .replace(/\s+/g, ' ')
    .trim()
    .toLocaleLowerCase();
}

/** Remove a leading H1 that repeats the title already rendered from frontmatter. */
export function stripDuplicateTitle(body, title) {
  if (!title) return body;
  const marked = new Marked({ gfm: true });
  const tokens = marked.lexer(body);
  let offset = 0;
  for (const token of tokens) {
    if (token.type === 'space' || (token.type === 'html' && /^\s*<!--[\s\S]*-->\s*$/.test(token.text))) {
      offset += token.raw.length;
      continue;
    }
    if (token.type === 'heading' && token.depth === 1 && normalizedHeading(token.text) === normalizedHeading(title)) {
      return body.slice(0, offset) + body.slice(offset + token.raw.length);
    }
    break;
  }
  return body;
}

function createMarked(fileToRoute, svgs, currentRoute) {
  const marked = new Marked({ gfm: true });
  const slugger = new GithubSlugger();
  let mermaidCount = 0;

  marked.use({
    renderer: {
      html({ text }) {
        if (/^\s*<!--[\s\S]*-->\s*$/.test(text)) return '';
        return escapeHtml(text);
      },
      heading({ tokens, depth }) {
        const text = this.parser.parseInline(tokens);
        const plain = window.document.createElement('div');
        plain.innerHTML = text;
        const id = slugger.slug(plain.textContent);
        return `<h${depth} id="${escapeHtml(id)}">${text}</h${depth}>\n`;
      },
      code({ text, lang }) {
        if (isMermaid(lang)) {
          const svg = svgs[mermaidCount++];
          if (!svg) throw new Error('Mermaid diagram count does not match parsed Markdown');
          return `<figure class="diagram">${svg}</figure>`;
        }
        const escaped = escapeHtml(text);
        return `<pre><code${lang ? ` class="language-${escapeHtml(lang)}"` : ''}>${escaped}</code></pre>\n`;
      },
      link({ href, title, tokens }) {
        const text = this.parser.parseInline(tokens);
        const target = String(href ?? '');
        const titleAttribute = title ? ` title="${escapeHtml(title)}"` : '';
        if (/^(?:https?:|mailto:)/i.test(target)) {
          return `<a href="${escapeHtml(target)}"${titleAttribute} rel="noopener noreferrer">${text}</a>`;
        }
        const hashIndex = target.indexOf('#');
        let path = hashIndex < 0 ? target : target.slice(0, hashIndex);
        let anchor = hashIndex < 0 ? '' : target.slice(hashIndex + 1);
        try {
          path = decodeURIComponent(path);
          anchor = decodeURIComponent(anchor);
        } catch {
          return `<span class="xref-dead">${text} <code>${escapeHtml(target)}</code></span>`;
        }
        const route = path ? fileToRoute.get(posix.normalize(path)) : target.startsWith('#') ? currentRoute : undefined;
        if (route) {
          const hash = anchor ? `${route.replace(/\/$/, '')}/${encodeURIComponent(anchor)}` : route;
          return `<a href="${escapeHtml(hash)}"${titleAttribute}>${text}</a>`;
        }
        return `<span class="xref-dead">${text} <code>${escapeHtml(target)}</code></span>`;
      },
      image({ href, title, text }) {
        // No images are carried in the single file; name them honestly.
        return `<span class="xref-dead">[image: ${escapeHtml(String(text ?? ''))}] <code>${escapeHtml(String(href ?? ''))}</code></span>`;
      },
    },
  });

  return { marked, count: () => mermaidCount };
}

/**
 * Render one Markdown document to HTML.
 *
 * @param {string} body markdown source (frontmatter already stripped)
 * @param {object} opts
 * @param {string} opts.title frontmatter title, used to suppress a repeated leading H1
 * @param {Map<string,string>} opts.fileToRoute section file name -> hash route
 * @param {string[]} opts.svgs pre-rendered mermaid SVGs for this document
 * @param {string} opts.svgClass css class for inline svg wrappers
 */
export async function renderMarkdown(body, { fileToRoute, svgs = [], currentRoute = '#/', title = '' }) {
  const renderer = createMarked(fileToRoute, svgs, currentRoute);
  const html = await renderer.marked.parse(stripDuplicateTitle(body, title));
  if (renderer.count() !== svgs.length) throw new Error('Unused Mermaid SVGs: Markdown extraction and rendering disagree');
  return { html, mermaidCount: renderer.count() };
}

/**
 * Extract mermaid sources and a body with placeholders removed is not needed:
 * instead we count diagrams by scanning fenced blocks before rendering, so the
 * SVGs can be pre-rendered in one batch per document.
 */
export function extractMermaidSources(body) {
  const marked = new Marked({ gfm: true });
  const sources = [];
  marked.walkTokens(marked.lexer(body), (token) => {
    if (token.type === 'code' && isMermaid(token.lang)) sources.push(token.text);
  });
  return sources;
}
