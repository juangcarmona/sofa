#!/usr/bin/env node
import { readFileSync } from 'node:fs';
import { execFileSync } from 'node:child_process';

const mode = process.argv[2];
const branchPattern = new RegExp("^(feat|fix|docs|refactor|test|chore|build|ci|perf|revert)/[1-9]\\d*-[a-z0-9]+(?:-[a-z0-9]+)*$", 'u');
function fail(message) { console.error(message); process.exit(1); }
function branch() {
  try { return execFileSync('git', ['branch', '--show-current'], { encoding: 'utf8' }).trim(); }
  catch { return ''; }
}
if (mode === 'commit-msg') {
  const file = process.argv[3];
  if (!file) fail('Usage: lighthouse-guard commit-msg <message-file>');
  const message = readFileSync(file, 'utf8').split('\n').find((line) => line.trim() && !line.startsWith('#'))?.trim() ?? '';
  if (!/^(?:feat|fix|docs|refactor|test|chore|build|ci|perf|revert)(?:\([^)]+\))?!?: .+/u.test(message)) fail('Commit message must follow Conventional Commits.');
} else if (mode === 'branch' || mode === 'verify') {
  const current = branch();
  if (current && !['main', 'master'].includes(current) && !branchPattern.test(current)) fail('Branch ' + current + ' does not match ' + branchPattern.source + '.');
} else fail('Usage: lighthouse-guard <commit-msg|branch|verify>');
