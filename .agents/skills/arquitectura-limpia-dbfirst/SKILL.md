---
name: arquitectura-limpia-dbfirst
description: Usa este skill cuando necesites definir estructura de solución, capas, dependencias, adaptación pragmática de Clean Architecture a Database-First y reglas para DbContext, EntidadesBaseDatos, parciales y mapeo manual.
---

# Skill: arquitectura-limpia-dbfirst

## Cuándo usar este skill
Usa este skill cuando el usuario pida:
- estructura de solución
- decisiones de arquitectura
- separación por capas
- adaptación de Clean Architecture a Database-First
- dudas sobre repositorios, DbContext, EntidadesBaseDatos o UnitOfWork

## Objetivo
Diseñar soluciones ASP.NET Core MVC con Clean Architecture pragmática y Entity Framework Core Database-First.

## Reglas obligatorias
- `DbContext` solo en `Infraestructura`
- `EntidadesBaseDatos` solo en `Infraestructura`
- `Dominio` no depende de EF Core
- `Aplicacion` no depende directamente de `DbContext`
- MVC no usa `EntidadesBaseDatos`
- No usar AutoMapper
- Mapear manualmente
- No crear `GenericRepository<T>` por defecto
- `UnitOfWork` es opcional, no obligatorio

## Regla sobre ASP.NET Core Identity
- Se permite usar ASP.NET Core Identity
- No usar las tablas por defecto del paquete
- Las tablas de identidad deben adaptarse al modelo del proyecto
- La implementación concreta de Identity pertenece a `Infraestructura`
- Si se usan entidades personalizadas de Identity, deben mapearse en el `DbContext` y mantenerse fuera de `Dominio`

## Estructura recomendada
- `X.Web.Mvc`
- `X.Aplicacion`
- `X.Dominio`
- `X.Infraestructura`

## Reglas de dependencia
- `Dominio` no depende de nadie
- `Aplicacion` depende de `Dominio`
- `Infraestructura` depende de `Aplicacion` y `Dominio`
- `Web.Mvc` depende de `Aplicacion` e `Infraestructura`

## Regla sobre archivos parciales de EntidadesBaseDatos
Se permite extender clases scaffoldeadas mediante archivos parciales separados.

### Convención sugerida
- `Usuario.extended.cs`
- `Cliente.extended.cs`
- `Pedido.extended.cs`

### Ubicación
Estas extensiones deben permanecer en `Infraestructura`, preferentemente en:
- `Persistencia/EntidadesBaseDatos/Extensiones/`

### Uso permitido
Los parciales de `EntidadesBaseDatos` pueden contener:
- propiedades calculadas
- propiedades no mapeadas
- helpers ligeros
- lógica técnica asociada al modelo de persistencia

### Uso no permitido
Los parciales de `EntidadesBaseDatos` no deben contener:
- reglas centrales del negocio
- invariantes del dominio
- decisiones de aplicación
- autorización
- comportamiento transaccional complejo

### Regla clave
Si la lógica pertenece al negocio, moverla a `Dominio` o `Aplicacion`.
Si la lógica pertenece al modelo generado por persistencia, dejarla en el partial dentro de `Infraestructura`.

## Regla pragmática
Si el caso es simple y puramente transaccional, no forzar un dominio rico artificial.
Si hay reglas reales de negocio, separar claramente `EntidadDominio` y `EntidadBaseDatos`.

## Formato de respuesta esperado
1. Resumen técnico
2. Estructura propuesta
3. Justificación de capas
4. Riesgos o trade-offs
5. Recomendación pragmática