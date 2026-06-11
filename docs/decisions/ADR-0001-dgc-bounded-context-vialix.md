# ADR-0001: Bounded context DGC vialix — schema, OCR, contraventor y entrega feature-batch

**Fecha**: 2026-06-10  
**Status**: Propuesto  
**Deciders**: Willyn Londoño Calle (aprobación diseño), Líder Técnico FLIT (promoción a Aceptado)  
**Tags**: arquitectura, backend, frontend, modulo-dgc, vialix, postgresql, ocr

## Contexto

El Feature ADO **#9560** ([DGC] Módulo Gestión Comparendos) requiere master data multi-tenant de infracciones: OCR con buffer, maestra 15 columnas, descuentos, asociación de contraventor y auditoría de correos (lectura). El ecosistema vialix encadena DGC → NOTIF (#9563) → GDC (#9564) → REGLAS (#9710).

Restricciones acordadas:

- 8 HUs (#9735–#9742), entrega en **rama única** `feature/AB-9560-dgc-maestra`, commits `HU####:`, PR a `develop` **sin merge automático** (`implement-feature.md`).
- OCR: motor gratuito; proveedor API placa+fecha **TBD**.
- Diseño aprobado: `docs/designs/9560-dgc-maestra.md`.

## Decisión

Implementar el módulo **Gdc.Modules.Dgc** como vertical slice en `core-api` con schema PostgreSQL **`dgc`**, OCR vía **Tesseract 5 en `python-ml`**, asociación de contraventor con **`IExternalVehicleRegistry` + stub** y job **`IHostedService`**, frontend feature-sliced alineado a prototipo Flit Ready.

## Alternativas consideradas

### Opción 1: Vertical slice DGC + python-ml OCR + stub contraventor (recomendada)

**Pros:**
- Alineado a arquitectura FLIT (OCR en python-ml, dominio en core-api).
- Schema `dgc` aísla bounded context; RLS multi-tenant.
- Stub permite RF06/RF07 sin bloquear por proveedor TBD.
- Una rama por Feature reduce fricción de review vialix.

**Cons:**
- Requiere scaffold mínimo `python-ml` si no existe en repo.
- PR puede superar 800 líneas con 8 HUs.
- Adapter real de vehículos queda para fase posterior.

**Esfuerzo:** L  
**Riesgos:** Calidad OCR variable; integración NOTIF (#9563) pendiente para `email_log`.

### Opción 2: OCR embebido en core-api (Tesseract.NET)

**Pros:**
- Un solo servicio desplegable; sin HTTP interno OCR.
- Menor latencia en upload.

**Cons:**
- Contradice ADR servicios FLIT (OCR → python-ml).
- Acopla procesamiento de imagen al API .NET.
- Duplica capacidad ML futura (antifraude, cédulas).

**Esfuerzo:** M  
**Riesgos:** Deuda técnica al migrar a python-ml.

### Opción 3: Monolito de tablas en schema `core` sin job de contraventor

**Pros:**
- Menos tablas y sin `IHostedService`.
- Solo formulario manual RF07.

**Cons:**
- No cumple RF06 (ventanas horarias).
- Mezcla dominios en `core`.
- Re-trabajo cuando exista API placa+fecha.

**Esfuerzo:** M  
**Riesgos:** Incumplimiento funcional del Feature.

## Tradeoff aceptado

Opción 1 equilibra cumplimiento RF01–RF10, convenciones FLIT y capacidad de avanzar sin proveedor externo. El buffer OCR (RF02) y el formulario manual (RF07) mitigan calidad OCR y ausencia de API vehicular. Hangfire se descarta en favor de `IHostedService` por simplicidad (1–3 ventanas diarias).

## Consecuencias

### Lo que se gana

- Master data comparendos listo para NOTIF, GDC y REGLAS.
- Contratos estables (`IOcrExtractor`, `IExternalVehicleRegistry`) intercambiables por DI.
- Trazabilidad ADO + diseño + OpenSpec alineados.

### Lo que se pierde

- Asociación automática de contraventor en producción hasta adapter real.
- Un único PR grande si no se divide con LT.

### Cambios operacionales

- Nueva migración EF Core schema `dgc`.
- Nuevo endpoint python-ml `/ocr/comparendo:extract`.
- Docker dev: `tesseract-ocr`, `tesseract-ocr-spa`, `poppler-utils`.

## Modelo de datos (referencia)

Schema `dgc`: `comparendo`, `contraventor`, `ocr_lote`, `ocr_item`, `email_log`, `descuento_matriz`, `contraventor_job_config`.  
Constraint RF03: `UNIQUE (tenant_id, numero_comparendo)`.

## APIs (referencia)

Prefijo `/api/v1/dgc`. OpenAPI: `contracts/openapi/core-api.v1.yaml`.

## ADRs relacionados

- Ninguno previo en este repo (ADR-0001 inaugural).

## Notas para agentes

- **backend-agent**: Módulo `Gdc.Modules.Dgc`; commits por HU; no mergear PR.
- **database-agent**: Materializar DDL §3 de `9560-dgc-maestra.md`; validar con `db-schema-validator`.
- **frontend-agent**: `frontend/src/features/dgc/`; 4 estados UI; ref. `dgc.tsx`.
- **python-ml** (vía backend-agent coordinación HU #9736): Tesseract + OpenCV + pdf2image.
- **QA Agent**: Tests stub contraventor + manual RF07; fixture OCR.
- **Security Agent**: Sin PII en logs OCR; sanitizar HTML evidencia correos (DOMPurify).
- **Infra Agent**: Variables `VehicleRegistry:*` reservadas; sin deploy automático en modo feature-batch.

## Referencias

- Diseño aprobado: `docs/designs/9560-dgc-maestra.md`
- OpenSpec: `openspec/changes/vialix-dgc-maestra/`
- Feature ADO: #9560 · HUs #9735–#9742
- Workflow entrega: `.cursor/workflows/implement-feature.md`
