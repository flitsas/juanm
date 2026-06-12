# Tasks — Feature #9710 REGLAS (vialix-reglas-dinamicas)

> Orden: #9759 → #9760 → #9761 → (#9762 + #9763) → #9764 → #9765 → #9766  
> Rama: `feature/AB-9710-reglas-dinamicas` · Commits: `HU####:` · PR → `develop` (sin merge)  
> **Nota:** #9564 (GDC plantillas) en paralelo por otro equipo — usar stub `IGdcPdfTemplateRenderer` hasta merge.

---

## HU #9759 — Schema y migración motor reglas

- [ ] 1.1 Crear módulo `Gdc.Modules.Reglas` (Domain, Application, Infrastructure)
- [ ] 1.2 Entidades: `dynamic_rule`, `rule_condition`, `rule_execution_run`, `rule_processing_record`, `secretariat_contact`
- [ ] 1.3 Migración `ReglasInitialSchema` — schema `reglas` + RLS + unique anti-duplicidad
- [ ] 1.4 Registrar DbSets en `GdcDbContext`
- [ ] 1.5 Validar con `db-schema-validator`
- [ ] 1.6 Commit `HU9759:` + dev-tester evidencias

## HU #9760 — API CRUD reglas y constructor condiciones

- [ ] 2.1 Endpoints CRUD `/api/v1/reglas/rules`
- [ ] 2.2 Persistencia árbol condiciones AND/OR
- [ ] 2.3 Validación activación (condiciones + plantilla PDF)
- [ ] 2.4 OpenAPI + FluentValidation
- [ ] 2.5 Commit `HU9760:` + dev-tester

## HU #9761 — API scheduler y motor de evaluación

- [ ] 3.1 `ReglasExecutionHostedService` + configuración frecuencia
- [ ] 3.2 `ReglasRuleEvaluator` sobre `IDgcComparendoReader`
- [ ] 3.3 Endpoints ejecución manual + listado coincidencias
- [ ] 3.4 OpenAPI `/api/v1/reglas/runs`, `/api/v1/reglas/matches`
- [ ] 3.5 Commit `HU9761:` + dev-tester

## HU #9762 — API orquestación PDF, correo y trazabilidad

- [ ] 4.1 Contrato `IGdcPdfTemplateRenderer` (+ stub hasta #9564)
- [ ] 4.2 `IReglasEmailDispatcher` con adjunto PDF vía NOTIF
- [ ] 4.3 Persistencia `rule_processing_record` + métricas corrida
- [ ] 4.4 Commit `HU9762:` + dev-tester

## HU #9763 — API contactos secretaría y control acceso

- [ ] 5.1 CRUD `secretariat_contact`
- [ ] 5.2 Resolución destinatario por secretaría del comparendo
- [ ] 5.3 Políticas RBAC en endpoints REGLAS
- [ ] 5.4 Commit `HU9763:` + dev-tester

## HU #9764 — UI constructor reglas dinámicas

- [ ] 6.1 Feature slice `frontend/src/features/reglas/`
- [ ] 6.2 RuleBuilder con operadores y agrupaciones
- [ ] 6.3 Ruta `/admin/reglas` + tab Constructor
- [ ] 6.4 Commit `HU9764:` + dev-tester

## HU #9765 — UI scheduler ejecución y resultados

- [ ] 7.1 Panel scheduler + ejecución manual
- [ ] 7.2 Tabla coincidencias por corrida
- [ ] 7.3 Commit `HU9765:` + dev-tester

## HU #9766 — UI logs trazabilidad y contactos secretaría

- [ ] 8.1 Panel logs/métricas de ejecución
- [ ] 8.2 CRUD contactos secretaría en UI
- [ ] 8.3 E2E smoke Playwright (mocks API)
- [ ] 8.4 Commit `HU9766:` + dev-tester

## Cierre Feature

- [ ] 9.1 Exportar diseño a `docs/designs/9710-reglas-dinamicas.md`
- [ ] 9.2 ADR-0003 Propuesto en `docs/decisions/`
- [ ] 9.3 PR único a `develop` + code-review + security
