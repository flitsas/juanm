# Security Checklist — FLIT · GDC 2.0

Stack: **.NET 10 + EF Core + PostgreSQL** (backend) · **Next.js 16 + React** (frontend).

## Para desarrolladores (inline review — antes de abrir PR)

Verifica manualmente estos 7 patrones ANTES de abrir la PR:

| # | Patrón | Verificación |
|---|--------|--------------|
| 1 | **SQL injection** | No hay strings concatenados en queries. Usa EF Core LINQ, `FromSqlRaw` con parámetros `{0}` o Dapper parametrizado. |
| 2 | **Hardcoded credentials** | No hay passwords, API keys, tokens como literales en el código. |
| 3 | **Secret logging** | No se logean campos: `password`, `token`, `jwt`, `secret`, `apiKey`, `authorization`. |
| 4 | **dangerouslySetInnerHTML** | Si se usa, siempre con `DOMPurify.sanitize()` en la misma línea. |
| 5 | **eval() / new Function()** | No se usan con input de usuarios. |
| 6 | **CSRF** | Formularios POST/PUT/DELETE tienen token CSRF o el endpoint requiere Authorization header. |
| 7 | **Direct DB en endpoint** | No hay queries directas a la DB en Minimal API endpoints. Toda lógica en handlers/use cases. |

## Habeas Data Colombia — Ley 1581 de 2012

Aplica a cualquier campo PII (Personally Identifiable Information):

| Campo PII | Controles requeridos |
|-----------|---------------------|
| `cedula` / `documento` | Consentimiento explícito + encryption at rest + retention policy |
| `email` | Consentimiento explícito + retention policy |
| `telefono` | Consentimiento explícito + retention policy |
| `nombreCompleto` / `nombre` + `apellido` | Consentimiento explícito |
| `direccion` | Consentimiento explícito + retention policy |
| `fechaNacimiento` | Consentimiento explícito |
| Datos sensibles (salud, política, religión) | Consentimiento explícito ESPECIAL (Ley 1581 Art. 6) |

### Controles técnicos mínimos

```csharp
// ✅ Encryption at rest para PII sensible (EF Core + value converter o servicio de cifrado)
builder.Property(p => p.Cedula)
    .HasConversion(new EncryptedStringConverter(encryptionService))
    .HasMaxLength(512);

// ✅ Consentimiento explícito en el modelo
public bool ConsentimientoDatosPersonales { get; set; }
public DateTimeOffset? FechaConsentimiento { get; set; }
```

```typescript
// ✅ Frontend: no persistir PII en localStorage/sessionStorage sin cifrado y consentimiento
// ✅ No enviar PII en query strings de URL
```

### Retention policy

Documenta en `docs/data-retention.md`:
- ¿Cuánto tiempo se conserva cada tipo de dato PII?
- ¿Qué proceso elimina datos al vencer la retención?
- ¿Hay ADR sobre retención? (`docs/decisions/ADR-XXXX-data-retention.md`)

## Para el Security Agent (análisis profundo)

### Layer 1 — SAST con Semgrep

```bash
# Reglas mínimas
semgrep --config=p/owasp-top-ten \
        --config=p/cwe-top-25 \
        --config=.semgrep.yml \
        --json --output semgrep-report.json .
```

Crear `.semgrep.yml` en la raíz con reglas custom para patrones específicos de FLIT.

### Layer 2 — SCA (dependencias)

```bash
# Frontend (pnpm)
pnpm audit --prod > audit-report-frontend.txt

# Backend (.NET)
cd services/core-api && dotnet list package --vulnerable > ../../audit-report-backend.txt
```

Tolerancia FLIT:
- Critical: 0
- High: 0 (o con excepción documentada en `docs/security-exceptions/`)
- Medium: máximo 5 con justificación

### Layer 3 — Secrets scan

```bash
gitleaks detect --source . --report-format json --report-path gitleaks-report.json
```

Si hay hallazgos → reset del secreto INMEDIATO + documentar en `docs/security-exceptions/`.

### Documentar excepciones

Si un hallazgo es falso positivo, crear `docs/security-exceptions/<hash>.md`:

```markdown
# Excepción de Seguridad: <descripción>

**Fecha**: YYYY-MM-DD
**Herramienta**: semgrep | gitleaks | pnpm audit | dotnet list package
**Regla/CVE**: <nombre>
**Archivo:línea**: <path>
**Razón del falso positivo**: <explicación técnica>
**Aprobado por**: <Líder Técnico>
**Próxima revisión**: YYYY-MM-DD
```
