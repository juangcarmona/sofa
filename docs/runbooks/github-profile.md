# GitHub repository profile

The owner selected Lighthouse's optional recommended GitHub profile. [Configuration](../../.lighthouse/github.json), [provider readback](../../.lighthouse/reconciliation/github-profile.json) and [tool provenance](../../.lighthouse/reconciliation/github-profile-provenance.json) record this repository's adoption.

## Verified setup

On 2026-10-25, the packaged candidate from [Lighthouse PR #75](https://github.com/juangcarmona/lighthouse/pull/75), source `f2471806f4abfdda73ba5b4506ca9d728c2de2f8`, applied the reviewed plan and verified all selected automated controls with zero remaining differences. The package was installed in an isolated operator environment; it is not an npm release and did not replace this repository's installed dependencies. Main and the runtime source pin remain unchanged by this settings operation.

The policy keeps collaborator-only issue and PR creation, the existing required CI check and protected main/process branches. It enables merge-update suggestions, automatic cleanup of merged feature branches, PR-derived squash messages, availability of auto-merge, Dependabot alerts/security updates, secret scanning and push protection. Enabling auto-merge availability does not activate it on an existing PR or grant an agent merge authority.

Existing merge/rebase methods are retained as documented exceptions while work is open. Release immutability is excluded until a draft-and-attach publisher is validated. Wiki remains deferred. The receipt covers automated controls only; manual recommendations remain listed in the provenance record.

## Recheck and change policy

Use a Lighthouse build containing `github-profile`; the dependency version currently installed in this application predates the adapter. The exact reviewed source and tarball digests are recorded above. Once that build is selected, run these commands from the repository root:

```sh
lighthouse capabilities verify --id github-profile --format json
lighthouse capabilities plan --id github-profile --plan-file .lighthouse/reconciliation/github-profile.plan.json
lighthouse capabilities apply --plan-file .lighthouse/reconciliation/github-profile.plan.json --authorize <plan-hash>
```

Edit the configuration and review a new plan before changing policy. General `lighthouse verify` is not a live GitHub settings audit. Upgrade the application dependency through its normal package-adoption PR after the adapter is released; no runtime or workflow revision change is implied by this hosting setup.
