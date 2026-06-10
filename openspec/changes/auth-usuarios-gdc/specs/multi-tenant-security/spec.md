## ADDED Requirements

### Requirement: Tenant-scoped data access

The system SHALL restrict API data queries to the authenticated user's tenant id unless Super Admin policy explicitly allows cross-tenant operations.

#### Scenario: Tenant user reads own data

- **WHEN** authenticated tenant user requests tenant-scoped resource
- **THEN** only records matching user's tenant id are returned

#### Scenario: Cross-tenant access attempt

- **WHEN** tenant user requests resource from another tenant id
- **THEN** the system returns HTTP 403 or empty result without leaking existence
