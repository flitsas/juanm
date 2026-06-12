# gdc-dp-lifecycle

Ciclo de vida DP, grilla 1:N y versionamiento (RF04–RF06).

## Requirements

### Requirement: Estados DP (RF05)

DP MUST transitar: No Enviado → Enviado → Sin Respuesta → Con Respuesta.

#### Scenario: Radicación

- **WHEN** el operador marca DP como Enviado
- **THEN** el estado actualiza y no permite regeneración automática por plantilla

### Requirement: Grilla multidocumento (RF04)

El detalle comparendo MUST listar DPs con fecha, estado y enlace descarga.

#### Scenario: Auditoría trámite

- **WHEN** el operador abre detalle comparendo
- **THEN** ve grilla con todos los DPs del comparendo

### Requirement: Versionamiento condicional (RF06)

Al editar plantilla maestra, solo DPs en estado No Enviado MUST regenerarse.

#### Scenario: Bump versión plantilla

- **WHEN** la plantilla sube de v1 a v2
- **THEN** DPs Enviados conservan PDF v1; No Enviados se marcan para regeneración
