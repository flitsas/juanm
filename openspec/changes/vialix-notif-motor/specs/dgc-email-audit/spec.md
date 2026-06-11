# dgc-email-audit (delta)

## MODIFIED Requirements

### Requirement: Email log write ownership

DGC module SHALL remain read-only for `dgc.email_logs`. NOTIF module SHALL be the sole writer per `contracts/dgc/email-log-write-contract.md`.

#### Scenario: DGC cannot insert logs

- **WHEN** DGC API receives a request to create email log
- **THEN** the request is rejected or endpoint does not exist

#### Scenario: NOTIF writes after send

- **WHEN** NOTIF completes an outbound email
- **THEN** DGC read endpoints return the new log entry for the comparendo
