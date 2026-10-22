---
name: crud-mvc-empresarial
description: Usa este skill para generar CRUDs MVC completos con DTOs, ViewModels, mapeo manual, seguridad, autorización visual en vistas y persistencia Database-First.
---

# Skill: crud-mvc-empresarial

## Cuándo usar este skill
Usa este skill cuando el usuario pida:
- CRUD completo
- pantallas de listado, crear, editar, detalle y eliminar
- formularios y tablas MVC
- mantenimiento de catálogos o entidades operativas

## Objetivo
Crear CRUDs mantenibles, seguros y eficientes sin exponer `EntidadesBaseDatos` a la UI.

## Reglas obligatorias
- No usar AutoMapper
- No usar `EntidadesBaseDatos` en controllers ni vistas
- Usar DTOs y ViewModels
- Usar métodos de extensión manuales
- Formularios con anti-forgery
- Validaciones de servidor

## Patrón de UI por defecto
Todo CRUD generado debe integrarse como parte de una **web administrativa**.

### Requisitos visuales mínimos
- Vista `Index` con tabla administrativa
- filtros o búsqueda
- columna de acciones
- botones de crear, editar, detalle y eliminar según permisos
- formularios consistentes con el layout general
- mensajes de éxito o error visibles
- diseño coherente con sidebar, navbar o layout administrativo del sistema

## Rendimiento
- Aplicar `Where`, `OrderBy`, `Skip`, `Take` sobre `EntidadesBaseDatos`
- Proyectar a DTO o ViewModel después
- Evitar `ToList()` prematuro
- Para consultas complejas, usar servicios de consulta específicos

## Seguridad
- Protección ante edición o eliminación indebida
- Validaciones del lado servidor
- Autorización por políticas si aplica

## Autorización visual requerida
En vistas de listado, detalle y formularios:
- ocultar botones, enlaces, columnas de acciones o bloques completos según permisos
- no confiar solo en ocultar UI como mecanismo de seguridad
- para Razor, preferir `AutorizacionTagHelper`
- consultar el skill `autorizacion-vistas-mvc` cuando existan permisos por policy o claim

## Formato de respuesta esperado
1. Resumen del CRUD
2. Estructura de archivos
3. DTOs y ViewModels
4. Mapeos
5. Servicios o interfaces
6. Infraestructura
7. Controller
8. Vistas
9. Notas de seguridad y rendimiento