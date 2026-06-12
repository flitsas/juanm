# gdc-persistence

Schema PostgreSQL `gdc` para plantillas PDF y derechos de petición multi-tenant.

## Requirements

### Requirement: Schema aislado gdc

El sistema MUST crear schema `gdc` separado de `dgc` (comparendos).

#### Scenario: Migración inicial

- **WHEN** se ejecuta `GdcPlantillasInitialSchema`
- **THEN** existen tablas `pdf_template`, `pdf_template_field`, `derecho_peticion` en schema `gdc`

### Requirement: RLS por tenant

Todas las tablas `gdc.*` MUST tener RLS con `app.current_tenant_id`.

#### Scenario: Aislamiento tenant

- **WHEN** un tenant consulta plantillas
- **THEN** solo ve registros con su `tenant_id`

### Requirement: Relación DP 1:N comparendo

`gdc.derecho_peticion` MUST referenciar `dgc.compareendos.id` sin duplicar master data.

#### Scenario: Múltiples DP

- **WHEN** un comparendo tiene 3 DPs
- **THEN** la grilla lista 3 registros con estados independientes
