## ADDED Requirements

### Requirement: Multi-tenant comparendo schema

The system SHALL persist comparendos in PostgreSQL schema `dgc` with `tenant_id` on every business table and row-level security enabled.

#### Scenario: Migration creates DGC entities

- **WHEN** the DGC schema migration is applied for an active tenant
- **THEN** tables `comparendo`, `contraventor`, `ocr_lote`, `ocr_item`, and `email_log` exist with `tenant_id` and RLS policies

### Requirement: Duplicate comparendo prevention

The system SHALL reject insertion of a comparendo when the same `numero_comparendo` already exists for the tenant.

#### Scenario: Unique constraint violation

- **WHEN** a second comparendo with an existing `numero_comparendo` is inserted for the same tenant
- **THEN** the database rejects the operation via unique constraint on `(tenant_id, numero_comparendo)`
