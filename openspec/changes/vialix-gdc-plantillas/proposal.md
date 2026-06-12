## Why

DGC (#9560) expone master data de comparendos y contraventores, pero no existe generación de correspondencia legal PDF ni ciclo de Derechos de Petición (DP) 1:N. Sin GDC-PLANTILLAS (#9564) no hay AcroForm, compilación dinámica ni trazabilidad de trámites frente a secretarías — bloqueando REGLAS (#9710) y el flujo operativo vialix.

## What Changes

- Nuevo bounded context **Plantillas** en `services/core-api/` con schema PostgreSQL **`gdc`** (distinto de `dgc` comparendos).
- Entidades: plantilla PDF maestra, campos AcroForm tipados, derecho de petición (DP) con relación 1:N por comparendo.
- APIs REST: carga PDF, extracción tags, mapeo variables, compilación, ciclo de vida DP y versionamiento condicional.
- Almacenamiento binario PDF vía abstracción `IBinaryAssetStore` (MVP: in-memory alineado a NOTIF).
- Módulo frontend `/gdc` (Plantillas) + grilla DP en detalle comparendo (`DgcDetailPanel`).
- Spike AcroForm con **PdfSharpCore** (lectura/relleno); QuestPDF fuera de alcance AcroForm.
- Entrega modo **feature-batch**: rama `feature/AB-9564-gdc-plantillas`, commits `HU####:`, PR a `develop` sin merge automático.

## Capabilities

### New Capabilities

- `gdc-persistence`: Schema `gdc` multi-tenant con RLS; tablas plantilla, campo, derecho_peticion.
- `gdc-acroform-api`: Upload PDF, extracción AcroForm, tipado y mapeo variables (RF01–RF02).
- `gdc-pdf-compilation`: Compilación PDF estático + creación DP (RF03, RF07).
- `gdc-dp-lifecycle`: Estados DP, grilla 1:N, versionamiento solo No Enviado (RF04–RF06).
- `gdc-frontend`: UI catálogo plantillas, preview/generación y grilla DP en comparendo.

### Modified Capabilities

- `dgc-maestra-api`: Panel detalle comparendo consume API DP (reemplaza stub `dp_referencia` readonly).

## Impact

| Área | Impacto |
|------|---------|
| `services/core-api/` | Nuevo módulo `Gdc.Modules.Plantillas` |
| `frontend/` | Ruta `/gdc`, feature `plantillas/`, extensión `DgcDetailPanel` |
| PostgreSQL | Schema `gdc`; FK lógica a `dgc.compareendos` |
| ADO | Feature #9564, HUs #9751–#9758 |
| Dependencias | DGC (#9560) mergeado; AUTH (#9561) sesión/headers; NOTIF (#9563) sin writer cruzado |
| Fuera de alcance | Firma digital, edición geometría PDF, lector respuestas secretarías, envío correo DP (#9710) |
