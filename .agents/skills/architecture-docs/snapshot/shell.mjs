/**
 * shell.mjs — build the single self-contained HTML page.
 *
 * Structure mirrors productshape's buildSnapshotHtml:
 *  - one inline <style> with light + dark design tokens;
 *  - one inert JSON data block;
 *  - one ES5 application script with a single hash router;
 *  - no external resources, no forms, nothing persisted outside the address.
 */
import { escapeHtml as esc } from './render.mjs';
import { createHash } from 'node:crypto';

/* ------------------------------------------------------------------ */
/* Design tokens: a calm engineering instrument in two appearances.   */
/* ------------------------------------------------------------------ */
const lightBlock = `  --bg: #fbfbfa;
  --panel: #f2f2ef;
  --raise: #ffffff;
  --ink: #1c1e21;
  --text: #33373d;
  --muted: #6b7078;
  --line: #dcdcd6;
  --line-strong: #b9bab2;
  --accent: #0b5cad;
  --accent-soft: #e3edf7;
  --mark: #ffe9a8;
  --code-bg: #f0efe9;`;

const darkBlock = `  --bg: #16181b;
  --panel: #1d2024;
  --raise: #24272c;
  --ink: #e8eaed;
  --text: #c6cad0;
  --muted: #8d939c;
  --line: #33373d;
  --line-strong: #4a4f57;
  --accent: #6cb2eb;
  --accent-soft: #22303c;
  --mark: #6a5b1e;
  --code-bg: #22252a;
  --cat-quality: #6cb2eb;
  --cat-views: #57c6a1;
  --cat-concepts: #d4a64b;
  --cat-other: #9da7b4;`;

const lightBlockWithSectionColors = `${lightBlock}
  --cat-quality: #0b5cad;
  --cat-views: #147d64;
  --cat-concepts: #946200;
  --cat-other: #6b7078;`;

const SECTION_ICONS = [
  '<circle cx="8" cy="8" r="5.5"/><circle cx="8" cy="8" r="2"/><path d="M8 1v2M15 8h-2M8 15v-2M1 8h2"/>',
  '<path d="M3 4h10M3 8h10M3 12h10"/><circle cx="6" cy="4" r="1.4"/><circle cx="10" cy="8" r="1.4"/><circle cx="7" cy="12" r="1.4"/>',
  '<rect x="2" y="3" width="5" height="5" rx="1"/><rect x="9" y="8" width="5" height="5" rx="1"/><path d="M7 5.5h2v5H9"/>',
  '<circle cx="8" cy="8" r="6"/><path d="m8 2 1.2 5.2L14 8l-5.2 1.2L8 14l-1.2-4.8L2 8l4.8-.8z"/>',
  '<path d="m2 5 6-3 6 3-6 3zM2 5v6l6 3 6-3V5M8 8v6"/>',
  '<circle cx="3" cy="8" r="1.5"/><circle cx="13" cy="4" r="1.5"/><circle cx="13" cy="12" r="1.5"/><path d="m4.5 7.5 7-3M4.5 8.5l7 3"/>',
  '<rect x="2" y="2" width="12" height="4" rx="1"/><rect x="2" y="7" width="12" height="4" rx="1"/><rect x="2" y="12" width="12" height="2" rx="1"/>',
  '<path d="m8 2 6 3-6 3-6-3zM2 8l6 3 6-3M2 11l6 3 6-3"/>',
  '<path d="M3 3v10h10M5 10l3-3 2 2 4-5M11 4h3v3"/>',
  '<path d="M2 12a6 6 0 1 1 12 0M8 10l3-4M4 12h8"/><circle cx="8" cy="10" r="1"/>',
  '<path d="m8 2 6 11H2zM8 6v3.5M8 11.5v.1"/>',
  '<path d="M3 2.5h7l3 3V14H3zM10 2.5V6h3M5 8h6M5 10h6M5 12h4"/>',
];

const SECTION_CODES = ['GOAL', 'CONS', 'CTX', 'STRAT', 'BLDG', 'RUN', 'DEP', 'CONC', 'ADR', 'QUAL', 'RISK', 'TERM'];

function sectionCategory(number) {
  const n = Number(number);
  if (n <= 3 || n === 10) return 'quality';
  if (n >= 5 && n <= 7) return 'views';
  if (n >= 4 && n <= 9) return 'concepts';
  return 'other';
}

function sectionIcon(number) {
  const index = Number(number) - 1;
  return `<svg viewBox="0 0 16 16" aria-hidden="true">${SECTION_ICONS[index] ?? SECTION_ICONS[11]}</svg>`;
}

