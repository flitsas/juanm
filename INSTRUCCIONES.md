# INSTRUCCIONES.md — Cómo usar el Boilerplate FLIT con Agentes IA



Guía paso a paso para el equipo. Sin saltar pasos.



---



## Parte 1 — Configuración inicial (solo una vez)



### Paso 1: Clonar e instalar



```bash

git clone <URL-del-repo>

cd gdc-2.0

corepack enable && corepack prepare pnpm@latest --activate

pnpm install:all

```



Esto instala dependencias del workspace raíz (pnpm), del frontend (Next.js) y restaura la solución .NET en `services/core-api/`.



### Paso 2: Variables de entorno



```bash

cp .env.example .env

cp .env.user-identity.example .env.user-identity

# Edita .env y .env.user-identity con tu org/proyecto ADO y credenciales

```



Para modo guiado:



```bash

./scripts/setup-env.sh --interactive

```



PostgreSQL local (DEV): usuario `postgres`, contraseña `postgres`, base de datos `gdc_dev` en `localhost:5432`.

La cadena de conexión está en `services/core-api/src/Gdc.Api/appsettings.Development.json` (o variable de entorno `ConnectionStrings__Core`).

Crear la base de datos si no existe (EF la crea al migrar si el usuario tiene permisos):

```sql
CREATE DATABASE gdc_dev;
```



### Paso 3: Secretos de runtime



```bash

./scripts/gen-secrets.sh

```



Genera JWT y passwords para contenedores en `infra/secrets/` (gitignored).



### Paso 4: Levantar la base de datos local



```bash

# Requiere Docker Desktop corriendo

cd infra

docker compose up -d postgres

```



Verifica que PostgreSQL está listo:



```bash

docker compose ps   # postgres debe estar en "healthy"

```



### Paso 5: Correr migraciones EF Core



```bash

# Desde la raíz del monorepo

pnpm migrate

```



Equivale a `dotnet ef database update` en `services/core-api/`.



### Paso 6: Iniciar el proyecto completo



```bash

# En la raíz

pnpm dev

# Frontend (Next.js): http://localhost:40103

# Gateway (YARP):     http://localhost:40203

# API (core-api):     http://localhost:40303

```



Prueba que funciona:



```bash

curl http://localhost:40203/health

```



---



## Parte 2 — Configurar los agentes IA



### Para Claude Code (recomendado)



Los agentes en `.claude/agents/` se cargan automáticamente cuando abres Claude Code en este directorio.



```bash

# Abrir Claude Code en el repo

claude .

```



Comandos disponibles (escríbelos en el chat):



```

/flit:full-flow          # Pipeline completo Feature → DEV

/flit:implement          # Implementar una US

/flit:review             # Review de una PR

/flit:deploy             # Deploy a ambiente

/flit:decompose          # Descomponer Feature en US

/flit:health             # Health check del repo

/flit:validate           # Validar DoR/DoD

/flit:crear-feature      # Crear una nueva Feature

/flit:crear-hu           # Crear una nueva User Story (HU)

/flit:gestion-hu         # Gestionar/refinar una User Story

/flit:gestion-qa         # Gestionar tareas de QA para una User Story

```



**Primera prueba — el orquestador:**



```

Use the orchestrator to list available flows

```



### Para Cursor



1. Abre el repo en Cursor

2. Las reglas en `.cursor/rules/` se aplican automáticamente

3. Para activar un agente específico, invócalo por nombre o pega el contenido de `.cursor/agents/<nombre>-agent.md`



### Para GitHub Copilot



Las instrucciones en `.github/copilot-instructions.md` se aplican automáticamente en VS Code con Copilot.



### Para OpenAI Codex / Antigravity / otras herramientas



1. Lee `AGENTS.md` (en la raíz) — es la fuente de verdad del stack

2. Copia el contenido del agente que necesitas desde `.cursor/agents/<nombre>-agent.md`

3. Úsalo como system prompt o instrucción de contexto en tu herramienta



### Mantener agentes en Cursor



Edita y commitea directamente `.cursor/agents/`, `.cursor/skills/` y las reglas en `.cursor/rules/`.



### OpenSpec (diseño local, opcional)



OpenSpec complementa FLIT **solo en exploración y diseño** (Fase 2). ADO y el orquestador siguen siendo la fuente de verdad para implementación, PRs y deploy.



**Instalación (una vez por máquina):**



```bash

npm install -g @fission-ai/openspec@latest

# Ya inicializado en este repo con: openspec init --tools cursor --force

```



**Comandos en Cursor:**



| Comando | Uso |
|---------|-----|
| `/opsx:propose "nombre"` | Crear un change con propuesta, diseño y tasks |
| `/opsx:explore` | Explorar ideas sin implementar |
| `/opsx:sync` | Sincronizar specs delta → `openspec/specs/` |
| `/opsx:archive` | Archivar change tras exportar diseño |

**No usar** `/opsx:apply` como sustituto del orquestador FLIT.



**Flujo híbrido recomendado:**



1. (Opcional) `/opsx:propose` para borrador local en `openspec/changes/`
2. Orquestador → Feature en ADO (Fase 1)
3. Exportar diseño a `docs/designs/` + `architecture-agent` (Fase 2)
4. HUs, implementación, review, merge — siempre vía orquestador



Regla: `.cursor/rules/openspec-integration.mdc`



---



