## 0. Preparación y diseño FLIT

- [ ] 0.1 Exportar diseño a `docs/designs/9560-dgc-maestra.md` y solicitar aprobación humana
- [ ] 0.2 Invocar `architecture-agent` para ADR Propuesto (bounded context Dgc)
- [ ] 0.3 Crear rama `feature/AB-9560-dgc-maestra` desde `develop`

## 1. HU #9735 — Schema y migración (commit `HU9735: …`)

- [ ] 1.1 Crear módulo `Gdc.Modules.Dgc` (Domain/Application/Infrastructure)
- [ ] 1.2 Definir entidades EF: comparendo, contraventor, ocr_lote, ocr_item, email_log, descuento_matriz
- [ ] 1.3 Migración EF: schema `dgc`, RLS, UNIQUE(tenant_id, numero_comparendo)
- [ ] 1.4 Ejecutar `database-agent` modo C + `db-schema-validator`
- [ ] 1.5 `dev-tester` — tests migración y constraint duplicado
- [ ] 1.6 Commit `HU9735: Schema DGC comparendos con RLS multi-tenant`

## 2. HU #9736 — API OCR y buffer (commit `HU9736: …`)

- [ ] 2.1 Scaffold mínimo `services/python-ml` + endpoint `POST /ocr/comparendo:extract` (Tesseract 5 + OpenCV + pdf2image)
- [ ] 2.2 Cliente `IOcrExtractor` en core-api → HTTP python-ml
- [ ] 2.3 Endpoints upload lote, listar buffer, confirmar/rechazar ítem
- [ ] 2.4 Validación duplicado en confirmación
- [ ] 2.5 Actualizar OpenAPI `/api/v1/dgc/ocr/*`
- [ ] 2.6 `dev-tester` — AC carga y duplicado
- [ ] 2.7 Commit `HU9736: API carga OCR y buffer de validación`

## 3. HU #9737 — API maestra y días descuento (commit `HU9737: …`)

- [ ] 3.1 Query handler listado 15 columnas con paginación/filtros
- [ ] 3.2 Servicio cálculo `dias_restantes` con matriz global/secretaría
- [ ] 3.3 Endpoints GET list y GET detail
- [ ] 3.4 Actualizar OpenAPI `/api/v1/dgc/comparendos`
- [ ] 3.5 `dev-tester` — AC listado, cálculo y filtros
- [ ] 3.6 Commit `HU9737: API maestra comparendos y motor días descuento`

## 4. HU #9738 — Planificador contraventor (commit `HU9738: …`)

- [ ] 4.1 Tabla `contraventor_job_config` (1–3 ventanas) + `ContraventorAssociationHostedService` (`IHostedService`)
- [ ] 4.2 Puerto `IExternalVehicleRegistry` + `StubVehicleRegistry` (`NotFound` en prod; fixture `Found` en tests)
- [ ] 4.3 Endpoint PUT contraventor manual (RF07 — flujo principal sin API real)
- [ ] 4.4 `dev-tester` — AC ventanas y formulario manual
- [ ] 4.5 Commit `HU9738: Planificador API contraventor y contingencia manual`

## 5. HU #9739 — API log correos (commit `HU9739: …`)

- [ ] 5.1 Contrato interno escritura `email_log` (documentar para NOTIF #9563)
- [ ] 5.2 Endpoints GET log list y GET evidencia HTML
- [ ] 5.3 Seed/fixture stub para pruebas sin NOTIF
- [ ] 5.4 `dev-tester` — AC tracking y evidencia HTML
- [ ] 5.5 Commit `HU9739: API consulta log tracking correos`

## 6. HU #9740 — Frontend tabla maestra (commit `HU9740: …`)

- [ ] 6.1 Feature `frontend/src/features/dgc/` — listado TanStack Query
- [ ] 6.2 Tabla 15 columnas, búsqueda, filtros, paginación (ref. dgc.tsx)
- [ ] 6.3 Estados vacío/cargando/error/lleno + tokens Flit Ready
- [ ] 6.4 `dev-tester` — tests Vitest AC tabla
- [ ] 6.5 Commit `HU9740: Vista tabla maestra DGC 15 columnas`

## 7. HU #9741 — Frontend OCR/buffer (commit `HU9741: …`)

- [ ] 7.1 Dialog dropzone PDF/PNG/JPG
- [ ] 7.2 Formulario editable por ítem buffer + validación inline
- [ ] 7.3 Integración API OCR #9736
- [ ] 7.4 `dev-tester` — tests Vitest AC buffer
- [ ] 7.5 Commit `HU9741: UI carga OCR y buffer pre-guardado`

## 8. HU #9742 — Frontend detalle y log (commit `HU9742: …`)

- [ ] 8.1 Sheet lateral con tabs Detalle / Contraventor / Log correos
- [ ] 8.2 Modal evidencia HTML (DOMPurify)
- [ ] 8.3 Columna DP solo lectura
- [ ] 8.4 `dev-tester` — tests Vitest AC detalle
- [ ] 8.5 Commit `HU9742: Panel detalle contraventor y log correos`

## 9. Cierre Feature — PR a develop (sin merge)

- [ ] 9.1 `pnpm fix:all:fullstack` en rama feature
- [ ] 9.2 Push `feature/AB-9560-dgc-maestra`
- [ ] 9.3 Abrir PR → `develop` con tabla commits/HUs
- [ ] 9.4 `code-review-agent` + `security-agent`
- [ ] 9.5 **STOP** — entregar PR al Líder Técnico (no `integration-agent`, no Resolved)