const style = `
:root {
${lightBlockWithSectionColors}
  --sans: system-ui, -apple-system, 'Segoe UI', Roboto, Ubuntu, Cantarell, 'Helvetica Neue', Arial, sans-serif;
  --mono: ui-monospace, SFMono-Regular, 'SF Mono', 'Cascadia Mono', Consolas, 'Liberation Mono', Menlo, monospace;
  color-scheme: light;
}
@media (prefers-color-scheme: dark) {
  :root:not([data-appearance='light']) {
${darkBlock}
    color-scheme: dark;
  }
}
:root[data-appearance='dark'] {
${darkBlock}
  color-scheme: dark;
}
* { box-sizing: border-box; }
[hidden] { display: none !important; }
.visually-hidden { position:absolute; width:1px; height:1px; padding:0; margin:-1px; overflow:hidden; clip:rect(0,0,0,0); white-space:nowrap; border:0; }
html, body { height: 100%; }
body {
  margin: 0; display: flex; flex-direction: column; overflow: hidden;
  font: 15px/1.6 var(--sans); color: var(--text); background: var(--bg);
  -webkit-text-size-adjust: 100%;
}
h1, h2, h3, h4, h5, h6 { color: var(--ink); line-height: 1.3; }
a { color: var(--accent); text-decoration: none; }
a:hover { text-decoration: underline; }
button { font: inherit; color: inherit; }
code, kbd, .rev { font-family: var(--mono); font-size: 0.88em; }
code { background: var(--code-bg); border: 1px solid var(--line); border-radius: 3px; padding: 0.05em 0.35em; }
pre { background: var(--panel); border: 1px solid var(--line); border-radius: 4px; padding: 0.6rem 0.8rem; overflow-x: auto; font-size: 0.85rem; line-height: 1.5; }
pre code { background: none; border: none; padding: 0; font-size: inherit; }
:focus-visible { outline: 2px solid var(--accent); outline-offset: 1px; }

.skip {
  position: absolute; left: -9999px; top: 0; z-index: 20;
  background: var(--raise); color: var(--ink); padding: 0.4rem 0.8rem;
  border: 1px solid var(--line-strong);
}
.skip:focus { left: 0.5rem; top: 0.5rem; }

/* ---- Header ---- */
header.site {
  display: flex; align-items: center; gap: 0.9rem; flex: none;
  padding: 0.45rem 1rem; border-bottom: 1px solid var(--line);
  background: var(--raise);
}
header.site h1 { margin: 0; min-width: 0; font-size: 0.98rem; overflow-wrap: anywhere; }
.revshort {
  font-family: var(--mono); font-size: 0.74rem; color: var(--muted);
  border: 1px solid var(--line); border-radius: 3px; padding: 0.05rem 0.4rem; white-space: nowrap;
}
header.site .spacer { flex: 1; }
.iconbtn {
  display: inline-flex; align-items: center; justify-content: center;
  flex: none;
  width: 1.7rem; height: 1.7rem; border: 1px solid var(--line-strong); border-radius: 4px;
  background: var(--raise); cursor: pointer; color: var(--muted);
}
.iconbtn:hover { color: var(--ink); border-color: var(--accent); }
.appearance { display: inline-flex; flex: none; border: 1px solid var(--line-strong); border-radius: 4px; overflow: hidden; }
.appearance button {
  border: none; background: var(--raise); color: var(--muted); cursor: pointer;
  padding: 0.15rem 0.5rem; font-size: 0.78rem;
}
.appearance button + button { border-left: 1px solid var(--line); }
.appearance button[aria-pressed='true'] { background: var(--accent-soft); color: var(--ink); }

/* ---- Layout ---- */
#shell { flex: 1 1 auto; min-height: 0; display: flex; }
aside.nav {
  position:relative; flex:none; width:17.5rem; overflow-y:auto; border-right:1px solid var(--line);
  background: var(--panel); padding: 0.9rem 0.75rem;
  transition:width .18s ease,padding .18s ease;
}
main#main { flex: 1 1 auto; min-width: 0; overflow-y: auto; padding: 1.4rem 2.2rem 4rem; scroll-behavior: smooth; }
.doc { max-width: 78ch; margin: 0 auto; }
.doc > h2.doc-title { margin: 0 0 0.2rem; font-size: 1.45rem; }
p.doc-desc { margin: 0 0 1.2rem; color: var(--muted); font-size: 0.9rem; }

/* ---- Sidebar ---- */
aside.nav .nav-heading { display:flex; align-items:center; justify-content:space-between; gap:.5rem; margin:0 0 .5rem; }
aside.nav h2 { margin:0; font-size:.72rem; text-transform:uppercase; letter-spacing:.08em; color:var(--muted); }
aside.nav ol { list-style: none; margin: 0; padding: 0; }
aside.nav li { margin: 0; border-bottom: 1px solid var(--line); }
aside.nav a {
  display:flex; gap:.55rem; align-items:center; padding:.32rem .4rem;
  color: var(--text); border-left: 2px solid transparent;
}
aside.nav a:hover { color: var(--ink); text-decoration: none; background: var(--raise); }
aside.nav a[aria-current='page'] { color: var(--ink); border-left-color: var(--accent); background: var(--raise); }
aside.nav .nav-icon { display:inline-flex; flex:none; width:1rem; height:1rem; color:var(--category); }
aside.nav .nav-icon svg, .prevnext svg { width:100%; height:100%; fill:none; stroke:currentColor; stroke-width:1.6; stroke-linecap:round; stroke-linejoin:round; }
aside.nav a[data-category="quality"], .prevnext a[data-category="quality"] { --category:var(--cat-quality); }
aside.nav a[data-category="views"], .prevnext a[data-category="views"] { --category:var(--cat-views); }
aside.nav a[data-category="concepts"], .prevnext a[data-category="concepts"] { --category:var(--cat-concepts); }
aside.nav a[data-category="other"], .prevnext a[data-category="other"] { --category:var(--cat-other); }
aside.nav .nav-code { display:none; font:600 .58rem/1 var(--mono); color:var(--category); }
aside.nav .t { flex:1 1 auto; min-width:0; font-size:.86rem; }
.nav-foot { margin-top: 1rem; padding-top: 0.6rem; border-top: 1px solid var(--line); font-size: 0.78rem; color: var(--muted); }
.nav-foot .prevnext { display: flex; justify-content: space-between; gap: 0.5rem; }
.nav-foot .prevnext a { font-size: 0.78rem; }
.prevnext a { display:flex; align-items:flex-start; gap:.35rem; max-width:48%; color:var(--category); }
.prevnext .nav-icon { display:inline-flex; width:.9rem; height:.9rem; flex:none; margin-top:.15rem; }
body.nav-collapsed aside.nav { width:4.25rem; padding:.9rem .55rem; }
body.nav-collapsed aside.nav .nav-heading { justify-content:center; }
body.nav-collapsed aside.nav h2, body.nav-collapsed aside.nav .nav-title, body.nav-collapsed aside.nav .nav-foot { display:none; }
body.nav-collapsed aside.nav a { flex-direction:column; gap:.12rem; align-items:center; padding:.48rem .15rem; }
body.nav-collapsed aside.nav .nav-icon { width:1.2rem; height:1.2rem; }
body.nav-collapsed aside.nav .nav-code { display:block; }
.mobile-nav-toggle, #nav-close, #nav-backdrop { display:none; }

/* ---- Content ---- */
.doc h3 { margin: 1.6rem 0 0.5rem; font-size: 1.12rem; border-bottom: 1px solid var(--line); padding-bottom: 0.25rem; }
.doc h4 { margin: 1.35rem 0 0.35rem; font-size: 1rem; }
.doc h5 { margin: 1.1rem 0 0.3rem; font-size: 0.9rem; }
.doc h6 { margin: 1rem 0 0.3rem; font-size: 0.85rem; }
.doc p, .doc ul, .doc ol { margin: 0.55rem 0; }
.doc ul, .doc ol { padding-left: 1.4rem; }
.doc li { margin: 0.18rem 0; }
.doc table { border-collapse: collapse; margin: 0.8rem 0; font-size: 0.88rem; display: block; overflow-x: auto; max-width: 100%; }
.doc th, .doc td { border: 1px solid var(--line); padding: 0.32rem 0.6rem; text-align: left; vertical-align: top; }
.doc th { background: var(--panel); color: var(--ink); }
.doc blockquote { margin: 0.6rem 0; padding: 0.15rem 0.9rem; border-left: 3px solid var(--line-strong); color: var(--muted); }
.doc hr { border: none; border-top: 1px solid var(--line); margin: 1.4rem 0; }
.xref-dead code { word-break: break-all; }
figure.diagram {
  margin: 1rem 0; padding: 0.7rem; border: 1px solid var(--line); border-radius: 4px;
  /* Diagrams keep mermaid's light palette: dark text on a light paper ground,
     so they stay readable in both appearances. */
  background: #ffffff; color: #333; overflow-x: auto; text-align: center;
}
figure.diagram svg { max-width: 100%; height: auto; }
.diagram-missing { color: var(--muted); font-style: italic; }
mark { background: var(--mark); color: inherit; border-radius: 2px; padding: 0 0.05em; }

/* ---- Search dialog ---- */
.dlg { position: fixed; inset: 0; background: rgba(0,0,0,0.35); display: flex; align-items: flex-start; justify-content: center; padding-top: 12vh; z-index: 30; }
.dlgbox {
  width: min(38rem, 92vw); background: var(--raise); border: 1px solid var(--line-strong);
  border-radius: 6px; box-shadow: 0 12px 40px rgba(0,0,0,0.25); overflow: hidden;
}
.dlgbox input {
  width: 100%; border: none; border-bottom: 1px solid var(--line); padding: 0.7rem 0.9rem;
  font: inherit; color: var(--ink); background: var(--raise); outline: none;
}
.dlgbox ul { list-style: none; margin: 0; padding: 0; max-height: 50vh; overflow-y: auto; }
.dlgbox li a { display: block; padding: 0.45rem 0.9rem; color: var(--text); border-left: 2px solid transparent; }
.dlgbox li a:hover, .dlgbox li a.sel { background: var(--accent-soft); color: var(--ink); border-left-color: var(--accent); text-decoration: none; }
.dlgbox .where { display: block; font-size: 0.76rem; color: var(--muted); }
.dlgbox .empty { padding: 0.7rem 0.9rem; color: var(--muted); font-size: 0.88rem; }
kbd {
  border: 1px solid var(--line-strong); border-bottom-width: 2px; border-radius: 3px;
  padding: 0 0.3em; font-size: 0.8em; background: var(--raise); color: var(--muted);
}

/* ---- Keyboard help ---- */
.keysbox { width: min(28rem, 92vw); }
.keysbox dl { margin: 0; padding: 0.6rem 0.9rem 0.9rem; }
.keysbox dt { font-weight: 650; color: var(--ink); margin-top: 0.55rem; }
.keysbox dd { margin: 0.1rem 0 0; color: var(--muted); font-size: 0.88rem; }

/* ---- Toast ---- */
.toast {
  position: fixed; bottom: 1rem; left: 50%; transform: translateX(-50%);
  background: var(--raise); border: 1px solid var(--line-strong); border-radius: 4px;
  padding: 0.35rem 0.8rem; font-size: 0.85rem; color: var(--ink); z-index: 40;
  opacity: 0; transition: opacity 0.15s; pointer-events: none;
}
.toast.show { opacity: 1; }

/* ---- Narrow viewports ---- */
@media (max-width: 900px) {
  header.site .mobile-nav-toggle { display:inline-flex; }
  #shell { position:relative; }
  aside.nav { position:absolute; z-index:25; inset:0 auto 0 0; width:min(19rem,86vw); transform:translateX(-105%); transition:transform .2s ease; box-shadow:0 12px 36px rgba(0,0,0,.28); }
  body.mobile-nav-open aside.nav { transform:translateX(0); }
  #nav-close { display:inline-flex; }
  #nav-backdrop { position:absolute; z-index:24; inset:0; display:block; border:0; background:rgba(0,0,0,.48); }
  body.nav-collapsed aside.nav { width:min(19rem,86vw); padding:.9rem .75rem; }
  body.nav-collapsed aside.nav .nav-heading { justify-content:space-between; }
  body.nav-collapsed aside.nav h2, body.nav-collapsed aside.nav .nav-title, body.nav-collapsed aside.nav .nav-foot { display:block; }
  body.nav-collapsed aside.nav a { flex-direction:row; gap:.55rem; align-items:center; padding:.32rem .4rem; }
  body.nav-collapsed aside.nav .nav-icon { width:1rem; height:1rem; }
  body.nav-collapsed aside.nav .nav-code { display:none; }
  main#main { padding:1rem 1rem 3rem; }
}
@media (max-width: 520px) {
  header.site { flex-wrap: wrap; gap: 0.5rem; }
  header.site h1 { flex: 1 1 calc(100% - 6rem); }
  header.site .spacer { display: none; }
  header.site .appearance { margin-left: auto; }
}
.overview-intro { max-width:62rem; margin:0 auto 1.6rem; }
.overview-intro h2 { margin:0 0 .4rem; font-size:clamp(1.7rem,3vw,2.5rem); }
.overview-intro p { margin:0; color:var(--muted); font-size:1.05rem; }
.section-cards { display:grid; grid-auto-flow:column; grid-template-columns:repeat(2,minmax(0,1fr)); grid-template-rows:repeat(6,minmax(5rem,auto)); gap:.7rem; max-width:75rem; margin:auto; }
.section-card { display:flex; align-items:flex-start; gap:.85rem; min-height:5rem; padding:.9rem 1rem; color:var(--ink); border:1px solid var(--line); border-radius:7px; background:color-mix(in srgb,var(--category) 9%,var(--raise)); }
.section-card:hover { border-color:var(--category); text-decoration:none; }
.section-card[data-category="quality"] { --category:var(--cat-quality); }
.section-card[data-category="views"] { --category:var(--cat-views); }
.section-card[data-category="concepts"] { --category:var(--cat-concepts); }
.section-card[data-category="other"] { --category:var(--cat-other); }
.section-card .card-number { display:grid; place-items:center; width:2rem; height:2rem; flex:none; border-radius:50%; background:var(--category); color:#fff; font-weight:700; }
.section-card strong { display:block; color:var(--ink); font-size:1rem; }
.section-card small { display:block; margin-top:.2rem; color:var(--muted); font-size:.83rem; }
.overview-details { max-width:75rem; margin:2rem auto 0; }
@media(max-width:650px) { .section-cards { grid-auto-flow:row; grid-template-columns:1fr; grid-template-rows:none; } }
@media print {
  body { overflow: visible; }
  aside.nav, header.site .appearance, header.site .iconbtn { display: none; }
  main#main { overflow: visible; }
}
@media (prefers-reduced-motion: reduce) {
  * { animation-duration: 0.001ms !important; transition-duration: 0.001ms !important; scroll-behavior: auto !important; }
}
`.trim();

