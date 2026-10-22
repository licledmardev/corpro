---
name: ui-tailwind
description: Usa este skill para generar vistas Razor con Tailwind CSS, diseño mobile-first, accesibilidad y soporte de autorización visual mediante TagHelpers cuando existan policies o permisos.
---

# Skill: ui-tailwind

## Cuándo usar este skill
Usa este skill cuando el proyecto o módulo de UI deba construirse con Tailwind CSS.

## Objetivo
Generar interfaces ASP.NET Core MVC con Tailwind CSS, diseño mobile-first y componentes reutilizables.

## Reglas obligatorias
- No usar Bootstrap
- No mezclar Bootstrap y Tailwind salvo migración controlada
- Mantener diseño mobile-first
- Usar clases utilitarias consistentes
- Mantener accesibilidad, focus visible y semántica HTML
- Si hay repetición visual, usar parciales o `ViewComponents`

## Patrón visual por defecto
Salvo que el usuario indique lo contrario, la UI debe construirse como una **web administrativa**.

## Estructura esperada
- Layout con sidebar o navegación superior
- Header con título de pantalla, breadcrumbs y acciones
- Contenido principal con tarjetas métricas, filtros, tablas y formularios
- Componentes orientados a gestión interna y operación
- Diseño mobile-first, pero optimizado principalmente para escritorio administrativo

## Estilo esperado
- No generar una landing page ni una web promocional por defecto
- Priorizar legibilidad, consistencia, formularios claros, tablas utilizables y acciones visibles
- Si hay dashboard, usar cards de resumen, filtros rápidos, tablas de trabajo y módulos de acceso frecuentes

## Componentes esperados
- Layout principal
- Navbar o menú
- Formularios
- Tablas
- Botones
- Alertas
- Paginación
- Estados vacíos

## Autorización visual
Si la pantalla tiene acciones sensibles:
- usar el skill `autorizacion-vistas-mvc`
- aplicar `asp-policy`, `asp-permiso` y `asp-operador` cuando corresponda
- ocultar botones o acciones no permitidas
- mantener consistencia visual cuando un elemento no se renderice

## Reglas de calidad
- Buen contraste
- Estados `hover`, `focus`, `disabled`, `invalid`
- HTML semántico
- Vistas limpias y mantenibles

## Salida esperada
Cuando se generen vistas:
- usar Razor + Tailwind
- mantener coherencia visual
- priorizar legibilidad sobre exceso de utilidades