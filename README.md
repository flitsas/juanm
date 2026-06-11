# GDC 2.0 — Agentes IA + Full-Stack



Monorepo del equipo FLIT con agentes IA integrados al pipeline de desarrollo.



## Quickstart



```bash

# 1. Clonar e instalar dependencias (Node + .NET + frontend)

git clone <URL-del-repo>

cd gdc-2.0

corepack enable && corepack prepare pnpm@latest --activate

pnpm install:all
# Primera vez en pnpm 11+: aprobar build scripts de esbuild y sharp
pnpm approve-builds



# 2. Configurar entorno de tooling (ADO, Sonar, etc.)

cp .env.example .env

cp .env.user-identity.example .env.user-identity

./scripts/setup-env.sh --interactive   # opcional, modo guiado



# 3. Secretos de runtime (JWT, passwords de contenedores)

./scripts/gen-secrets.sh



# 4. Levantar infraestructura local

cd infra && docker compose up -d && cd ..



# 5. Migraciones EF Core

pnpm migrate



# 6. Iniciar frontend + backend en paralelo

pnpm dev

```



**URLs en DEV:**



| Servicio | URL |

|----------|-----|

| Frontend (Next.js) | http://localhost:40103 |

| Gateway (YARP) | http://localhost:40203 |

| API (core-api) | http://localhost:40303 |

| Health check | http://localhost:40203/health |



## OpenSpec + FLIT (diseño híbrido)



OpenSpec (`/opsx:propose`, `/opsx:explore`) sirve para **explorar y diseñar** cambios en `openspec/changes/` antes del diseño formal. La implementación, ADO, PRs y deploy siguen el **orquestador FLIT**. No usar `/opsx:apply` en lugar del pipeline FLIT. Detalle en `INSTRUCCIONES.md` y `.cursor/rules/openspec-integration.mdc`.



## Stack



| Capa | Tecnología | Path |

|------|------------|------|

| **Backend** | .NET 10 + C# + EF Core 10 + PostgreSQL 17 | `services/core-api/` |

| **Frontend** | React 19 + Next.js 16 + TypeScript + Tailwind 4 + Biome | `frontend/` |

| **Tests** | xUnit (backend) + Vitest (frontend) + Playwright (E2E) |

| **CI/CD** | GitHub Actions + pnpm workspaces |

| **Gestión** | Azure DevOps Boards (Feature → US → Task → Bug) |



## Estructura del proyecto



```

.cursor/agents/         ← Agentes IA (fuente operativa en Cursor)

.cursor/skills/         ← Skills del pipeline FLIT

.cursor/rules/          ← Reglas Cursor

openspec/               ← Diseño local OpenSpec (Fase 2 opcional)

agent-templates/        ← Plantillas FLIT corporativas (DoR, DoD, conventions)

services/core-api/      ← Backend .NET 10 + EF Core + PostgreSQL

frontend/               ← UI Next.js 16 (App Router, feature-sliced)

infra/                  ← Docker Compose + secretos de runtime

contracts/openapi/      ← Contratos API versionados

docs/decisions/         ← ADRs

scripts/                ← Setup, secrets, utilidades

```



## Agentes IA disponibles



```bash

# Abrir el repo en Cursor o Claude Code

claude .

```



| Comando | Qué hace |

|---------|----------|

| `/flit:full-flow <feature>` | Pipeline completo Feature → DEV |

| `/flit:implement <US>` | Implementar una US end-to-end |

| `/flit:review <PR>` | Code Review + Security |

| `/flit:deploy <env>` | Deploy a DEV/QA/PDN |

| `/flit:validate dor/dod <item>` | Validar Definition of Ready/Done |



Los agentes aceptan historias desde múltiples fuentes: ID ADO, archivo local, URL o texto directo.



## Crear un nuevo módulo



```bash

# Backend (.NET — vertical slice por módulo)

mkdir -p services/core-api/src/Flit.Modules.<Modulo>/{Domain,Application,Infrastructure}



# Frontend (feature-sliced dentro de Next.js App Router)

mkdir -p frontend/src/features/<feature>/{api,components,hooks}

mkdir -p frontend/src/app/<ruta>

```



Luego invoca:



```

Use the backend-agent to implement story #<ID>

Use the frontend-agent to implement story #<ID>

```



## Comandos útiles



```bash

pnpm dev          # frontend + core-api en paralelo

pnpm test         # tests .NET + frontend

pnpm test:e2e     # Playwright E2E

pnpm lint         # biome (frontend) + dotnet format

pnpm migrate      # dotnet ef database update

pnpm doctor       # diagnóstico de puertos y toolchain

```



## Para el equipo



- Lee `AGENTS.md` — guía del monorepo para agentes IA

- Lee `INSTRUCCIONES.md` — guía paso a paso para usar los agentes



## Agentes IA (Cursor)



Fuente operativa: `.cursor/agents/`, `.cursor/skills/` y `.cursor/rules/`. Editar y commitear directamente; no hay script de sincronización en CI.



## Inmutables (no tocar sin OK humano explícito)



- `agent-templates/` (corporativo FLIT)

- `.env`, `.env.user-identity`, `infra/secrets/`

- `docs/decisions/ADR-*.md` en estado `Aceptado` (solo Líder Técnico)

- Migraciones EF Core ya aplicadas


