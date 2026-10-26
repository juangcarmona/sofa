const $ = id => document.getElementById(id);
const node = (tag, text, className) => { const n = document.createElement(tag); if (text !== undefined) n.textContent = text; if (className) n.className = className; return n; };
const active = run => ['queued', 'in_progress', 'waiting', 'pending', 'requested'].includes(run.status);
function link(text, href, repository, kind) {
  const n = node('a', text);
  try {
    const u = new URL(href), prefix = `/${repository}/`;
    if (u.origin !== 'https://github.com' || !u.pathname.startsWith(prefix) || (kind && !u.pathname.startsWith(prefix + kind))) throw Error();
    n.href = u.href; n.rel = 'noreferrer';
  } catch { n.removeAttribute('href'); }
  return n;
}
function badge(text, tone = '') { return node('span', text, `badge ${tone}`); }
function relativeDate(value) {
  const date = new Date(value);
  return Number.isNaN(date.valueOf()) ? 'Time unavailable' : date.toLocaleString(undefined, { dateStyle: 'medium', timeStyle: 'short' });
}
function render(data) {
  const focused = document.activeElement;
  const focusedRepository = $('repositories').contains(focused) ? focused.closest('.repository')?.dataset.repository : undefined;
  const focusedHref = focusedRepository ? focused.getAttribute('href') : undefined;
  const rows = data.repositories, available = rows.filter(r => r.availability === 'available');
  $('repo-count').textContent = rows.length;
  $('active-count').textContent = available.reduce((n, r) => n + r.runs.filter(active).length, 0);
  $('attention-count').textContent = available.reduce((n, r) => n + r.attention.length, 0);
  $('unknown-count').textContent = rows.length - available.length;
  const fragment = document.createDocumentFragment();
  for (const repo of rows) {
    const card = node('article', undefined, 'repository');
    card.dataset.repository = repo.repository;
    const heading = node('div', undefined, 'repo-heading'), title = node('div');
    title.append(node('p', repo.repository.split('/')[0], 'owner'), node('h3', repo.repository.split('/')[1]));
    heading.append(title, badge(repo.availability === 'available' ? 'Observed' : 'Evidence unavailable', repo.availability === 'available' ? 'positive' : 'caution'));
    card.append(heading, node('p', `Observed ${relativeDate(repo.observedAt)}`, 'timestamp'));
    const actions = node('nav', undefined, 'repo-actions'); actions.setAttribute('aria-label', `${repo.repository} actions`);
    actions.append(link('Open workflow actions ↗', `https://github.com/${repo.repository}/actions/workflows/lighthouse-sdlc-router.yml`, repo.repository, 'actions/'), link('Issues ↗', `https://github.com/${repo.repository}/issues`, repo.repository));
    card.append(actions);
    if (repo.availability !== 'available') card.append(node('p', 'GitHub evidence could not be collected. Activity and backlog are unknown; this does not mean the repository is idle.', 'notice'));
    else {
      const columns = node('div', undefined, 'repo-columns'), attention = node('section'), runs = node('section');
      attention.append(node('h4', 'Needs attention'), node('p', `${repo.attention.length} items in this observation`, 'muted'));
      const issues = node('ul', undefined, 'items');
      for (const issue of repo.attention) {
        const li = node('li'); li.append(link(`#${issue.number} ${issue.title}`, issue.url, repo.repository, 'issues/'));
        const labels = node('div', undefined, 'labels');
        for (const label of issue.labels) labels.append(badge(label, label === 'agent-working' ? 'activity' : ['waiting-review', 'needs-info'].includes(label) ? 'caution' : ''));
        li.append(labels); issues.append(li);
      }
      attention.append(repo.attention.length ? issues : node('p', 'No attention labels in the collected issues.', 'empty'));
      runs.append(node('h4', 'Latest executions'), node('p', `${repo.runs.filter(active).length} active in the collected runs`, 'muted'));
      const runList = node('ul', undefined, 'items runs');
      for (const run of repo.runs.slice(0, 8)) {
        const li = node('li'), row = node('div', undefined, 'run-heading');
        row.append(link(run.name || `Run ${run.id}`, run.url, repo.repository, 'actions/runs/'), badge(run.conclusion || run.status || 'unknown', active(run) ? 'activity' : run.conclusion === 'failure' ? 'negative' : ''));
        li.append(row, node('p', `${(run.sourceRevision || 'unknown').slice(0, 7)} · ${relativeDate(run.updatedAt)}`, 'timestamp')); runList.append(li);
      }
      runs.append(repo.runs.length ? runList : node('p', 'No workflow runs returned.', 'empty'));
      columns.append(attention, runs); card.append(columns, node('p', 'Showing up to 8 of the latest 20 runs and attention labels from the first 100 open issue records. This is a partial observation, not a delivery audit.', 'scope'));
    }
    fragment.append(card);
  }
  $('repositories').replaceChildren(fragment);
  if (focusedRepository) {
    const card = [...$('repositories').children].find(n => n.dataset.repository === focusedRepository);
    const links = [...(card?.querySelectorAll('a[href]') || [])];
    (links.find(n => n.getAttribute('href') === focusedHref) || links[0] || $('refresh')).focus({ preventScroll: true });
  }
}
let busy = false;
async function refresh() {
  if (busy) return;
  busy = true; $('refresh').disabled = true; $('repositories').setAttribute('aria-busy', 'true');
  try {
    const response = await fetch('/api/observations', { signal: AbortSignal.timeout(35000) });
    if (!response.ok) throw Error();
    const data = await response.json(); render(data);
    $('connection').textContent = `Observation received ${relativeDate(new Date())}. Refreshes every 30 seconds; source timestamps are shown below.`;
  } catch {
    $('connection').textContent = 'Refresh failed. Previously shown observations may be stale. Try refreshing again.';
  } finally { busy = false; $('refresh').disabled = false; $('repositories').setAttribute('aria-busy', 'false'); }
}
$('refresh').addEventListener('click', refresh);
$('appearance').addEventListener('change', event => { document.documentElement.dataset.lhTheme = event.target.value; });
refresh();
setInterval(() => { if (!document.hidden) refresh(); }, 30000);
