# create-branch: issue-linked convention

Create issue-linked branches as `<type>/<issue-number>-<kebab-case-slug>`, for example `feat/123-export-report` or `fix/183-session-resume`. Use `feat`, `fix`, `docs`, `refactor`, `test`, `chore`, `build`, `ci`, `perf`, or `revert` as the type. Keep the issue number in every branch name. Do not use a tool or agent name as a prefix.

Fetch the configured base immediately before creating a branch. Conventional commits use `<type>(<optional-scope>): <description>`. Pull requests carry `Closes #<number>` in the body.
