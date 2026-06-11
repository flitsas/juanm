## ADDED Requirements

### Requirement: Super Admin invites user to tenant

The system SHALL allow Super Admin to register a user email linked to an existing tenant from the master tenant list.

#### Scenario: Invitation sent

- **WHEN** Super Admin submits valid email and tenant id
- **THEN** a pending user record is created and activation email is sent

#### Scenario: Duplicate email in tenant

- **WHEN** email already exists for the same tenant
- **THEN** the system returns HTTP 409 with conflict message

### Requirement: Account activation within 48 hours

The system SHALL allow invited users to set initial password using a single-use token valid for 48 hours.

#### Scenario: Valid activation token

- **WHEN** user opens activation link within 48 hours and sets a compliant password
- **THEN** account becomes active and token is consumed

#### Scenario: Expired activation token

- **WHEN** user opens activation link after 48 hours
- **THEN** the system returns HTTP 410 with expired token message
