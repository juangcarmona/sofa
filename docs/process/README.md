# Process reports and evidence

This branch retains publishable process reports and evidence independently from the application history. Never merge it into `main`. GitHub rules block branch deletion and force-push; administrators can change those rules.

Reserve one stable folder per issue at `docs/process/{YYYYMMDD}-{issue_ID}-{issue-slug}/`, then append bundles under `runs/<run-id>/attempt-0001/pr-ready/` or `post-integration/`. Published reports should link to the exact source and evidence commits.

Publish reviewed summaries and small artifacts. Keep credentials, raw session transcripts, sensitive telemetry and large videos outside this branch; use access-controlled references and hashes where appropriate.

The branch was initialized on 2026-10-25. Automated collection, retrospective generation and publication are pending; this README is not evidence of an implemented publisher.
