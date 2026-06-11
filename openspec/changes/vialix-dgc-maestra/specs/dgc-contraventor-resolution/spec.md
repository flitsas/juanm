## ADDED Requirements

### Requirement: Scheduled contraventor association

The system SHALL run between one and three daily time windows to query external registry by placa and infraction date and associate contraventor when a match exists.

#### Scenario: Automatic association in window

- **WHEN** a configured execution window starts and the external API returns a match
- **THEN** the comparendo is linked to the resolved contraventor record

### Requirement: Manual contingency form

The system SHALL allow operators to manually capture Nombre, Cédula, and Correo when automatic association fails.

#### Scenario: Manual contraventor save

- **WHEN** the operator submits the manual contingency form with required fields
- **THEN** the contraventor data is stored on the comparendo
