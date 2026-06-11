# Checklist de aceptación UX/UI — flit-design-guardian

## Encabezado y navegación

- [ ] Título del módulo coincide con `Section title` del prototipo
- [ ] Subtítulo descriptivo presente (ej. «Multi-compañía, usuarios y roles»)
- [ ] Tabs secundarios usan pills rounded-full (activo = action blue)

## Tokens y estilos

- [ ] Solo variables `--flit-*` o clases `.flit-*` (sin hex sueltos)
- [ ] Fondo de página `--flit-bg-app`
- [ ] Cards con `.flit-card` y padding consistente (`p-3` en admin)
- [ ] Header de tabla `--flit-table-head` sticky

## Layout admin (master-detail)

- [ ] Grid 12 columnas responsive
- [ ] Panel izquierdo: lista seleccionable con estado activo (borde action + bg/action 5%)
- [ ] Panel derecho: tabla + paginación + CTA invitar
- [ ] Scroll vertical interno en listas/tablas (`data-vertical-scroll`, `scrollbar-thin`)

## Componentes interactivos

- [ ] Botones primarios: `PrimaryButton` o `.flit-btn-primary`, rounded-full
- [ ] Iconos Lucide con `aria-hidden` cuando son decorativos
- [ ] Badges de estado: rounded-full, colores por token
- [ ] Inputs: `.flit-input`, labels en español

## Accesibilidad (WCAG 2.1 AA)

- [ ] Estados loading: `role="status"`, `aria-live="polite"`
- [ ] Errores: `role="alert"`
- [ ] Botones con nombre accesible (texto o `aria-label`)
- [ ] Contraste mínimo en texto primary sobre fondos claros

## Copy

- [ ] Español Colombia — sin «Tenant», «Save», «User» en UI visible
- [ ] Mensajes vacío alineados al tono del prototipo

## Tests

- [ ] Vitest actualizado si cambian labels (`getByLabelText`, `getByRole`)
- [ ] Sin regresiones en specs del módulo
