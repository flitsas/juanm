## ADDED Requirements

### Requirement: Interactive RBAC matrix by role

The system SHALL provide an API and UI to assign functional permissions to roles.

#### Scenario: Assign permission to role

- **WHEN** Super Admin toggles a permission for a role and saves
- **THEN** users with that role gain or lose access on next authorized request

#### Scenario: Unauthorized RBAC edit

- **WHEN** non-Super Admin attempts to modify role permissions
- **THEN** the system returns HTTP 403
