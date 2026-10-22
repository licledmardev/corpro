---
name: ui-bootstrap
description: Usa este skill para generar vistas Razor con Bootstrap, diseño responsivo, accesibilidad y soporte de autorización visual mediante TagHelpers cuando existan policies o permisos.
---

# Skill: ui-bootstrap

## Cuándo usar este skill
Usa este skill cuando el proyecto o módulo de UI deba construirse con Bootstrap.

## Objetivo
Generar interfaces ASP.NET Core MVC con Bootstrap 5, diseño responsivo y componentes convencionales de administración.

## Reglas obligatorias
- No usar Tailwind
- No mezclar Bootstrap y Tailwind salvo migración controlada
- Mantener diseño mobile-first
- Usar clases Bootstrap de forma consistente
- Mantener accesibilidad y semántica HTML
- Si hay repetición visual, usar parciales o `ViewComponents`

## Patrón visual por defecto
Salvo que el usuario indique lo contrario, la UI debe construirse como una **web administrativa**.

## Estructura esperada
- Layout con sidebar o navbar administrativa
- Header con título, breadcrumbs y acciones
- Área de contenido con tablas, formularios, filtros, modales y alertas
- Componentes orientados a backoffice, mantenimiento y operación

## Estilo esperado
- No generar una landing page ni una web promocional por defecto
- Priorizar claridad, mantenimiento, rapidez de implementación y consistencia entre pantallas
- Si hay dashboard, usar cards de indicadores, tablas operativas y paneles de resumen

## Componentes esperados
- Layout principal
- Navbar o menú
- Formularios
- Tablas responsivas
- Botones
- Alertas
- Modales si aplica
- Paginación

## Autorización visual
Si la pantalla tiene acciones sensibles:
- usar el skill `autorizacion-vistas-mvc`
- aplicar `asp-policy`, `asp-permiso` y `asp-operador` cuando corresponda
- ocultar botones o acciones no permitidas
- mantener consistencia visual cuando un elemento no se renderice

## Reglas de calidad
- Usar grid de Bootstrap de forma coherente
- Mantener consistencia de espaciados
- Validaciones visuales claras
- Vistas limpias y mantenibles

## Salida esperada
Cuando se generen vistas:
- usar Razor + Bootstrap
- mantener coherencia visual
- priorizar claridad y mantenibilidad