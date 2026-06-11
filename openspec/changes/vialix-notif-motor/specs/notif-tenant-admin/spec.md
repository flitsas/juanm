# notif-tenant-admin

## ADDED Requirements

### Requirement: Super Admin company CRUD

The system SHALL expose REST endpoints for Super Admin to create, read, update, and deactivate tenant companies (RF01).

#### Scenario: Create company

- **WHEN** Super Admin POSTs a valid company payload
- **THEN** a new tenant record is created and returned with id

### Requirement: Tenant profile self-service

The system SHALL allow Tenant Admin to update their own company profile (name, contact, branding metadata) without accessing other tenants (RF02).

#### Scenario: Tenant isolation on profile update

- **WHEN** Tenant Admin updates profile
- **THEN** only the authenticated tenant's profile is modified
