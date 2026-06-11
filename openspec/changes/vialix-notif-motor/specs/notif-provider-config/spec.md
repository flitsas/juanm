# notif-provider-config

## ADDED Requirements

### Requirement: Configure email provider

The system SHALL allow Tenant Admin to configure email provider type (API, SendGrid, FLIT Mail) with encrypted credential storage (RF03, RF09).

#### Scenario: Save SendGrid credentials

- **WHEN** Tenant Admin saves SendGrid API key and from address
- **THEN** credentials are encrypted at rest and provider type is `sendgrid`

### Requirement: Test provider connection

The system SHALL expose an endpoint to send a test email validating provider configuration.

#### Scenario: Successful test send

- **WHEN** Tenant Admin requests test send to a valid email
- **THEN** the system returns success and does not persist the test in production queue
