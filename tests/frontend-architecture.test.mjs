import assert from 'node:assert/strict';
import { readdirSync, readFileSync, realpathSync, statSync } from 'node:fs';
import { mkdtemp, mkdir, symlink, writeFile, rm } from 'node:fs/promises';
import path from 'node:path';
import test from 'node:test';
import { tmpdir } from 'node:os';
import { parse } from 'acorn';

const repositoryRoot = path.resolve(import.meta.dirname, '..');
const publicRoot = path.join(repositoryRoot, 'src/web/public');

const expectedImports = new Map(Object.entries({
  'account.js': ['workspace-ui.js'],
  'app.js': ['read-projection-cache.js', 'workspace-ui.js'],
  'attention-controls.js': ['issue-actions.js'],
  'attention.js': [],
  'factory.js': ['attention-controls.js', 'attention.js', 'read-projection-cache.js', 'workspace-ui.js'],
  'health.js': ['workspace-ui.js'],
  'issue-actions.js': ['workspace-ui.js'],
  'issue.js': ['issue-actions.js', 'markdown.js', 'workspace-ui.js'],
  'ideas.js': ['workspace-ui.js'],
  'markdown.js': [],
  'read-projection-cache.js': [],
  'runtime.js': ['workspace-ui.js'],
  'workspace-ui.js': [],
}));

function parseModule(filename, source) {
  return parse(source, { ecmaVersion: 'latest', sourceType: 'module' });
}

function walk(node, visit, parent = null) {
  if (!node || typeof node !== 'object') return;
  if (typeof node.type === 'string') visit(node, parent);
  for (const [key, value] of Object.entries(node)) {
    if (key === 'parent') continue;
    if (Array.isArray(value)) {
      for (const child of value) walk(child, visit, node);
    } else if (value && typeof value === 'object' && typeof value.type === 'string') {
      walk(value, visit, node);
    }
  }
}

function moduleImports(ast) {
  const imports = [];
  const dynamicImports = [];
  walk(ast, node => {
    if ((node.type === 'ImportDeclaration' || node.type === 'ExportNamedDeclaration' || node.type === 'ExportAllDeclaration') && node.source) {
      imports.push(node.source.value);
    }
    if (node.type === 'ImportExpression' || (node.type === 'CallExpression' && node.callee.type === 'Import')) dynamicImports.push(node);
    if (node.type === 'CallExpression' && node.callee.type === 'Identifier' && node.callee.name === 'require') dynamicImports.push(node);
  });
  return { imports, dynamicImports };
}

function inside(parent, child) {
  const relative = path.relative(parent, child);
  return relative !== '' && relative !== '..' && !relative.startsWith('..' + path.sep) && !path.isAbsolute(relative);
}

function resolveImport(root, from, specifier) {
  if (typeof specifier !== 'string' || !specifier.startsWith('./') || specifier.includes('\\') || specifier.split('/').includes('..')) {
    return { error: `Import from ${from} must be a relative in-tree path: ${String(specifier)}` };
  }
  const candidate = path.resolve(root, path.dirname(from), specifier);
  if (!inside(root, candidate) || path.extname(candidate) !== '.js') {
    return { error: `Import from ${from} escapes the browser module tree or is not a .js module: ${specifier}` };
  }
  try {
    const realRoot = realpathSync(root), realCandidate = realpathSync(candidate);
    if (!inside(realRoot, realCandidate)) return { error: `Import from ${from} resolves through an escaping symlink: ${specifier}` };
    if (!statSync(realCandidate).isFile()) return { error: `Import from ${from} is not a file: ${specifier}` };
    return { path: path.relative(root, candidate).split(path.sep).join('/') };
  } catch {
    return { error: `Import from ${from} does not resolve to a file: ${specifier}` };
  }
}

function sourceFiles(root) {
  const files = new Map(), errors = [];
  function visit(directory) {
    for (const entry of readdirSync(directory, { withFileTypes: true })) {
      const filename = path.join(directory, entry.name);
      if (entry.isSymbolicLink()) {
        if (entry.name.endsWith('.js')) errors.push(`Browser module cannot be a symlink: ${path.relative(root, filename)}`);
        continue;
      }
      if (entry.isDirectory()) visit(filename);
      else if (entry.isFile() && entry.name.endsWith('.js')) {
        const relative = path.relative(root, filename).split(path.sep).join('/');
        files.set(relative, readFileSync(filename, 'utf8'));
      }
    }
  }
  visit(root);
  return { files, errors };
}

