# Issue context

<!-- pdac-scope: cited -->

## ADDED Requirements

### Requirement: Read ordinary source issues and conversation

SoFa SHALL allow the signed-in owner with verified configured-repository authority to open an ordinary issue by repository/number and read its description and paginated conversation with source identities, source times, observation time and collection bounds. A partial or unavailable collection SHALL NOT be presented as empty or complete.

<!-- pdac:cite id="FR-ISSUE-CONTEXT" digest="sha256:11f4c196c15675fa20c05bb320f02a2bab4bdbf96cf275a408c849924faed270" -->

#### Scenario: Long ordinary issue without attention labels

- **WHEN** the owner opens an authorized ordinary issue containing more than one page of comments
- **THEN** its full body and first conversation page are readable and explicit load-more reads subsequent pages with source identities and honest coverage

#### Scenario: Refresh fails or authority is lost

- **WHEN** a temporary refresh fails
- **THEN** previous readable context retains source times and is marked stale
- **WHEN** owner or connected-user authority is lost
- **THEN** prior issue data is removed and further source reads stop

### Requirement: Distinguish and correlate lifecycle evidence

SoFa SHALL show issue state, lifecycle stage/result, requested command, execution, reservation and human wait as separate source-backed facts. Related results, runs/attempts, revisions, PRs, checks and artifacts SHALL preserve exact source association. Unsupported or unavailable verification SHALL remain explicit and SHALL NOT grant command authority.

<!-- pdac:cite id="BR-EXPLICIT-WORK-COMMANDS" digest="sha256:5d2f308b670bdf6e577d2205ebc246ec56505253d81d6a5a5efe0b92b55d735e" -->

#### Scenario: Successful execution publishes a blocked proposal

- **WHEN** an exactly associated source report records a blocked proposal and its GitHub workflow completed successfully
- **THEN** the issue view displays the blocked stage outcome separately from the successful workflow execution

#### Scenario: Artifact or association is unavailable

- **WHEN** an artifact is expired, oversized or inaccessible, or a recent run cannot be associated with the issue
- **THEN** missing evidence and limits are explained without asserting a successful result or substituting that run

### Requirement: Read safe context with keyboard and phone layouts

SoFa SHALL render source Markdown without executing source HTML, scripts, embedded forms or unsafe links, and SHALL preserve readable original content. Issue navigation, paging and refresh SHALL remain read-only and usable with keyboard and a narrow phone viewport.

<!-- pdac:cite id="QR-MOBILE-OPERATIONS" digest="sha256:2485b5877f0872af3dbdaa0a7ac00ac7651b28173adf6e3cf8d9ed643029bb35" -->

#### Scenario: Hostile content in a long conversation

- **WHEN** a comment contains executable HTML, unsafe links and long code
- **THEN** the source remains readable without execution or horizontal page overflow and keyboard users can reach context and evidence controls

## Related product grounding

<!-- pdac:cite id="UC-OPERATE-ISSUE" digest="sha256:e1c11763cbb6a3f3ff8aa2a7101bc672a6a45595a256134fb04d1d2070f66d2f" -->

<!-- pdac:cite id="FR-ISSUE-CONTEXT" digest="sha256:11f4c196c15675fa20c05bb320f02a2bab4bdbf96cf275a408c849924faed270" -->

<!-- pdac:cite id="BR-GITHUB-SOURCE" digest="sha256:fd8141424d5fead38ed55d54689511fc445467133706e875371cc3199af8ab1a" -->

<!-- pdac:cite id="BR-EXPLICIT-WORK-COMMANDS" digest="sha256:5d2f308b670bdf6e577d2205ebc246ec56505253d81d6a5a5efe0b92b55d735e" -->

<!-- pdac:cite id="BR-LAN-PRIVILEGE-BOUNDARY" digest="sha256:318c7d8d4d3b2e27109e24628f99dd1825494fa9858dc13410032d73650f1cc8" -->

<!-- pdac:cite id="QR-MOBILE-OPERATIONS" digest="sha256:2485b5877f0872af3dbdaa0a7ac00ac7651b28173adf6e3cf8d9ed643029bb35" -->

<!-- pdac:cite id="BR-EXPLAINABLE-SIGNALS" digest="sha256:1b8ec288714357b03b030a921f70b7f1dd0e82a590d05430cc8b4bf638c933a9" -->
