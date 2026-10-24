#!/usr/bin/env node
import http from 'node:http';
import { readFile, writeFile, rm } from 'node:fs/promises';
import { createHash } from 'node:crypto';

const socketPath = process.env.SOFA_SOCKET ?? '/run/sofa/bridge.sock';
const hash = (value) => createHash('sha256').update(value).digest('hex');
function call(method, path, body) {
  return new Promise((resolve, reject) => {
    const request = http.request({ socketPath, method, path, headers: { 'content-type': 'application/json' }, timeout: 15000 }, (response) => {
      let data = '';
      response.setEncoding('utf8');
      response.on('data', (chunk) => { data += chunk; if (data.length > 262144) request.destroy(new Error('Host response too large')); });
      response.on('end', () => { try { const value = data ? JSON.parse(data) : {}; if (response.statusCode >= 400) reject(new Error(value.error ?? `Host HTTP ${response.statusCode}`)); else resolve(value); } catch (error) { reject(error); } });
    });
    request.on('timeout', () => request.destroy(new Error('Host request timed out')));
    request.on('error', reject);
    request.end(body ? JSON.stringify(body) : undefined);
  });
}

const command = process.argv[2];
if (command === 'status') console.log(JSON.stringify(await call('GET', `/runs/${process.argv[3]}`)));
else if (command === 'cancel') console.log(JSON.stringify(await call('POST', `/runs/${process.argv[3]}/cancel`)));
else if (command === 'github-event') {
  const receiptFile = process.env.LIGHTHOUSE_RECEIPT ?? 'lighthouse-host-result.json';
  await rm(receiptFile, { force: true });
  const policy = await call('GET', '/policy');
  const event = JSON.parse(await readFile(process.env.GITHUB_EVENT_PATH, 'utf8'));
  if (process.env.GITHUB_REPOSITORY !== policy.repository || process.env.GITHUB_ACTOR !== policy.owner || event.issue?.pull_request) throw new Error('Unauthorized pilot event');
  const issue = Number(event.issue?.number ?? process.env.LIGHTHOUSE_ISSUE);
  const comment = event.comment;
  const stage = comment?.body?.startsWith('/lh:refine ') ? 'refine' : 'triage';
  if (comment && (comment.user?.login !== policy.owner || !(comment.body === '/lh:triage' || /^\/lh:refine [a-f0-9]{64}$/.test(comment.body)))) throw new Error('Unrelated comment does not resume a run');
  let predecessor = null;
  if (stage === 'refine') {
    const prior = await call('GET', `/runs/${comment.body.split(' ')[1]}`);
    if (!prior.result) throw new Error('Missing predecessor result');
    predecessor = { runId: prior.id, resultDigest: hash(prior.result) };
  }
  const identity = `${policy.repository}:${issue}:${stage}:${comment?.id ?? event.issue?.created_at ?? process.env.GITHUB_RUN_ID}`;
  const runId = hash(identity);
  // GitHub retries recover the previously persisted request, including its deadline.
  let run;
  try { run = await call('GET', `/runs/${runId}`); } catch (error) { if (!error.message.includes('404')) throw error; }
  if (!run) run = await call('POST', '/runs', { schemaVersion: 1, runId, repository: policy.repository, issue, stage, route: 'supervised', sourceRevision: process.env.GITHUB_SHA, workflowRevision: process.env.LIGHTHOUSE_WORKFLOW_REVISION, workflowRunId: Number(process.env.GITHUB_RUN_ID), packageVersion: policy.packageVersion, policyDigest: policy.policyDigest, deadline: new Date(Date.now() + (policy.maxSeconds - 5) * 1000).toISOString(), writeScope: [], predecessor, approvalCommentId: comment?.id && stage === 'refine' ? comment.id : null });
  const cancel = () => { call('POST', `/runs/${runId}/cancel`).finally(() => process.exit(130)); };
  process.once('SIGTERM', cancel); process.once('SIGINT', cancel);
  const end = Date.now() + (policy.maxSeconds + 30) * 1000;
  while (['admitted', 'running', 'publishing', 'cancelling'].includes(run.state)) {
    if (Date.now() >= end) { await call('POST', `/runs/${runId}/cancel`); throw new Error('Bridge wait exceeded its bound'); }
    await new Promise((resolve) => setTimeout(resolve, 2000));
    run = await call('GET', `/runs/${runId}`);
  }
  await writeFile(receiptFile, JSON.stringify(run, null, 2) + '\n');
  console.log(JSON.stringify({ runId, state: run.state, receipt: run.receipt, error: run.error }));
  if (run.state !== 'waiting-human') process.exitCode = 1;
} else throw new Error('Usage: lighthouse-host-bridge <github-event|status id|cancel id>');
