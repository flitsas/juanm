# ADR-0003: Bounded context GDC-PLANTILLAS vialix — AcroForm, DP y schema `gdc`

**Fecha**: 2026-06-11  
**Status**: Propuesto  
**Deciders**: Líder Técnico FLIT (promoción a Aceptado)  
**Tags**: arquitectura, backend, frontend, modulo-plantillas, vialix, postgres, pdf, acroform

## Contexto

El Feature ADO **#9564** ([GDC-PLANTILLAS]) implementa plantillas PDF AcroForm y Derechos de Petición 1:N sobre comparendos DGC (#9560). Incluye 8 HUs (#9751–#9758). El schema `dgc` ya existe para comparendos; el generador documental requiere bounded context propio.

Restricciones:

- Entrega **rama única** `feature/AB-9564-gdc-plantillas`, commits `HU####:`, PR a `develop` sin merge automático.
- Sin PDFs de plantilla en el monorepo (spike Fase 0 con PdfSharpCore + fixture pendiente del equipo).
- Diseño: `docs/designs/9564-gdc-plantillas.md`.

## Decisión

Implementar **Gdc.Modules.Plantillas** como vertical slice con schema PostgreSQL **`gdc`**, compilación AcroForm con **PdfSharpCore**, almacenamiento binario vía **`IBinaryAssetStore`** (MVP in-memory como NOTIF), y APIs `/api/v1/gdc/*`.

## Alternativas consideradas

### Opción 1: Vertical slice Plantillas + schema `gdc` + PdfSharpCore (recomendada)

**Pros:**
- Alineado a convenciones FLIT (schema por dominio, RLS).
- Distinción clara `dgc` comparendos vs `gdc` documentos.
- PdfSharpCore ya en CPM; soporte AcroForm read/fill.
- Patrón idéntico a DGC/NOTIF (módulo + Infrastructure services).

**Cons:**
- PdfSharpCore AcroForm incompleto para casos edge (choice, apariencias).
- In-memory store no production-ready sin segunda implementación S3.

**Esfuerzo:** L  
**Riesgos:** Fidelidad PDF sin spike con plantilla vialix real.

### Opción 2: QuestPDF para todo el flujo PDF

**Pros:**
- API moderna, buena para reportes.

**Cons:**
- No soporta relleno AcroForm sobre plantillas existentes (RF01–RF03).
- Requeriría rediseñar plantillas legales (fuera de alcance).

**Esfuerzo:** XL  
**Riesgos:** Incumple RF de fidelidad gráfica.

### Opción 3: Extender schema `dgc` con tablas plantilla

**Pros:**
- Un solo schema aparente.

**Cons:**
- Mezcla master data y generación documental.
- Confusión operativa `dgc` vs `gdc` en código y migraciones.

**Esfuerzo:** M  
**Riesgos:** Violación bounded context; deuda en REGLAS (#9710).

## Consecuencias

- Schema `gdc`: `pdf_template`, `pdf_template_field`, `derecho_peticion`.
- Frontend: `frontend/src/features/plantillas/`, ruta `/gdc`.
- `DgcDetailPanel`: grilla DP vía API (reemplaza stub).
- OpenAPI: tag `GdcPlantillas` en paths `/api/v1/gdc/*`.
- Envío correo DP permanece en #9710 (no duplicar NOTIF).

## Referencias

- `docs/designs/9564-gdc-plantillas.md`
- `openspec/changes/vialix-gdc-plantillas/`
- ADR-0001 (DGC bounded context)
- ADR-0002 (NOTIF bounded context)
