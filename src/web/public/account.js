const status = document.getElementById('account-status');
const identity = document.getElementById('github-account');
const repositories = document.getElementById('repository-authority');
let busy = false;
async function refresh() {
  if (busy || document.hidden) return;
  busy = true;
  try {
    const response = await fetch('/api/connections/github', { signal: AbortSignal.timeout(35000) });
    if ([401, 403].includes(response.status)) {
      identity.textContent = 'No verified acting account is available.';
      repositories.replaceChildren();
      status.textContent = 'Your owner session has ended or access is denied. Sign in again to continue.';
      return;
    }
    if (!response.ok) throw Error();
    const account = await response.json();
    if (account.state !== 'linked') {
      identity.textContent = account.state === 'unconfigured' ? 'GitHub connection is not configured.'
        : account.state === 'unlinked' ? 'GitHub is not connected.' : 'GitHub verification is unavailable.';
      repositories.replaceChildren();
      status.textContent = 'Source operations require a verified acting account and sufficient repository permissions.';
      return;
    }
    if (typeof account.login !== 'string' || !Number.isSafeInteger(account.userId) || !Array.isArray(account.repositories)) throw Error();
    identity.textContent = `GitHub acting account: ${account.login} · User ID ${account.userId}.`;
    const fragment = document.createDocumentFragment();
    for (const repository of account.repositories) {
      const item = document.createElement('li');
      const checked = Number.isFinite(Date.parse(repository.checkedAt)) ? new Date(repository.checkedAt).toISOString() : 'unavailable';
      item.textContent = `${repository.repository}: ${repository.availability === 'available'
        ? `issues read ${repository.issuesRead === true ? 'allowed' : 'denied'}; Actions read ${repository.actionsRead === true ? 'allowed' : 'denied'}; issue write permission ${repository.issuesWrite === true ? 'confirmed' : 'not confirmed'}`
        : 'authority ' + (repository.availability === 'denied' ? 'denied' : 'unavailable')}. Checked: ${checked}.`;
      fragment.append(item);
    }
    repositories.replaceChildren(fragment);
    const verified = Number.isFinite(Date.parse(account.verifiedAt)) ? new Date(account.verifiedAt).toISOString() : 'unavailable';
    status.textContent = `GitHub identity last verified: ${verified}. Account status refreshes every four minutes while visible. These permission observations do not grant command eligibility.`;
  } catch {
    repositories.replaceChildren();
    status.textContent = 'Current GitHub authority is unavailable. Previously displayed identity may be stale; source operations require a new successful check.';
  } finally { busy = false; }
}
refresh();
setInterval(refresh, 240000);
document.addEventListener('visibilitychange', () => { if (!document.hidden) refresh(); });
