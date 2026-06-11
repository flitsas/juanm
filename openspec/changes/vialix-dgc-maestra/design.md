## Context

- **Feature ADO:** #9560 [DGC] Módulo Gestión Comparendos — Active, Sprint 2.
- **HUs:** #9735 (schema) → #9736 (OCR) → #9737 (maestra) → #9738 (contraventor) → #9739 (log correos) → #9740–#9742 (frontend).
- **Prototipo UI:** `flitready-suite/src/components/modules/dgc.tsx` + tokens `.lovable/memory/design/flit-ready.md`.
- **Backend actual:** `GdcDbContext` con schema `core` vacío; monorepo GDC 2.0 listo para vertical slices.
- **Cadena vialix:** DGC es master data → NOTIF (#9563) escribe logs → GDC (#9564) consume comparendos → REGLAS (#9710).
- **Modo entrega acordado:** rama `feature/AB-9560-dgc-maestra`, commits por HU, PR a `develop` sin merge automático (workflow `implement-feature.md`).

## Goals / Non-Goals

**Goals:**

- Persistir comparendos multi-tenant con unicidad por número de comparendo (RF03).
- Exponer maestra con 15 columnas y días restantes calculados (RF04–RF05).
- Flujo OCR con buffer editable pre-persistencia (RF01–RF02).
- Asociación automática y manual de contraventor (RF06–RF07).
- Consulta de auditoría de correos y evidencia HTML (RF08–RF10) vía contrato con NOTIF.
- UI operativa según prototipo Flit Ready.

**Non-Goals:**

- Integración SIMIT, planillas de pago, generación de DP.
- Motor de envío de correos (Feature #9563).
- Reglas dinámicas de automatización (#9710).
- Merge/deploy/Resolved automáticos en este ciclo.

## Decisions

### D1 — Schema PostgreSQL dedicado `dgc`

**Decisión:** Tablas de negocio bajo schema `dgc`, no `core`.

**Alternativas:**
- *Todo en `core`* — rechazado: acopla módulos y dificulta ownership por bounded context.
- *Schema por tenant* — rechazado: anti-patrón operativo en PostgreSQL FLIT.

**Rationale:** Alineado a convenciones FLIT de un schema por dominio; facilita RLS y permisos.

### D2 — Entidades principales

| Tabla | Propósito |
|-------|-----------|
| `dgc.comparendo` | Registro maestro de infracción (15 columnas derivadas + FKs) |
| `dgc.contraventor` | Persona responsable (nombre, documento, correo) — 1:N opcional histórico, 1:1 activo por comparendo |
| `dgc.ocr_lote` | Cabecera de carga masiva/individual |
| `dgc.ocr_item` | Buffer pre-guardado con JSON de campos OCR + estado (`pending`, `confirmed`, `rejected`, `duplicate`) |
| `dgc.descuento_matriz` | Parámetros global o por `secretaria_id` para cálculo días restantes |
| `dgc.email_log` | Proyección de lectura de envíos (escrita por NOTIF vía evento/API interna) |

**Constraint RF03:** `UNIQUE (tenant_id, numero_comparendo)` en `dgc.comparendo`.

**Columnas maestra (proyección API):** estado, numero_comparendo, infractor_nombre, documento, placa, infraccion_codigo, fecha_comparendo, fecha_notificacion, dias_restantes (calculado), secretaria_id, total_valor, estado_pago, contraventor_display, dp_referencia (nullable FK futuro), fuente (`ocr`, `manual`, `import`).

### D3 — RLS multi-tenant

**Decisión:** `tenant_id UUID NOT NULL` en todas las tablas `dgc.*`; políticas RLS `tenant_id = current_setting('app.tenant_id')::uuid`.

**Alternativas:** filtro solo en aplicación — rechazado por convenciones de datos FLIT.

### D4 — Arquitectura backend — Vertical Slice

```
services/core-api/src/
├── Gdc.Modules.Dgc/
│   ├── Domain/
│   ├── Application/
│   └── Infrastructure/
├── Gdc.Api/Endpoints/DgcEndpoints.cs
└── Gdc.Infrastructure/Persistence/GdcDbContext.cs  (+ DbSet Dgc)
```

**Patrones:** Result pattern, UUIDv7, FluentValidation, Minimal APIs, handlers por comando/query.

### D5 — OCR (Tesseract 5 en python-ml)

**Decisión:** Motor **Tesseract 5** (gratis, Apache 2.0) en `services/python-ml` (FastAPI). `core-api` consume vía `IOcrExtractor` → HTTP `POST /ocr/comparendo:extract`.

**Por qué Tesseract:**
- 100 % gratuito y open source, sin cuota ni API key.
- Ya previsto en arquitectura FLIT (`python-ml`: Tesseract + OpenCV).
- Suficiente para comparendos escaneados (PDF/PNG/JPG) con buffer humano RF02.

**Pipeline python-ml:**
1. PDF → `pdf2image` (Poppler) → PNG por página.
2. PNG/JPG → OpenCV (deskew, binarización, contraste).
3. Tesseract `lang=spa` (+ `eng` opcional) → texto crudo.
4. Parser heurístico (regex) → campos estructurados: `numero_comparendo`, `placa`, `documento`, `infractor`, `fecha`, `valor`, `infraccion`.
5. Respuesta JSON con `confidence` por campo + `raw_text` para auditoría.

**Fallback opcional (misma HU, no bloqueante):** EasyOCR si confianza Tesseract &lt; umbral en un campo crítico.

**Alternativas descartadas:**
- *Azure Document Intelligence* — tier gratis limitado; vendor lock-in.
- *Tesseract.NET embebido en core-api* — viable para MVP rápido, pero duplica responsabilidad OCR fuera de `python-ml` (contrario ADR servicios FLIT). Reservado solo si el equipo decide no levantar `python-ml` en este sprint.

**core-api:** solo orquesta; archivos en blob (`ocr_item.archivo_uri`); no ejecuta OCR localmente.

**Flujo:** Upload → `ocr_lote` + `ocr_item` → llamada python-ml → buffer editable → `POST confirm` → `comparendo`.

### D6 — Cálculo días restantes (RF05)

**Decisión:** Campo calculado en query handler, no columna persistida (excepto cache opcional).

```
dias_restantes = max(0, matriz.dias_descuento - (today - fecha_notificacion))
```

Matriz resuelta: secretaría específica > global del tenant.

### D7 — Planificador contraventor (RF06) + API placa+fecha (proveedor TBD)

**Decisión:** Job programado (1–3 ventanas CRON) + puerto `IExternalVehicleRegistry`. **Proveedor externo aún no definido** (Verifik, RUNT u otro).

**Scheduler (cerrado):** `IHostedService` + `PeriodicTimer` en `core-api`. Hangfire descartado en esta fase (1–3 ventanas simples, sin deps extra).

**Implementación por fases:**
- **Fase A (HU #9738):** `StubVehicleRegistry` (prod → `NotFound`) + `ContraventorAssociationHostedService` + endpoint manual RF07. Tests con fixture `Found`.
- **Fase B (futura):** adapter real cuando el equipo elija proveedor; swap en DI sin tocar dominio.

```csharp
Task<ContraventorLookupResult> LookupAsync(string placa, DateOnly fechaInfraccion, CancellationToken ct);
// Found | NotFound | ProviderUnavailable
```

Sin API real, **RF07 es el flujo operativo principal**; el job deja comparendos en `pendiente_contraventor` y registra `ultimo_intento_asociacion`.

Config futura: `VehicleRegistry:Provider`, `BaseUrl`, `ApiKey` (sin acoplar a `.env.verifik` hasta confirmar).

**Contingencia manual (RF07):** `PUT /comparendos/{id}/contraventor` — siempre disponible si stub/real no retorna match.

### D8 — Log de correos (RF08–RF10)

**Decisión:** `dgc.email_log` poblada por contrato interno desde NOTIF (#9563). DGC solo lectura.

Campos: `sent_at`, `origen`, `destino`, `cc`, `tipo_alerta`, `estado_entrega`, `html_evidencia` (TEXT, sanitizado en frontend con DOMPurify).

**Stub en #9739:** seed de datos de prueba hasta NOTIF disponible.

### D9 — Frontend

**Decisión:** Feature-sliced en `frontend/src/features/dgc/` — páginas bajo `app/(shell)/dgc/`.

Componentes mapeados desde prototipo:
- Tabla maestra → #9740
- Dialog carga masiva → #9741
- Sheet detalle + tabs → #9742

TanStack Query para listados; 4 estados UI (vacío, cargando, error, lleno).

### D10 — OpenAPI y versionado

Prefijo: `/api/v1/dgc`. Actualizar `contracts/openapi/core-api.v1.yaml` por HU que exponga endpoints nuevos.

### D11 — Git / entrega

| Elemento | Valor |
|----------|-------|
| Rama | `feature/AB-9560-dgc-maestra` |
| Commits | `HU9735: …`, `HU9736: …`, … |
| PR | Uno al cerrar lote de HUs del Feature |
| Merge | Manual — fuera de alcance agente |

## Risks / Trade-offs

| Riesgo | Mitigación |
|--------|------------|
| NOTIF no listo para escribir `email_log` | Stub + contrato documentado; HU #9739 con tests contra fixture |
| API externa contraventor inestable | Formulario manual RF07 siempre disponible; job con reintentos |
| PR > 800 líneas con 8 HUs | Priorizar backend en PR1; frontend PR2 sobre misma rama si LT aprueba |
| OCR calidad variable | Buffer obligatorio RF02; operador corrige antes de confirmar |
| `docs/database-conventions.md` ausente en repo | Seguir checklist `db-schema-validator` y plantilla de tabla de negocio del skill |

## Migration Plan

1. Migración `HU9735`: crear schema `dgc`, tablas, RLS, índices, seed matriz descuento demo.
2. APIs incrementales #9736–#9739 sin breaking changes.
3. Frontend #9740–#9742 consume APIs ya desplegables en DEV local.
4. Rollback: `Down()` de migración EF Core por versión; no deploy automático en este modo.

## Open Questions

1. ~~¿Servicio OCR concreto?~~ → **Resuelto: Tesseract 5 en python-ml** (EasyOCR fallback opcional).
2. ~~¿API externa placa+fecha?~~ → **Proveedor TBD**; stub en #9738, adapter real después.
3. ~~¿Job scheduler?~~ → **`IHostedService` + `PeriodicTimer`** (Hangfire descartado en esta fase).
4. ¿PR único con 8 HUs o split backend/frontend con aprobación LT?
