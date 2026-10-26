import { createServer } from 'node:http';
import { readFile } from 'node:fs/promises';
import { createRequire } from 'node:module';
import { fileURLToPath } from 'node:url';
import { observeRepositories } from '../observation/github.mjs';

const require = createRequire(import.meta.url);
const assets = new Map([
  ['/', [new URL('./public/index.html', import.meta.url), 'text/html']],
  ['/app.js', [new URL('./public/app.js', import.meta.url), 'text/javascript']],
  ['/app.css', [new URL('./public/app.css', import.meta.url), 'text/css']],
  ['/tokens.css', [require.resolve('@juangcarmona/lighthouse-ui/tokens.css'), 'text/css']],
]);

export function createObservationServer({ repositories, hosts = ['127.0.0.1', 'localhost'], observe = observeRepositories, ttl = 15000, now = Date.now }) {
  if (!Array.isArray(repositories) || !repositories.length || repositories.length > 10 || repositories.some(r => !/^[A-Za-z0-9][A-Za-z0-9-]*\/[A-Za-z0-9][A-Za-z0-9_.-]*$/.test(r))) throw Error('Configure between 1 and 10 owner/repository names');
  const selected = [...new Set(repositories)];
  let cached, expires = 0, pending;
  async function snapshot() {
    if (cached && now() < expires) return cached;
    if (!pending) pending = observe(selected).then(data => {
      cached = { repositories: data, refreshAfterSeconds: ttl / 1000 };
      expires = now() + ttl;
      return cached;
    }).finally(() => { pending = null; });
    return pending;
  }
  const server = createServer(async (req, res) => {
    res.setHeader('Content-Security-Policy', "default-src 'self'; script-src 'self'; style-src 'self'; connect-src 'self'; img-src 'self'; frame-ancestors 'none'; base-uri 'none'; form-action 'none'");
    res.setHeader('X-Content-Type-Options', 'nosniff');
    res.setHeader('Referrer-Policy', 'no-referrer');
    res.setHeader('Cache-Control', 'no-store');
    const reply = (status, message) => { res.writeHead(status, { 'Content-Type': 'text/plain; charset=utf-8' }); res.end(message); };
    try {
      const host = new URL(`http://${req.headers.host || ''}`).hostname;
      if (!hosts.includes(host) || req.headers['sec-fetch-site'] === 'cross-site') return reply(403, 'Forbidden');
      if (!['GET', 'HEAD'].includes(req.method)) { res.setHeader('Allow', 'GET, HEAD'); return reply(405, 'Read-only surface'); }
      if (req.url === '/api/observations') {
        const body = JSON.stringify(await snapshot());
        res.writeHead(200, { 'Content-Type': 'application/json; charset=utf-8' });
        return res.end(req.method === 'HEAD' ? undefined : body);
      }
      const asset = assets.get(req.url);
      if (!asset) return reply(404, 'Not found');
      const body = await readFile(asset[0]);
      res.writeHead(200, { 'Content-Type': `${asset[1]}; charset=utf-8` });
      res.end(req.method === 'HEAD' ? undefined : body);
    } catch { reply(503, 'Observation unavailable. Try again later.'); }
  });
  server.requestTimeout = 35000;
  server.headersTimeout = 10000;
  server.maxConnections = 64;
  return server;
}

if (process.argv[1] === fileURLToPath(import.meta.url)) {
  const host = process.env.SOFA_HOST || '127.0.0.1';
  const port = Number(process.env.SOFA_PORT || 5168);
  if (!Number.isInteger(port) || port < 1024 || port > 65535) throw Error('SOFA_PORT must be between 1024 and 65535');
  const repositories = (process.env.SOFA_REPOSITORIES || 'juangcarmona/lighthouse,juangcarmona/sofa').split(',').map(r => r.trim());
  createObservationServer({ repositories, hosts: [host, 'localhost', '127.0.0.1'] }).listen(port, host, () => console.log(`SoFa observation: http://${host}:${port}`));
}
