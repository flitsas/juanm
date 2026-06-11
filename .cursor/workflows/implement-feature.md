# Workflow: Implement Feature (batch HUs)

**Objetivo:** Implementar todas las Historias de Usuario de un Feature en **una sola rama**, con **commits atómicos por HU**, y entregar **un PR a `develop`** sin ejecutar merge ni integración ADO.

**Invocación típica:**
```
Implementa el Feature #9560 — modo feature-batch
```

**Aplica a:** flit-vialix y cualquier Feature donde el equipo defina entrega consolidada por PR.

---

## Reglas de entrega (obligatorias en este modo)

| Regla | Convención | Ejemplo |
|-------|------------|---------|
| **Rama por Feature** | `feature/AB-{feature_id}-{slug}` desde `develop` | `feature/AB-9560-dgc-maestra` |
| **Commit por HU** | `HU{hu_id}: descripción breve` — un commit = una HU | `HU9735: Schema DGC comparendos con RLS` |
| **Tope del flujo** | PR abierto a `develop` con review + security | **No** merge, **no** `integration-agent`, **no** Deploy DEV, **no** `Resolved` |

> Las convenciones globales FLIT (`agent-templates/conventions.md`) siguen vigentes para target `develop`, tamaño de PR ≤ 800 líneas y reviewers humanos. En este modo el PR puede acumular varios commits HU siempre que el diff total respete el límite; si se supera, dividir en PRs secuenciales sobre la misma rama feature o escalar al Líder Técnico.

---

## Precondiciones

- Feature en ADO con estado `Active` y HUs hijas con tag `DOR`.
- Diseño aprobado en `docs/designs/[feature_id]-[slug].md` (o change OpenSpec exportado).
- Orden de implementación definido por dependencias entre HUs.
- Prototipo UI de referencia si aplica (`flitready-suite`).

---

## Fases — resumen

| # | Fase | Agente / Skill | Gate humano |
|---|------|----------------|-------------|
| 0 | Verificar diseño y orden de HUs | orquestador | Aprobación diseño (si aún no) |
| 1 | Crear rama del Feature | orquestador / agente implementador | — |
| 2 | Por cada HU (en orden): DoR | `tech-lead-agent` (modo C) | — |
| 3 | Por cada HU: Activar | `flit-gestion-hu` (Motivo A) | **Confirmación por HU** |
| 4 | Por cada HU: Implementar + commit | `backend-agent` / `frontend-agent` / `database-agent` | — |
| 4b | Lint fullstack (tras cada HU o al cierre) | `pnpm fix:all:fullstack` | — |
| 4c | Validación schema (si hay migraciones) | `database-agent` (modo C) + `db-schema-validator` | — |
| 5 | Por cada HU: Tests + evidencias | `dev-tester` | — |
| 6 | Abrir PR único a `develop` | orquestador / agente implementador | — |
| 7 | Review del PR | `code-review-agent` + `security-agent` | — |
| 8 | **STOP** — Entregar PR listo | orquestador | Usuario / Líder Técnico mergea manualmente |

**Fases explícitamente omitidas en este modo:**

- Fase 6 de `implement-story.md` — Integrar PR (`integration-agent`)
- Fase 7 de `implement-story.md` — Marcar HU `Resolved` (Motivo B)
- Deploy DEV (`infra-agent`, `Custom.DeployDEV`)

---

## Fase 0 — Diseño y orden

**Precondición:** Feature #[feature_id] con HUs #9735–#9742 (u otras).

1. Confirmar `docs/designs/[feature_id]-[slug].md` aprobado.
2. Listar HUs en orden de dependencia (backend schema → APIs → frontend).
3. Publicar checkpoint en Discussion del Feature:

```html
<div>[Orchestrator] Modo feature-batch iniciado.<br/>
Feature: #[feature_id]<br/>
Rama: feature/AB-[feature_id]-[slug]<br/>
HUs planificadas: #[id1], #[id2], …<br/>
Tope: PR a develop (sin merge automático).</div>
```

---

## Fase 1 — Rama del Feature

```bash
git checkout develop
git pull origin develop
git checkout -b feature/AB-{feature_id}-{slug}
```

