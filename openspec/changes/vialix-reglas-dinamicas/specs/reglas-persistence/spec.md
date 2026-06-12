# reglas-persistence

## ADDED Requirements

### Requirement: Schema reglas multi-tenant

The system SHALL persist dynamic rules engine data in PostgreSQL schema `reglas` with `tenant_id` on every table and RLS policies.

#### Scenario: Migration creates core tables

- **WHEN** the REGLAS migration runs
- **THEN** tables `dynamic_rule`, `rule_condition`, `rule_execution_run`, `rule_processing_record`, and `secretariat_contact` exist with RLS enabled

### Requirement: Anti-duplicidad por regla-comparendo

The system SHALL enforce at most one successful processing record per `(tenant_id, dynamic_rule_id, comparendo_id)`.

#### Scenario: Duplicate success rejected

- **WHEN** a second successful processing is inserted for the same rule-comparendo pair
- **THEN** the database rejects the operation via unique constraint
