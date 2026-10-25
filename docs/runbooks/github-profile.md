# GitHub repository profile

The owner selected Lighthouse's optional recommended profile. [Configuration](../../.lighthouse/github.json) owns the checks, contribution policy and exceptions; `.lighthouse/upgrade.json` records adoption. GitHub is the source of truth for effective settings.

The reviewed policy was applied on 2026-10-25 using the isolated package candidate from [Lighthouse PR #75](https://github.com/juangcarmona/lighthouse/pull/75), source `f2471806f4abfdda73ba5b4506ca9d728c2de2f8`. Live verification then reported zero remaining differences. This was not an npm release or an application dependency upgrade.

## Verify or change policy

Use a Lighthouse build containing `github-profile`; the installed application dependency predates this adapter. Review configuration changes and the resulting plan before applying them:

```sh
lighthouse capabilities verify --id github-profile --format json
profile_plan=$(mktemp)
lighthouse capabilities plan --id github-profile --plan-file "$profile_plan"
lighthouse capabilities apply --plan-file "$profile_plan" --authorize <plan-hash> --format json
lighthouse capabilities verify --id github-profile --format json
rm -- "$profile_plan"
```

Keep plans and execution output outside Git. Capture output in operator or CI logs when evidence is needed. General `lighthouse verify` checks local configuration; use the explicit command above to inspect live GitHub settings. Wiki remains deferred, and release immutability is excepted until a publisher is validated. Auto-merge availability does not authorize integration or activate it on a PR.
