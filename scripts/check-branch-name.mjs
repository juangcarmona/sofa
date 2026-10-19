import { execFileSync } from "node:child_process";

const branchName =
  process.argv[2] ??
  execFileSync("git", ["branch", "--show-current"], {
    encoding: "utf8",
  }).trim();

const allowedBranchName =
  /^(feat|fix|docs|refactor|test|chore|build|ci|perf|revert)\/[1-9]\d*-[a-z0-9]+(?:-[a-z0-9]+)*$/;

if (!allowedBranchName.test(branchName)) {
  console.error(
    `Invalid branch name: ${branchName || "(detached HEAD)"}. Use <type>/<issue-number>-<kebab-case-slug>, for example feat/47-export-report.`,
  );
  process.exitCode = 1;
}