function importBoundaryErrors(files, root = publicRoot) {
  const errors = [], parsed = new Map();
  for (const [filename, source] of files) {
    let ast;
    try { ast = parseModule(filename, source); }
    catch (error) { errors.push(`${filename} is not parseable ESM: ${error.message}`); continue; }
    parsed.set(filename, ast);
    const { imports, dynamicImports } = moduleImports(ast);
    for (const item of dynamicImports) errors.push(`${filename} uses dynamic import or CommonJS require at offset ${item.start}`);
    for (const specifier of imports) {
      const resolved = resolveImport(root, filename, specifier);
      if (resolved.error) errors.push(resolved.error);
      else if (!files.has(resolved.path)) errors.push(`Import from ${filename} is outside the declared browser graph: ${specifier}`);
    }
  }
  return { errors, parsed };
}

function dependencyErrors(files, root = publicRoot) {
  const errors = [];
  for (const [name, allowed] of expectedImports) {
    const source = files.get(name);
    if (source === undefined) { errors.push(`Expected browser module is missing: ${name}`); continue; }
    let ast;
    try { ast = parseModule(name, source); }
    catch { continue; }
    const actual = moduleImports(ast).imports.map(specifier => path.basename(specifier)).sort();
    if (JSON.stringify(actual) !== JSON.stringify([...allowed].sort())) {
      errors.push(`${name} dependency ownership changed; expected [${[...allowed].sort().join(', ')}], found [${actual.join(', ')}]`);
    }
  }
  for (const name of files.keys()) if (!expectedImports.has(path.basename(name))) errors.push(`Unreviewed browser module added to the static graph: ${name}`);
  return errors;
}

const transportNames = new Set(['fetch', 'XMLHttpRequest', 'WebSocket', 'EventSource']);
function propertyName(member) {
  const key = member.type === 'Property' ? member.key : member.property;
  if (!key) return null;
  if (!member.computed && key.type === 'Identifier') return key.name;
  if (member.computed && key.type === 'Literal' && typeof key.value === 'string') return key.value;
  return null;
}

function networkReferences(ast) {
  const found = [];
  walk(ast, (node, parent) => {
    if (node.type === 'Identifier' && transportNames.has(node.name)) {
      const isProperty = parent?.type === 'MemberExpression' && parent.property === node && !parent.computed;
      const isObjectKey = parent?.type === 'Property' && parent.key === node && !parent.computed;
      if (!isProperty && !isObjectKey) found.push(node.name);
    }
    if (node.type === 'MemberExpression' && ['window', 'globalThis'].includes(node.object.name)
        && transportNames.has(propertyName(node))) found.push(`${node.object.name}.${propertyName(node)}`);
    if (node.type === 'MemberExpression' && node.object.type === 'Identifier' && node.object.name === 'navigator'
        && propertyName(node) === 'sendBeacon') found.push('navigator.sendBeacon');
    if (node.type === 'ImportExpression' || (node.type === 'CallExpression' && node.callee.type === 'Import')) found.push('dynamic import');
  });
  return [...new Set(found)];
}

function indirectTransportErrors(filename, ast) {
  const errors = [];
  walk(ast, (node, parent) => {
    if (node.type === 'Identifier' && node.name === 'fetch') {
      const isDirectCall = parent?.type === 'CallExpression' && parent.callee === node;
      const isOrdinaryMemberName = parent?.type === 'MemberExpression' && parent.property === node && !parent.computed;
      if (!isDirectCall && !isOrdinaryMemberName) errors.push(`${filename} aliases or passes the global fetch transport instead of calling it directly`);
    }
    if (node.type === 'MemberExpression' && ['window', 'globalThis'].includes(node.object.name)
        && propertyName(node) === 'fetch') {
      const isDirectCall = parent?.type === 'CallExpression' && parent.callee === node;
      if (!isDirectCall) errors.push(`${filename} aliases or passes ${node.object.name}.fetch instead of calling it directly`);
    }
    if ((node.type === 'Identifier' && ['XMLHttpRequest', 'WebSocket', 'EventSource'].includes(node.name))
        || (node.type === 'MemberExpression' && node.object.name === 'navigator' && propertyName(node) === 'sendBeacon')) {
      errors.push(`${filename} uses an unreviewed browser transport`);
    }
  });
  return errors;
}

