# reglas-evaluation-engine

## ADDED Requirements

### Requirement: Configurable scheduler

The system SHALL run automatic evaluation on a configurable interval per tenant without manual intervention.

### Requirement: Mass evaluation

On each run, the system SHALL evaluate eligible comparendos from DGC against all active rules and record matches.

#### Scenario: Multiple rules per comparendo

- **WHEN** one comparendo satisfies two different rules
- **THEN** both matches are recorded

#### Scenario: Skip already processed

- **WHEN** a comparendo was already successfully processed under a rule
- **THEN** that rule-comparendo pair is skipped on subsequent runs
