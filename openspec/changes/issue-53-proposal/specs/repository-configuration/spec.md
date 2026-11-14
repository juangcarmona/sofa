## Purpose

<!-- pdac-scope: cited -->

Let operators inspect repository configuration facts and explained differences through source-bound evidence without introducing a second policy authority or requesting maintenance work.

## ADDED Requirements

### Requirement: Configuration facts retain provenance

Supported configuration facts SHALL preserve source provenance, with declarations and lock resolutions independently inspectable.
<!-- pdac:cite id="FR-CONFIGURATION-DRIFT" digest="sha256:abb59ea4de5de304e41806960f9d7cc593cd9086082a2c04ce7fe1b4dc67eff7" -->
<!-- pdac:cite id="FR-PROJECT-DISCOVERY" digest="sha256:fb93a1c32232be6a679aa7aa074e9b6c0efd871cd48592b6b8d56996c36a2382" -->

#### Scenario: Separate source dimensions

- **WHEN** a configured repository declares manifest baseline 0.1.0, CLI adoption 0.12.65, SDLC package 0.22.4 and a different lock resolution
- **THEN** each value retains its own provenance and dimension, with conflicts between comparable declarations visible and no silent winner
- **THEN** an SDLC contract version of 0.5.0, when evidenced, remains a contract fact rather than a package version

#### Scenario: Complete configuration facts retain provenance contract

- **WHEN** the operator inspects the evidence or comparison covered by this requirement
- **THEN** The system SHALL expose each supported discovered configuration fact with repository identity, exact path, immutable revision, observation time, fact kind and source contract identity. Distinct declarations and lock resolutions MUST remain separately inspectable.

### Requirement: Comparisons name compatible expectations

Comparisons SHALL name their compatible expectation and origin; absent or unsupported expectations remain unassessed without implying policy violations.
<!-- pdac:cite id="FR-CONFIGURATION-DRIFT" digest="sha256:abb59ea4de5de304e41806960f9d7cc593cd9086082a2c04ce7fe1b4dc67eff7" -->
<!-- pdac:cite id="BR-EXPLAINABLE-SIGNALS" digest="sha256:8b54c80507f22f6883aaad91a53f479da5892bcc8def4f0e836076d1cf2063d5" -->

#### Scenario: Matching baseline

- **WHEN** an operator selects a revision-bound supported declaration as the expectation for an equal declaration of the same component and comparable contract
- **THEN** the view reports match only in that named scope and displays both sources

#### Scenario: Outdated comparable declaration

- **WHEN** an exact numeric package declaration is lower than a selected comparable expected declaration
- **THEN** the view explains the lower declaration and both values without asserting incompatibility, required upgrade or policy violation

#### Scenario: Unsupported baseline comparison

- **WHEN** no expectation is selected, or the expectation has another component, unknown contract, range or incomparable identifier
- **THEN** the comparison is unassessed with the specific reason, not match or pass

#### Scenario: Complete comparisons name compatible expectations contract

- **WHEN** the operator inspects the evidence or comparison covered by this requirement
- **THEN** Every comparison SHALL identify its expected value, origin, component, fact kind and compatible contract scope. Unsupported or absent comparable expectations SHALL produce unassessed with a reason. A difference MUST NOT automatically become a security or policy violation.

### Requirement: Version evidence categories remain distinct

Declared, lock-resolved, evidenced installed, latest available and policy-required values SHALL remain distinct. Installed state, registry availability and policy requirements MUST remain unknown without supporting evidence. Manifest baseline MUST NOT substitute for a latest CLI version or universal requirement.
<!-- pdac:cite id="FR-CONFIGURATION-DRIFT" digest="sha256:abb59ea4de5de304e41806960f9d7cc593cd9086082a2c04ce7fe1b4dc67eff7" -->

#### Scenario: Declarations do not establish runtime or policy

- **WHEN** only package declarations and a lockfile are observed
- **THEN** declared and lock-resolved values are visible while installed, latest available and required-by-policy are explicitly unknown with reasons

#### Scenario: Independently evidenced registry value

- **WHEN** a latest-available value is presented by a supported future source
- **THEN** it has its own registry origin and observation time and is not promoted to a policy requirement