function browserStorageOwners(ast) {
  const found = new Set();
  walk(ast, node => {
    if (node.type === 'Identifier' && ['localStorage', 'sessionStorage'].includes(node.name)) found.add(node.name);
    if (node.type === 'MemberExpression' && ['window', 'globalThis'].includes(node.object.name)
        && ['localStorage', 'sessionStorage'].includes(propertyName(node))) found.add(propertyName(node));
  });
  return found;
}

function storageOwnershipErrors(filename, ast) {
  const owner = path.basename(filename), errors = [];
  for (const storage of browserStorageOwners(ast)) {
    const allowedOwner = storage === 'sessionStorage' ? 'read-projection-cache.js' : 'workspace-ui.js';
    if (owner !== allowedOwner) errors.push(`${owner} accesses ${storage}; storage owner is ${allowedOwner}`);
  }
  return errors;
}

function stringFragments(node) {
  const fragments = [];
  walk(node, current => {
    if (current.type === 'Literal' && typeof current.value === 'string') fragments.push(current.value);
    if (current.type === 'TemplateElement') fragments.push(current.value.cooked ?? current.value.raw);
  });
  return fragments;
}

function fetchMethod(init) {
  if (!init) return 'GET';
  if (init.type !== 'ObjectExpression') return 'unknown';
  const method = init.properties.find(property => property.type === 'Property' && propertyName(property) === 'method');
  if (!method && init.properties.some(property => property.type === 'SpreadElement')) return 'unknown';
  if (!method) return 'GET';
  if (method.value.type === 'Literal' && typeof method.value.value === 'string') return method.value.value.toUpperCase();
  return 'unknown';
}

function isFetchCall(node) {
  if (node.type !== 'CallExpression') return false;
  if (node.callee.type === 'Identifier') return node.callee.name === 'fetch';
  return node.callee.type === 'MemberExpression' && ['window', 'globalThis'].includes(node.callee.object.name)
    && propertyName(node.callee) === 'fetch';
}

function issueControlWriteErrors(parsedModules) {
  const errors = [];
  let declaredOwnerPosts = 0;
  for (const [filename, ast] of parsedModules) {
    walk(ast, node => {
      if (!isFetchCall(node) || !node.arguments[0]) return;
      const fragments = stringFragments(node.arguments[0]);
      if (!fragments.some(value => value.includes('/api/issues/')) || !fragments.some(value => value.includes('/controls'))) return;
      const method = fetchMethod(node.arguments[1]);
      if (method === 'GET') return;
      if (filename !== 'issue-actions.js') errors.push(`${filename} owns a non-GET issue-controls request (${method})`);
      else if (method !== 'POST') errors.push(`issue-actions.js issue-controls mutation must use its reviewed POST transport, found ${method}`);
      else declaredOwnerPosts++;
    });
  }
  if (parsedModules.has('issue-actions.js') && declaredOwnerPosts !== 1) errors.push(`Expected one issue-controls POST transport in issue-actions.js, found ${declaredOwnerPosts}`);
  return errors;
}

function architectureReport(root = publicRoot) {
  const { files, errors: collectionErrors } = sourceFiles(root);
  const imported = importBoundaryErrors(files, root);
  const errors = [
    ...collectionErrors,
    ...imported.errors,
    ...dependencyErrors(files, root),
  ];
  for (const [filename, ast] of imported.parsed) {
    const owner = path.basename(filename);
    errors.push(...indirectTransportErrors(owner, ast));
    errors.push(...storageOwnershipErrors(owner, ast));
    if (owner === 'read-projection-cache.js') {
      const network = networkReferences(ast);
      if (network.length) errors.push(`read-projection-cache.js contains browser network/import transport: ${network.join(', ')}`);
      if (moduleImports(ast).imports.length) errors.push('read-projection-cache.js must not import page or feature modules');
    }
  }
  errors.push(...issueControlWriteErrors(imported.parsed));
  return errors;
}

test('the authored browser ESM graph follows reviewed frontend ownership', () => {
  assert.deepEqual(architectureReport(), []);
});

