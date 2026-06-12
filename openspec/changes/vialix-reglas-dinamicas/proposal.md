## Why

La cadena vialix requiere automatizar Derechos de Petición sobre comparendos SIMIT mediante reglas parametrizables. DGC (#9560) ya expone la maestra de comparendos y NOTIF (#9563) el despacho de correos, pero no existe un motor que evalúe condiciones dinámicas, genere PDFs desde plantillas GDC y orqueste el envío a secretarías con trazabilidad anti-duplicidad (RF01–RF26). Sin REGLAS (#9710) el proceso sigue siendo manual.

## What Changes

- Nuevo bounded context **REGLAS** en `services/core-api/` con schema PostgreSQL `reglas`.
- Entidades: `dynamic_rule`, `rule_condition` (árbol AND/OR), `rule_execution_run`, `rule_processing_record`, `secretariat_contact`.
- APIs REST: CRUD reglas + constructor de condiciones, scheduler, ejecución manual/automática, orquestación PDF+correo, contactos secretaría, logs y métricas.
- Motor programado (`IHostedService`) que evalúa comparendos DGC, genera PDF vía contrato GDC-PLANTILLAS (#9564), despacha correo con adjunto vía NOTIF (#9563).
- Módulo frontend `frontend/src/features/reglas/` con constructor visual, scheduler/resultados y logs/contactos.
- Contratos OpenAPI en `contracts/openapi/core-api.v1.yaml`.
- Entrega modo **feature-batch**: rama `feature/AB-9710-reglas-dinamicas`, commits `HU####:`, PR a `develop` sin merge automático.

## Capabilities

### New Capabilities

- `reglas-persistence`: Schema `reglas` multi-tenant con RLS; tablas regla, condición, corrida, procesamiento y contactos (HU #9759).
- `reglas-rule-api`: CRUD reglas dinámicas, constructor de condiciones AND/OR, activación y vínculo plantilla PDF + plantilla correo (HU #9760).
- `reglas-evaluation-engine`: Scheduler configurable, evaluación masiva de comparendos, coincidencias y anti-duplicidad por par regla-comparendo (HU #9761).
- `reglas-orchestration`: Generación PDF con tags dinámicos, envío individual a secretaría vía NOTIF con adjunto, registro de procesamiento y métricas de corrida (HU #9762).
- `reglas-secretariat-access`: Directorio de contactos por secretaría, resolución de destinatario y control de acceso por rol (HU #9763).
- `reglas-frontend`: UI constructor de reglas, scheduler/resultados y logs/contactos secretaría (HUs #9764–#9766).

### Modified Capabilities

- _(ninguna en `openspec/specs/` — capacidades nuevas)_

## Impact

| Área | Impacto |
|------|---------|
| `services/core-api/` | Nuevo módulo `Gdc.Modules.Reglas` |
| `frontend/` | Ruta admin reglas bajo shell |
| `contracts/openapi/core-api.v1.yaml` | Endpoints `/api/v1/reglas/*` |
| PostgreSQL | Schema `reglas`; lectura `dgc.comparendos`; integración NOTIF y GDC plantillas |
| ADO | Feature #9710, HUs #9759–#9766 |
| Dependencias upstream | DGC maestra (#9560 mergeado), NOTIF (#9563 mergeado) |
| Dependencia paralela | GDC plantillas PDF (#9564 — otro equipo); contrato/interfaz mínima o stub hasta merge |
| Fuera de alcance | Versionamiento histórico de reglas, aprobación manual previa, formatos ≠ PDF, correos agrupados |
