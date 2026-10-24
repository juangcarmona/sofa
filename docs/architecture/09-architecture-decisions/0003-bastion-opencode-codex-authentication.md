---
name: bastion-opencode-codex-authentication
description: Bastion runs persistent OpenCode authenticated with the operator's OpenAI Codex account rather than a GitHub workflow API secret.
status: Accepted
date: 2026-10-24
deciders: [Juan García Carmona]
tags: [sofa, bastion, opencode, codex, openai, authentication]
---

# ADR-0003: Bastion OpenCode uses the operator's OpenAI Codex account without a GitHub API secret

## Context

The GitHub Actions workflow is the transport and repository-lifecycle boundary. It must assign work to the self-hosted runner on Bastion, but it must not turn GitHub Actions into the model-provider boundary.

The desired execution engine is OpenCode running on Bastion. OpenCode is configured to use OpenAI/Codex through the operator's ChatGPT Plus/Pro authentication. The credential is therefore a Bastion runtime credential owned by the dedicated execution account, not a GitHub Actions secret. OpenCode's provider documentation describes ChatGPT Plus/Pro authentication through `/connect` and model selection through `/models`.

The current generated Lighthouse projection declares `engine: copilot` and validates `COPILOT_GITHUB_TOKEN` before its agent job. That is observed current configuration, not the target SoFa execution architecture. The built-in gh-aw Codex engine is also a different path: it expects an API-key or GitHub Copilot inference contract and would move provider authentication into the workflow.

Current Bastion evidence confirms OpenCode `v2.1.0` is installed for `juan`, while the dedicated `github-runner` service account has no observed OpenCode authentication file. The subscription login is therefore still an installation/operations step, not a completed runtime capability.

## Decision

1. Lighthouse remains responsible for events, routes, stages, gates, skills, workflow composition, `stage-result`, and delivery evidence.
2. GitHub Actions remains the initial transport: router -> `workflow_call` worker -> job assigned to the repository's Bastion runner registration.
3. The Bastion job enters the SoFa-owned admission and supervision boundary. SoFa creates the isolated execution workspace and supervises the OpenCode process for the admitted run.
4. OpenCode is the execution engine for the first target slice. It runs on Bastion under the dedicated execution account and uses that account's OpenAI Codex/ChatGPT authentication.
5. The GitHub workflow does not receive or persist `OPENAI_API_KEY`, `CODEX_API_KEY`, or `COPILOT_GITHUB_TOKEN` for this path. The OpenCode credential remains outside the repository workspace and outside GitHub workflow inputs.
6. OpenCode's model configuration selects the model profile for the capability being executed. Lighthouse stage names and gates do not select providers or infer model authorization. The exact model-to-capability table is a Bastion configuration decision and must be observable without exposing credentials.
7. Additional providers remain possible behind the OpenCode/provider boundary. A gateway may mediate those providers when it supports the required authentication and usage controls; SoFa does not become a model server or duplicate provider routing.

## Consequences

- **Positive:** A GitHub Actions job can use the operator's existing Codex subscription without putting an OpenAI API key in GitHub.
- **Positive:** OpenCode, provider configuration, and model selection remain on Bastion and can be supervised by SoFa.
- **Positive:** Lighthouse does not acquire a second lifecycle or model-selection authority.
- **Negative:** The Bastion execution account must be provisioned, authenticated, monitored, and protected as a long-lived runtime identity.
- **Negative:** OpenCode subscription authentication must be verified in the exact non-interactive service context used by the runner; a successful interactive login as Juan is not evidence that `github-runner` can execute.
- **Negative:** The current gh-aw/Copilot projection cannot be treated as the target adapter. A versioned job-to-SoFa handoff is still required before the first vertical slice is executable.

## Alternatives considered

- **gh-aw built-in `codex` engine with `OPENAI_API_KEY` or `CODEX_API_KEY`:** rejected for the initial path because it requires API-key authentication in the workflow and bypasses the desired Bastion-local Codex subscription boundary.
- **gh-aw Copilot engine with `COPILOT_GITHUB_TOKEN`:** rejected because Copilot is not the selected engine or subscription boundary.
- **gh-aw's imported OpenCode sample:** rejected as the first implementation boundary because its documented non-AWF path is API-key based and it installs/configures a per-job CLI instead of supervising the persistent Bastion OpenCode account.
- **Python/DeepSeek or another agent runtime:** rejected because OpenCode is the selected execution engine.
- **SoFa becomes a model server:** rejected because provider inference and model serving remain outside SoFa.

## References

- [OpenCode providers: ChatGPT Plus/Pro authentication](https://opencode.ai/docs/providers)
- [OpenCode v2 providers: ChatGPT and Codex sign-in](https://opencode.ai/v2/docs/providers)
- [GitHub Agentic Workflows: custom engine constraints](https://github.github.com/gh-aw/reference/third-party-agent/)
- [ADR-0001: Lighthouse owns delivery workflow and SoFa owns the Bastion execution runtime](0001-lighthouse-delivery-workflow-sofa-bastion-runtime.md)
- [SoFa runtime architecture](../../roadmap/runtime-architecture.md)
