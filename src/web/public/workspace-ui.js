const namespace = 'http://www.w3.org/2000/svg';
const paths = {
  light: 'M12 8a4 4 0 1 0 0 8 4 4 0 0 0 0-8ZM12 2v2m0 16v2M2 12h2m16 0h2M5 5l1.5 1.5m11 11L19 19M5 19l1.5-1.5m11-11L19 5',
  dark: 'M20 15.5A8 8 0 0 1 8.5 4 8 8 0 1 0 20 15.5Z',
  system: 'M3 4h18v13H3ZM8 21h8m-4-4v4',
  operations: 'M3 3h7v7H3Zm11 0h7v7h-7ZM3 14h7v7H3Zm11 0h7v7h-7Z',
  factory: 'M3 7h7l2 2h9v11H3ZM3 7V4h7l2 3',
  attention: 'M12 3 2 21h20ZM12 9v5m0 3v1',
  issues: 'M21 12a9 9 0 1 1-18 0 9 9 0 0 1 18 0ZM12 7v6m0 3v1',
  ideas: 'M9 18h6m-5 3h4M8 14a6 6 0 1 1 8 0l-1 2H9Z',
  runtime: 'M3 3h18v7H3Zm0 11h18v7H3ZM6 6h1m-1 11h1',
  health: 'M3 12h4l3-8 4 16 3-8h4',
  account: 'M16 7a4 4 0 1 1-8 0 4 4 0 0 1 8 0ZM4 21v-3a8 8 0 0 1 16 0v3',
  refresh: 'M20 7v5h-5M4 17v-5h5M6 7a7 7 0 0 1 12-1l2 6M4 12l2 6a7 7 0 0 0 12-1',
  collapse: 'M3 4h18v16H3ZM8 4v16m8-12-4 4 4 4',
};
function icon(name) {
  const svg = document.createElementNS(namespace, 'svg');
  for (const [key, value] of Object.entries({ viewBox: '0 0 24 24', 'aria-hidden': 'true', fill: 'none', stroke: 'currentColor', 'stroke-width': '1.6', 'stroke-linecap': 'round', 'stroke-linejoin': 'round' })) svg.setAttribute(key, value);
  const path = document.createElementNS(namespace, 'path'); path.setAttribute('d', paths[name]); svg.append(path); return svg;
}
const preferenceKey = 'lcc:appearance';
let preference = 'system';
try { const saved = localStorage.getItem(preferenceKey); if (['light', 'dark', 'system'].includes(saved)) preference = saved; } catch { /* Browser storage can be disabled. */ }
function applyTheme(value) {
  preference = value; document.documentElement.dataset.lhTheme = value;
  document.querySelectorAll('[data-theme-choice]').forEach(button => button.setAttribute('aria-pressed', String(button.dataset.themeChoice === value)));
}
applyTheme(preference);
let ownerName = '', accountState, ownerEpoch = 0;
let sourceIdentityGeneration = 0;
let collapsed = false;
try { collapsed = localStorage.getItem('lcc:navigation') === 'collapsed'; } catch { /* Optional preference. */ }
document.documentElement.dataset.navCollapsed = String(collapsed);
function renderIdentity() {
  for (const link of document.querySelectorAll('.sidebar-identity,.header-identity')) {
    const linked = accountState?.state === 'linked' && typeof accountState.login === 'string';
    const name = linked ? accountState.login : ownerName || 'Account';
    link.querySelector('strong').textContent = name;
    const detail = linked ? 'GitHub connected' : accountState?.state === 'unlinked' ? 'Connect GitHub' : ownerName ? 'Signed in' : 'Account and connection';
    link.querySelector('small').textContent = detail;
    link.setAttribute('aria-label', name + ' ' + detail + ' · Account'); link.title = 'Account · ' + name;
  }
}
export function updateWorkspaceAccount(account) {
  if (account.state === 'expired') { ownerEpoch++; ownerName = ''; }
  if (['linked', 'unlinked', 'unconfigured', 'unavailable', 'expired'].includes(account.state)) sourceIdentityGeneration++;
  accountState = account; renderIdentity();
}
function validActorId(value) { return Number.isSafeInteger(value) && value > 0; }
export function workspaceSourceIdentityGeneration() { return sourceIdentityGeneration; }
export function validateWorkspaceSourceIdentity(data, expectedScope, expectedActorId, requestGeneration) {
  if (requestGeneration !== sourceIdentityGeneration) return false;
  if (!data || typeof data !== 'object') return true;
  const accountScope = accountState?.state === 'linked' ? accountState.dataScope : undefined;
  const accountActorId = accountState?.state === 'linked' ? accountState.userId : undefined;
  const scope = expectedScope ?? accountScope;
  const actorId = validActorId(expectedActorId) ? expectedActorId : accountActorId;
  const scopeChanged = typeof scope === 'string' && scope.length > 0
    && typeof data.dataScope === 'string' && data.dataScope.length > 0 && data.dataScope !== scope;
  const reportedIds = [data.actingUserId, data.facts?.actor?.id].filter(validActorId);
  const actorConflict = reportedIds.some(value => value !== reportedIds[0])
    || validActorId(actorId) && reportedIds.some(value => value !== actorId);
  if (data.availability === 'disconnected' || scopeChanged || actorConflict) {
    sourceIdentityGeneration++;
    accountState = { state: 'unavailable' }; renderIdentity();
    return false;
  }
  return true;
}
export function projectWorkspaceSourceIdentity(account, requestGeneration) {
  if (requestGeneration !== sourceIdentityGeneration) return false;
  accountState = account; renderIdentity(); return true;
}
function identityLink(className) {
  const link = document.createElement('a'); link.href = '/workspace'; link.className = className; link.append(icon('account'));
  const copy = document.createElement('span'); copy.className = 'identity-copy';
  copy.append(document.createElement('strong'), document.createTextNode(' '), document.createElement('small')); link.append(copy); return link;
}
function enhance() {
  document.querySelectorAll('.workspace-nav').forEach(navigation => {
    if (!navigation.querySelector('a[href="/workspace/health"]') && navigation.childElementCount) {
      const link = document.createElement('a'); link.href = '/workspace/health'; link.textContent = 'Health';
      navigation.insertBefore(link, navigation.querySelector('a[href="/workspace/runtime"]'));
    }
  });
  let changedNavigation = false;
  document.querySelectorAll('.workspace-nav a').forEach(link => {
    if (link.querySelector('svg')) return;
    const route = link.getAttribute('href'), name = route === '/workspace' ? 'account' : route?.split('/')[2];
    const label = link.textContent;
    link.setAttribute('aria-label', label); link.title = label;
    const text = document.createElement('span'); text.className = 'nav-label'; text.textContent = label; link.replaceChildren(text);
    if (paths[name === 'repositories' ? 'factory' : name]) { link.prepend(icon(name === 'repositories' ? 'factory' : name)); changedNavigation = true; }
  });
  if (changedNavigation) document.querySelector('.workspace-nav [aria-current="page"]')?.scrollIntoView({ block: 'nearest', inline: 'nearest' });
  document.querySelectorAll('.topbar').forEach(header => {
    let actions = header.querySelector('.header-actions');
    if (!actions) { actions = document.createElement('div'); actions.className = 'header-actions'; header.append(actions); }
    if (!header.querySelector('.header-identity')) { header.insertBefore(identityLink('header-identity'), actions); renderIdentity(); }
    const refresh = document.querySelector('#refresh, #factory-refresh, #issue-refresh, #idea-refresh');
    if (refresh && !refresh.dataset.headerRefresh) {
      refresh.dataset.headerRefresh = 'true'; refresh.setAttribute('aria-label', refresh.textContent); refresh.title = refresh.textContent;
      refresh.classList.add('header-refresh'); refresh.replaceChildren(icon('refresh')); actions.prepend(refresh);
    }
    if (header.querySelector('.theme-switcher')) return;
    const group = document.createElement('div'); group.className = 'theme-switcher'; group.setAttribute('role', 'group'); group.setAttribute('aria-label', 'Appearance');
    for (const value of ['light', 'dark', 'system']) {
      const button = document.createElement('button'); button.type = 'button'; button.className = 'theme-option'; button.dataset.themeChoice = value;
      const label = value[0].toUpperCase() + value.slice(1); button.title = label; button.setAttribute('aria-label', label + ' appearance');
      button.setAttribute('aria-pressed', String(value === preference)); button.append(icon(value));
      button.addEventListener('click', () => { applyTheme(value); try { localStorage.setItem(preferenceKey, value); } catch { /* The current page still changes. */ } });
      group.append(button);
    }
    header.querySelector('.appearance')?.remove(); actions.append(group);
  });
  document.querySelectorAll('.site-sidebar').forEach(sidebar => {
    if (!sidebar.querySelector('.sidebar-toggle')) {
      const toggle = document.createElement('button'); toggle.type = 'button'; toggle.className = 'sidebar-toggle'; toggle.append(icon('collapse'));
      const state = () => { toggle.setAttribute('aria-expanded', String(!collapsed)); toggle.setAttribute('aria-label', collapsed ? 'Expand navigation' : 'Collapse navigation'); };
      state(); toggle.addEventListener('click', () => { collapsed = !collapsed; document.documentElement.dataset.navCollapsed = String(collapsed); state(); try { localStorage.setItem('lcc:navigation', collapsed ? 'collapsed' : 'expanded'); } catch { /* Optional preference. */ } });
      sidebar.append(toggle);
    }
    if (!sidebar.querySelector('.sidebar-identity')) {
      sidebar.append(identityLink('sidebar-identity')); renderIdentity();
    }
  });
}
const favicon = document.createElement('link'); favicon.rel = 'icon'; favicon.type = 'image/svg+xml'; favicon.href = '/lighthouse.svg';
document.head.querySelectorAll('link[rel="icon"]').forEach(link => link.remove()); document.head.append(favicon);
enhance();
new MutationObserver(enhance).observe(document.body, { childList: true, subtree: true });
window.addEventListener('storage', event => { if (event.key === preferenceKey) applyTheme(['light', 'dark', 'system'].includes(event.newValue) ? event.newValue : 'system'); });
const sessionEpoch = ownerEpoch;
fetch('/api/session', { cache: 'no-store', signal: AbortSignal.timeout(10000) }).then(async response => {
  if (!response.ok) return;
  const session = await response.json();
  if (sessionEpoch === ownerEpoch && session.authenticated === true && typeof session.displayName === 'string') { ownerName = session.displayName; renderIdentity(); }
}).catch(() => {});
