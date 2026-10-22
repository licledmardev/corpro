# Uso de autorización en vistas MVC

## Ejemplos de uso

### Solo por policy
```cshtml
<a asp-controller="Entidades"
   asp-action="Index"
   asp-policy="PuedeVerEntidades"
   class="btn btn-primary">
    Ver entidades
</a>
```

### Solo por permiso
```cshtml
<a asp-controller="Entidades"
   asp-action="Crear"
   asp-permiso="Entidades.Crear"
   class="btn btn-success">
    Nueva entidad
</a>
```

### Combinando policy + permiso
```cshtml
<button type="submit"
        asp-policy="PuedeGestionarEntidades"
        asp-permiso="Entidades.Editar"
        asp-operador="AND"
        class="btn btn-warning">
    Guardar cambios
</button>
```

### Operador OR
```cshtml
<div asp-policy="PuedeAprobarSolicitudes"
     asp-permiso="Solicitudes.Aprobar"
     asp-operador="OR">
    <partial name="_PanelAprobacion" />
</div>
```

## Regla
Ocultar UI no reemplaza autorización real en controllers o servicios.