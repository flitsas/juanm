---
name: flit-design-guardian
description: Valida y aplica lineamientos UX/UI del prototipo DESIGNS/flitready-suite al frontend Next.js de GDC 2.0. Audita módulos contra tokens, layout, copy en español y patrones de componentes; corrige desviaciones antes de cerrar HUs frontend. Triggers flit-design-guardian, diseño UX, UI kit, flitready-suite, tokens FLIT, alinear diseño, auditoría visual, Administración, multi-compañía.
---

# flit-design-guardian

Guardián de diseño FLIT: asegura que cada módulo frontend replique fielmente el prototipo canónico en `DESIGNS/flitready-suite` antes de mergear una HU de UI.

```
frontend-agent (implementa pantalla)
        │
        ▼  (recomendado — misma sesión, tras código funcional)
  flit-design-guardian  ──► PASO 1…5
        │         ├── PASS → informar alineación
        └────────► FAIL → listar desviaciones + diff sugerido
```

---

## Fuente de verdad

| Ámbito | Ruta canónica |
|--------|---------------|
| Prototipo visual | `DESIGNS/flitready-suite/` |
| Tokens en producción | `frontend/src/app/globals.css` (`--flit-*`) |
| Primitives compartidos | `frontend/src/components/flit/` |
| Reglas de diseño | `references/prototype_rules.md` |
| Tokens JSON | `references/flit_design_tokens.json` |
| Checklist de aceptación | `references/acceptance_checklist.md` |
| Mapa módulos → secciones | `references/module_mapping.md` |

**Regla:** Nunca inventar colores, tipografías ni layouts que no existan en el prototipo. Si el prototipo no cubre un flujo (p. ej. RBAC), extender usando los mismos tokens y patrones de cards/tablas del módulo más cercano (`admin.tsx`).

---

## Cuándo invocar

| Escenario | Acción |
|-----------|--------|
| HU frontend nueva o refactor UI | Auditar + corregir desviaciones |
| Code review detecta UI inconsistente | Modo auditoría (solo reporte) |
| Usuario pide «alinear con diseño» | Modo corrección completa |
| Pre-PR frontend con cambios visuales | Checklist rápido (PASO 4) |

---

## PASO 1 — Identificar módulo y pantalla de referencia

1. Leer la HU o el path del feature (`frontend/src/features/<modulo>/` o `frontend/src/app/<ruta>/`).
2. Consultar `references/module_mapping.md` para la sección del prototipo.
3. Abrir el componente equivalente en `DESIGNS/flitready-suite/src/components/modules/`.

### Mapa rápido — Administración (HU #9709 y similares)

| Producción | Prototipo |
|------------|-----------|
| `/admin` → `AdminConsole` | Sección `admin` en `app.tsx` |
| Título **Administración** | `Section title="Administración"` |
| Subtítulo **Multi-compañía, usuarios y roles** | `subtitle` del `Section` |
| Panel compañías + usuarios | `AdminModule` en `modules/admin.tsx` |
| Matriz RBAC | No existe en prototipo — usar card + tabla sticky del mismo módulo |

---

## PASO 2 — Auditar tokens y CSS

Verificar que `globals.css` exponga al menos:

- `--flit-action`, `--flit-lime`, `--flit-table-head`, `--flit-gradient-flit`
- `--flit-text-primary`, `--flit-text-secondary`, `--flit-bg-app`
- Acentos de rol: `--flit-purple`, `--flit-amber`, `--flit-tech`

**Prohibido en producción:**

- Colores hex sueltos tipo `bg-[#003eff]` — usar `var(--flit-action)` o clases `.flit-*`
- Bordes/radios distintos al prototipo sin justificación en ADR

Ejecutar script opcional:

```bash
python .cursor/skills/flit-design-guardian/scripts/flit_audit_helper.py frontend/src/features/<modulo>
```

---

## PASO 3 — Auditar layout y componentes

Checklist por pantalla (detalle en `references/acceptance_checklist.md`):

| Patrón | Prototipo | Producción |
|--------|-----------|------------|
| Encabezado de sección | `title` + `subtitle` | `SectionHeader` |
| Cards | `flit-card`, padding `p-3` | Igual |
| Botón primario | `variant="primary"`, rounded-full | `PrimaryButton` / `.flit-btn-primary` |
| Tabla | sticky header `--table-head`, scroll interno | `.flit-table` + `data-vertical-scroll` |
| Master-detail admin | grid 12 cols (4+8 / 3+9) | `grid-cols-12` |
| Lista compañías | `Building2`, botón **Nueva** | Icono + CTA (deshabilitar si API no existe) |
| Badges estado/rol | `rounded-full`, colores por token | `StatusBadge` / `.flit-badge` |
| Paginación | Anterior · página activa · Siguiente | Mismo patrón visual |
| Copy UI | Español Colombia | Sin labels en inglés (`Tenant` → **Compañía**) |
| Tabs secundarios | Pills rounded-full | `TabPills` |

---

## PASO 4 — Checklist pre-PR (rápido)

- [ ] Título/subtítulo coinciden con el `Section` del prototipo
- [ ] Tokens `--flit-*` (no hex ad-hoc)
- [ ] Primitives en `components/flit/` reutilizados antes de duplicar estilos
- [ ] Tablas: header sticky + hover row
- [ ] Estados vacío/carga/error con roles ARIA (`role="status"`, `role="alert"`)
- [ ] Iconos: `lucide-react`, tamaño `size-3` / `size-4`, `aria-hidden` en decorativos
- [ ] Tests Vitest actualizados si cambian labels accesibles

---

## PASO 5 — Reporte

Usar plantilla `templates/audit_report.md`. Clasificar hallazgos:

| Severidad | Criterio |
|-----------|----------|
| **BLOQUEANTE** | Tokens incorrectos, layout roto, copy inglés en UI usuario, sin estados ARIA |
| **MAYOR** | Desviación de grid/spacing, botones sin variant primary, tabla sin sticky |
| **MENOR** | Micro-spacing, icono ausente cuando el prototipo lo incluye |

---

## Implementación — primitives obligatorios

Antes de estilos inline en un feature, verificar existencia en `frontend/src/components/flit/`:

| Componente | Uso |
|------------|-----|
| `SectionHeader` | Título + subtítulo de módulo |
| `PrimaryButton` | CTAs primarios (Invitar, Guardar, Nueva) |
| `TabPills` | Navegación secundaria dentro del módulo |
| `StatusBadge` | Estados de usuario (Active, Pending, Locked) |

Clases CSS globales: `.flit-card`, `.flit-input`, `.flit-table`, `.flit-btn-primary`, `.flit-badge`, `.flit-tab-pill-*`, `.scrollbar-thin`.

---

## Integración con otros agentes

| Agente | Cuándo encadenar |
|--------|------------------|
| `frontend-agent` | Tras implementar UI, antes de `dev-tester` |
| `code-review-agent` | Dimensión UX/UI en PRs frontend |
| `qa-agent` | Validar copy y layout en E2E headed |

---

## Referencias

- `references/prototype_rules.md` — reglas del kit Lovable/flitReady
- `references/flit_design_tokens.json` — mapa token prototipo → producción
- `references/acceptance_checklist.md` — checklist extendido
- `references/module_mapping.md` — rutas prototipo ↔ producción
