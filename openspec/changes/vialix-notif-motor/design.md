## Context

- **Feature ADO:** #9563 [CONFIG-NOTIFICACIONES] — Active, Sprint 2.
- **HUs:** #9743 (schema) → #9744+#9745 (paralelo) → #9746 → #9747 → #9748 → #9749 → #9750.
- **Prototipo UI:** `flitready-suite/src/components/modules/admin.tsx`.
- **Dependencia upstream:** DGC #9560 — comparendos + contraventor + lectura `dgc.email_logs`.
- **Contrato cruzado:** `contracts/dgc/email-log-write-contract.md` — NOTIF es el único writer.
- **Modo entrega:** rama `feature/AB-9563-notificaciones`, commits por HU, PR a `develop` sin merge automático.

## Goals / Non-Goals

**Goals:**

- Configurar proveedor email por tenant (API propia, SendGrid, FLIT Mail) con credenciales cifradas.
- Plantillas HTML marca blanca con banner/pie opcionales (PNG/JPG).
- Reglas de comunicación: disparadores cronológicos y por cambio de estado del comparendo.
- Cola de envío con switch On/Off y registro en `dgc.email_logs`.
- Super Admin: CRUD compañías; Tenant Admin: autogestión perfil.
- UI Admin alineada a Flit Ready.

**Non-Goals:**

- Logs de auditoría de errores SMTP detallados (RF10).
- Fallback automático entre proveedores (RF11).
- Adjuntos PDF en correos (RF12 parcial — solo imágenes en plantilla).
- SSO / MFA (#9561).
- Merge/deploy/Resolved automáticos en este ciclo.

## Decisions

### D1 — Schema PostgreSQL `notif`

| Tabla | Propósito |
|-------|-----------|
| `notif.email_provider_config` | Proveedor activo por tenant (tipo, credenciales cifradas, from_address) |
| `notif.email_template` | Plantilla HTML + metadatos banner/pie |
| `notif.notification_rule` | Regla con disparador + plantilla + activo |
| `notif.email_queue` | Cola pendiente/procesando/enviado/fallido |

**RLS:** `tenant_id = current_setting('app.tenant_id')::uuid` en todas las tablas.

**Alternativas:** schema `core` — rechazado (convención bounded context).

### D2 — Escritura cruzada `dgc.email_logs`

**Decisión:** Servicio de dominio `IEmailLogWriter` en módulo NOTIF que inserta vía EF/SQL raw respetando contrato DGC. Sin FK desde `notif` hacia `dgc` en schema NOTIF.

**Alternativas:** eventos outbox — pospuesto; inserción directa más simple para MVP vialix.

### D3 — Arquitectura backend — Vertical Slice

```
services/core-api/src/
├── Gdc.Modules.Notif/
│   ├── Domain/
│   ├── Application/
│   └── Infrastructure/
├── Gdc.Api/Endpoints/NotifEndpoints.cs
└── Gdc.Infrastructure/Persistence/GdcDbContext.cs (+ DbSet Notif)
```

### D4 — Proveedores email

**Decisión:** Strategy pattern `IEmailSender` con implementaciones:
- `ApiEmailSender` — REST genérico configurable.
- `SendGridEmailSender` — SDK SendGrid.
- `FlitMailEmailSender` — SMTP interno FLIT.

Credenciales en columna `credentials_encrypted` (AES-256 con clave de app).

### D5 — Motor de reglas y cola

**Decisión:** Background job `NotifDispatchWorker` (HostedService) que:
1. Evalúa reglas activas contra comparendos DGC (query cross-module vía `IDgcComparendoReader`).
2. Encola en `notif.email_queue`.
3. Despacha si switch tenant = On.
4. Escribe `dgc.email_logs` tras envío.

Disparadores:
- **Cronológico:** días desde `fecha_comparendo` o `fecha_notificacion`.
- **Estatal:** cambio de `estado` del comparendo.

### D6 — Frontend

Rutas bajo `/admin/notificaciones/*` o sección en shell Admin:
- Compañías (Super Admin)
- Proveedor email
- Editor plantillas (WYSIWYG ligero o textarea HTML + preview)
- Reglas + switch global

TanStack Query + 4 estados UI (vacío, cargando, error, lleno).

### D7 — OpenAPI

Extender `contracts/openapi/core-api.v1.yaml` con tag `Notif` y paths `/api/v1/notif/*`.

## Risks / Trade-offs

| Riesgo | Mitigación |
|--------|------------|
| Merge #9560 aún pendiente en develop | Rama desde develop; rebase si necesario post-merge LT |
| Escritura cruzada DGC viola bounded context | Contrato documentado; interfaz única `IEmailLogWriter` |
| Credenciales SMTP en BD | Cifrado + denylist en logs |
| Cola sin retry sofisticado | MVP: estado `fallido` + reintento manual; RF11 fuera de alcance |

## Migration Plan

1. Migración EF `NotifInitialSchema` — schema `notif` + RLS.
2. Seeds opcionales: plantilla demo por tenant dev.
3. Deploy: migración antes de activar worker en DEV.

## Open Questions

- ¿Clave de cifrado en `appsettings` vs Azure Key Vault? — MVP: `EmailEncryption:Key` en secrets local.
- ¿Frecuencia del worker? — Default 60s en DEV, configurable.
