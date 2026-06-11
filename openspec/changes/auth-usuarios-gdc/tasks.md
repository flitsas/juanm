## 1. Backend — Schema y persistencia

- [ ] 1.1 Crear módulo `Flit.Modules.Auth` (Domain, Application, Infrastructure)
- [ ] 1.2 Definir entidades: User, Role, Permission, tokens
- [ ] 1.3 Migración EF schema `auth` con RLS/tenant_id
- [ ] 1.4 Seed roles y permisos base

## 2. Backend — Auth session (RF01, RF02)

- [ ] 2.1 Endpoints POST `/auth/login`, POST `/auth/logout`
- [ ] 2.2 JWT issuer, validación, blacklist/revocation
- [ ] 2.3 Tests unitarios login/logout

## 3. Backend — Multi-tenant (RF08)

- [ ] 3.1 `ITenantContext` + global query filters
- [ ] 3.2 Middleware y policies de autorización
- [ ] 3.3 Tests de aislamiento por tenant

## 4. Backend — User lifecycle (RF03, RF04)

- [ ] 4.1 POST `/auth/users/invite` (Super Admin)
- [ ] 4.2 POST `/auth/users/activate` con token 48h
- [ ] 4.3 Integración `IEmailSender` activación

## 5. Backend — Password management (RF05, RF06, RF09)

- [ ] 5.1 Forgot/reset password flow
- [ ] 5.2 Admin password override endpoint
- [ ] 5.3 Failed login counter y lockout

## 6. Backend — RBAC (RF07)

- [ ] 6.1 CRUD permisos por rol
- [ ] 6.2 GET/PUT matriz RBAC
- [ ] 6.3 Policy-based authorization

## 7. Frontend — Login/Logout

- [ ] 7.1 Páginas login y logout
- [ ] 7.2 Auth context + token storage
- [ ] 7.3 Rutas protegidas

## 8. Frontend — Admin console

- [ ] 8.1 Listado e invitación de usuarios
- [ ] 8.2 Matriz RBAC interactiva
- [ ] 8.3 Estados vacío/cargando/error/lleno (WCAG)

## 9. Contratos y documentación

- [ ] 9.1 Actualizar `contracts/openapi/core-api.v1.yaml`
- [ ] 9.2 ADR Propuesto en `docs/decisions/`
