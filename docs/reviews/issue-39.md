# Issue 39 retrospective

Date: 2026-10-26

Work item: [Include issues awaiting review in repository attention observations](https://github.com/juangcarmona/sofa/issues/39). Delivered by [PR #40](https://github.com/juangcarmona/sofa/pull/40), merge `3c87683c324575b20a8f25226f26487c2bfed9b9`.

## What worked

The failing regression isolated the missing `waiting-review` label before the filter changed. Independent audits checked the source diff, preserved labels, exclusions, collection bounds and final PR checks. Local verification passed 24 Node tests and 38 runtime tests plus documentation, security and provider checks. The owner delegated routine completion; this does not record a personal human review or an `/ff` verdict.

## What did not

The live presentations query had no open issue carrying only `waiting-review`. It confirmed that read-only collection worked, but the added behavior is supported by the regression fixture rather than a live example. Collection still covers only 20 runs and the first 100 open issue records.

## What to do next time

Exercise each supported attention label in observer regressions, including waiting states without an active agent. Keep fixture evidence distinct from live evidence; do not mutate real issues solely to manufacture a demonstration.

## Post-merge evidence

The merged checkout successfully observed presentations with availability `available`, 20 runs and two `ready` issues. This was a CLI smoke check; no running service was redeployed. Both target-branch checks passed: [repository verification](https://github.com/juangcarmona/sofa/actions/runs/40251896239) and [Lighthouse guardrails](https://github.com/juangcarmona/sofa/actions/runs/40251896435).

## Effort

Human active time, agent active time, tokens and cost: not captured. No configured session-to-work-item usage mapping was found in the repository documentation; CI duration is not a substitute for those measurements.

## Amendments

None. The regression added in PR #40 is the guard for this omission. No lifecycle, product, architecture or permission change is warranted by this bounded defect.
