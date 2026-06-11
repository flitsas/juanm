# notif-email-templates

## ADDED Requirements

### Requirement: White-label email templates

The system SHALL support CRUD for HTML email templates with optional banner and footer image URLs (PNG/JPG) per tenant (RF04, RF12 partial).

#### Scenario: Create template with banner

- **WHEN** Tenant Admin creates a template with HTML body and banner image
- **THEN** the template is stored and retrievable with all metadata

### Requirement: Template variables

The system SHALL support merge variables for comparendo fields (numero, placa, infractor, dias_restantes, etc.) rendered at send time.

#### Scenario: Render template preview

- **WHEN** Tenant Admin requests preview with sample comparendo data
- **THEN** merged HTML is returned without enqueueing a send
