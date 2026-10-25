import { execFile } from 'node:child_process';
import { promisify } from 'node:util';
import { fileURLToPath } from 'node:url';
const exec = promisify(execFile);
export async function observeRepositories(repositories, get = async path => JSON.parse((await exec('gh', ['api', path], { timeout: 30000, maxBuffer: 2 * 1024 * 1024 })).stdout)) {
  return Promise.all(repositories.map(async repository => {
    if (!/^[A-Za-z0-9][A-Za-z0-9-]*\/[A-Za-z0-9][A-Za-z0-9_.-]*$/.test(repository)) throw Error('Expected owner/repository');
    const observedAt = new Date().toISOString();
    try {
      const [runs, issues] = await Promise.all([get(`repos/${repository}/actions/runs?per_page=20`), get(`repos/${repository}/issues?state=open&per_page=100`)]);
      return { repository, observedAt, availability: 'available', scope: 'Latest 20 workflow runs and first 100 open issue records; not a complete delivery audit', runs: runs.workflow_runs.map(run => ({ id: run.id, name: run.name, status: run.status, conclusion: run.conclusion, sourceRevision: run.head_sha, url: run.html_url, updatedAt: run.updated_at })), attention: issues.filter(issue => !issue.pull_request && issue.labels.some(label => ['needs-info', 'agent-working', 'ready'].includes(label.name))).map(issue => ({ number: issue.number, title: issue.title, labels: issue.labels.map(label => label.name), url: issue.html_url, updatedAt: issue.updated_at })) };
    } catch { return { repository, observedAt, availability: 'unavailable', reason: 'GitHub evidence could not be collected; no delivery or health conclusion is available' }; }
  }));
}
if (process.argv[1] === fileURLToPath(import.meta.url)) {
  const repositories = process.argv.slice(2);
  if (!repositories.length) throw Error('Usage: npm run observe -- owner/repository [...]');
  console.log(JSON.stringify(await observeRepositories(repositories), null, 2));
}
