---
name: modulo-mvc-empresarial
description: Usa este skill para crear módulos nuevos en ASP.NET Core MVC con Clean Architecture pragmática, Database-First, separación por capas, DTOs, ViewModels y reglas de seguridad y autorización visual.
---

# Skill: modulo-mvc-empresarial

## Cuándo usar este skill
Usa este skill cuando el usuario pida:
- crear un módulo nuevo
- agregar una funcionalidad vertical
- crear una sección del sistema
- diseñar un caso de uso completo

## Objetivo
Generar un módulo empresarial con separación por capas y UI MVC.

## Reglas obligatorias
- Usar nombres del dominio en español
- Mantener sufijos `Controller`, `ViewModel`, `Dto`
- No usar `EntidadesBaseDatos` en MVC
- No usar AutoMapper
- Mapear manualmente
- Aplicar seguridad por defecto

## Arquitectura esperada
### Dominio
- Entidades si realmente aportan valor
- Reglas del negocio
- Value objects si aplica

### Aplicacion
- DTOs
- interfaces
- validaciones
- casos de uso o servicios

### Infraestructura
- `DbContext`
- `EntidadesBaseDatos`
- repositorios específicos
- servicios de consulta
- integraciones externas
- configuración concreta de Identity con tablas propias

### Web.Mvc
- Controllers
- ViewModels
- Views
- ViewComponents
- TagHelpers

## Patrón de salida de UI
Si el módulo requiere interfaz gráfica y el usuario no indica otro estilo, generar una **web administrativa** para uso interno.

### La salida debe contemplar
- pantalla de listado
- filtros de búsqueda
- acciones por fila
- formularios create/edit
- vista de detalle cuando aplique
- navegación coherente dentro del panel administrativo

## Regla de autorización visual
Si el módulo incluye acciones sensibles como crear, editar, aprobar, anular o eliminar:
- aplicar autorización en endpoint o controller
- aplicar también autorización visual en Razor
- consultar el skill `autorizacion-vistas-mvc`

## Orden de salida esperado
1. Resumen técnico
2. Estructura de carpetas
3. SQL y scaffold si hay cambios de BD
4. Aplicacion
5. Infraestructura
6. MVC
7. Mapeos manuales
8. Seguridad y rendimiento