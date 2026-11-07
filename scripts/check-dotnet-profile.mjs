// Consumer verification adapted from the installed Lighthouse dotnet-backend profile.
// Legacy runtime tests remain in verify-runtime's provisioned PostgreSQL / Docker lanes.
import { spawnSync } from 'node:child_process';
import { mkdtemp, mkdir, readdir, writeFile, readFile, rm } from 'node:fs/promises';
import { tmpdir } from 'node:os';
import { resolve, join } from 'node:path';

import { prepareAspire } from './prepare-aspire.mjs';

const root = process.cwd();
const environment = await prepareAspire();
function dotnet(args, options = {}) {
  const result = spawnSync('dotnet', args, { cwd: root, env: environment, encoding: 'utf8', ...options });
  if (result.error) throw result.error;
  return result;
}
const build = dotnet(['build', 'SoFa.slnx', '--no-restore', '-c', 'Release', '--disable-build-servers'], { stdio: 'inherit' });
if (build.status !== 0) process.exit(build.status ?? 1);

const files = [];
async function collect(folder) {
  for (const entry of await readdir(folder, { withFileTypes: true })) {
    if (['bin', 'obj'].includes(entry.name)) continue;
    const path = join(folder, entry.name);
    if (entry.isDirectory()) await collect(path);
    else if (entry.isFile() && entry.name.endsWith('.cs')) files.push(path);
  }
}
for (const folder of ['src/SoFa.AppHost', 'src/SoFa.Web', 'src/SoFa.Modules.Workspace', 'src/dotnet', 'tests/SoFa.Web.Tests']) await collect(folder);
const format = dotnet(['format', 'whitespace', '.', '--folder', '--verify-no-changes', '--include', ...files, '--verbosity', 'quiet'], { stdio: 'inherit' });
if (format.status !== 0) process.exit(format.status ?? 1);

for (const match of (await readFile('SoFa.slnx', 'utf8')).matchAll(/<Project Path="([^"]+\.csproj)"/g)) {
  const evaluated = dotnet(['msbuild', match[1], '-getProperty:AssemblyName,LighthouseModule', '-getItem:ProjectReference']);
  if (evaluated.status !== 0) throw Error('Cannot evaluate the solution module contract.');
  const project = JSON.parse(evaluated.stdout), assembly = project.Properties.AssemblyName;
  if (!assembly.includes('.Modules.') || assembly.endsWith('.Contracts') || assembly.endsWith('Tests')) continue;
  if (project.Properties.LighthouseModule !== 'true' || !project.Items.ProjectReference.some(reference =>
    reference.Identity.endsWith('Lighthouse.Analyzers.csproj') && reference.OutputItemType === 'Analyzer' && reference.ReferenceOutputAssembly === 'false'))
    throw Error('Every implementation module must activate and reference the shared analyzer.');
}

const probe = await mkdtemp(join(tmpdir(), 'sofa-dotnet-rejection-'));
const xml = value => value.replaceAll('&', '&amp;').replaceAll('"', '&quot;').replaceAll('<', '&lt;');
try {
  await mkdir(join(probe, 'Sibling'));
  await mkdir(join(probe, 'Invalid'));
  await writeFile(join(probe, 'Sibling/Sibling.csproj'), '<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><TargetFramework>net10.0</TargetFramework><AssemblyName>SoFa.Probe.Modules.Sibling</AssemblyName></PropertyGroup></Project>');
  await writeFile(join(probe, 'Sibling/Sibling.cs'), 'namespace SoFa.Probe.Modules.Sibling; public sealed class SiblingExport {}');
  await writeFile(join(probe, 'Invalid/Invalid.csproj'), `<Project Sdk="Microsoft.NET.Sdk">
    <PropertyGroup><TargetFramework>net10.0</TargetFramework><ImplicitUsings>enable</ImplicitUsings><AssemblyName>SoFa.Probe.Modules.Invalid</AssemblyName><LighthouseModule>true</LighthouseModule></PropertyGroup>
    <ItemGroup><CompilerVisibleProperty Include="LighthouseModule" />
      <ProjectReference Include="../Sibling/Sibling.csproj" />
      <ProjectReference Include="${xml(resolve(root, 'src/dotnet/Lighthouse.Cqrs/Lighthouse.Cqrs.csproj'))}" />
      <ProjectReference Include="${xml(resolve(root, 'src/dotnet/Lighthouse.Analyzers/Lighthouse.Analyzers.csproj'))}" OutputItemType="Analyzer" ReferenceOutputAssembly="false" />
    </ItemGroup></Project>`);
  await writeFile(join(probe, 'Invalid/Invalid.cs'), `using JuanGCarmona.Lighthouse.Cqrs;
    namespace SoFa.Probe.Modules.Invalid;
    public class ExposedHandler : IQueryHandler<string, string> {
      public Task<string> Handle(string query, CancellationToken token) => Task.FromResult(query);
    }`);
  const rejected = dotnet(['build', join(probe, 'Invalid/Invalid.csproj'), '-c', 'Release', '--disable-build-servers']);
  const output = rejected.stdout + rejected.stderr;
  if (rejected.status === 0 || !/error LH1001\b/.test(output) || !/error LH1002\b/.test(output)) {
    process.stderr.write(output); throw Error('The real compiler must reject the exposed handler and cross-module implementation reference.');
  }
  console.log('Module activation and real LH1001 / LH1002 compiler rejection verified.');
} finally { await rm(probe, { recursive: true, force: true }); }
