# reglas-rule-api

## ADDED Requirements

### Requirement: CRUD dynamic rules

The system SHALL expose REST APIs to create, read, update, and delete dynamic rules with name, description, and active/inactive state.

#### Scenario: Only active rules execute

- **WHEN** a rule is marked inactive
- **THEN** it is excluded from scheduled and manual evaluation runs

### Requirement: Condition builder

The system SHALL support nested AND/OR condition trees on DGC comparendo fields with operators equal, not equal, greater, less, contains, and not contains.

#### Scenario: Invalid rule activation blocked

- **WHEN** a user activates a rule without conditions or without an associated PDF template
- **THEN** the API returns a validation error

### Requirement: Template and email binding

Each rule SHALL reference a GDC PDF template id and configurable email subject/body for secretariat dispatch.