### Requirement: Coverage freshness and applicability are explained

Evidence states SHALL have reasons, independent coverage and freshness, and no inference of non-adoption from missing access.
<!-- pdac:cite id="BR-EXPLAINABLE-SIGNALS" digest="sha256:8b54c80507f22f6883aaad91a53f479da5892bcc8def4f0e836076d1cf2063d5" -->
<!-- pdac:cite id="FR-PROJECT-DISCOVERY" digest="sha256:fb93a1c32232be6a679aa7aa074e9b6c0efd871cd48592b6b8d56996c36a2382" -->

#### Scenario: Conflicting declarations

- **WHEN** two comparable source declarations disagree
- **THEN** both remain inspectable and the view identifies the conflict without hiding either source

#### Scenario: Stale partial observation

- **WHEN** a partially collected observation ages or its refresh fails while authority remains verified
- **THEN** it is both incomplete and stale with original source revisions and observation times, never newly observed or passing

#### Scenario: Inaccessible configuration

- **WHEN** a file is denied, unavailable or not distinguishable from missing
- **THEN** the reason and unknown coverage are visible without asserting non-adoption or hiding the configured repository

#### Scenario: No applicable package within an evidenced scope

- **WHEN** a supported complete root npm declaration contains none of the inspected packages in supported dependency sections
- **THEN** that root npm package scope is not applicable with the declaration as evidence
- **THEN** the view does not extend that finding to nested projects, other ecosystems or Lighthouse adoption generally

#### Scenario: Read limit reached

- **WHEN** the bounded inspection cannot collect all requested supported sources
- **THEN** collected facts remain inspectable with incomplete coverage and the limit reason, not an exhaustive result

#### Scenario: Complete coverage freshness and applicability are explained contract

- **WHEN** the operator inspects the evidence or comparison covered by this requirement
- **THEN** The system SHALL distinguish unknown, conflicting, incomplete, stale, unavailable, unsupported and not-applicable evidence with reasons. Coverage and freshness SHALL be independent. Inaccessible or absent files MUST NOT prove non-adoption, and not-applicable MUST require supported applicability evidence.

### Requirement: Declared exceptions do not imply validity

Exception declarations SHALL retain their sources; validity remains unsupported/unassessed without a released shared contract.
<!-- pdac:cite id="BR-EXPLAINABLE-SIGNALS" digest="sha256:8b54c80507f22f6883aaad91a53f479da5892bcc8def4f0e836076d1cf2063d5" -->
<!-- pdac:cite id="FR-CONFIGURATION-DRIFT" digest="sha256:abb59ea4de5de304e41806960f9d7cc593cd9086082a2c04ce7fe1b4dc67eff7" -->

#### Scenario: Intentional exception without validation contract

- **WHEN** source-linked evidence declares an intentional version exception
- **THEN** the operator can inspect its declaration and source, validity reads unsupported/unassessed, and the difference remains visible
- **THEN** no locally invented owner, reason or review-date rule is enforced

#### Scenario: Complete declared exceptions do not imply validity contract

- **WHEN** the operator inspects the evidence or comparison covered by this requirement
- **THEN** Declared exception evidence SHALL remain inspectable with its source. Without a released shared validity contract, validity SHALL be unsupported/unassessed. The system MUST NOT invent exception requirements, call an exception valid or suppress an observed difference.

### Requirement: Unsupported contracts retain honest source facts

The system SHALL use released definitions, expose incompatible contracts and distinguish raw evidence from validated facts.
<!-- pdac:cite id="FR-CONFIGURATION-DRIFT" digest="sha256:abb59ea4de5de304e41806960f9d7cc593cd9086082a2c04ce7fe1b4dc67eff7" -->
<!-- pdac:cite id="BR-EXPLAINABLE-SIGNALS" digest="sha256:8b54c80507f22f6883aaad91a53f479da5892bcc8def4f0e836076d1cf2063d5" -->

#### Scenario: Released SDLC contract conflicts with its schema

- **WHEN** the supplied SDLC 0.22.4 contract includes asset keys forbidden by its declared closed schema
- **THEN** compatibility is unsupported/incompatible with the offending paths explained, while raw package/contract identities remain inspectable
- **THEN** the consumer neither relaxes the schema nor uses that contract to validate a policy comparison

