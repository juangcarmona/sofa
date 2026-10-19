---
name: git-worktrees
description: "Create, use and retire git worktrees so parallel work items cannot contaminate each other's branches, build output, tooling or telemetry. Use when starting a second item while one is in flight."
license: MIT
metadata:
  author: juangcarmona
---

# git-worktrees

## Purpose

One responsibility: create, use and retire a worktree so that two work items can proceed at once without either one polluting the other.

A worktree is a second checkout of the same repository sharing one `.git` directory. It earns its keep when switching branches would cost more than the disk: a long-running item in flight, a review to perform without disturbing the current tree, an urgent fix while a feature is half-applied.

It is not free. Every step below is a way the second checkout differs from the first in a way that has bitten someone.

## When to use

- Starting a second work item while one is still in flight and would otherwise be stashed.
- Reviewing a branch without disturbing the current tree.
- Applying an urgent fix while a feature is half-applied.
- Diagnosing why a worktree's tooling behaves differently from the main checkout.

## When not to use

- A quick branch switch on a clean tree. Switch the branch, via `checkout-branch`, and skip the worktree.
- Keeping a copy of work for safety. A worktree is not a backup: it shares one object database with the main checkout, so a destructive history operation reaches every tree.
- Verifying a change. The checks belong to `verify-like-ci`, run in the worktree that holds the change.

## Workflow

1. **Decide whether a worktree earns its place.** A quick branch switch on a clean tree does not need one. A worktree is worth it when the current tree holds work you would otherwise stash, or when both checkouts must exist simultaneously. *Done when:* the reason is stated. "It seemed tidier" is not one.
2. **Place it outside the repository, or somewhere the repository ignores.** A worktree nested in the main checkout appears in every glob, every search, every file watcher, and every tool that walks the tree: including the ones that then try to build it. *Done when:* the path is either outside the repository root, or inside a directory the repository already ignores.
3. **Avoid a leading dot anywhere in the worktree's path.** Several test runners and bundlers treat a path segment beginning with `.` as an escaped glob character, and silently match nothing. The failure mode is the worst kind: the runner exits 0 having collected zero tests. *Done when:* no path segment starts with `.`, or a run has proved the project's test command still collects its tests there.
4. **Create it with a branch, named after the work item.**

   ```bash
   git worktree add <path> -b <branch-name>     # new branch
   git worktree add <path> <existing-branch>    # existing branch
   ```

   Git refuses to check the same branch out twice. That refusal is a feature: two checkouts of one branch is how you lose a commit. *Done when:* `git worktree list` shows the new tree on its intended branch.
5. **Install dependencies in the worktree.** Dependency directories, build output, caches and virtual environments are **not** shared: the new tree starts empty even though the source is there. A build that "works" before installing is reading artifacts from somewhere it should not be. *Done when:* the project's install command has run in the worktree.
6. **Reproduce the untracked local configuration.** Environment files, local settings and credentials are untracked by design, so they do not come along. Copy what the project needs, and **never** copy a secret into a path that is not ignored in the new tree. *Done when:* the project's own instructions for local setup have been satisfied in the worktree.
7. **Check what tooling resolves to the main checkout.** Hooks, generated configuration, telemetry mapping files and anything holding an absolute path may still point at the original tree. Two symptoms to look for: work performed in the worktree recorded against the main checkout, and a tool reading a configuration file the worktree does not have. *Done when:* anything that writes a record has been confirmed to write it against *this* worktree, or the discrepancy is reported.
8. **Retire it deliberately.**

   ```bash
   git worktree remove <path>       # refuses if the tree is dirty
   git worktree prune               # clears entries whose directory is gone
   ```

   Deleting the directory by hand leaves a stale administrative entry that makes git refuse to reuse the path or the branch later. *Done when:* `git worktree list` no longer shows it, and the branch is either merged or deliberately kept.

## Validation

- The reason for the worktree is stated, not assumed.
- The path is outside the repository root or inside an ignored directory, and no segment starts with `.`.
- `git worktree list` shows the tree on its intended branch.
- The project's install command has run in the worktree, and its local setup instructions are satisfied there.
- No secret sits in a path the new tree does not ignore.
- Anything that writes a record writes it against this worktree, or the discrepancy is reported.
- One work item per worktree, one worktree per work item.
- On retirement, `git worktree list` no longer shows it and no stale administrative entry remains.

## Common pitfalls

| Pitfall | Fix |
| --- | --- |
| Creating a worktree because it seemed tidier | State the reason. A quick branch switch on a clean tree does not need one |
| Nesting the worktree inside the main checkout | Every glob, search, watcher and build tool then walks it. Place it outside the root or in an ignored directory |
| A dot-segment in the worktree path | Runners read it as an escaped glob and collect nothing, exiting 0. Step 3 |
| A build failing on dependencies that exist in the main checkout | Dependency directories, caches and virtual environments are not shared. Install in the worktree. Step 5 |
| A build that works before installing anything | It is reading artifacts from somewhere it should not be |
| Missing environment files or local settings | They are untracked by design and do not come along. Reproduce them, and never copy a secret into a non-ignored path |
| Effort or session records attributed to the wrong branch | Tooling resolved a mapping file relative to the main checkout. Step 7 |
| Two work items sharing one worktree | That is the situation worktrees exist to prevent |
| Deleting the worktree directory by hand | Use `git worktree remove`, then `git worktree prune`. A stale entry makes git refuse to reuse the path or the branch |
| Git refusing to create a worktree at a path that no longer exists | A stale administrative entry. Run `git worktree prune` |
| Git refusing to check out a branch | It is already checked out in another worktree. Find it with `git worktree list` |
| Treating a worktree as a backup | It shares one object database; a destructive history operation reaches every tree |
| Proving one tree and pushing the other | A verdict belongs to the tree that produced it. Verify in the worktree you changed |
| Committing a worktree's path into shared configuration | It is local to one machine and meaningless to everyone else |
