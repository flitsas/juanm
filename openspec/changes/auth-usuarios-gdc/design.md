## Context

GDC 2.0 es un monorepo greenfield (.NET 10 + PostgreSQL + Next.js 16). El Feature ADO #9561 define autenticación multi-tenant con RBAC para el ecosistema flit-vialix. El `GdcDbContext` existe con schema `core` vacío; no hay módulo Auth implementado.

## Goals / Non-Goals

**Goals:**
- Autenticación segura email/password con JWT (access + refresh opcional en fase 1: solo access con expiración corta).
- Aislamiento estricto por `tenant_id` en queries y APIs.
- Gestión de usuarios por Super Admin con flujos de invitación y activación.
- RBAC configurable por rol sin redeploy.
- Contratos OpenAPI y diseño alineados a Clean Architecture (Domain → Application → Infrastructure → API).

**Non-Goals:**
- Creación de tenants, SSO, 2FA, OAuth social.

## Decisions

### D1 — Autenticación: JWT propio vs Identity Server externo

| Opción | Pros | Contras | Esfuerzo |
|--------|------|---------|----------|
| JWT propio (ASP.NET) | Control total, sin deps externas, alineado a monolito modular | Más código de seguridad propio | M |
| Keycloak/Entra ID | Estándar enterprise, SSO futuro | Complejidad ops, fuera de alcance SSO | L |
| Session cookies only | Simple en browser | Peor para API móvil/futura | S |

**Decisión:** JWT propio con `Microsoft.AspNetCore.Authentication.JwtBearer`, claims `sub`, `tenant_id`, `role`, expiración 15 min access token. Refresh token en tabla `auth.refresh_tokens` (fase posterior si se requiere persistencia de sesión larga).

### D2 — Password hashing: ASP.NET Identity PasswordHasher vs BCrypt

**Decisión:** `PasswordHasher<T>` de ASP.NET Core Identity (PBKDF2) — integrado, auditado, sin dependencia extra.

### D3 — Multi-tenant: global query filter EF Core vs middleware manual

**Decisión:** `ITenantContext` inyectado + global query filters en EF Core para entidades `ITenantEntity`. Middleware valida JWT y establece tenant. Super Admin con claim `is_super_admin` bypassa filtro solo en endpoints explícitos de administración.

### D4 — RBAC: tabla permisos vs claims estáticos

**Decisión:** Tablas `roles`, `permissions`, `role_permissions` en schema `auth`. Permisos como strings `module.action` (ej. `users.invite`). Policy-based authorization en ASP.NET.

### D5 — Email: abstracción `IEmailSender`

**Decisión:** Interfaz en Application; implementación SMTP en Infrastructure (config en appsettings). Templates HTML para activación y recuperación.

## Risks / Trade-offs

- **[Riesgo] Tokens de activación en URL** → Mitigación: token hash en BD, expiración 48h, un solo uso.
- **[Riesgo] Super Admin bypass tenant** → Mitigación: endpoints admin bajo policy `SuperAdminOnly`, audit log.
- **[Riesgo] Fuerza bruta login** → Mitigación: rate limiting + contador intentos + sugerencia cambio clave (RF09).
- **[Trade-off] Sin refresh token en v1** → Usuario re-login cada 15 min; aceptable para MVP admin.

## Migration Plan

1. Migración EF `AddAuthSchema` crea schema `auth` y tablas.
2. Seed roles base (`SuperAdmin`, `TenantAdmin`, `Operator`) y permisos mínimos.
3. Deploy backend con endpoints auth; luego frontend login.
4. Rollback: revert migración Down (solo si no hay usuarios productivos).

## Open Questions

- ¿Servidor SMTP definitivo para DEV/QA? (usar Mailhog en docker-compose para DEV)
- ¿Tabla `tenants` ya existe en otro módulo o se referencia por UUID externo? → Asumir FK a `core.tenants` existente o stub hasta módulo tenants.

## Modelo de datos (conceptual)

Schema `auth`:
- `users` (id, tenant_id, email, password_hash, status, failed_login_count, ...)
- `roles`, `permissions`, `role_permissions`, `user_roles`
- `activation_tokens`, `password_reset_tokens`
- RLS por `tenant_id` en tablas de negocio auth

## Archivos a crear/modificar

**Backend:**
- `services/core-api/src/Flit.Modules.Auth/{Domain,Application,Infrastructure}/`
- `services/core-api/src/Gdc.Api/Endpoints/Auth*.cs`
- Migración EF en `Gdc.Infrastructure/Migrations/`
- `contracts/openapi/core-api.v1.yaml` — paths `/auth/*`

**Frontend:**
- `frontend/src/features/auth/{api,components,hooks,pages}/`
