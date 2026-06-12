## Context

- **Feature ADO:** #9564 [GDC-PLANTILLAS] — Active.
- **HUs:** #9751 (schema) → #9752 → #9753 → #9754 → #9755 → #9756 → #9757 → #9758.
- **Dependencia upstream:** DGC #9560 (`dgc.compareendos`, `dgc.contraventors`).
- **Prototipo UI:** `flitready-suite` — módulo Plantillas / GDC.
- **Modo entrega:** rama `feature/AB-9564-gdc-plantillas`, commits por HU, PR a `develop` sin merge automático.
- **Fixtures PDF:** `services/core-api/tests/fixtures/pdf/` — **vacío en repo**; spike usa PDF mínimo embebido hasta que el equipo aporte plantillas vialix reales.

## Goals / Non-Goals

**Goals:**

- Carga plantillas PDF con extracción automatizada de tags AcroForm (RF01).
- Tipado polimórfico y mapeo a variables del sistema (RF02).
- Compilación PDF estática con datos del comparendo (RF03).
- Grilla multidocumento DP 1:N en detalle comparendo (RF04).
- Estados: No Enviado → Enviado → Sin Respuesta → Con Respuesta (RF05).
- Regeneración solo DPs No Enviado al editar plantilla maestra (RF06).
- Bloqueo generación sin contraventor identificado (RF07).

**Non-Goals:**

