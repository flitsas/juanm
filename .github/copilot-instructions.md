# GitHub Copilot — Instrucciones del repositorio GDC 2.0

> Instrucciones del repositorio. Editar directamente en este archivo.
> Agentes y skills en Cursor: `.cursor/agents/`, `.cursor/skills/`.

## Stack

- **Backend:** .NET 10 + C# + EF Core 10 + PostgreSQL 17 (`services/core-api/`)
- **Frontend:** React 19 + Next.js 16 + TypeScript + Tailwind 4 + Biome (`frontend/`)
- **Calidad:** Biome (frontend), `dotnet format`, SonarCloud
- **Gestión:** Azure DevOps Boards (Feature → US → Task → Bug)
- **Repo:** pnpm workspaces (monorepo)

## Convenciones críticas (extracto de `agent-templates/conventions.md`)

- **Branches:** `feature/AB-1234-descripcion`
- **Commits:** `HU1234: descripción breve`
- PRs target: `develop` (nunca `main`), ≤ 800 líneas
- TypeScript strict (no `any`, no `as`, no `!`)
- 4 estados de UI: vacío / cargando / error / lleno
- WCAG 2.1 AA en todo frontend
- Backend: Clean Architecture (Domain → Application → Infrastructure → API)

## Rutas canónicas

| Capa | Path |
|------|------|
| Backend módulos | `services/core-api/src/Flit.Modules.<Modulo>/` |
| API endpoints | `services/core-api/src/Flit.Api/Endpoints/` |
| Migraciones EF | `services/core-api/src/Flit.Infrastructure/Migrations/` |
| Frontend features | `frontend/src/features/<feature>/` |
| Frontend páginas | `frontend/src/app/<ruta>/page.tsx` |
| Contratos API | `contracts/openapi/core-api.v1.yaml` |

## Agentes disponibles

Ver `.github/instructions/agent-*.instructions.md` para reglas específicas por scope.
Lista completa en `AGENTS.md`.

## No-go zones

- `infra/secrets/` — secretos generados por `scripts/gen-secrets.sh`
- `.env`, `.env.user-identity`
- `agent-templates/` — corporativo FLIT (no modificar sin OK humano)
- `docs/decisions/ADR-*.md` en estado `Aceptado` (solo Líder Técnico modifica)
- Migraciones EF Core ya aplicadas
