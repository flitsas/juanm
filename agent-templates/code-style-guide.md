# Code Style Guide — Equipo FLIT · GDC 2.0

Stack canónico: **Backend .NET 10 + EF Core 10 + PostgreSQL** · **Frontend React 19 + Next.js 16 + Tailwind 4**.

---

## C# / Backend (.NET 10 + EF Core)

```csharp
// ✅ Clean Architecture — dirección de dependencias
// Domain ← Application ← Infrastructure → Api (endpoints)

// Domain: puro, sin referencias a EF Core ni ASP.NET
public sealed class Persona
{
    public Guid Id { get; }
    public string Nombre { get; }
    public string Documento { get; }

    private Persona(Guid id, string nombre, string documento)
    {
        Id = id;
        Nombre = nombre;
        Documento = documento;
    }

    public static Persona Create(string nombre, string documento)
    {
        if (string.IsNullOrWhiteSpace(nombre))
            throw new PersonaInvalidaException("nombre requerido");
        return new Persona(Guid.NewGuid(), nombre.Trim(), documento);
    }
}

// Application: handler con inyección de dependencias
public sealed class CreatePersonaHandler
{
    private readonly IPersonaRepository _repo;

    public CreatePersonaHandler(IPersonaRepository repo) => _repo = repo;

    public async Task<Guid> HandleAsync(CreatePersonaCommand cmd, CancellationToken ct)
    {
        var persona = Persona.Create(cmd.Nombre, cmd.Documento);
        await _repo.AddAsync(persona, ct);
        return persona.Id;
    }
}

// Api: endpoint delgado — sin lógica de negocio
app.MapPost("/api/v1/personas", async (
    CreatePersonaCommand cmd,
    CreatePersonaHandler handler,
    CancellationToken ct) =>
{
    var id = await handler.HandleAsync(cmd, ct);
    return Results.Created($"/api/v1/personas/{id}", new { id });
});
```

```csharp
// ❌ Prohibido

// Lógica de negocio en el endpoint
app.MapPost("/api/v1/personas", async (CreatePersonaDto dto, AppDbContext db) =>
{
    var exists = await db.Personas.AnyAsync(p => p.Documento == dto.Documento); // ❌
    // ...
});

// SQL con interpolación de strings (SQL injection)
var sql = $"SELECT * FROM personas WHERE id = '{id}'";  // ❌

// Hardcoded credentials
const string DbPassword = "secreto123";  // ❌
```

### Naming (C#)

| Elemento | Convención | Ejemplo |
|----------|------------|---------|
| Clases, records | PascalCase | `PersonaRepository` |
| Interfaces | `I` + PascalCase | `IPersonaRepository` |
| Métodos, propiedades | PascalCase | `FindByIdAsync` |
| Parámetros, locales | camelCase | `personaId` |
| Constantes | PascalCase | `MaxRetries` |
| Archivos | 1 tipo público = 1 archivo | `CreatePersonaHandler.cs` |

### EF Core

```csharp
// ✅ Queries parametrizadas con LINQ
await _context.Personas
    .Where(p => p.Documento == documento)
    .FirstOrDefaultAsync(ct);

// ✅ Configuración en clase dedicada (Fluent API)
public class PersonaConfiguration : IEntityTypeConfiguration<PersonaEntity>
{
    public void Configure(EntityTypeBuilder<PersonaEntity> builder)
    {
        builder.ToTable("personas", "core");
        builder.HasKey(p => p.Id);
    }
}

// ❌ Prohibido
context.Database.ExecuteSqlRaw($"DELETE FROM personas WHERE id = '{id}'");  // ❌
```

### Logging (ILogger)

```csharp
// ✅ Logging estructurado
_logger.LogInformation("Persona creada {PersonaId} en {DurationMs}ms", personaId, elapsed);

// ❌ Nunca logues secretos
_logger.LogDebug("Password: {Password}", password);  // ❌
_logger.LogInformation("Token: {Token}", token);      // ❌
```

### Configuración

```csharp
// ✅ Options pattern o IConfiguration — nunca literales
services.Configure<DatabaseOptions>(configuration.GetSection("ConnectionStrings"));

// Cadena Npgsql en appsettings.Development.json o ConnectionStrings__Core en env
```

---

## TypeScript / Frontend (React 19 + Next.js 16)

