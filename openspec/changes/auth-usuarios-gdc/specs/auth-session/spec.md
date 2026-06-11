## ADDED Requirements

### Requirement: User login with email and password

The system SHALL authenticate users with email and password and return a JWT containing user id, tenant id, and role claims.

#### Scenario: Successful login

- **WHEN** a user submits valid email and password for an active account
- **THEN** the system returns HTTP 200 with access token and user profile summary

#### Scenario: Invalid credentials

- **WHEN** a user submits wrong password
- **THEN** the system returns HTTP 401 with a generic error message without revealing which field failed

### Requirement: Secure logout

The system SHALL invalidate the active session token and prevent further use of the same token.

#### Scenario: Logout success

- **WHEN** an authenticated user calls logout
- **THEN** the token is invalidated and subsequent requests with that token return HTTP 401
