# Tasks — Feature #9563 NOTIF (vialix-notif-motor)

> Orden: #9743 → (#9744 + #9745) → #9746 → #9747 → #9748 → #9749 → #9750  
> Rama: `feature/AB-9563-notificaciones` · Commits: `HU####:` · PR → `develop` (sin merge)

---

## HU #9743 — Schema y migración motor notificaciones

- [ ] 1.1 Crear módulo `Gdc.Modules.Notif` (Domain, Application, Infrastructure)
- [ ] 1.2 Entidades EF: `email_provider_config`, `email_template`, `notification_rule`, `email_queue`
- [ ] 1.3 Migración `NotifInitialSchema` — schema `notif` + RLS + índices
- [ ] 1.4 Registrar DbSets en `GdcDbContext`
- [ ] 1.5 Validar con `db-schema-validator`
- [ ] 1.6 Commit `HU9743:` + dev-tester evidencias

## HU #9744 — API CRUD compañías y perfil tenant

- [ ] 2.1 Endpoints Super Admin companies CRUD
- [ ] 2.2 Endpoint Tenant Admin profile self-service
- [ ] 2.3 FluentValidation + autorización por rol
- [ ] 2.4 OpenAPI paths `/api/v1/notif/companies`, `/api/v1/notif/profile`
- [ ] 2.5 Commit `HU9744:` + dev-tester

## HU #9745 — API integración proveedor email

- [ ] 3.1 `IEmailSender` strategy (API, SendGrid, FLIT Mail)
- [ ] 3.2 Cifrado credenciales + endpoint config provider
- [ ] 3.3 Endpoint test connection
- [ ] 3.4 OpenAPI `/api/v1/notif/provider`
- [ ] 3.5 Commit `HU9745:` + dev-tester

## HU #9746 — API plantillas email marca blanca

- [ ] 4.1 CRUD plantillas HTML + banner/pie
- [ ] 4.2 Motor variables merge + preview endpoint
- [ ] 4.3 OpenAPI `/api/v1/notif/templates`
- [ ] 4.4 Commit `HU9746:` + dev-tester

## HU #9747 — Motor reglas, cola y log de envíos

- [ ] 5.1 CRUD reglas (disparadores cronológico/estatal)
- [ ] 5.2 `NotifDispatchWorker` + cola + switch On/Off
- [ ] 5.3 `IEmailLogWriter` → `dgc.email_logs` (contrato DGC)
- [ ] 5.4 `IDgcComparendoReader` para evaluación reglas
- [ ] 5.5 OpenAPI `/api/v1/notif/rules`, `/api/v1/notif/queue`, `/api/v1/notif/switch`
- [ ] 5.6 Commit `HU9747:` + dev-tester

## HU #9748 — UI Admin compañías y proveedor email

- [ ] 6.1 Feature slice `frontend/src/features/notif/`
- [ ] 6.2 Páginas compañías (Super Admin) + proveedor
- [ ] 6.3 TanStack Query + 4 estados UI
- [ ] 6.4 Commit `HU9748:` + dev-tester

## HU #9749 — UI editor plantillas email

- [ ] 7.1 Editor HTML + upload banner/pie
- [ ] 7.2 Preview con variables sample
- [ ] 7.3 Commit `HU9749:` + dev-tester

## HU #9750 — UI reglas de comunicación y switch

- [ ] 8.1 Formulario reglas + listado
- [ ] 8.2 Switch global On/Off
- [ ] 8.3 Commit `HU9750:` + dev-tester

## Cierre Feature

- [ ] 9.1 Lint fullstack (`biome check --write` + `pnpm format` + `dotnet format`)
- [ ] 9.2 `pnpm test` verde
- [ ] 9.3 PR a `develop` — code-review + security
- [ ] 9.4 STOP — entregar URL PR al LT (sin merge)
