## Why

El módulo DGC es el master data de infracciones en flit-vialix. Hoy no existe persistencia ni APIs en GDC 2.0 para comparendos, lo que bloquea NOTIF (#9563), GDC plantillas (#9564) y reglas (#9710). El Feature #9560 ya está descompuesto en 8 HUs (Sprint 2); se requiere diseño técnico unificado antes de implementar en rama única por Feature.

## What Changes

- Nuevo bounded context `Dgc` en `services/core-api/` con schema PostgreSQL `dgc`.
- Entidades: comparendo, contraventor, lote OCR, ítem OCR buffer, log de correo (lectura), matriz de descuento.
- APIs REST para carga OCR, maestra 15 columnas, planificador de contraventor y consulta de tracking de correos.
- Módulo frontend Next.js alineado al prototipo `flitready-suite` (`dgc.tsx`).
- Contratos OpenAPI en `contracts/openapi/core-api.v1.yaml`.
- Stubs/contratos de integración hacia Feature NOTIF (#9563) sin implementar el motor de envío.
- Entrega en modo **feature-batch**: rama `feature/AB-9560-dgc-maestra`, commits `HU####: …`, PR a `develop` sin merge automático.

## Capabilities

### New Capabilities

- `dgc-persistence`: Modelo relacional multi-tenant con RLS, constraints de duplicado y auditoría.
- `dgc-ocr-ingestion`: Carga individual/masiva PDF/PNG/JPG, buffer pre-guardado y rechazo por duplicado.
- `dgc-maestra-api`: Listado paginado con 15 columnas operativas y cálculo de días restantes.
- `dgc-contraventor-resolution`: Job programado (1–3 ventanas) + formulario manual de contingencia.
- `dgc-email-audit`: API de solo lectura del log de correos y evidencia HTML (alimentado por NOTIF).
- `dgc-frontend`: Vista tabla, diálogo OCR/buffer y panel detalle con pestañas.

### Modified Capabilities

- _(ninguna — `openspec/specs/` vacío en este repo)_

## Impact

| Área | Impacto |
|------|---------|
| `services/core-api/` | Nuevo módulo vertical slice `Gdc.Modules.Dgc` (o `Flit.Modules.Dgc` según naming vigente) |
| `frontend/` | Nueva ruta/sección DGC en App Router |
| `contracts/openapi/core-api.v1.yaml` | ~12 endpoints nuevos bajo `/api/v1/dgc/*` |
| PostgreSQL | Schema `dgc`, RLS por `tenant_id`, migración EF Core |
| ADO | Feature #9560, HUs #9735–#9742 |
| Dependencias externas | API placa+fecha (contraventor), NOTIF #9563 (escritura log), AUTH #9561 (sesión) |
| Fuera de alcance | SIMIT, planillas de pago, generación DP (#9564), motor envío correo (#9563) |
