# Issue context design

<!-- pdac-scope: cited -->

## Context and approach

Extend the current internal sealed query handlers and user-scoped GitHub transport in the Workspace module. A repository must be explicitly configured and its current effective user/App issue-read authority must pass before any issue or evidence read. Optional Actions evidence can fail independently of the issue/conversation.

Use `/workspace/issues/{owner}/{repo}/{number}` and the matching `/api/issues/...` GET. Validate all path components and positive safe numbers. Issue records with a pull_request member are rejected; PR inspection is related evidence, not an issue identity.

Conversation pages are read with an explicit page cursor and a maximum of 100 records per response. Load more retains previous pages and exposes source identity, source update time, observation time, coverage and transient failure separately. Each request has a 35-second total budget and a 2 MiB JSON response bound; each browser issue view retains at most 10 pages / 1,000 comments and 8 MiB of Markdown text. These are hard pagination stops: a view issues at most 10 conversation source requests (at most 20 MiB downloaded at the 2 MiB response cap), never fetches a page twice and disables Load more before issuing any further request once its page/comment/text cap is reached. The server rejects page cursors outside 1–10. If the next bounded response reaches the text cap, stop reading it at the remaining byte allowance, mark the final page truncated and do not advance its cursor or request another page. Expose the last completed cursor, the stop reason and bounded partial coverage with a source inspection link. A user may explicitly reopen/refresh as a new bounded snapshot, but no automatic continuation or discard-and-refetch loop occurs. Reaching either bound never claims completeness; navigating to another issue clears that memory. No automatic drain of later pages occurs. Finite bounds are explicit, never a claim of completeness. A collection changing between reads is identified as a new snapshot instead of silently mixing identities.

## Evidence projection

Consume the adopted Lighthouse wire contract as source evidence; do not implement command eligibility or admission policy. A stage result requires the exact source report, request/envelope run identity, repository/issue, revision and workflow attempt association. Source claims without available verification stay labelled unverified with readable report content. Completed GitHub execution and blocked stage outcome remain separate fields.

Only exact repository-local source references identify related runs, PRs and checks. Source-supplied links are validated and do not control credential-bearing fetch destinations. Artifact inventory is bounded and must be complete for a claim of absence; expired, oversized, inaccessible, malformed or unsupported artifacts remain explicit. Only exact expected request.json/envelope.json/report Markdown or text members are read. Enforce a 2 MiB compressed download limit while streaming (also reject larger Content-Length or inventory sizes before reading), a 4 MiB total expanded limit, at most 100 members and a 1 MiB selected-member limit before opening any member. Read through a bounded stream, reject traversal/absolute/duplicate selected names and nested archives or non-JSON/text members, and never extract to disk. A rejected/truncated artifact is unavailable with its specific size/type/coverage reason; it never replaces the readable issue or report. At most 10 source reports and 10 related PR/run inventories are inspected per context request; additional evidence is explicitly incomplete. Download credentials never follow an asset redirect. Unsupported contract versions remain unknown, not a guessed lifecycle state.

Reservations and pending human questions are shown only with their source record and observed scope. Labels are source labels; an isolated label does not prove a live lease, identity, result or command authority. Prior refinement/proposal summaries retain source revision and exact run/attempt association; stale evidence is readable and clearly distinguished from current eligibility, which this issue does not decide.

## Browser and security

Render a conservative Markdown subset with DOM text nodes, explicit safe links and inert code/raw HTML; no source HTML, remote images, embedded forms or scripts execute. Preserve the original Markdown in an accessible source disclosure. The issue API does not share a global cache. Browser refresh preserves readable context and focus during temporary unavailability; 401/403/409 removes prior data. Identity changes reload source scope. Navigation and paging are GETs and cannot submit work.

Use existing tokens and stylesheet hierarchy, semantic headings, a labelled repository/issue form, visible focus, 44px controls and a single-column phone layout. Keep overview links and direct issue entry consistent.

## Alternatives

Linking to GitHub for all reading fails the accepted in-app context journey. Selecting the newest workflow invents correlation. Adding command policy or SQLite to a read view mixes #48 and the still-proposed ADR-0008. A new framework or generic projection platform is unnecessary; extend the existing adapter/module.

## Verification and Done gate

Exercise ordinary/closed/non-issue targets, multi-page conversations, mutation during pagination, malformed and malicious Markdown, stale/incomplete responses, account/grant loss and concurrent refresh; exact blocked report with successful workflow, mismatched run/revision/attempt, spoofed markers, active reservation and expired/oversized artifact. Use HTTP integration tests plus browser keyboard/phone-width/accessibility cases. A real configured issue and long paginated source conversation must be observed through the connected user; fixtures do not establish that consent. #49 retains actual phone identity evidence.

All 11 lifecycle Done dimensions apply: acceptance, completeness, tests, quality checks, security, conformance, docs, product/spec consistency, evidence, independent PR review and stated limits. Run root verification, the host identity lane when its affected paths change, current-head hosted CI, and the independent final audit before integration. Actual source acceptance remains explicit when unavailable.

## Product grounding

<!-- pdac:cite id="UC-OPERATE-ISSUE" digest="sha256:e1c11763cbb6a3f3ff8aa2a7101bc672a6a45595a256134fb04d1d2070f66d2f" -->

<!-- pdac:cite id="FR-ISSUE-CONTEXT" digest="sha256:11f4c196c15675fa20c05bb320f02a2bab4bdbf96cf275a408c849924faed270" -->

<!-- pdac:cite id="BR-GITHUB-SOURCE" digest="sha256:fd8141424d5fead38ed55d54689511fc445467133706e875371cc3199af8ab1a" -->

<!-- pdac:cite id="BR-EXPLICIT-WORK-COMMANDS" digest="sha256:5d2f308b670bdf6e577d2205ebc246ec56505253d81d6a5a5efe0b92b55d735e" -->

<!-- pdac:cite id="BR-LAN-PRIVILEGE-BOUNDARY" digest="sha256:318c7d8d4d3b2e27109e24628f99dd1825494fa9858dc13410032d73650f1cc8" -->

<!-- pdac:cite id="QR-MOBILE-OPERATIONS" digest="sha256:2485b5877f0872af3dbdaa0a7ac00ac7651b28173adf6e3cf8d9ed643029bb35" -->

<!-- pdac:cite id="BR-EXPLAINABLE-SIGNALS" digest="sha256:1b8ec288714357b03b030a921f70b7f1dd0e82a590d05430cc8b4bf638c933a9" -->
