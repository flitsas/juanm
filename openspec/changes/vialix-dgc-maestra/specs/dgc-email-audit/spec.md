## ADDED Requirements

### Requirement: Email tracking log read API

The system SHALL expose a read-only email log per comparendo with Fecha/Hora, origen, destino, CC, tipo de alerta, and delivery estado (Entregado / No Entregado / Fallido).

#### Scenario: List email log entries

- **WHEN** an operator requests the email log for a comparendo with registered sends
- **THEN** the API returns tracking metadata for each send

### Requirement: HTML evidence renderer data

The system SHALL return the full HTML structure of a sent message when evidence is requested for a log entry.

#### Scenario: HTML evidence detail

- **WHEN** the client requests detail for a log entry with stored evidence
- **THEN** the API returns the complete HTML body of the message sent to the contraventor
