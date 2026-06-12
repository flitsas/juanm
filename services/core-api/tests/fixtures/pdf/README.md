# Fixtures PDF — spike AcroForm GDC (#9564)

## Plantilla principal

| Archivo | Origen |
|---------|--------|
| `vialix-dp-template.pdf` | Generado con `generate_vialix_dp_template.py` (diseño tokens FLIT) |
| `generate_vialix_dp_template.py` | Script de regeneración |

Regenerar:

```bash
python services/core-api/tests/fixtures/pdf/generate_vialix_dp_template.py
```

### Campos AcroForm incluidos

- `tenant_nombre`, `comparendo_*` (7 campos), `contraventor_*` (3), `secretaria_destino`
- `comparendo_estado` (choice: Pendiente, Notificado, En trámite, Pagado, Cerrado)

## Plantillas adicionales

Coloca variantes en esta carpeta (`vialix-*.pdf`) para spikes de secretarías distintas.

## Validación local

```bash
cd services/core-api
dotnet test tests/Gdc.Infrastructure.Tests --filter PdfAcroFormSpike
```

Tras agregar PDFs, el spike debe listar tags AcroForm y validar relleno con PdfSharpCore.