/* ------------------------------------------------------------------ */
/* The application: one router, views rendered from inert data.        */
/* ------------------------------------------------------------------ */
const script = String.raw`
(function () {
  'use strict';
  var doc = document;
  var root = doc.documentElement;
  var dataEl = doc.getElementById('arc42-data');
  if (!dataEl) return;
  var DATA = JSON.parse(dataEl.textContent || '{}');
  var README = DATA.readme || {};
  var SECTIONS = DATA.sections || [];
  var byNumber = {};
  for (var i = 0; i < SECTIONS.length; i += 1) byNumber[SECTIONS[i].number] = SECTIONS[i];

  var el = function (tag, cls, text) {
    var node = doc.createElement(tag);
    if (cls) node.className = cls;
    if (text !== undefined && text !== null) node.textContent = String(text);
    return node;
  };
  var clear = function (node) { while (node.firstChild) node.removeChild(node.firstChild); };

  /* ---------- Search index (built lazily, offline) ---------- */
  var index = null;
  function buildIndex() {
    index = [];
    var push = function (number, route, where, text) {
      var plain = text.replace(/<[^>]+>/g, ' ').replace(/&[a-z]+;/g, ' ');
      index.push({ number: number, route: route, where: where, text: plain });
    };
    push('', '#/', 'Overview', (README.title || '') + ' ' + (README.html || ''));
    for (var i = 0; i < SECTIONS.length; i += 1) {
      var s = SECTIONS[i];
      push(s.number, '#/section/' + s.number, s.title, s.title + ' ' + s.description + ' ' + (s.html || ''));
    }
  }
  function search(query) {
    if (!index) buildIndex();
    var terms = query.toLowerCase().split(/\s+/).filter(Boolean);
    if (terms.length === 0) return [];
    var results = [];
    for (var i = 0; i < index.length; i += 1) {
      var entry = index[i];
      var hay = entry.text.toLowerCase();
      var score = 0;
      var all = true;
      for (var t = 0; t < terms.length; t += 1) {
        var term = terms[t];
        var inTitle = entry.where.toLowerCase().indexOf(term) !== -1;
        var hits = hay.split(term).length - 1;
        if (hits === 0 && !inTitle) { all = false; break; }
        score += inTitle ? 12 : 0;
        score += Math.min(hits, 8);
      }
      if (all) results.push({ score: score, entry: entry });
    }
    results.sort(function (a, b) { return b.score - a.score || (a.entry.number < b.entry.number ? -1 : 1); });
    return results.slice(0, 24);
  }

  /* ---------- Views ---------- */
  function renderDoc(host, title, description, html) {
    clear(host);
    var article = el('article', 'doc');
    var h = el('h2', 'doc-title', title);
    article.appendChild(h);
    if (description) article.appendChild(el('p', 'doc-desc', description));
    var body = el('div', 'doc-body');
    body.innerHTML = html; /* generator-escaped markdown output only */
    article.appendChild(body);
    host.appendChild(article);
    doc.getElementById('main').scrollTop = 0;
  }

  function showOverview(host) {
    clear(host);
    var intro = el('section', 'overview-intro');
    intro.appendChild(el('h2', null, README.title || 'Architecture overview'));
    if (README.description) intro.appendChild(el('p', null, README.description));
    host.appendChild(intro);
    var cards = el('div', 'section-cards');
    for (var i = 0; i < SECTIONS.length; i += 1) {
      var section = SECTIONS[i];
      var card = doc.createElement('a');
      card.className = 'section-card';
      card.href = '#/section/' + section.number;
      card.setAttribute('data-category', section.category);
      card.appendChild(el('span', 'card-number', String(Number(section.number))));
      var copy = el('span');
      copy.appendChild(el('strong', null, section.title));
      copy.appendChild(el('small', null, section.description || 'Explore this architecture section.'));
      card.appendChild(copy);
      cards.appendChild(card);
    }
    host.appendChild(cards);
    if (README.html) {
      var details = el('section', 'overview-details');
      details.innerHTML = README.html; /* generator-escaped markdown output only */
      host.appendChild(details);
    }
  }

  function showSection(host, number) {
    var section = byNumber[number];
    if (!section) {
      clear(host);
      var box = el('div', 'doc');
      box.appendChild(el('h2', 'doc-title', 'Section not found'));
      var p = el('p');
      p.appendChild(doc.createTextNode('This snapshot does not contain section ' + number + '.'));
      box.appendChild(p);
      var ways = el('p');
      var home = doc.createElement('a');
      home.href = '#/';
      home.textContent = 'Open the overview';
      ways.appendChild(home);
      ways.appendChild(doc.createTextNode(' or '));
      var list = doc.createElement('a');
      list.href = '#/section/' + (SECTIONS[0] ? SECTIONS[0].number : '01');
      list.textContent = 'start at the first section';
      ways.appendChild(list);
      ways.appendChild(doc.createTextNode('.'));
      box.appendChild(ways);
      host.appendChild(box);
      return;
    }
    renderDoc(host, section.title, section.description, section.html || '');
  }

  /* ---------- Sidebar state ---------- */
  function syncNav(route) {
    var links = doc.querySelectorAll('aside.nav a[data-route]');
    for (var i = 0; i < links.length; i += 1) {
      var current = links[i].getAttribute('data-route') === route;
      if (current) links[i].setAttribute('aria-current', 'page');
      else links[i].removeAttribute('aria-current');
    }
    var m = /^#\/section\/(\d+)/.exec(route);
    var number = m ? m[1] : null;
    var prev = doc.getElementById('nav-prev');
    var next = doc.getElementById('nav-next');
    var set = function (a, target, direction) {
      clear(a);
      if (!target) {
        a.style.visibility = 'hidden';
        a.setAttribute('aria-label', direction === 'previous' ? 'Previous section' : 'Next section');
        return;
      }
      a.style.visibility = 'visible';
      a.href = '#/section/' + target.number;
      a.setAttribute('aria-label', (direction === 'previous' ? 'Previous: ' : 'Next: ') + target.title);
      a.setAttribute('data-category', target.category);
      var icon = el('span', 'nav-icon');
      icon.innerHTML = direction === 'previous'
        ? '<svg viewBox="0 0 16 16" aria-hidden="true"><path d="m10 3-5 5 5 5"/></svg>'
        : '<svg viewBox="0 0 16 16" aria-hidden="true"><path d="m6 3 5 5-5 5"/></svg>';
      a.appendChild(icon);
      a.appendChild(el('span', null, target.title));
    };
    if (number) {
      var pos = -1;
      for (var s = 0; s < SECTIONS.length; s += 1) if (SECTIONS[s].number === number) pos = s;
      set(prev, pos > 0 ? SECTIONS[pos - 1] : null, 'previous');
      set(next, pos >= 0 && pos < SECTIONS.length - 1 ? SECTIONS[pos + 1] : null, 'next');
    } else {
      set(prev, null); set(next, null);
    }
  }

  /* ---------- Router ---------- */
  function go() {
    var hash = location.hash || '#/';
    var host = doc.getElementById('content');
    var route = hash.split('?')[0];
    var m = /^#\/section\/(\d{2})(?:\/([^/]*))?$/.exec(route);
    if (m) showSection(host, m[1]);
    else showOverview(host);
    syncNav(m ? '#/section/' + m[1] : '#/');
    var anchor = m ? m[2] : /^#\/([^/]*)$/.exec(route);
    if (!m) anchor = anchor ? anchor[1] : '';
    if (anchor) {
      try {
        var heading = doc.getElementById(decodeURIComponent(anchor));
        if (heading && host.contains(heading)) heading.scrollIntoView({ block: 'start' });
      } catch (error) { /* malformed anchors leave the document at its start */ }
    }
  }

  /* ---------- Appearance: carried in the address only ---------- */
  function applyAppearance() {
    var m = /[?&]a=(light|dark)/.exec(location.hash);
    var choice = m ? m[1] : 'auto';
    if (choice !== 'auto') root.setAttribute('data-appearance', choice);
    else root.removeAttribute('data-appearance');
    var buttons = doc.querySelectorAll('.appearance button');
    for (var i = 0; i < buttons.length; i += 1) {
      buttons[i].setAttribute('aria-pressed', String(buttons[i].getAttribute('data-appearance-set') === choice));
    }
  }
  function setAppearance(choice) {
    var hash = (location.hash || '#/').split('?')[0];
    var next = choice === 'auto' ? hash : hash + (hash.indexOf('?') === -1 ? '?' : '&') + 'a=' + choice;
    location.hash = next;
  }

  /* ---------- Search dialog ---------- */
  var findState = { sel: 0, results: [] };
  function openFind() {
    var dlg = doc.getElementById('find');
    var input = doc.getElementById('find-input');
    dlg.hidden = false;
    input.value = '';
    runSearch('');
    input.focus();
  }
  function closeFind() { doc.getElementById('find').hidden = true; }
  function runSearch(query) {
    var list = doc.getElementById('find-results');
    clear(list);
    findState.results = search(query);
    findState.sel = 0;
    if (query.trim() === '') {
      var li = el('li');
      li.appendChild(el('div', 'empty', 'Type to search across the overview and all ' + SECTIONS.length + ' sections.'));
      list.appendChild(li);
      return;
    }
    if (findState.results.length === 0) {
      var li2 = el('li');
      li2.appendChild(el('div', 'empty', 'No matches.'));
      list.appendChild(li2);
      return;
    }
    for (var i = 0; i < findState.results.length; i += 1) {
      var r = findState.results[i].entry;
      var a = doc.createElement('a');
      a.href = r.route;
      a.addEventListener('click', closeFind);
      var where = el('span', null, r.where);
      a.appendChild(where);
      a.appendChild(el('span', 'where', r.number ? 'section ' + r.number : 'overview'));
      list.appendChild(a);
    }
    var first = list.querySelector('a');
    if (first) first.classList.add('sel');
  }
  function moveSel(delta) {
    var links = doc.querySelectorAll('#find-results a');
    if (links.length === 0) return;
    links[findState.sel].classList.remove('sel');
    findState.sel = Math.min(Math.max(findState.sel + delta, 0), links.length - 1);
    links[findState.sel].classList.add('sel');
    links[findState.sel].scrollIntoView({ block: 'nearest' });
  }

  /* ---------- Keyboard shortcuts ---------- */
  function shortcuts(event) {
    if (event.defaultPrevented) return;
    var target = event.target;
    var typing = target && (target.tagName === 'INPUT' || target.tagName === 'TEXTAREA' || target.isContentEditable);
    var findOpen = !doc.getElementById('find').hidden;
    if (findOpen) {
      if (event.key === 'Escape') { closeFind(); event.preventDefault(); }
      else if (event.key === 'ArrowDown') { moveSel(1); event.preventDefault(); }
      else if (event.key === 'ArrowUp') { moveSel(-1); event.preventDefault(); }
      else if (event.key === 'Enter') {
        var links = doc.querySelectorAll('#find-results a');
        if (links[findState.sel]) { location.hash = links[findState.sel].getAttribute('href'); closeFind(); }
        event.preventDefault();
      }
      return;
    }
    if (event.key === 'Escape' && doc.body.classList.contains('mobile-nav-open')) { closeMobileNav(); event.preventDefault(); return; }
    if (typing) return;
    if (event.key === '/') { openFind(); event.preventDefault(); }
    else if (event.key === '?') { doc.getElementById('keys').hidden = false; event.preventDefault(); }
    else if (event.key === 'Escape') { doc.getElementById('keys').hidden = true; }
    else if (event.key === 'ArrowRight' || event.key === 'PageDown') {
      var nx = doc.getElementById('nav-next');
      if (nx && nx.style.visibility !== 'hidden') location.hash = nx.getAttribute('href');
    }
    else if (event.key === 'ArrowLeft' || event.key === 'PageUp') {
      var pv = doc.getElementById('nav-prev');
      if (pv && pv.style.visibility !== 'hidden') location.hash = pv.getAttribute('href');
    }
  }

  var mobileNavReturnFocus = null;
  function closeMobileNav() {
    doc.body.classList.remove('mobile-nav-open');
    var toggle = doc.getElementById('mobile-nav-toggle');
    if (toggle) toggle.setAttribute('aria-expanded', 'false');
    doc.getElementById('nav-backdrop').hidden = true;
    if (mobileNavReturnFocus && typeof mobileNavReturnFocus.focus === 'function') mobileNavReturnFocus.focus();
    mobileNavReturnFocus = null;
  }

  /* ---------- Toast ---------- */
  var toastTimer = null;
  function toast(message) {
    var t = doc.getElementById('toast');
    t.textContent = message;
    t.classList.add('show');
    if (toastTimer) clearTimeout(toastTimer);
    toastTimer = setTimeout(function () { t.classList.remove('show'); }, 1600);
  }

  /* ---------- Wire up ---------- */
  window.addEventListener('hashchange', function () { applyAppearance(); go(); });
  doc.addEventListener('keydown', shortcuts);
  doc.getElementById('ov-find').addEventListener('click', openFind);
  doc.getElementById('keys-open').addEventListener('click', function () { doc.getElementById('keys').hidden = false; });
  doc.getElementById('keys-close').addEventListener('click', function () { doc.getElementById('keys').hidden = true; });
  doc.getElementById('keys').addEventListener('click', function (event) { if (event.target === this) this.hidden = true; });
  doc.getElementById('nav-collapse').addEventListener('click', function () {
    doc.body.classList.toggle('nav-collapsed');
    var expanded = !doc.body.classList.contains('nav-collapsed');
    this.setAttribute('aria-expanded', String(expanded));
    this.setAttribute('aria-label', expanded ? 'Collapse navigation' : 'Expand navigation');
    this.title = expanded ? 'Collapse navigation' : 'Expand navigation';
  });
  doc.getElementById('mobile-nav-toggle').addEventListener('click', function () {
    mobileNavReturnFocus = this;
    doc.body.classList.add('mobile-nav-open'); this.setAttribute('aria-expanded', 'true');
    doc.getElementById('nav-backdrop').hidden = false;
    var firstLink = doc.querySelector('#arc42-nav a');
    if (firstLink) firstLink.focus();
  });
  doc.getElementById('nav-close').addEventListener('click', closeMobileNav);
  doc.getElementById('nav-backdrop').addEventListener('click', closeMobileNav);
  doc.querySelector('aside.nav ol').addEventListener('click', function (event) {
    if (event.target.closest('a')) closeMobileNav();
  });
  var findInput = doc.getElementById('find-input');
  findInput.addEventListener('input', function () { runSearch(findInput.value); });
  var appearanceButtons = doc.querySelectorAll('.appearance button');
  for (var b = 0; b < appearanceButtons.length; b += 1) {
    appearanceButtons[b].addEventListener('click', function () { setAppearance(this.getAttribute('data-appearance-set')); });
  }
  applyAppearance();
  go();
})();
`;

