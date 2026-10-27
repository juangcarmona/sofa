import { spawnSync } from 'node:child_process';
import { randomUUID } from 'node:crypto';

const scope = process.argv.includes('--host') ? 'host' : (process.env.SOFA_RUNTIME_TEST_SCOPE ?? 'all');
if (!['all', 'container', 'host'].includes(scope)) throw new Error('Unknown runtime test scope');
if (scope === 'container' && !process.env.SOFA_TEST_DATABASE) throw new Error('Container verification requires host-provisioned SOFA_TEST_DATABASE');
console.log(`Runtime test scope: ${scope}${scope === 'container' ? ' (Docker engine tests run separately with npm run test:runtime:host)' : ''}`);
let connection = process.env.SOFA_TEST_DATABASE;
const name = `sofa-test-${randomUUID()}`;
function command(cmd, args, options = {}) {
  const run = spawnSync(cmd,args,{encoding:'utf8',...options});
  if (run.status !== 0) throw new Error(`${cmd} failed: ${run.stderr ?? ''}`);
  return run.stdout?.trim();
}
try {
  if (!connection && scope !== 'host') {
    command('docker',['run','-d','--name',name,'-e','POSTGRES_PASSWORD=sofa-test-only','-e','POSTGRES_DB=sofa','-p','127.0.0.1::5432','postgres:17-alpine']);
    let ready = false;
    for (let i=0;i<30;i++) {
      if (spawnSync('docker',['exec',name,'pg_isready','-U','postgres'],{stdio:'ignore'}).status===0) { ready=true; break; }
      await new Promise(resolve=>setTimeout(resolve,500));
    }
    if (!ready) throw new Error('Test PostgreSQL did not become ready');
    const port = command('docker',['port',name,'5432/tcp']).split(':').at(-1);
    connection=`Host=127.0.0.1;Port=${port};Database=sofa;Username=postgres;Password=sofa-test-only`;
  }
  const buildServers = scope === 'container' ? ['--disable-build-servers'] : [];
  command('dotnet',['restore','SoFa.slnx','--locked-mode',...buildServers],{stdio:'inherit'});
  const filter = scope === 'all' ? [] : ['--filter', `FullyQualifiedName${scope === 'host' ? '~' : '!~'}SoFa.Runtime.Tests.EngineTests`];
  command('dotnet',['test','SoFa.slnx','--no-restore',...filter,...buildServers],{stdio:'inherit',env:{...process.env,SOFA_TEST_DATABASE:connection}});
} finally { if (!process.env.SOFA_TEST_DATABASE && scope !== 'host') spawnSync('docker',['rm','-f',name],{stdio:'ignore'}); }
