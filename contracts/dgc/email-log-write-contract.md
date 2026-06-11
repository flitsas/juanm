# Contrato interno — escritura `dgc.email_logs` (Feature #9560 / HU #9739)

> **Consumidor previsto:** Feature NOTIF #9563 (motor de envío de correos).  
> **Productor en DGC:** solo lectura vía `GET /api/v1/dgc/comparendos/{id}/emails` y `GET /api/v1/dgc/emails/{id}/evidence`.

## Tabla destino

`dgc.email_logs`

## Campos obligatorios al insertar

| Columna | Tipo | Regla |
|---------|------|-------|
| `id` | `uuid` | UUIDv7 recomendado |
| `tenant_id` | `uuid` | Tenant del comparendo |
| `comparendo_id` | `uuid` | FK a `dgc.comparendos.id` |
| `sent_at` | `timestamptz` | Momento real del envío |
| `origen` | `varchar(256)` | Remitente (ej. `notificaciones@tenant.com`) |
| `destino` | `varchar(256)` | Destinatario principal |
| `tipo_alerta` | `varchar(64)` | Catálogo operativo (ej. `Notificación inicial`, `Recordatorio`) |
| `estado_entrega` | `varchar(32)` | `Entregado`, `Rebotado`, `Pendiente`, etc. |
| `created_at` | `timestamptz` | Auditoría |

## Campos opcionales

| Columna | Tipo | Regla |
|---------|------|-------|
| `cc` | `varchar(512)` | Copias |
| `html_evidencia` | `text` | HTML completo del mensaje (`@pii:medium`) |

## Reglas de negocio

1. Un comparendo puede tener **0..N** registros de log (historial de envíos).
2. DGC **no** modifica ni elimina logs escritos por NOTIF.
3. RLS: el insert debe ejecutarse con `app.tenant_id` alineado al comparendo.
4. La evidencia HTML se sirve **sin transformación** en lectura; el frontend sanitiza con DOMPurify.

## Ejemplo mínimo (SQL)

```sql
INSERT INTO dgc.email_logs (
  id, tenant_id, comparendo_id, sent_at, origen, destino,
  tipo_alerta, estado_entrega, html_evidencia, created_at)
VALUES (
  gen_random_uuid(),
  :tenant_id,
  :comparendo_id,
  now(),
  'notificaciones@flit.dev',
  'contraventor@example.com',
  'Notificación inicial',
  'Entregado',
  '<html><body><p>Mensaje</p></body></html>',
  now());
```