/* ------------------------------------------------------------------ */
/* Snapshot assembly                                                   */
/* ------------------------------------------------------------------ */

/** Stable JSON: sorted keys, no incidental whitespace variation. */
function stableJson(value) {
  return JSON.stringify(value).replace(/</g, '\\u003c');
}

/**
 * @param {object} input
 * @param {{title: string, description: string, html: string}} input.readme
 * @param {Array<{number: string, title: string, description: string, html: string}>} input.sections
 * @param {string} input.stackName
 * @param {string|undefined} input.revision
 */
export function buildSnapshotHtml({ readme, sections, stackName, revision }) {
  const revisionShort = revision
    ? `<span class="revshort" title="revision ${esc(revision)}">${esc(revision.slice(0, 7))}</span>`
    : '<span class="revshort" title="Not generated from a Git checkout">no revision</span>';

  const navItems = sections
    .map(
      (s) =>
        `        <li><a href="#/section/${s.number}" data-route="#/section/${s.number}" data-category="${sectionCategory(s.number)}"><span class="nav-icon">${sectionIcon(s.number)}</span><span class="nav-title t">${esc(s.title)}</span><span class="nav-code">${SECTION_CODES[Number(s.number) - 1]}</span></a></li>`,
    )
    .join('\n');

  const data = {
    readme: { title: readme.title, description: readme.description, html: readme.html },
    sections: sections.map((s) => ({
      number: s.number,
      title: s.title,
      description: s.description,
      html: s.html,
      category: sectionCategory(s.number),
    })),
  };

  const lines = [
    '<!DOCTYPE html>',
    '<html lang="en">',
    '<head>',
    '<meta charset="utf-8">',
    '<meta name="viewport" content="width=device-width, initial-scale=1">',
    `<meta http-equiv="Content-Security-Policy" content="default-src 'none'; script-src 'sha256-${createHash('sha256').update(`\n${script}\n`).digest('base64')}'; style-src 'unsafe-inline'; base-uri 'none'; form-action 'none'">`,
    '<meta name="color-scheme" content="light dark">',
    `<title>${esc(stackName)} — arc42 Architecture Snapshot</title>`,
    `<style>\n${style}\n</style>`,
    '</head>',
    '<body>',
    '<a class="skip" href="#main">Skip to content</a>',
    '<header class="site">',
    `<h1>${esc(stackName)} · arc42</h1>`,
    revisionShort,
    '<span class="spacer"></span>',
    '<div class="appearance" role="group" aria-label="Appearance">',
    '  <button type="button" data-appearance-set="auto" aria-pressed="true">Auto</button>',
    '  <button type="button" data-appearance-set="light" aria-pressed="false">Light</button>',
    '  <button type="button" data-appearance-set="dark" aria-pressed="false">Dark</button>',
    '</div>',
    '<button type="button" class="iconbtn" id="ov-find" aria-haspopup="dialog" aria-label="Search (/)" title="Search (/)">\u2315</button>',
    '<button type="button" class="iconbtn mobile-nav-toggle" id="mobile-nav-toggle" aria-controls="arc42-nav" aria-expanded="false" aria-label="Open sections navigation" title="Open navigation">☰</button>',
    '<button type="button" class="iconbtn" id="keys-open" aria-haspopup="dialog" aria-label="Keyboard shortcuts (?)" title="Keyboard shortcuts (?)">?</button>',
    '</header>',
    '<div id="shell">',
    '<aside class="nav" id="arc42-nav" aria-label="arc42 sections">',
    '  <div class="nav-heading"><h2>Sections</h2><button type="button" class="iconbtn" id="nav-collapse" aria-label="Collapse navigation" aria-expanded="true" title="Collapse navigation">◧</button><button type="button" class="iconbtn" id="nav-close" aria-label="Close navigation" title="Close navigation">×</button></div>',
    '  <ol>',
    `    <li><a href="#/" data-route="#/" data-category="other"><span class="nav-icon"><svg viewBox="0 0 16 16" aria-hidden="true"><path d="m2 7 6-5 6 5v7H9v-4H7v4H2z"/></svg></span><span class="nav-title t">Overview</span><span class="nav-code">HOME</span></a></li>`,
    navItems,
    '  </ol>',
    '  <div class="nav-foot">',
    '    <div class="prevnext">',
    '      <a id="nav-prev" href="#/" aria-label="Previous section"></a>',
    '      <a id="nav-next" href="#/" aria-label="Next section"></a>',
    '    </div>',
    '  </div>',
    '</aside>',
    '<button type="button" id="nav-backdrop" aria-label="Close navigation" hidden></button>',
    '<main id="main">',
    '  <div id="content"></div>',
    '</main>',
    '</div>',
    '<div class="dlg" id="find" role="dialog" aria-modal="true" aria-labelledby="find-title" hidden>',
    '<div class="dlgbox">',
    '  <h2 class="visually-hidden" id="find-title">Search architecture</h2>',
    `  <input id="find-input" type="text" placeholder="Search ${esc(stackName)} docs…" aria-label="Search query" autocomplete="off">`,
    '  <ul id="find-results"></ul>',
    '</div>',
    '</div>',
    '<div class="dlg" id="keys" role="dialog" aria-modal="true" aria-labelledby="keys-title" hidden>',
    '<div class="dlgbox keysbox">',
    '  <h2 class="visually-hidden" id="keys-title">Keyboard shortcuts</h2>',
    '  <input type="text" value="Keyboard shortcuts" aria-label="Keyboard shortcuts" readonly>',
    '  <dl>',
    '    <dt><kbd>/</kbd></dt><dd>Search the overview and all sections</dd>',
    '    <dt><kbd>←</kbd> / <kbd>→</kbd></dt><dd>Previous / next section</dd>',
    '    <dt><kbd>?</kbd></dt><dd>Show this list</dd>',
    '    <dt><kbd>Esc</kbd></dt><dd>Close dialogs</dd>',
    '  </dl>',
    '  <p style="margin:0; padding:0.6rem 0.9rem 0.9rem; text-align:right;"><button type="button" id="keys-close">Close</button></p>',
    '</div>',
    '</div>',
    '<p class="toast" id="toast" role="status" aria-live="polite"></p>',
    `<script id="arc42-data" type="application/json">${stableJson(data)}</script>`,
    `<script>\n${script}\n</script>`,
    '</body>',
    '</html>',
  ];
  return `${lines.filter((l) => l !== '').join('\n')}\n`;
}