## Parte 3 — Flujo de trabajo diario



### 1. Empezar con una Feature nueva



**Opción A — Con el orquestador (recomendado):**



```

Use the orchestrator to run FULL_FEATURE for: <pega aquí la descripción de la Feature o da el ID>

```



**Opción B — Comando específico:**



```

/flit:crear-feature

```



### 2. Descomponer la Feature en User Stories



```

Use the orchestrator to run DECOMPOSE_FEATURE for: <referencia>

```



O:



```

Use the tech-lead-agent (mode B) to decompose feature #4520

```



### 3. Gestionar o Refinar una User Story



```

/flit:gestion-hu <referencia>

```



### 4. Gestionar QA



```

/flit:gestion-qa <referencia>

```



### 5. Implementar una User Story



```

Use the orchestrator to run IMPLEMENT_US for: story #4521

```



### 6. Revisar una Pull Request



```

Use the orchestrator to run REVIEW_PIPELINE for PR: !456

```



### 7. Deploy



```

Use the orchestrator to run DEPLOY_ENV to: qa build #890

```



### 8. Validar DoR antes de Active



```

Use the tech-lead-agent (mode C) to validate DoR for story #4521

```



---



## Parte 4 — Crear un nuevo módulo (ejemplo: Contratos)



### Estructura backend (.NET 10 + EF Core)



```bash

# Vertical slice por módulo dentro de services/core-api/

mkdir -p services/core-api/src/Flit.Modules.Contratos/{Domain,Application,Infrastructure}

mkdir -p services/core-api/tests/Flit.Modules.Contratos.Tests

```



Archivos típicos (en orden):



1. `Domain/Contrato.cs` — entidad de dominio

2. `Domain/IContratoRepository.cs` — puerto del repositorio

3. `Application/CreateContrato/CreateContratoCommand.cs` — comando + handler

4. `Application/CreateContrato/CreateContratoValidator.cs` — FluentValidation

5. `Infrastructure/Persistence/ContratoConfiguration.cs` — EF Core fluent config

6. `Infrastructure/Persistence/ContratoRepository.cs` — implementación del puerto

7. `Infrastructure/Migrations/<timestamp>_AddContratos.cs` — migración EF Core

8. `Flit.Api/Endpoints/ContratosEndpoints.cs` — Minimal API endpoint



Con el agente:



```

Use the architecture-agent to design the solution for the Contratos module

Use the backend-agent to implement story #4525  (la US [BACKEND] de Contratos)

```



### Estructura frontend (Next.js 16 + React)



```bash

mkdir -p frontend/src/features/contratos/{api,components,hooks}

mkdir -p frontend/src/app/contratos

```



Archivos típicos:



1. `features/contratos/api/contratos.api.ts` — cliente HTTP + TanStack Query

2. `features/contratos/api/contratos.schemas.ts` — Zod schemas

3. `features/contratos/components/ContratosList.tsx` — UI con 4 estados

4. `app/contratos/page.tsx` — página App Router



Con el agente:



```

Use the frontend-agent to implement story #4526  (la US [FRONTEND] de Contratos)

```



---



## Parte 5 — Cómo agregar un agente nuevo



1. Crea el archivo en `.cursor/agents/<nombre>-agent.md` siguiendo los existentes

2. Sigue la estructura de los agentes actuales (Rol, Reglas, Pre-flight, etc.)

3. Incluye el Story Intake Protocol si el agente procesa work items

4. Commitea el agente y, si aplica, una regla en `.cursor/rules/`



---



## Referencia rápida



| Tarea | Comando |

|-------|---------|

| Ver flujos disponibles | `Use the orchestrator to list available flows` |

| Pipeline completo | `/flit:full-flow <feature>` |

| Crear Feature | `/flit:crear-feature` |

| Crear US | `/flit:crear-hu` |

| Descomponer Feature | `/flit:decompose <feature>` |

| Gestionar US | `/flit:gestion-hu <US>` |

| Gestionar QA | `/flit:gestion-qa <US>` |

| Implementar US | `/flit:implement <US>` |

| Review PR | `/flit:review <PR>` |

| Deploy | `/flit:deploy <env> <build>` |

| Validar DoR | `/flit:validate dor <item>` |

| Validar DoD | `/flit:validate dod <item>` |

| Health check | `/flit:health` |

| Agentes Cursor | `.cursor/agents/`, `.cursor/skills/` |

| OpenSpec (diseño) | `/opsx:propose`, `/opsx:explore` — ver `openspec/` |



## Solución de problemas frecuentes



**El agente pide ADO CLI y no lo tengo:**



> El agente detecta que el CLI no está disponible y te ofrecerá recibir la historia en texto directo. Responde con la opción 4 (texto directo) y pega el contenido de la historia.



**El agente no aparece en Claude Code:**



> Verifica que el archivo tiene frontmatter YAML correcto en `.claude/agents/`. Edita `.cursor/agents/` y `.cursor/skills/` directamente.



**Error de base de datos al iniciar:**



> Verifica PostgreSQL local en `localhost:5432` (usuario `postgres`). Revisa `ConnectionStrings:Core` en `services/core-api/src/Gdc.Api/appsettings.Development.json`.



**Las migraciones fallan:**



> Ejecuta `pnpm migrate` desde la raíz. Si la BD está corrupta: `pnpm docker:reset:infra` y vuelve a migrar.