#### Scenario: Unknown schema version

- **WHEN** a repository declares an unsupported source schema version or contract combination
- **THEN** its source remains available as unsupported evidence and no validated interpretation or comparison is inferred

#### Scenario: Complete unsupported contracts retain honest source facts contract

- **WHEN** the operator inspects the evidence or comparison covered by this requirement
- **THEN** The system SHALL use released source definitions and expose unsupported schema or contract combinations without silently accepting them. An artifact incompatible with its declared schema SHALL be identified as incompatible; available raw source facts MUST remain distinct from validated facts.

### Requirement: Safe read-only evidence navigation

Operators SHALL be able to open exact revision-bound configuration evidence and explicitly source-linked maintenance work through authorized readers or safe repository-bound GitHub links. Viewing, refreshing, selecting a comparison or navigating MUST NOT upgrade, write configuration, create issues or dispatch work. Missing linkage MUST NOT prove absence of work.
<!-- pdac:cite id="FR-CONFIGURATION-DRIFT" digest="sha256:abb59ea4de5de304e41806960f9d7cc593cd9086082a2c04ce7fe1b4dc67eff7" -->

#### Scenario: Exact evidence and linked work

- **WHEN** a fact contains revision-bound configuration provenance and a source explicitly links relevant maintenance work
- **THEN** the operator can open the exact configuration revision and the existing work through safe authorized navigation
- **THEN** inspection emits no source mutation or workflow request

#### Scenario: No or unsafe maintenance link

- **WHEN** linkage is missing or contains an unsafe URL
- **THEN** the view says no linked work was observed or explains the rejected link without asserting no work exists, executing source text or enabling unsafe navigation

### Requirement: Authority changes invalidate inspection

Inspection SHALL stay within configured repositories and the effective connected-user read scope. Current authority MUST precede reuse. Account, session, connection or repository authority changes MUST clear affected projections and reject late responses from the former scope; credentials MUST remain server-side.
<!-- pdac:cite id="QR-EXECUTION-SECURITY" digest="sha256:314e97415b3211a4f4e6187bb16495661d31129062e6bda38ceb8a2a4d4e9790" -->

#### Scenario: Lost or changed source authority

- **WHEN** the source account changes, the session ends, access is denied or authority cannot be verified
- **THEN** dependent inspection is blocked and affected facts and baseline selections are removed rather than reused as stale evidence
- **THEN** an earlier request completing later cannot repopulate the cleared scope

#### Scenario: Unconfigured repository

- **WHEN** a client selects an unconfigured repository or tries a mutating method on the inspection endpoint
- **THEN** the request is rejected without expanding discovery or producing a repository mutation

### Requirement: Accessible explainable inspection

Inspection SHALL support keyboard and narrow-screen use with visible focus and explanations beyond color.
<!-- pdac:cite id="QR-MOBILE-OPERATIONS" digest="sha256:2485b5877f0872af3dbdaa0a7ac00ac7651b28173adf6e3cf8d9ed643029bb35" -->
<!-- pdac:cite id="BR-EXPLAINABLE-SIGNALS" digest="sha256:8b54c80507f22f6883aaad91a53f479da5892bcc8def4f0e836076d1cf2063d5" -->

#### Scenario: Keyboard and narrow-screen evidence review

- **WHEN** the operator inspects the eight fixture states mapped to proposal verification group VG-8, selects a comparison, refreshes and opens evidence by keyboard at desktop and 390px widths in light and dark themes
- **THEN** text explains each state, controls retain visible accessible focus and long paths, values and revisions do not make the page overflow
- **THEN** refresh follows the same focused source identity or a persistent inspection control if that source disappears, without stealing focus from unrelated controls

#### Scenario: Complete accessible explainable inspection contract

- **WHEN** the operator inspects the evidence or comparison covered by this requirement
- **THEN** Inspection controls, state explanations and evidence links SHALL be keyboard operable with visible focus, readable without color alone and usable on a narrow viewport. Source identity and accessible focus SHALL survive refresh without unrelated focus theft.