```typescript
// ✅ Estricto: no any, no as, no !
const id: string = getId() ?? ''

// ✅ Naming
class PersonasApi {}             // PascalCase para clases
interface PersonaDto {}          // PascalCase para tipos
const findById = () => {}        // camelCase para funciones
const MAX_RETRIES = 3            // UPPER_SNAKE_CASE para constantes
```

```typescript
// ❌ Prohibido
const result: any = getData()    // no any
const id = (value as string)     // no as (usa type guards)
const name = config!.name        // no ! (non-null assertion)
console.log('debug')             // no console.log en producción
```

### Componentes y UI

```tsx
// ✅ Los 4 estados de UI — siempre todos ('use client' donde aplique)
'use client'

function PersonasList() {
  const { data, isLoading, error, refetch } = usePersonas()

  if (isLoading) return <LoadingSkeleton rows={5} />
  if (error) return <ErrorState error={error} onRetry={refetch} />
  if (!data?.length) return <EmptyState message="No hay personas registradas" />
  return <PersonasTable data={data} />
}

// ✅ Hooks con TanStack Query
function usePersonas() {
  return useQuery({
    queryKey: ['personas'],
    queryFn: () => personasApi.list(),
  })
}

// ✅ Server Component por defecto; 'use client' solo para interactividad
// app/personas/page.tsx — composición fina, lógica en features/
```

```tsx
// ❌ Prohibido

// Fetch directo en componente
useEffect(() => { fetch('/api/personas').then(...) }, [])  // ❌

// Variables de entorno sin NEXT_PUBLIC_ en cliente
const apiUrl = process.env.API_URL  // ❌ — usa process.env.NEXT_PUBLIC_API_URL

// XSS
<div dangerouslySetInnerHTML={{ __html: userContent }} />  // ❌ — falta DOMPurify
```

### Imports (TypeScript)

```typescript
// ✅ Orden de imports (Biome enforces this)
// 1. External packages
import { useQuery } from '@tanstack/react-query'
// 2. Internal aliases (@/)
import { personasApi } from '@/features/personas/api/personas.api'
// 3. Relative
import { PersonasTable } from './PersonasTable'

// ❌ Prohibido
import { something } from '../../../shared/utils'  // deep relative — usa @/
```

### Env vars (frontend)

```typescript
// ✅ Solo NEXT_PUBLIC_* en código cliente
const apiBaseUrl = process.env.NEXT_PUBLIC_API_BASE_URL

// Validar en build time con Zod si es crítico (server-side o next.config)
```

---

## Tests

### Backend (xUnit)

```csharp
public class CreatePersonaHandlerTests
{
    [Fact]
    public async Task HandleAsync_CreaPersona_ConDatosValidos()
    {
        // Arrange
        var repo = Substitute.For<IPersonaRepository>();
        var handler = new CreatePersonaHandler(repo);
        var cmd = new CreatePersonaCommand("Juan Pérez", "1234567890");

        // Act
        var id = await handler.HandleAsync(cmd, CancellationToken.None);

        // Assert
        id.Should().NotBeEmpty();
        await repo.Received(1).AddAsync(Arg.Any<Persona>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_LanzaExcepcion_CuandoNombreVacio()
    {
        var repo = Substitute.For<IPersonaRepository>();
        var handler = new CreatePersonaHandler(repo);

        await Assert.ThrowsAsync<PersonaInvalidaException>(() =>
            handler.HandleAsync(new CreatePersonaCommand("", "123"), CancellationToken.None));
    }
}
```

### Frontend (Vitest + RTL)

```typescript
describe('PersonasList', () => {
  it('muestra estado vacío cuando no hay datos', () => {
    render(<PersonasList />)
    expect(screen.getByText(/no hay personas/i)).toBeInTheDocument()
  })
})
```

---

## Rutas canónicas

| Capa | Path |
|------|------|
| Backend módulos | `services/core-api/src/Flit.Modules.<Modulo>/` |
| API endpoints | `services/core-api/src/Flit.Api/Endpoints/` |
| Migraciones EF | `services/core-api/src/Flit.Infrastructure/Migrations/` |
| Frontend features | `frontend/src/features/<feature>/` |
| Frontend páginas | `frontend/src/app/<ruta>/page.tsx` |
| Contratos API | `contracts/openapi/core-api.v1.yaml` |
