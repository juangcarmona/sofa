import test from 'node:test';
import assert from 'node:assert/strict';
import { renderMarkdown, stripDuplicateTitle } from '../render.mjs';

const fileToRoute = new Map();

test('removes a leading numeric section heading when it repeats the frontmatter title', () => {
  const source = '<!-- markdownlint-disable MD025 -->\n\n# 12. Glossary\n\nTerms follow.\n';
  const cleaned = stripDuplicateTitle(source, 'Glossary');
  assert.doesNotMatch(cleaned, /^\s*# 12\. Glossary/m);
  assert.match(cleaned, /Terms follow\./);
});

test('preserves a non-matching leading heading', () => {
  const source = '# Different heading\n\nContent.\n';
  assert.equal(stripDuplicateTitle(source, 'Glossary'), source);
});

test('hides source HTML comments and keeps fenced examples escaped', async () => {
  const source = [
    '<!-- markdownlint-disable MD025 -->',
    '',
    '```html',
    '<!-- this is an example -->',
    '```',
  ].join('\n');
  const { html } = await renderMarkdown(source, { fileToRoute });
  assert.doesNotMatch(html, /markdownlint-disable/);
  assert.match(html, /&lt;!-- this is an example --&gt;/);
});

test('renders section body without a duplicate title and preserves following content', async () => {
  const source = '<!-- markdownlint-disable MD025 -->\n\n# 12. Glossary\n\n| Term | Meaning |\n| --- | --- |\n| Example | Definition |';
  const { html } = await renderMarkdown(source, { fileToRoute, title: 'Glossary' });
  assert.doesNotMatch(html, /markdownlint-disable/);
  assert.doesNotMatch(html, /<h1[^>]*>12\. Glossary<\/h1>/);
  assert.match(html, /<td>Example<\/td>/);
});
