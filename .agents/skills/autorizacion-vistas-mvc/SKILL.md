---
name: autorizacion-vistas-mvc
description: Usa este skill cuando necesites mostrar u ocultar elementos en vistas Razor según policies y permisos granulares basados en claims, usando un TagHelper declarativo para MVC.
---

# Skill: autorizacion-vistas-mvc

## Cuándo usar este skill
Usa este skill cuando el usuario pida:
- ocultar o mostrar botones, enlaces, tarjetas o bloques en Razor
- manejar permisos en vistas MVC
- combinar policies y permisos granulares
- evitar `if` repetitivos de autorización en vistas
- aplicar autorización visual por niveles

## Objetivo
Implementar autorización visual declarativa en Razor Views usando un `TagHelper` combinado para `Policy` y `Permiso`.

## Regla principal
La autorización visual en vistas complementa la seguridad de controllers y endpoints, pero no la reemplaza.

## Estrategia recomendada de 3 niveles
1. Seguridad de acceso en endpoint o acción mediante `[Authorize]`
2. Seguridad visual en Razor mediante `AutorizacionTagHelper`
3. Permiso granular por claim, por ejemplo claim `Permiso`

## Ubicación obligatoria
- El `AutorizacionTagHelper` debe vivir en `X.Web.Mvc/TagHelpers`
- No debe vivir en `Aplicacion` ni en `Dominio`
- Registrar TagHelpers en `_ViewImports.cshtml`

## Convención de atributos
- `asp-policy`: policy de alto nivel, por ejemplo `PuedeGestionarEntidades`
- `asp-permiso`: permiso granular, por ejemplo `Entidades.Crear`
- `asp-operador`: `AND` o `OR`

## Regla de evaluación
- Si solo existe `asp-policy`, validar policy
- Si solo existe `asp-permiso`, validar claim `Permiso`
- Si existen ambos:
  - `AND`: ambos deben cumplirse
  - `OR`: basta uno

## Comportamiento esperado
- Si el usuario no está autenticado, suprimir salida
- Si la validación falla, suprimir salida
- Si la validación pasa, renderizar normalmente
- Ejecutar temprano si comparte vista con otros TagHelpers

## Reglas de diseño
- No usar bloques `if` repetitivos en la vista para autorización simple
- Ocultar botones de crear, editar, eliminar, aprobar, exportar o anular según corresponda
- En tablas, ocultar columna de acciones o acciones individuales según permisos
- En menús, ocultar módulos o enlaces no autorizados
- Mantener consistencia visual aunque un bloque no se renderice

## Reglas de rendimiento
- No cargar datos costosos solo para bloques que probablemente no se rendericen
- Si una sección depende de autorización estricta, cargar solo lo necesario para el usuario actual

## Formato de respuesta esperado
1. Ubicación del TagHelper
2. Código del TagHelper
3. Registro en `_ViewImports.cshtml`
4. Ejemplos de uso en vistas
5. Recomendaciones de seguridad y rendimiento