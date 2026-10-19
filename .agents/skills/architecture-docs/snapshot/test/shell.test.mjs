import test from 'node:test';
import assert from 'node:assert/strict';
import { JSDOM } from 'jsdom';
import { buildSnapshotHtml } from '../shell.mjs';

const sections = Array.from({ length: 12 }, (_, index) => ({
  number: String(index + 1).padStart(2, '0'),
  title: `Section ${index + 1}`,
  description: `Description ${index + 1}`,
  html: `<p>Details for section ${index + 1}.</p>`,
}));

function createPage(hash = '#/') {
  const html = buildSnapshotHtml({
    readme: {
      title: 'Platform Architecture',
      description: 'A guide to the system.',
      html: '<p>Overview details remain available.</p>',
    },
    sections,
    stackName: 'Example Platform',
    revision: 'abcdef123456',
  });
  return new JSDOM(html, {
    runScripts: 'dangerously',
    pretendToBeVisual: true,
    url: `file:///snapshot.html${hash}`,
  });
}

test('shows a section-card overview and retains README content', () => {
  const { window } = createPage();
  const { document } = window;

  assert.equal(document.querySelectorAll('.section-card').length, 12);
  assert.equal(document.querySelector('.overview-intro h2').textContent, 'Platform Architecture');
  assert.match(document.querySelector('.overview-details').textContent, /Overview details remain available/);
  assert.equal(document.querySelector('.section-card[href="#/section/01"] strong').textContent, 'Section 1');
  assert.equal(document.querySelector('.section-card[data-category="quality"]'), document.querySelector('.section-card'));
  assert.equal(document.querySelector('.section-card[data-category="views"] strong').textContent, 'Section 5');
  assert.ok(document.querySelector('.section-card[data-category="concepts"]'));
  assert.ok(document.querySelector('.section-card[data-category="other"]'));
  assert.equal(document.querySelectorAll('.section-card .card-number').length, 12);
  window.close();
});

test('renders category icons and accessible desktop and mobile navigation controls', () => {
  const { window } = createPage();
  const { document } = window;
  const nav = document.querySelector('#arc42-nav');
  const collapse = document.querySelector('#nav-collapse');
  const toggle = document.querySelector('#mobile-nav-toggle');

  assert.equal(nav.getAttribute('aria-label'), 'arc42 sections');
  assert.equal(nav.querySelectorAll('.nav-icon svg[aria-hidden="true"]').length, 13);
  assert.equal(toggle.getAttribute('aria-controls'), 'arc42-nav');
  assert.equal(toggle.getAttribute('aria-expanded'), 'false');
  assert.equal(document.querySelector('#find').getAttribute('aria-labelledby'), 'find-title');
  assert.ok(document.querySelector('#find-title'));
  assert.ok(document.querySelector('#keys-title'));
  assert.match(document.querySelector('style').textContent, /@media \(max-width: 900px\)/);
  assert.match(document.querySelector('style').textContent, /body\.mobile-nav-open aside\.nav/);
  assert.doesNotMatch(document.querySelector('style').textContent, /#(?:f27ec2|43c4da|e3b544|a9b2c5|b0287a|0b7285|8a6100|56607a)\b/i);

  collapse.click();
  assert.ok(document.body.classList.contains('nav-collapsed'));
  assert.equal(collapse.getAttribute('aria-expanded'), 'false');
  assert.equal(collapse.getAttribute('aria-label'), 'Expand navigation');
  window.close();
});

test('opens and dismisses the mobile drawer with focus, keyboard, backdrop, and section selection', async () => {
  const { window } = createPage();
  const { document } = window;
  const toggle = document.querySelector('#mobile-nav-toggle');
  const backdrop = document.querySelector('#nav-backdrop');
  const firstLink = document.querySelector('#arc42-nav a');

  toggle.focus();
  toggle.click();
  assert.ok(document.body.classList.contains('mobile-nav-open'));
  assert.equal(toggle.getAttribute('aria-expanded'), 'true');
  assert.equal(backdrop.hidden, false);
  assert.equal(document.activeElement, firstLink);

  document.dispatchEvent(new window.KeyboardEvent('keydown', { key: 'Escape', bubbles: true }));
  assert.ok(!document.body.classList.contains('mobile-nav-open'));
  assert.equal(toggle.getAttribute('aria-expanded'), 'false');
  assert.equal(backdrop.hidden, true);
  assert.equal(document.activeElement, toggle);

  toggle.click();
  backdrop.click();
  assert.ok(!document.body.classList.contains('mobile-nav-open'));
  assert.equal(toggle.getAttribute('aria-expanded'), 'false');

  toggle.click();
  document.querySelector('#arc42-nav a[href="#/section/01"]').click();
  assert.ok(!document.body.classList.contains('mobile-nav-open'));
  assert.equal(toggle.getAttribute('aria-expanded'), 'false');
  await new Promise((resolve) => window.setTimeout(resolve, 25));
  assert.equal(window.location.hash, '#/section/01');
  assert.equal(document.querySelector('#content h2').textContent, 'Section 1');

  window.close();
});

test('uses section titles in the rendered view and previous/next navigation', async () => {
  const { window } = createPage('#/section/02');
  const { document } = window;
  assert.equal(document.querySelector('#content h2').textContent, 'Section 2');
  assert.equal(document.querySelector('#nav-prev').textContent.trim(), 'Section 1');
  assert.equal(document.querySelector('#nav-prev').getAttribute('aria-label'), 'Previous: Section 1');
  assert.equal(document.querySelector('#nav-next').textContent.trim(), 'Section 3');
  assert.equal(document.querySelector('#nav-next').getAttribute('aria-label'), 'Next: Section 3');

  document.dispatchEvent(new window.KeyboardEvent('keydown', { key: 'ArrowRight', bubbles: true }));
  await new Promise((resolve) => window.setTimeout(resolve, 0));
  assert.equal(window.location.hash, '#/section/03');
  assert.equal(document.querySelector('#content h2').textContent, 'Section 3');
  window.close();
});
