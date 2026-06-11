# notif-frontend

## ADDED Requirements

### Requirement: Admin companies UI

The frontend SHALL provide Super Admin UI to manage companies aligned with `flitready-suite` admin module (RF01).

#### Scenario: List companies

- **WHEN** Super Admin navigates to companies section
- **THEN** a table lists tenants with empty, loading, error, and data states

### Requirement: Provider configuration UI

The frontend SHALL provide forms to configure email provider and test connection (RF03, RF09).

#### Scenario: Provider form validation

- **WHEN** required fields are missing
- **THEN** inline validation prevents save

### Requirement: Template editor UI

The frontend SHALL provide template editor with HTML editing and preview (RF04).

#### Scenario: Preview template

- **WHEN** user clicks preview
- **THEN** merged HTML renders in a sandboxed preview panel

### Requirement: Rules and switch UI

The frontend SHALL provide rules management and global On/Off switch (RF05–RF08).

#### Scenario: Toggle switch

- **WHEN** user toggles dispatch switch
- **THEN** API updates tenant switch and UI reflects new state
