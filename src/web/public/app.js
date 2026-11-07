const $ = id => document.getElementById(id);
const node = (tag, text, className) => { const n = document.createElement(tag); if (text !== undefined) n.textContent = text; if (className) n.className = className; return n; };
const active = run => ['queued', 'in_progress', 'waiting', 'pending', 'requested'].includes(run.status);
const humanWait = issue => issue.labels.some(label => ['needs-info', 'waiting-review'].includes(label));
const sourceKey = (repository, kind, id) => JSON.stringify([repository, kind, id]);
function link(text, href, repository, kind, key) {
  const n = node('a', text);
  try {
    const u = new URL(href), prefix = `/${repository}/`;
    if (u.origin !== 'https://github.com' || u.username || u.password || !u.pathname.startsWith(prefix) || (kind && !u.pathname.startsWith(prefix + kind))) throw Error();
    n.href = u.href; n.rel = 'noreferrer';
  } catch { n.removeAttribute('href'); }
  n.dataset.key = key; n.dataset.repository = repository;
  return n;
}
function badge(text, tone = '') { return node('span', text, `badge ${tone}`); }
function timestamp(value) { return typeof value === 'string' && value.trim() && Number.isFinite(Date.parse(value)) ? Date.parse(value) : NaN; }
function absolute(value) { return Number.isFinite(timestamp(value)) ? new Date(value).toISOString() : 'Time unavailable'; }
function timing(label, value, observedAt) {
  const interval = timestamp(observedAt) - timestamp(value);
  if (!Number.isFinite(interval) || interval < 0) return node('p', `${label}: unavailable`, 'timestamp');
  const minutes = Math.floor(interval / 60000);
  const count = minutes < 60 ? minutes : minutes < 1440 ? Math.floor(minutes / 60) : Math.floor(minutes / 1440);
  const unit = minutes < 60 ? 'minute' : minutes < 1440 ? 'hour' : 'day';
  const age = minutes < 1 ? 'less than 1 minute' : `${count} ${unit}${count === 1 ? '' : 's'}`;
  const p = node('p', `${label}: ${age} as of observation · `, 'timestamp');
  const time = node('time', absolute(value)); time.dateTime = value; p.append(time);
  return p;
}
function uniqueIssues(repo) { return [...new Map(repo.attention.map(issue => [issue.number, issue])).values()]; }
function issueCard(repo, issue) {
  const li = node('li');
  li.append(node('p', repo.repository, 'muted'), link(`#${issue.number} ${issue.title}`, issue.url, repo.repository, 'issues/', sourceKey(repo.repository, 'issue', issue.number)));
  const labels = node('div', undefined, 'labels');
  for (const label of issue.labels) labels.append(badge(label, ['waiting-review', 'needs-info'].includes(label) ? 'caution' : label === 'agent-working' ? 'activity' : ''));
  li.append(labels, timing('Since source update (not wait duration)', issue.updatedAt, repo.observedAt));
  return li;
}
function runCard(repo, run) {
  const li = node('li'), heading = node('div', undefined, 'run-heading');
  heading.append(link(run.name || `Run ${run.id}`, run.url, repo.repository, 'actions/runs/', sourceKey(repo.repository, 'run', run.id)), badge(run.status || 'Status unavailable', active(run) ? 'activity' : ''));
  li.append(node('p', `${repo.repository} · Run ${run.id}`, 'muted'), heading, node('p', `Source revision: ${run.sourceRevision || 'unavailable'}`, 'timestamp'));
  if (run.conclusion) li.append(node('p', `Source conclusion: ${run.conclusion}`, 'muted'));
  if (run.status === 'in_progress') li.append(timing('Elapsed from source start', run.startedAt, repo.observedAt));
  li.append(timing('Since source update', run.updatedAt, repo.observedAt));
  return li;
}
function list(items) { const ul = node('ul', undefined, 'items'); ul.append(...items); return ul; }
let snapshot, busy = false;
function render(data) {
  const root = $('repositories'), focused = document.activeElement;
  const hadFocus = root.contains(focused), key = hadFocus ? focused.dataset.key : undefined;
  const repository = hadFocus ? focused.dataset.repository : undefined;
  const disclosures = new Map([...root.querySelectorAll('details')].map(n => [n.dataset.key, n.open]));
  const filter = $('repository-filter'), selected = filter.value;
  const names = data.repositories.map(repo => repo.repository);
  if (JSON.stringify([...filter.options].slice(1).map(o => o.value)) !== JSON.stringify(names)) {
    const all = node('option', 'All repositories'); all.value = '';
    filter.replaceChildren(all, ...names.map(name => { const option = node('option', name); option.value = name; return option; }));
    filter.value = names.includes(selected) ? selected : '';
  }
  const rows = data.repositories.filter(r => !filter.value || r.repository === filter.value);
  const available = rows.filter(r => r.availability === 'available'), unknown = rows.length - available.length;
  $('repo-count').textContent = `${available.length} / ${rows.length}`;
  $('active-count').textContent = available.length ? available.reduce((n, r) => n + r.runs.filter(active).length, 0) : 'Unavailable';
  $('attention-count').textContent = available.length ? available.reduce((n, r) => n + uniqueIssues(r).filter(humanWait).length, 0) : 'Unavailable';
  $('unknown-count').textContent = unknown;
  $('count-scope').textContent = `${filter.value || 'All configured repositories'}: counts include only collected active runs and needs-info / waiting-review issues, counted once per issue. ${unknown ? 'Known collected counts only; unavailable repositories leave an unknown remainder.' : 'No matching collected records does not prove inactivity or health.'}`;
  const fragment = document.createDocumentFragment(), waits = node('section'), workflows = node('section');
  waits.append(node('h2', 'Human intervention'), node('p', 'Source-labeled needs-info or waiting-review issues. Workflow waiting does not identify an approver. Agent-working / ready alone does not request owner action.', 'muted'));
  const waitingIssues = available.flatMap(repo => uniqueIssues(repo).filter(humanWait).map(issue => issueCard(repo, issue)));
  waits.append(waitingIssues.length ? list(waitingIssues) : node('p', available.length ? 'No human waits in the collected issues.' : 'Human waits unavailable.', 'empty'));
  workflows.append(node('h2', 'Active workflows'));
  for (const [title, statuses] of [['Running', ['in_progress']], ['Queued / pending / requested', ['queued', 'pending', 'requested']], ['Waiting', ['waiting']]]) {
    const group = node('section'), cards = available.flatMap(repo => repo.runs.filter(run => statuses.includes(run.status)).map(run => runCard(repo, run)));
    group.append(node('h3', title), cards.length ? list(cards) : node('p', available.length ? 'No matching collected runs.' : 'Workflow evidence unavailable.', 'empty'));
    workflows.append(group);
  }
  fragment.append(waits, workflows, node('h2', 'Repository evidence and history'));
  for (const repo of rows) {
    const card = node('article', undefined, 'repository'); card.dataset.repository = repo.repository;
    const heading = node('div', undefined, 'repo-heading');
    heading.append(node('h3', repo.repository), badge(repo.availability === 'available' ? 'Observed' : 'Evidence unavailable', repo.availability === 'available' ? 'positive' : 'caution'));
    card.append(heading, node('p', `Source observation time: ${absolute(repo.observedAt)}`, 'timestamp'));
    const actions = node('nav', undefined, 'repo-actions'); actions.setAttribute('aria-label', `${repo.repository} actions`);
    actions.append(link('Open workflow actions ↗', `https://github.com/${repo.repository}/actions/workflows/lighthouse-sdlc-router.yml`, repo.repository, 'actions/', sourceKey(repo.repository, 'actions', '')), link('Issues ↗', `https://github.com/${repo.repository}/issues`, repo.repository, undefined, sourceKey(repo.repository, 'issues', '')));
    card.append(actions);
    if (repo.availability !== 'available') card.append(node('p', 'GitHub evidence could not be collected. Activity and backlog are unknown; this does not mean the repository is idle.', 'notice'));
    else {
      const other = uniqueIssues(repo).filter(issue => !humanWait(issue));
      card.append(node('h4', 'Other collected source labels'), other.length ? list(other.map(issue => issueCard(repo, issue))) : node('p', 'No other attention labels in the collected issues.', 'empty'));
      if (!repo.attention.length) card.append(node('p', 'No attention labels in the collected issues.', 'empty'));
      if (!repo.runs.length) card.append(node('p', 'No workflow runs returned.', 'empty'));
      for (const [title, predicate, kind] of [['Recent completed runs', run => run.status === 'completed', 'completed'], ['Unclassified source statuses', run => !active(run) && run.status !== 'completed', 'unclassified']]) {
        const details = node('details'), summary = node('summary', title), key = sourceKey(repo.repository, 'history', kind);
        details.dataset.key = key; details.open = disclosures.get(key) || false;
        summary.dataset.key = key; summary.dataset.repository = repo.repository;
        const records = repo.runs.filter(predicate);
        details.append(summary, records.length ? list(records.map(run => runCard(repo, run))) : node('p', 'No matching collected runs.', 'empty')); card.append(details);
      }
    }
    fragment.append(card);
  }
  root.replaceChildren(fragment);
  if (hadFocus) {
    const controls = [...root.querySelectorAll('a[href], summary')];
    const target = controls.find(n => n.dataset.key === key) || controls.find(n => n.dataset.key === sourceKey(repository, 'actions', '')) || filter;
    if (target.matches('a') && target.closest('details')) target.closest('details').open = true;
    target.focus({ preventScroll: true });
  }
}
async function refresh() {
  if (busy) return;
  const refreshFocused = document.activeElement === $('refresh');
  busy = true; $('refresh').disabled = true; $('repositories').setAttribute('aria-busy', 'true');
  try {
    const response = await fetch('/api/observations', { signal: AbortSignal.timeout(35000) });
    if ([401, 403, 409].includes(response.status)) {
      const heldFocus = $('repositories').contains(document.activeElement);
      snapshot = undefined;
      $('repository-filter').replaceChildren(Object.assign(node('option', 'All repositories'), { value: '' }));
      for (const id of ['repo-count', 'active-count', 'attention-count', 'unknown-count']) $(id).textContent = '—';
      $('count-scope').textContent = 'No authorized observation is available.';
      $('connection').textContent = response.status === 409 ? 'Connect or verify your GitHub account to view observations.'
        : response.status === 401 ? 'Your owner session has ended. Sign in again to view observations.' : 'Access to this workspace is denied.';
      const account = node('a', 'Open account'); account.href = '/workspace';
      $('repositories').replaceChildren(account);
      if (heldFocus) account.focus({ preventScroll: true });
      return;
    }
    if (!response.ok) throw Error();
    const data = await response.json();
    if (!Array.isArray(data.repositories) || data.repositories.some(r => typeof r.repository !== 'string' || (r.availability === 'available' && (!Array.isArray(r.runs) || !Array.isArray(r.attention))))) throw Error();
    render(data); snapshot = data;
    $('connection').textContent = `Observation received ${new Date().toISOString()}. Refreshes every 30 seconds; source observation times are shown below.`;
  } catch {
    $('connection').textContent = snapshot ? 'Refresh failed. Previously shown observations may be stale. Try refreshing again.' : 'Refresh failed. No observation is available. Try refreshing again.';
  } finally {
    busy = false; $('refresh').disabled = false; $('repositories').setAttribute('aria-busy', 'false');
    if (refreshFocused && [document.body, $('refresh')].includes(document.activeElement)) $('refresh').focus({ preventScroll: true });
  }
}
$('refresh').addEventListener('click', refresh);
$('repository-filter').addEventListener('change', () => { if (snapshot) render(snapshot); });
$('appearance').addEventListener('change', event => { document.documentElement.dataset.lhTheme = event.target.value; });
if (location.pathname === '/workspace/operations') {
  const account = node('a', 'Account'); account.href = '/workspace';
  document.querySelector('.topbar').append(account);
}
refresh();
setInterval(() => { if (!document.hidden) refresh(); }, 30000);
