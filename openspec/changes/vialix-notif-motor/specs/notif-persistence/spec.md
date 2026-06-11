# notif-persistence

## ADDED Requirements

### Requirement: Schema notif multi-tenant

The system SHALL persist notification engine data in PostgreSQL schema `notif` with `tenant_id` on every table and RLS policies enforcing `tenant_id = current_setting('app.tenant_id')::uuid`.

#### Scenario: Migration creates tables

- **WHEN** the NOTIF migration runs on a fresh database
- **THEN** tables `email_provider_config`, `email_template`, `notification_rule`, and `email_queue` exist under schema `notif` with RLS enabled

### Requirement: Email provider config entity

The system SHALL store one active email provider configuration per tenant with fields: provider type (`api`, `sendgrid`, `flit_mail`), encrypted credentials, from address, and active flag.

#### Scenario: Unique active provider per tenant

- **WHEN** a tenant configures a new active provider
- **THEN** any previous active provider for that tenant is deactivated

### Requirement: Email queue states

The system SHALL track queue items with status in (`pending`, `processing`, `sent`, `failed`) and FK to comparendo, rule, and template.

#### Scenario: Queue item lifecycle

- **WHEN** a rule triggers for a comparendo
- **THEN** a queue item is created with status `pending`
