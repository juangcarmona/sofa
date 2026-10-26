import test from 'node:test';
import assert from 'node:assert/strict';
import { observeRepositories } from '../src/observation/github.mjs';
test('observation preserves source facts and never dispatches work', async () => {
  const paths = [];
  const result = await observeRepositories(['owner/repo'], async path => {
    paths.push(path);
    return path.includes('/actions/runs') ? { workflow_runs: [{ id: 4, status: 'completed', conclusion: 'success', head_sha: 'abc', html_url: 'run-url' }] } : [{ number: 18, labels: [{ name: 'ready' }], html_url: 'issue-url' }];
  });
  assert.equal(result[0].runs[0].sourceRevision, 'abc');
  assert.equal(result[0].attention[0].number, 18);
  assert.equal(paths.length, 2);
  assert(paths.every(path => path.startsWith('repos/owner/repo/')));
  assert.equal(result[0].availability, 'available');
});
test('unavailable evidence stays unknown and invalid repository input is rejected', async () => {
  const [result] = await observeRepositories(['owner/repo'], async () => { throw Error('denied'); });
  assert.equal(result.availability, 'unavailable'); assert.equal(result.runs, undefined);
  await assert.rejects(observeRepositories(['../repo']), /owner\/repository/);
});

test('review waits remain visible without claiming agent work or including PR records', async () => {
  const issues = [
    { number: 40, labels: [{ name: 'waiting-review' }, { name: 'bug' }] },
    { number: 41, labels: [{ name: 'waiting-review' }], pull_request: { url: 'pr-url' } },
    { number: 42, labels: [{ name: 'bug' }] },
    { number: 43, labels: [{ name: 'agent-working' }] },
    { number: 44, labels: [{ name: 'needs-info' }] },
  ];
  const [result] = await observeRepositories(['owner/repo'], async path => path.includes('/actions/runs') ? { workflow_runs: [] } : issues);
  assert.deepEqual(result.attention.map(issue => issue.number), [40, 43, 44]);
  assert.deepEqual(result.attention[0].labels, ['waiting-review', 'bug']);
});
