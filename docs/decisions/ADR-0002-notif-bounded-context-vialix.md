# ADR-0002: Bounded context NOTIF vialix — motor email, reglas y escritura DGC

**Fecha**: 2026-06-10  
**Status**: Propuesto  
**Deciders**: Willyn Londoño Calle (aprobación diseño), Líder Técnico FLIT (promoción a Aceptado)  
**Tags**: arquitectura, backend, frontend, modulo-notif, vialix, postgresql, email

## Contexto

El Feature ADO **#9563** ([CONFIG-NOTIFICACIONES]) implementa el motor de envío de correos que DGC (#9560) dejó como lectura en `dgc.email_logs`. Incluye 8 HUs (#9743–#9750): persistencia, APIs admin/proveedor/plantillas/reglas, worker de cola y UI Admin.

Restricciones:

- Entrega **rama única** `feature/AB-9563-notificaciones`, commits `HU####:`, PR a `develop` sin merge automático.
- Contrato escritura: `contracts/dgc/email-log-write-contract.md`.
- Diseño: `docs/designs/9563-notif-motor.md`.

## Decisión

Implementar **Gdc.Modules.Notif** como vertical slice con schema **`notif`**, strategy **`IEmailSender`** (API / SendGrid / FLIT Mail), credenciales cifradas, **`NotifDispatchWorker`** (`IHostedService`) y **`IEmailLogWriter`** para insertar en `dgc.email_logs` tras cada envío.

## Alternativas consideradas

### Opción 1: Vertical slice NOTIF + worker hosted + escritura directa email_logs (recomendada)

**Pros:**
- Alineado a convenciones FLIT (schema por dominio, RLS).
- Contrato DGC ya documentado; integración acotada.
- Strategy pattern permite múltiples proveedores sin cambiar dominio.
- Una rama feature-batch coherente con #9560.

**Cons:**
- Escritura cross-schema acopla módulos a nivel infraestructura.
- Worker simple sin retry avanzado (RF11 fuera de alcance).
- Depende de DGC mergeado para pruebas E2E reales.

**Esfuerzo:** L  
**Riesgos:** Credenciales en BD; cola sin dead-letter sofisticado.

### Opción 2: Eventos outbox DGC ↔ NOTIF

**Pros:**
- Desacoplamiento estricto entre bounded contexts.
- Escalabilidad futura con bus de mensajes.

**Cons:**
- Complejidad prematura para MVP vialix.
- Retrasa entrega de 8 HUs en un solo sprint.

**Esfuerzo:** XL  
**Riesgos:** Infra adicional no disponible en DEV.

### Opción 3: Servicio email separado (microservicio)

**Pros:**
- Aislamiento de fallos SMTP.
- Deploy independiente.

**Cons:**
- Contradice monolito modular `core-api` acordado.
- Duplica auth, tenant context y operaciones.

**Esfuerzo:** XL  
**Riesgos:** Costo operativo y latencia.

## Consecuencias

- Schema `notif`: `email_provider_config`, `email_template`, `notification_rule`, `email_queue`.
- NOTIF es **único writer** de `dgc.email_logs`; DGC mantiene solo GET.
- Frontend: `frontend/src/features/notif/` bajo shell Admin.
- OpenAPI extendido con tag `Notif`.

## Referencias

- `docs/designs/9563-notif-motor.md`
- `openspec/changes/vialix-notif-motor/`
- `contracts/dgc/email-log-write-contract.md`
- ADR-0001 (DGC bounded context)
