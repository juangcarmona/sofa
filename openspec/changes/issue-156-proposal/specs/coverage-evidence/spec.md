# Coverage evidence spec delta

<!-- pdac-scope: cited -->

## ADDED Requirements

### Requirement: Coverage uses exact CI source identity

Coverage SHALL count only reports verified against the exact repository, commit, CI run and attempt. Missing or mismatched reports SHALL remain unknown, never zero or passing. Coverage MUST NOT gate CI.
<!-- pdac:cite id="FR-PROJECT-HEALTH" digest="sha256:00496ba5f763ca6f4062a45e000bda8fb429cfd674b0e638cd59fb3858449130" -->
<!-- pdac:cite id="BR-EXPLAINABLE-SIGNALS" digest="sha256:8b54c80507f22f6883aaad91a53f479da5892bcc8def4f0e836076d1cf2063d5" -->

#### Scenario: Source evidence matches the producing attempt

- **WHEN** source evidence establishes that a supported report belongs to the configured repository, exact commit, CI run and attempt
- **THEN** the observed coverage may be displayed with those source identities and remains informational, without changing CI success or failure

#### Scenario: Missing or mismatched attempt evidence

- **WHEN** the report is absent, any repository/commit/run identity differs, or the exact attempt cannot be established
- **THEN** coverage is shown as unknown, never as zero or passing, and contributes no accepted measurement
