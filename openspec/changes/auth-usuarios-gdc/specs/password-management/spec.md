## ADDED Requirements

### Requirement: Self-service password recovery

The system SHALL allow users to request password reset via email without revealing whether the email exists.

#### Scenario: Reset email requested

- **WHEN** user submits email on forgot-password form
- **THEN** system responds HTTP 202 always and sends reset link if account exists

### Requirement: Super Admin password override

The system SHALL allow Super Admin to set a new password for an active user from admin panel.

#### Scenario: Admin resets password

- **WHEN** Super Admin submits new password for active user
- **THEN** password is updated and user must login with new credentials

### Requirement: Failed login lockout suggestion

The system SHALL track failed login attempts and suggest password change after threshold.

#### Scenario: Threshold exceeded

- **WHEN** user exceeds configured failed login attempts
- **THEN** login is temporarily blocked and UI suggests password change or recovery
