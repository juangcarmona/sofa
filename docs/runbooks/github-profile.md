# GitHub repository profile

The owner selected Lighthouse's optional recommended profile. [Configuration](../../.lighthouse/github.json) owns the checks, contribution policy and exceptions; `.lighthouse/upgrade.json` records adoption. GitHub is the source of truth for effective settings.

Use the released Lighthouse dependency pinned in `package.json`. The configuration records the selected policy; verify it against GitHub whenever checking effective protection.

## Verify or change policy

Run the installed CLI from the repository root with `npx --no-install lighthouse`. Review configuration changes and the resulting plan before applying them:

```sh
npx --no-install lighthouse capabilities verify --id github-profile --format json
profile_plan=$(mktemp)
npx --no-install lighthouse capabilities plan --id github-profile --plan-file "$profile_plan"
npx --no-install lighthouse capabilities apply --plan-file "$profile_plan" --authorize <plan-hash> --format json
npx --no-install lighthouse capabilities verify --id github-profile --format json
rm -- "$profile_plan"
```

Keep plans and execution output outside Git. Capture output in operator or CI logs when evidence is needed. General `lighthouse verify` checks local configuration; use the explicit command above to inspect live GitHub settings. Wiki remains deferred, and release immutability is excepted until a publisher is validated. Auto-merge availability does not authorize integration or activate it on a PR.
