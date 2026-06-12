# Tasks — Feature #9564 GDC-PLANTILLAS (vialix-gdc-plantillas)

> Orden: #9751 → #9752 → #9753 → #9754 → #9755 → #9756 → #9757 → #9758  
> Rama: `feature/AB-9564-gdc-plantillas` · Commits: `HU####:` · PR → `develop` (sin merge)

---

## Fase 0 — Diseño y spike (pre-HU)

- [x] 0.1 OpenSpec `openspec/changes/vialix-gdc-plantillas/`
- [x] 0.2 `docs/designs/9564-gdc-plantillas.md`
- [x] 0.3 ADR-0003 bounded context Plantillas
- [x] 0.4 Spike PdfSharpCore + fixtures README
- [x] 0.5 Plantilla `vialix-dp-template.pdf` generada (tokens FLIT + AcroForm)
- [x] 0.6 Spike PdfSharpCore completo en `PdfAcroFormSpikeTests`

## HU #9751 — Schema y migración plantillas y DP

- [x] 1.1 Crear módulo `Gdc.Modules.Plantillas`
- [x] 1.2 Entidades EF: `pdf_template`, `pdf_template_field`, `derecho_peticion`
- [x] 1.3 Migración `GdcPlantillasInitialSchema` — schema `gdc` + RLS
- [x] 1.4 Registrar DbSets en `GdcDbContext`
- [x] 1.5 Tests schema `GdcPlantillasSchemaTests` (4 PASS)
- [ ] 1.6 Commit `HU9751:` + dev-tester evidencias ADO

## HU #9752 — API carga PDF y extracción AcroForm

- [x] 2.1 `IBinaryAssetStore` + upload endpoint
- [x] 2.2 `IAcroFormFieldExtractor` con PdfSharpCore
- [x] 2.3 Listar tags detectados (RF01)
- [x] 2.4 API `/api/v1/gdc/templates/upload`
- [x] 2.5 Commit `HU9752:` + evidencias ADO

## HU #9753 — API tipado y mapeo variables

- [ ] 3.1 CRUD campos + tipos (text, number, choice)
- [ ] 3.2 Catálogo variables del sistema
- [ ] 3.3 Validación mapeo (RF02)
- [ ] 3.4 Commit `HU9753:` + dev-tester

## HU #9754 — API compilación PDF y generación DP

- [x] 4.1 Compilador PDF + `NeedAppearances`
- [x] 4.2 Crear `derecho_peticion` con PDF output
- [x] 4.3 Bloqueo sin contraventor (RF07)
- [x] 4.4 Commit `HU9754:` + dev-tester

## HU #9755 — API ciclo de vida DP y versionamiento

- [x] 5.1 Transiciones estado (RF05)
- [x] 5.2 List DP por comparendo (RF04)
- [x] 5.3 Regenerar solo No Enviado al bump template (RF06)
- [x] 5.4 Download PDF generado
- [x] 5.5 Commit `HU9755:` + dev-tester

## HU #9756 — UI catálogo y carga plantillas

- [x] 6.1 Feature `frontend/src/features/plantillas/`
- [x] 6.2 Ruta `/gdc` + nav enabled
- [ ] 6.3 Commit `HU9756:` + dev-tester

## HU #9757 — UI preview y generación DP

- [ ] 7.1 Preview compilado + descarga
- [ ] 7.2 Commit `HU9757:` + dev-tester

## HU #9758 — UI grilla DP en detalle comparendo

- [ ] 8.1 Reemplazar stub `DgcDetailPanel`
- [ ] 8.2 Grilla estados + descarga
- [ ] 8.3 Commit `HU9758:` + dev-tester

## Cierre Feature

- [ ] 9.1 PR `feature/AB-9564-gdc-plantillas` → `develop`
- [ ] 9.2 code-review + security
- [ ] 9.3 integration-agent Modo A
