# AGENTS.override.md — Gdc.CoreApi

Scope del backend en `services/core-api/`.

## Stack

- .NET 10 + EF Core 10 + PostgreSQL 17
- Minimal APIs en `src/Gdc.Api/`
- Persistencia en `src/Gdc.Infrastructure/`

## Convenciones

- Namespace: `Gdc.*`
- Migraciones: `dotnet ef` con historial en schema `core`
- Connection string: `ConnectionStrings:Core`
- **PostgreSQL local (DEV):** `postgres` / `postgres` en `localhost:5432`, base de datos `gdc_dev`
- Configuración en `src/Gdc.Api/appsettings.Development.json`
