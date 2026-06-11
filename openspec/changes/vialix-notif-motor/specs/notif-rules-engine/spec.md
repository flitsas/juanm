# notif-rules-engine

## ADDED Requirements

### Requirement: Notification rules

The system SHALL support rules linking a template to a trigger: chronological (days from comparendo creation or notification date) or state change on comparendo (RF05–RF06).

#### Scenario: Chronological rule

- **WHEN** a rule is configured for 3 days after `fecha_notificacion`
- **THEN** eligible comparendos are evaluated on each dispatch cycle

### Requirement: Dispatch queue and switch

The system SHALL maintain an email queue and a tenant-level On/Off switch that pauses outbound sends when Off (RF07–RF08).

#### Scenario: Switch off pauses dispatch

- **WHEN** tenant switch is Off
- **THEN** queue items remain `pending` and no emails are sent

### Requirement: Write DGC email log

The system SHALL insert a row into `dgc.email_logs` after each successful or failed send per `contracts/dgc/email-log-write-contract.md`.

#### Scenario: Successful send logs audit row

- **WHEN** an email is delivered
- **THEN** `dgc.email_logs` contains sent_at, origen, destino, tipo_alerta, estado_entrega, and html_evidencia
