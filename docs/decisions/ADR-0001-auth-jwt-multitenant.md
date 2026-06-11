# ADR-0001: Autenticación JWT multi-tenant con RBAC en schema auth

**Fecha**: 2026-06-10  
**Status**: Propuesto  
**Deciders**: Líder Técnico FLIT, equipo GDC  
**Tags**: arquitectura, backend, seguridad, auth-usuarios

---

## Contexto

GDC 2.0 requiere identidad centralizada con aislamiento por empresa (tenant) y control granular por roles antes de módulos de negocio. El Feature ADO #9561 define login, invitación, activación, recuperación de clave y matriz RBAC. No hay módulo auth existente; PostgreSQL tiene schema `core` vacío.

## Decisión

Implementar autenticación con **JWT propio** (ASP.NET JwtBearer), módulo **Flit.Modules.Auth**, persistencia en schema PostgreSQL **`auth`**, enforcement multi-tenant vía **ITenantContext + global query filters EF Core**, y RBAC con **tablas roles/permissions** y authorization policies.

---

## Alternativas consideradas

### Opción 1: JWT propio + EF multi-tenant (seleccionada)

**Descripción**: Monolito modular con tokens firmados y filtros por tenant.

**Pros:**
- Sin dependencia de IdP externo
- Alineado a Clean Architecture del repo
- Control fino de claims tenant/role
- Menor complejidad operativa en DEV

**Cons:**
- Responsabilidad de seguridad en el equipo
- Rotación de claves JWT manual
- Refresh token requiere trabajo adicional

**Esfuerzo estimado**: M  
**Riesgos principales**: Errores en bypass Super Admin; mitigar con policies explícitas y tests.

---

### Opción 2: Keycloak / Entra ID

**Descripción**: Delegar identidad a proveedor OIDC.

**Pros:**
- Estándar enterprise
- Facilita SSO futuro
- MFA nativo en algunos proveedores

**Cons:**
- SSO explícitamente fuera de alcance
- Infra adicional
- Curva de integración

**Esfuerzo estimado**: L  
**Riesgos principales**: Scope creep hacia SSO/2FA.

---

### Opción 3: Session cookies sin JWT

**Descripción**: Autenticación solo por cookie de sesión server-side.

**Pros:**
- Revocación simple
- Menos exposición de token en cliente

**Cons:**
- Peor para APIs consumidas fuera del browser
- Escalado horizontal requiere session store

**Esfuerzo estimado**: S  
**Riesgos principales**: Limita integraciones futuras.

---

## Consecuencias

**Positivas:**
- Base reutilizable para todos los módulos vialix
- Contratos OpenAPI claros
- Tests de aislamiento tenant automatizables

**Negativas:**
- Deuda: refresh token y rotación de signing keys en iteración posterior
- Dependencia de servicio SMTP para flujos de correo

## Referencias

- Feature ADO #9561
- `docs/designs/9561-auth-usuarios-gdc.md`
- `openspec/changes/auth-usuarios-gdc/`
