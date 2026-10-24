import { spawnSync } from 'node:child_process';
import { randomUUID } from 'node:crypto';

let connection = process.env.SOFA_TEST_DATABASE;
const name = `sofa-test-${randomUUID()}`;
function command(cmd, args, options = {}) {
  const run = spawnSync(cmd,args,{encoding:'utf8',...options});
  if (run.status !== 0) throw new Error(`${cmd} failed: ${run.stderr ?? ''}`);
  return run.stdout?.trim();
}
try {
  if (!connection) {
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
  command('dotnet',['restore','SoFa.slnx','--locked-mode'],{stdio:'inherit'});
  command('dotnet',['test','SoFa.slnx','--no-restore'],{stdio:'inherit',env:{...process.env,SOFA_TEST_DATABASE:connection}});
} finally { if (!process.env.SOFA_TEST_DATABASE) spawnSync('docker',['rm','-f',name],{stdio:'ignore'}); }