test('the graph checker rejects feature coupling and unresolved local imports', () => {
  const imports = new Map([
    ['app.js', "import './issue-actions.js';"],
    ['issue-actions.js', ''],
  ]);
  assert.match(dependencyErrors(imports).join('\n'), /app\.js dependency ownership changed/);

  const unresolved = new Map([['app.js', "import './missing.js';"]]);
  assert.match(importBoundaryErrors(unresolved).errors.join('\n'), /does not resolve to a file/);
});

test('the graph checker rejects dynamic, external, traversal and escaping-symlink imports', async t => {
  const dynamic = importBoundaryErrors(new Map([['app.js', "await import('./late.js');"]]));
  assert.match(dynamic.errors.join('\n'), /dynamic import/);

  const external = importBoundaryErrors(new Map([['app.js', "import 'https://example.invalid/module.js';"]]));
  assert.match(external.errors.join('\n'), /must be a relative in-tree path/);

  const traversal = importBoundaryErrors(new Map([['app.js', "import '../../outside.js';"]]));
  assert.match(traversal.errors.join('\n'), /relative in-tree path|escapes the browser module tree/);

  const temporary = await mkdtemp(path.join(tmpdir(), 'lcc-frontend-graph-'));
  t.after(() => rm(temporary, { recursive: true, force: true }));
  const root = path.join(temporary, 'public'), outside = path.join(temporary, 'outside.js');
  await mkdir(root); await writeFile(outside, 'export const secret = true;');
  await symlink(outside, path.join(root, 'escape.js'));
  await writeFile(path.join(root, 'app.js'), "import './escape.js';");
  const report = architectureReport(root);
  assert.match(report.join('\n'), /escaping symlink|cannot be a symlink/);
});

test('the cache rejects direct, aliased and computed browser network access', () => {
  for (const source of [
    "fetch('/api/source');",
    'const send = globalThis.fetch; send(url);',
    "const send = window['fetch']; send(url);",
    "navigator.sendBeacon('/api/source', body);",
    'new XMLHttpRequest();',
    "import('./feature.js');",
  ]) {
    const [ast] = [parseModule('read-projection-cache.js', source)];
    assert.notEqual(networkReferences(ast).length, 0, `expected network escape to be detected: ${source}`);
  }
  assert.deepEqual(networkReferences(parseModule('read-projection-cache.js', "sessionStorage.setItem('projection', JSON.stringify(value));")), []);
  assert.match(indirectTransportErrors('runtime.js', parseModule('runtime.js', 'const request = globalThis.fetch; request(url);')).join('\n'), /aliases or passes/);
  assert.match(storageOwnershipErrors('factory.js', parseModule('factory.js', "sessionStorage.setItem('cache', value);")).join('\n'), /storage owner is read-projection-cache\.js/);
  assert.deepEqual(storageOwnershipErrors('read-projection-cache.js', parseModule('read-projection-cache.js', "sessionStorage.setItem('cache', value);")), []);
});

test('issue-controls POST has one feature owner without banning unrelated account POST', () => {
  const issueWrite = "fetch('/api/issues/' + repo + '/' + number + '/controls', { method: 'POST', body });";
  const controlsRead = "fetch('/api/issues/' + repo + '/' + number + '/controls', { method: 'GET' });";
  const owner = new Map([['issue-actions.js', parseModule('issue-actions.js', issueWrite)]]);
  assert.deepEqual(issueControlWriteErrors(owner), []);

  const misplaced = new Map([
    ['issue-actions.js', parseModule('issue-actions.js', issueWrite)],
    ['runtime.js', parseModule('runtime.js', issueWrite)],
  ]);
  assert.match(issueControlWriteErrors(misplaced).join('\n'), /runtime\.js owns a non-GET issue-controls request/);

  const dynamicMethod = new Map([['issue-actions.js', parseModule('issue-actions.js', "fetch('/api/issues/' + repo + '/controls', { method });")]]);
  assert.match(issueControlWriteErrors(dynamicMethod).join('\n'), /mutation must use its reviewed POST/);

  const readOnly = new Map([
    ['issue-actions.js', parseModule('issue-actions.js', issueWrite)],
    ['issue.js', parseModule('issue.js', controlsRead)],
  ]);
  assert.deepEqual(issueControlWriteErrors(readOnly), []);

  const accountPost = new Map([['account.js', parseModule('account.js', "fetch('/api/account/disconnect', { method: 'POST' });")]]);
  assert.deepEqual(issueControlWriteErrors(accountPost), []);
});
