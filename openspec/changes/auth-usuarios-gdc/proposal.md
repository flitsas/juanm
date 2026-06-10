## Why

GDC 2.0 requiere una capa transversal de identidad, autenticación y control de accesos multi-tenant antes de habilitar módulos de negocio (DGC, DP, reportes). Sin este módulo no hay segregación de datos por empresa ni gobernanza centralizada de usuarios.

## What Changes

- Módulo de autenticación email/password con JWT que expone Rol y Tenant en sesión.
- Login, logout seguro e invalidación de tokens.
- Consola de Super Administrador para invitar usuarios vinculados a tenant existente.
- Correo de activación con token temporal de 48 horas.
- Recuperación autónoma de contraseña y sobrescritura por Super Admin.
- Matriz RBAC interactiva por rol (acciones funcionales).
- Enforcement multi-tenant en backend (filtro por TenantID del usuario autenticado).
- Sugerencia de cambio de contraseña tras intentos fallidos de login.
- UI React para login/logout y consola de administración.

**Fuera de alcance:** creación de tenants, SSO, 2FA.

## Capabilities

### New Capabilities

- `auth-session`: Login, logout, emisión/validación JWT, claims de rol y tenant.
- `user-lifecycle`: Invitación manual, activación por token 48h, estados de usuario.
- `password-management`: Recuperación por correo, reset por Super Admin, bloqueo por intentos fallidos.
- `rbac-admin`: Roles, permisos funcionales y matriz RBAC editable.
- `multi-tenant-security`: Aislamiento de datos por tenant en API y persistencia.

### Modified Capabilities

- _(ninguna — greenfield en GDC 2.0)_

## Impact

- **Backend:** nuevo módulo `Flit.Modules.Auth` en `services/core-api/`, migraciones PostgreSQL schema `auth`, middleware multi-tenant, endpoints REST documentados en OpenAPI.
- **Frontend:** features `auth` en `frontend/src/features/auth/` (login, admin usuarios, matriz RBAC).
- **Infra:** servicio de correo (SMTP/config) para activación y recuperación.
- **Contratos:** extensión de `contracts/openapi/core-api.v1.yaml`.
- **ADO:** Feature #9561 descompuesto en 8 HUs BACKEND/FRONTEND.
