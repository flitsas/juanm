# gdc-acroform-api

APIs de carga PDF, extracción AcroForm y mapeo de variables (RF01–RF02).

## Requirements

### Requirement: Extracción tags AcroForm (RF01)

Al subir un PDF con formulario, el sistema MUST listar nombres de campos detectados.

#### Scenario: Upload plantilla DP

- **WHEN** el admin sube `vialix-dp-template.pdf`
- **THEN** la API retorna lista de tags AcroForm con tipos inferidos

### Requirement: Tipado polimórfico (RF02)

Cada campo MUST mapearse a una variable del sistema con tipo text, number o choice.

#### Scenario: Mapeo contraventor

- **WHEN** el admin asigna tag `nombre_infractor` → `contraventor.nombre`
- **THEN** el mapeo persiste y valida en compilación

### Requirement: Bloqueo sin contraventor (RF07)

La generación MUST fallar si el comparendo no tiene contraventor asociado.

#### Scenario: Comparendo pendiente

- **WHEN** se intenta generar DP sin `dgc.contraventors`
- **THEN** la API retorna error `GDC_CONTRAVENTOR_REQUIRED`
