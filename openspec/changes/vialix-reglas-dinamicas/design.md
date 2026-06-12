## Context

- **Feature ADO:** #9710 [DGC-REGLAS] Motor de Reglas Dinámicas — Active.
- **HUs:** #9759 (schema) → #9760 (CRUD reglas) → #9761 (scheduler/motor) → #9762 (PDF+correo) → #9763 (contactos/RBAC) → #9764–#9766 (frontend).
- **Cadena vialix:** DGC (#9560) master data → NOTIF (#9563) correos → GDC (#9564) plantillas PDF → **REGLAS (#9710)** orquestación DP.
- **#9564 en curso por otro equipo:** REGLAS consume plantillas vía contrato `IGdcPdfTemplateRenderer` (o equivalente); implementación stub hasta que #9564 esté en `develop`.
- **Modo entrega:** `implement-feature.md` — rama `feature/AB-9710-reglas-dinamicas`, commits por HU, PR sin merge automático.

## Goals / Non-Goals

**Goals:**

- Parametrizar reglas dinámicas multi-tenant con condiciones compuestas sobre campos de la maestra DGC (RF01–RF05).
- Ejecutar motor programado y manual con evaluación masiva, coincidencias y anti-duplicidad por regla (RF06–RF10, RF20–RF21).
- Orquestar PDF + correo individual a secretaría con trazabilidad completa (RF11–RF19, RF22–RF25).
- Directorio de contactos secretaría y RBAC (RF16–RF17, RF26).
- UI operativa alineada a Flit Ready (constructor, scheduler, logs).

**Non-Goals:**

- Versionamiento histórico de reglas.
- Aprobación manual previa al envío.
- Formatos distintos a PDF.
- Correos agrupados (un solo correo con N comparendos).
- Implementar plantillas PDF (responsabilidad #9564).

## Decisions

### D1 — Bounded context `Gdc.Modules.Reglas` + schema `reglas`

**Decisión:** Vertical slice con schema dedicado `reglas`, separado de `notif.notification_rule` (reglas de correo al contraventor ≠ reglas DP).

**Alternativas:**

- *Reutilizar `notif.notification_rule`* — rechazado: dominios, disparadores y salidas distintas (email contraventor vs PDF+secretaría).
- *Tablas en `dgc`* — rechazado: viola ownership por bounded context.

### D2 — Árbol de condiciones serializado + evaluador en memoria

**Decisión:** Persistir condiciones como filas `rule_condition` con `parent_id`, `logic_group`, `field_key`, `operator`, `value`; evaluador recursivo `ReglasRuleEvaluator` sobre `DgcComparendoSnapshot`.

**Operadores:** `eq`, `neq`, `gt`, `lt`, `contains`, `not_contains` (mapeo UI español).

### D3 — Scheduler `ReglasExecutionHostedService`

**Decisión:** `IHostedService` con intervalo configurable por tenant (`reglas_job_config` o campo en tenant settings), similar a `NotifDispatchHostedService`.

**Flujo por ciclo:** cargar reglas activas → cargar comparendos elegibles → evaluar → para cada coincidencia no procesada → orquestar PDF+correo → persistir `rule_processing_record`.

### D4 — Integraciones cross-module

| Integración | Contrato | Dirección |
|-------------|----------|-----------|
| DGC comparendos | `IDgcComparendoReader` (existente) | Lectura |
| GDC plantillas PDF | `IGdcPdfTemplateRenderer` (nuevo) | Lectura + render PDF bytes |
| NOTIF envío adjunto | `IReglasEmailDispatcher` → extensión NOTIF o `IEmailSender` con attachment | Escritura cola/envío |
| AUTH roles | `IUserRoleContext` + políticas endpoint | Lectura |

**Mitigación #9564:** Stub `GdcPdfTemplateRendererStub` devuelve PDF mínimo en DEV hasta merge de plantillas reales.

### D5 — Anti-duplicidad RF21

**Decisión:** Unique constraint `(tenant_id, dynamic_rule_id, comparendo_id)` en `rule_processing_record` donde `status = 'success'`.

### D6 — Frontend feature-sliced

**Decisión:** `frontend/src/features/reglas/` con tabs Constructor | Ejecución | Logs/Contactos; TanStack Query; 4 estados UI.

## Risks / Trade-offs

| Riesgo | Mitigación |
|--------|------------|
| #9564 no mergeado a tiempo | Contrato `IGdcPdfTemplateRenderer` + stub; tests con PDF fixture |
| NOTIF sin soporte adjuntos hoy | Extender `IEmailSender` o cola dedicada REGLAS; coordinar con equipo NOTIF |
| Evaluación masiva lenta | Batch de 50 comparendos/ciclo; índices en `rule_processing_record` |
| Solapamiento semántico NOTIF vs REGLAS | Documentar en ADR; nombres de dominio explícitos en UI |
| RLS sin `SET app.current_tenant_id` | Filtros EF + `ITenantContext` (deuda conocida vialix) |

## Migration Plan

1. Migración EF `ReglasInitialSchema` con RLS y constraints.
2. Seed `reglas_job_config` para tenant dev.
3. Feature flag `Reglas:Enabled` en `appsettings`.
4. Rollback: deshabilitar hosted service; schema permanece (no drop en DEV sin OK LT).

## Open Questions

1. ¿IDs exactos de HUs GDC plantilla en #9564 (#9752, #9754) ya en `develop`? — verificar antes de HU #9762.
2. ¿NOTIF soportará adjuntos en este sprint o REGLAS implementa adapter SMTP directo?
3. ¿Frecuencia scheduler global vs por-tenant en MVP?
