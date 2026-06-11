# Reglas del prototipo flitready-suite

Fuente: `DESIGNS/flitready-suite/.lovable/memory/design/flit-ready.md` y `src/styles.css`.

## Identidad visual

- **Marca:** lime `#b3ff1f` + action blue `#003eff`, gradiente `-45deg`.
- **Tipografía:** Poppins 300/400/500/700.
- **Fondo app:** `#e2eaf7` (light).
- **Texto:** primary `#162744`, secondary muted.

## Layout `/app`

- Shell: header sticky con borde inferior lime, fondo ambiental, scroll horizontal por módulos.
- **RightDock** fijo a la derecha — sin nav inferior.
- Tablas/listas largas: scroll vertical interno con `data-vertical-scroll`.
- Padding derecho `pr-16` cuando hay dock (producción `/admin` puede omitir dock hasta integrar shell completo).

## Componentes

- **Card:** borde sutil, radius ~14px, sombra suave.
- **Button primary:** azul acción, rounded-full, sombra.
- **Button secondary:** outline deep.
- **Table:** header `--table-head` sticky, hover row muted.
- **Badge:** outline rounded-full, colores por rol/estado.

## Copy

- UI en **español (Colombia)**.
- Moneda `es-CO` en módulos financieros.
- Administración: **Compañías**, **Invitar usuario**, **Usuarios de {nombre}**.

## Prohibiciones

- No introducir `bg-[#hex]` sueltos — usar tokens.
- No añadir nav inferior.
- No duplicar logo en transiciones de login.

## Módulo Administración (`admin.tsx`)

- Grid 12 columnas: compañías (3–4 cols) + usuarios (8–9 cols).
- Lista compañías: nombre, NIT/id mono, contador usuarios, estado activo con borde action.
- Tabla usuarios: Nombre, Email, Rol/Estado, Acciones (Editar).
- CTAs: **Nueva** (compañía), **Invitar usuario** con icono Plus.
- Paginación 8 filas por página.