**Una sola rama** para todas las HUs del Feature. No crear `feature/AB-{hu_id}-…` por historia.

---

## Fases 2–5 — Ciclo por HU

Repetir por cada HU en orden:

### 2 — DoR (`tech-lead-agent` modo C)

Si `MISSING_N` → detener hasta completar la HU en ADO.

### 3 — Activar HU (gate obligatorio)

```
⚠️ Voy a activar la HU en Azure DevOps.

  Feature: #[feature_id]
  HU: #[hu_id] — [título]
  Rama compartida: feature/AB-{feature_id}-{slug}
  Acción: cambiar estado → Active

¿Confirmas? (sí / no)
```

### 4 — Implementar

- Delegar al agente según tipo `[BACKEND]` / `[FRONTEND]`.
- Trabajar siempre en la rama del Feature.
- Al terminar la HU:

```bash
git add <archivos de la HU>
git commit -m "HU{hu_id}: descripción breve"
```

**Prohibido:** mezclar cambios de dos HUs en un mismo commit.

### 4b — Lint (opcional por HU, obligatorio antes del PR)

```bash
pnpm fix:all:fullstack
```

Si genera cambios de formato, incluirlos en el commit de la HU activa o en commit de chore posterior con referencia `HU{hu_id}: format`.

### 4c — Validación DB (condicional)

Si la HU tocó migraciones o repositorios → `database-agent` (modo C) + `db-schema-validator`.

### 5 — dev-tester

Obligatorio por HU antes de pasar a la siguiente. Evidencias en `Custom.Evidences` de la HU activa.

**Trazabilidad por HU:**

```html
<div>[Orchestrator] HU #[hu_id] implementada en rama feature/AB-{feature_id}-{slug}.<br/>
Commit: HU{hu_id}: …<br/>
Siguiente HU: #[next_hu_id] o apertura de PR.</div>
```

---

## Fase 6 — Abrir PR a develop

Cuando todas las HUs del lote estén implementadas (o el usuario indique corte parcial):

```bash
git push -u origin feature/AB-{feature_id}-{slug}
gh pr create --base develop --title "Feature #{feature_id}: [título corto]" --body "…"
```

**Cuerpo del PR sugerido:**

```markdown
## Summary
- Feature ADO: #[feature_id]
- HUs incluidas: #9735, #9736, …
- Diseño: docs/designs/[feature_id]-[slug].md

## Commits por HU
| HU | Commit |
|----|--------|
| #9735 | HU9735: … |

## Test plan
- [ ] CI verde
- [ ] Evidencias unitarias en ADO por HU
- [ ] Review humano asignado

## Nota
Entrega en modo feature-batch — merge manual por Líder Técnico.
```

**No invocar** `integration-agent` Modo A hasta que el usuario lo pida explícitamente (registro en `Custom.Commits` puede hacerse al abrir PR si el equipo lo requiere).

---

## Fase 7 — Review

Ejecutar `review-pr.md` sobre el PR único.

Si hay bloqueantes → corregir en la misma rama feature con commits `HU{hu_id}: fix review …`.

---

## Fase 8 — STOP (entrega)

Mostrar al usuario:

```
✅ Feature #[feature_id] — PR listo para revisión humana

  PR: !N → develop
  Rama: feature/AB-{feature_id}-{slug}
  HUs implementadas: [lista]
  Code Review: [pass/fail]
  Security: [PASS/FAIL]

⛔ Este modo NO ejecuta merge ni integración ADO.
   El Líder Técnico confirma merge cuando corresponda.
```

Publicar en Discussion del Feature:

```html
<div>[Orchestrator] Modo feature-batch — PR entregado sin merge.<br/>
PR: ![pr_number] → develop<br/>
HUs: #[ids]<br/>
Pendiente: merge manual + Deploy DEV por Líder Técnico.</div>
```

---

## Relación con otros workflows

| Workflow | Cuándo usar |
|----------|-------------|
| `implement-story.md` | Una HU = una rama = merge automático con gate |
| `implement-feature.md` | Un Feature = una rama = un PR = sin merge automático |
| `requirement-to-delivery.md` | Fase 2 diseño; Fase 4 puede delegar aquí si el usuario pide feature-batch |
