# ADR-0003: Bounded context REGLAS vialix — motor de reglas dinámicas DP

**Fecha**: 2026-06-10  
**Status**: Propuesto  
**Deciders**: Willyn Londoño Calle (aprobación diseño), Líder Técnico FLIT (promoción a Aceptado)  
**Tags**: arquitectura, backend, frontend, modulo-reglas, vialix, postgresql, reglas-dinamicas

## Contexto

El Feature ADO **#9710** ([DGC-REGLAS]) implementa el motor de reglas dinámicas que evalúa comparendos DGC (#9560), genera PDF vía plantillas GDC (#9564) y envía correo a secretarías usando capacidades NOTIF (#9563). Incluye 8 HUs (#9759–#9766): persistencia, APIs CRUD/ejecución, orquestación PDF+correo, contactos/RBAC y UI Admin.

Restricciones:

- Entrega **rama única** `feature/AB-9710-reglas-dinamicas`, commits `HU####:`, PR a `develop` sin merge automático.
- #9564 (plantillas PDF) en curso por otro equipo — contrato `IGdcPdfTemplateRenderer` con stub hasta merge.
- Diseño: `docs/designs/9710-reglas-dinamicas.md`.

## Decisión

Implementar **Gdc.Modules.Reglas** como vertical slice con schema **`reglas`**, evaluador `ReglasRuleEvaluator`, scheduler `ReglasExecutionHostedService`, anti-duplicidad en `rule_processing_records` y orquestación PDF+correo vía contratos cross-module.

## Alternativas consideradas

### Opción 1: Vertical slice REGLAS + schema dedicado (recomendada)

**Pros:**
- Separa semánticamente reglas DP (secretaría, PDF) de `notif.notification_rules` (correo al contraventor).
- Alineado a convenciones FLIT (schema por dominio, RLS, tenant_id).
- Una rama feature-batch coherente con #9560 y #9563.

**Cons:**
- Integraciones cross-module (DGC lectura, GDC PDF, NOTIF adjuntos).
- Stub GDC hasta merge de #9564.

**Esfuerzo:** L  
**Riesgos:** Solapamiento conceptual NOTIF vs REGLAS; adjuntos en NOTIF.

### Opción 2: Extender `notif.notification_rules`

**Pros:**
- Reutiliza worker y cola email existentes.

**Cons:**
- Dominios distintos (disparadores, salidas, RBAC).
- Mezcla reglas contraventor con reglas secretaría en un mismo modelo.

**Esfuerzo:** M  
**Riesgos:** Deuda técnica y confusión en UI/operaciones.

### Opción 3: Reglas embebidas en schema `dgc`

**Pros:**
- Acceso directo a comparendos sin reader.

**Cons:**
- Viola ownership por bounded context.
- Acopla evaluación batch al módulo maestra.

**Esfuerzo:** M  
**Riesgos:** Migraciones DGC bloquean evolución REGLAS.

## Consecuencias

- Schema `reglas`: `dynamic_rules`, `rule_conditions`, `rule_execution_runs`, `rule_processing_records`, `secretariat_contacts`.
- Unique parcial `(tenant_id, dynamic_rule_id, comparendo_id)` donde `status = 'success'`.
- Frontend: `frontend/src/features/reglas/` bajo shell Admin.
- OpenAPI extendido con tag `Reglas`.

## Referencias

- `docs/designs/9710-reglas-dinamicas.md`
- `openspec/changes/vialix-reglas-dinamicas/`
- ADR-0001 (DGC), ADR-0002 (NOTIF)
