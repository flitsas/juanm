# Mapa módulos — prototipo ↔ producción

| Prototipo (flitready-suite) | Sección / componente | Producción (frontend) | Ruta |
|-----------------------------|----------------------|------------------------|------|
| `Section id="dashboard"` | `DashboardModule` | `/dashboard` | Pendiente shell completo |
| `Section id="dgc"` | `DgcModule` | — | Por implementar |
| `Section id="peticiones"` | `PeticionesModule` | — | Por implementar |
| `Section id="gdc"` | `GdcModule` | — | Por implementar |
| `Section id="reportes"` | `ReportesModule` | — | Por implementar |
| **`Section id="admin"`** | **`AdminModule`** | **`/admin` → `AdminConsole`** | **Multi-compañía, usuarios y roles** |

## Administración — detalle

| Elemento prototipo | Archivo referencia | Equivalente producción |
|--------------------|--------------------|-------------------------|
| Título «Administración» | `routes/app.tsx` | `admin-console.tsx` → `SectionHeader` |
| Subtítulo multi-compañía | `routes/app.tsx` | `SectionHeader subtitle` |
| Lista compañías | `modules/admin.tsx` | `tenant-users-panel.tsx` (panel izq.) |
| Tabla usuarios | `modules/admin.tsx` | `tenant-users-panel.tsx` (panel der.) |
| Invitar usuario | `modules/admin.tsx` | `invite-user-form.tsx` |
| Matriz roles/permisos | — (no en prototipo) | `rbac-matrix-panel.tsx` — extender patrón card+tabla |

## Dock → rutas futuras

| Dock id | Label | Ruta producción objetivo |
|---------|-------|--------------------------|
| `admin` | Admin | `/admin` |

Cuando exista shell horizontal en producción, `/admin` debe renderizarse como sección «Administración» con el mismo subtítulo, no como página con branding «Super Admin» aislado.