- Firma digital, edición nativa de geometría PDF.
- Lector/analizador de respuestas de secretarías.
- Envío de DP por correo (orquestación #9710).
- MinIO/S3 en MVP (abstracción preparada; implementación in-memory como NOTIF).

## Spike AcroForm (Fase 0 — 2026-06-11)

### Fixtures en repo

| Recurso | Estado |
|---------|--------|
| `vialix-dp-template.pdf` | Generado — diseño FLIT + 13 campos AcroForm |
| `generate_vialix_dp_template.py` | Script ReportLab regenerable |
| Spike PdfSharpCore | **PASS** — extracción, relleno y verificación de valores |

### Decisión librería PDF

| Librería | AcroForm read/fill | Generación layout | Decisión |
|----------|-------------------|-------------------|----------|
| **PdfSharpCore** | Sí (con `NeedAppearances`, `PdfString.Value`) | Limitada | **Usar** para RF01–RF03 |
| **QuestPDF** | No | Sí | Reservar reportes/export (#9560 reportes futuro) |
| iText / AGPL | Sí | Sí | Rechazado (licencia) |

### Riesgos spike confirmados

1. **Fidelidad gráfica:** sin PDF real vialix no se valida layout post-fill en Chrome/Adobe.
2. **RenderAppearance:** usar `field.Value = new PdfString(...)` no `field.Text`; flag `/NeedAppearances = true`.
3. **Campos choice/list:** requiere validación con plantilla real (RF02 polimórfico).

**Acción:** aportar al menos un PDF DP de producción en `tests/fixtures/pdf/vialix-dp-template.pdf` antes de cerrar HU #9752.

## Decisions

### D1 — Schema PostgreSQL `gdc` (no `dgc`)

| Tabla | Propósito |
|-------|-----------|
| `gdc.pdf_template` | Plantilla maestra: nombre, storage_key, version, activo |
| `gdc.pdf_template_field` | Tag AcroForm, field_type, system_variable, opciones JSON |
| `gdc.derecho_peticion` | DP: comparendo_id, template_id, template_version, estado, output_storage_key |

**RLS:** `tenant_id = NULLIF(current_setting('app.current_tenant_id', true), '')::uuid` (patrón `dgc`).

**FK:** `derecho_peticion.comparendo_id` → `dgc.compareendos.id` (sin FK desde `gdc` hacia `notif`).

**Alternativas:** schema `core` — rechazado; schema `dgc` — rechazado (colisión con comparendos).

### D2 — Módulo `Gdc.Modules.Plantillas`

```
services/core-api/src/
├── Gdc.Modules.Plantillas/
│   ├── Domain/
│   ├── Application/
│   └── Infrastructure/Persistence/
├── Gdc.Api/Endpoints/GdcPlantillaEndpoints.cs
├── Gdc.Api/Endpoints/GdcDpEndpoints.cs
└── Gdc.Infrastructure/Plantillas/
```

**Alternativas:** `Gdc.Modules.Gdc` — rechazado (ambiguo con nombre producto).

### D3 — Lectura comparendo para compilación

**Decisión:** `IComparendoCompilationSource` en Application lee `dgc.compareendos` + `contraventors` vía `GdcDbContext` (patrón `IDgcComparendoReader` de NOTIF).

**RF07:** fallar con `GDC_CONTRAVENTOR_REQUIRED` si `Contraventor` es null o comparendo `PendienteContraventor`.

### D4 — Almacenamiento binario

**Decisión MVP:** `IBinaryAssetStore` con implementación `InMemoryBinaryAssetStore` (extensión de patrón `NotifTemplateAssetStore`).

**Futuro:** `S3BinaryAssetStore` (MinIO, ADR-0016 packages ya en CPM).

### D5 — Variables del sistema (catálogo inicial)

| Variable | Origen |
|----------|--------|
| `comparendo.numero` | `dgc.compareendos.numero_comparendo` |
| `comparendo.placa` | `dgc.compareendos.placa` |
| `comparendo.documento` | `dgc.compareendos.documento` |
| `comparendo.infractor_nombre` | `dgc.compareendos.infractor_nombre` |
| `comparendo.fecha_comparendo` | `dgc.compareendos.fecha_comparendo` |
| `comparendo.fecha_notificacion` | `dgc.compareendos.fecha_notificacion` |
| `comparendo.total` | `dgc.compareendos.total_valor` |
| `comparendo.estado` | `dgc.compareendos.estado` |
| `contraventor.nombre` | `dgc.contraventors.nombre` |
| `contraventor.documento` | `dgc.contraventors.documento` |
| `contraventor.correo` | `dgc.contraventors.correo` |
| `tenant.nombre` | `identity.tenants.name` |

### D6 — API routes

| Grupo | Paths |
|-------|-------|
| Plantillas | `GET/POST /api/v1/gdc/templates`, `POST /api/v1/gdc/templates/{id}/fields`, upload |
| DP | `GET/POST /api/v1/gdc/comparendos/{id}/derechos-peticion`, transiciones estado, download |

### D7 — Frontend

- Ruta `/gdc` — catálogo y editor mapeo (HU #9756).
- Preview/generación en flujo plantilla (HU #9757).
- Tab o sección en `DgcDetailPanel` — grilla DP (HU #9758); reemplaza stub readonly.

Patrón: TanStack Query + headers `X-Tenant-Id` (igual DGC/NOTIF).

### D8 — Auth

Endpoints GDC siguen patrón actual: tenancy por header en DEV; `RequireAuthorization` como hardening transversal opcional pre-merge.

## Risks / Trade-offs

| Riesgo | Mitigación |
|--------|------------|
| Sin PDF vialix en repo | Fixture README + spike embebido; gate HU #9752 con PDF real |
| AcroForm complejo (choice, firma) | Spike ampliado con plantilla real |
| `dp_referencia` legacy | UI migra a API DP; no extender string en maestra |
| In-memory store no escala | Interfaz `IBinaryAssetStore` desde día 1 |

## Migration Plan

1. Migración EF `GdcPlantillasInitialSchema` — schema `gdc` + RLS.
2. Sin seeds obligatorios hasta tener PDF fixture.
3. Deploy: migración antes de activar endpoints en DEV.

## Open Questions

- ¿Nombres exactos de tags en plantilla vialix? — Pendiente PDF real.
- ¿Actualizar `dgc.compareendos.dp_referencia` al crear primer DP? — Propuesta: no en MVP; grilla usa API DP.
