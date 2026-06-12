# gdc-frontend

UI Plantillas (`/gdc`) y grilla DP en detalle comparendo.

## Requirements

### Requirement: Catálogo plantillas (HU #9756)

La ruta `/gdc` MUST permitir listar, cargar y configurar plantillas PDF.

#### Scenario: Admin tenant

- **WHEN** el admin entra a Plantillas
- **THEN** ve catálogo vacío/cargando/error/lleno según datos

### Requirement: Preview y generación (HU #9757)

El operador MUST previsualizar PDF compilado y descargarlo antes de radicar.

#### Scenario: Validación pre-radicación

- **WHEN** el operador genera preview desde plantilla mapeada
- **THEN** obtiene PDF con campos rellenados

### Requirement: Grilla DP en comparendo (HU #9758)

`DgcDetailPanel` MUST reemplazar el stub readonly con grilla API DP.

#### Scenario: Detalle comparendo

- **WHEN** el operador abre tab detalle
- **THEN** ve sección DP con estados y descarga, no solo `dp_referencia`
