---
name: sqlserver-dbfirst
description: Usa este skill cuando trabajes con SQL Server en enfoque Database-First, incluyendo diseño T-SQL, scaffold de EF Core, separación entre DbContext y EntidadesBaseDatos, y extensión segura mediante parciales en Infraestructura.
---

# Skill: sqlserver-dbfirst

## Cuándo usar este skill
Usa este skill cuando el usuario pida:
- crear tablas
- modificar tablas
- agregar columnas
- definir índices
- generar scaffolding
- automatizar auditoría en `DbContext`

## Objetivo
Aplicar un flujo estrictamente Database-First sobre SQL Server.

## Flujo obligatorio
1. Entregar primero script T-SQL exacto
2. Incluir restricciones, índices y relaciones
3. Toda tabla transaccional debe incluir:
   - `FechaCreacion` DATETIME
   - `UsuarioCreacion` VARCHAR(100)
   - `FechaModificacion` DATETIME NULL
   - `UsuarioModificacion` VARCHAR(100) NULL
   - `Activo` BIT
4. Entregar después el comando `dotnet ef dbcontext scaffold ... --force`
5. Entregar luego el código C# necesario

## Regla sobre Identity
Si el módulo requiere autenticación/autorización con ASP.NET Core Identity:
- no usar nombres por defecto como `AspNetUsers` o `AspNetRoles`
- crear o mantener tablas propias del sistema
- mapear las entidades de Identity a las tablas personalizadas en `OnModelCreating`
- si se requiere, entregar primero el script SQL de las tablas de identidad propias y luego la configuración de EF Core

## Reglas del scaffold
- Orientado a SQL Server
- Apuntar al proyecto `Infraestructura`
- Separar carpeta de `DbContext` y carpeta de `EntidadesBaseDatos`
- Usar `--force`
- Usar `--table` si conviene limitar el alcance
- Considerar `--startup-project` si la app inicia desde `Web.Mvc`

## Extensión segura tras scaffolding
- Nunca modificar archivos generados directamente por scaffolding
- Extender entidades scaffoldeadas mediante archivos parciales separados
- Convención sugerida: `Entidad.extended.cs`
- Ubicación sugerida:
  - `Persistencia/EntidadesBaseDatos/Extensiones/Usuario.extended.cs`
  - `Persistencia/Contexto/Extensiones/AppDbContext.extended.cs`
- Estas extensiones sobreviven a `dotnet ef dbcontext scaffold --force`

## Regla de contenido
En los parciales de `EntidadesBaseDatos` solo colocar:
- propiedades calculadas
- propiedades no mapeadas
- helpers ligeros
- lógica técnica de persistencia

No colocar:
- reglas de negocio complejas
- invariantes del dominio
- lógica de aplicación

## Auditoría
Si aplica, proponer una clase parcial o extensión del `DbContext` para completar automáticamente:
- `FechaCreacion`
- `UsuarioCreacion`
- `FechaModificacion`
- `UsuarioModificacion`

## Seguridad y rendimiento
- Sugerir índices cuando haya búsquedas o crecimiento esperado
- Evitar diseños ambiguos
- Usar nombres de tablas y columnas en español

## Formato de respuesta esperado
1. Supuestos
2. Script T-SQL
3. Índices y relaciones
4. Comando scaffold
5. Código adicional
6. Notas de rendimiento y seguridad