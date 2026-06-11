## Why

DGC (#9560) expone lectura del log de correos (`dgc.email_log`) pero no existe motor de envío en GDC 2.0. Sin NOTIF no hay notificaciones automáticas al contraventor ni plantillas de marca blanca multi-tenant, bloqueando RF08–RF10 de DGC en producción y la cadena vialix hacia GDC (#9564).

## What Changes

- Nuevo bounded context `Notif` en `services/core-api/` con schema PostgreSQL `notif`.
- Entidades: proveedor email, plantilla, regla, cola de envío; integración de escritura en `dgc.email_log`.
- APIs REST para CRUD compañías (Super Admin), perfil tenant, proveedor, plantillas y reglas.
- Motor de reglas: disparadores cronológicos y estatales sobre comparendos DGC; cola con switch On/Off.
- Módulo frontend Admin/Config alineado a `flitready-suite` (`admin.tsx` + secciones nuevas).
- Contratos OpenAPI en `contracts/openapi/core-api.v1.yaml`.
- Entrega modo **feature-batch**: rama `feature/AB-9563-notificaciones`, commits `HU####:`, PR a `develop` sin merge automático.

## Capabilities

### New Capabilities

- `notif-persistence`: Schema `notif` multi-tenant con RLS; tablas proveedor, plantilla, regla, cola.
- `notif-tenant-admin`: CRUD compañías (Super Admin) y autogestión perfil tenant (RF01–RF02).
- `notif-provider-config`: Integración proveedor email API/SendGrid/FLIT Mail con credenciales cifradas (RF03, RF09).
- `notif-email-templates`: Plantillas marca blanca con banner/pie PNG/JPG (RF04, RF12).
- `notif-rules-engine`: Reglas, cola, despacho y escritura en `dgc.email_log` (RF05–RF08).
- `notif-frontend`: UI Admin compañías, proveedor, editor plantillas y reglas con switch (RF01–RF08).

### Modified Capabilities

- `dgc-email-audit`: NOTIF es el **writer** de `dgc.email_log`; DGC permanece solo lectura.

## Impact

| Área | Impacto |
|------|---------|
| `services/core-api/` | Nuevo módulo `Gdc.Modules.Notif` |
| `frontend/` | Rutas `/admin` o sección config en shell |
| `contracts/openapi/core-api.v1.yaml` | Endpoints `/api/v1/notif/*` |
| PostgreSQL | Schema `notif`; escritura cruzada controlada a `dgc.email_log` |
| ADO | Feature #9563, HUs #9743–#9750 |
| Dependencias | DGC maestra (#9737) para disparadores; AUTH (#9561) para sesión/roles |
| Fuera de alcance | Logs auditoría errores, fallback, adjuntos PDF, SSO (#9561 RF11 parcial) |
