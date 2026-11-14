import './workspace-ui.js';

const editor = document.querySelector('#idea-editor');
const status = document.querySelector('#idea-status');
const panel = document.querySelector('#idea-review-panel');
const result = document.querySelector('#idea-result');
let ideas = [], selected = null, dirty = false, busy = false;
const field = name => editor.elements.namedItem(name);
const button = name => document.querySelector(`#idea-${name}`);

function controls() {
  const editable = !selected || ['draft', 'reviewed'].includes(selected.state);
  for (const name of ['title', 'repository', 'outcome', 'scope']) field(name).disabled = busy || !editable;
  editor.querySelector('[type=submit]').disabled = busy || !editable;
  button('review').disabled = busy || !selected || dirty || !editable;
  button('abandon').disabled = busy || !selected || !editable;
  button('link').disabled = busy || !selected || dirty || !editable || selected.state !== 'reviewed';
  button('create').disabled = busy || dirty || panel.hidden || !button('checked').checked;
  button('reconcile').hidden = selected?.state !== 'uncertain';
  button('reconcile').disabled = busy;
  button('new').disabled = busy;
  button('refresh').disabled = busy;
}
function show(draft) {
  selected = draft; dirty = false; panel.hidden = true; button('checked').checked = false;
  for (const name of ['title', 'repository', 'outcome', 'scope']) field(name).value = draft?.[name] ?? '';
  field('number').value = '';
  result.replaceChildren(); result.hidden = !draft;
  if (draft) {
    result.textContent = `State: ${draft.state}. `;
    if (draft.source) {
      const a = document.createElement('a');
      a.href = `/workspace/issues/${draft.repository}/${draft.source.number}`;
      a.textContent = `Open confirmed issue ${draft.repository}#${draft.source.number}`;
      result.append(a);
    } else if (draft.state === 'uncertain') result.append('Creation may have reached GitHub. Reconcile it; do not create a replacement.');
  }
  controls();
}
function update(data, id = selected?.id) {
  if (!Array.isArray(data.ideas)) throw Error('Draft response was unavailable.');
  ideas = data.ideas;
  const select = field('repository'), previous = select.value;
  select.replaceChildren(new Option('Choose repository', ''), ...data.repositories.map(repo => new Option(repo, repo)));
  select.value = previous;
  const rows = ideas.map(draft => {
    const item = document.createElement('button'); item.type = 'button';
    item.textContent = `${draft.title} · ${draft.repository} · ${draft.state}`;
    item.addEventListener('click', () => { if (!busy && (!dirty || confirm('Discard unsaved edits?'))) show(draft); });
    return item;
  });
  document.querySelector('#idea-list').replaceChildren(...rows);
  show(ideas.find(draft => draft.id === id) ?? null);
  status.textContent = data.reason ?? (data.availability === 'available' ? `${ideas.length} retained ideas. No execution requested.` : 'Protected draft storage is unavailable.');
}
async function refresh() {
  if (dirty && !confirm('Discard unsaved edits and refresh?')) return;
  try { const response = await fetch('/api/ideas', { cache: 'no-store' }); if (!response.ok) throw Error(); update(await response.json()); }
  catch { status.textContent = 'Draft refresh failed. Your editor is retained; retry refresh when access is restored.'; }
}
async function act(operation, extra = {}) {
  if (busy) return;
  busy = true; controls();
  try {
    const csrf = await fetch('/api/controls/csrf', { cache: 'no-store' });
    if (!csrf.ok) throw Error();
    const { token } = await csrf.json();
    const response = await fetch('/api/ideas', { method: 'POST', headers: { 'Content-Type': 'application/json', 'X-SoFa-Csrf': token },
      body: JSON.stringify({ operation, ...(selected ? { id: selected.id, revision: selected.revision } : {}), ...extra }) });
    const data = await response.json();
    if (!response.ok) {
      if (operation === 'create') update(data);
      status.textContent = data.reason ?? 'The operation was not accepted. Refresh current authority and try again.';
      return;
    }
    update(data, selected?.id ?? data.ideas[0]?.id);
    if (operation === 'review') {
      panel.hidden = false; button('checked').checked = false;
      document.querySelector('#idea-actor').textContent = `Creating as ${data.actingUser} in ${selected.repository}: ${selected.title}. Nothing has been sent.`;
      document.querySelector('#idea-preview').textContent = data.preview;
      document.querySelector('#idea-related').replaceChildren(...data.related.map(issue => {
        const li = document.createElement('li'), a = document.createElement('a');
        a.href = `/workspace/issues/${selected.repository}/${issue.number}`;
        a.textContent = `#${issue.number} · ${issue.title}${issue.equivalent ? ' · equivalent title: link instead' : ''}`;
        const link = document.createElement('button'); link.type = 'button'; link.textContent = `Link #${issue.number}`;
        link.addEventListener('click', () => act('link', { number: issue.number })); li.append(a, ' ', link); return li;
      }));
      panel.scrollIntoView({ block: 'nearest' });
    }
    if (selected?.source) { result.focus(); result.scrollIntoView({ block: 'nearest' }); }
  } catch {
    panel.hidden = true;
    status.textContent = operation === 'create' ? 'Creation response was lost. Refresh the retained draft and reconcile; do not create a replacement.' : 'The operation could not be confirmed. Your draft is retained; refresh current access.';
  } finally { busy = false; controls(); }
}
editor.addEventListener('input', event => { if (event.target === field('number')) return; dirty = true; panel.hidden = true; controls(); });
editor.addEventListener('submit', event => {
  event.preventDefault();
  if (editor.reportValidity()) act('save', Object.fromEntries(['title', 'repository', 'outcome', 'scope'].map(name => [name, field(name).value])));
});
button('checked').addEventListener('change', controls);
button('review').addEventListener('click', () => act('review'));
button('create').addEventListener('click', () => act('create'));
button('link').addEventListener('click', () => { const number = Number(field('number').value); if (Number.isSafeInteger(number) && number > 0) act('link', { number }); else status.textContent = 'Enter a positive existing issue number.'; });
button('abandon').addEventListener('click', () => act('abandon'));
button('reconcile').addEventListener('click', () => act('reconcile'));
button('refresh').addEventListener('click', refresh);
button('new').addEventListener('click', () => { if (!dirty || confirm('Discard unsaved edits?')) { show(null); field('title').focus(); } });
refresh();
