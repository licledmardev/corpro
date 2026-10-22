---
name: postgresql-dbfirst
description: Usa este skill cuando el proyecto use PostgreSQL con Entity Framework Core en enfoque Database-First, incluyendo diseño SQL, scaffolding con Npgsql, separación de DbContext y EntidadesBaseDatos en Infraestructura, y adaptación pragmática a Clean Architecture.
---

# Skill: postgresql-dbfirst

## Cuándo usar este skill
Usa este skill cuando el usuario pida:
- crear tablas en PostgreSQL
- modificar tablas en PostgreSQL
- generar scaffolding EF Core desde PostgreSQL
- adaptar una solución Clean Architecture a PostgreSQL
- reemplazar SQL Server por PostgreSQL
- mantener un enfoque Database-First con Npgsql

## Objetivo
Aplicar un flujo estrictamente Database-First sobre PostgreSQL sin romper la arquitectura limpia pragmática del proyecto.

## Reglas obligatorias
- El `DbContext` pertenece solo a `Infraestructura`
- Las entidades generadas por scaffolding son exclusivamente `EntidadesBaseDatos`
- Las `EntidadesBaseDatos` pertenecen solo a `Infraestructura`
- `Dominio` no depende de EF Core ni de clases scaffoldeadas
- No mover entidades scaffoldeadas a `Dominio`
- No usar `EntidadesBaseDatos` en controllers, vistas ni APIs
- No usar AutoMapper
- Usar mapeo manual y explícito
- No crear `GenericRepository<T>` por defecto
- `UnitOfWork` es opcional, no obligatorio

## Regla sobre Identity
Si el módulo requiere autenticación/autorización con ASP.NET Core Identity:
- no usar nombres por defecto como `AspNetUsers` o `AspNetRoles`
- crear o mantener tablas propias del sistema
- mapear las entidades de Identity a las tablas personalizadas en `OnModelCreating`
- si se requiere, entregar primero el script SQL de las tablas de identidad propias y luego la configuración de EF Core

## Flujo obligatorio
1. Entregar primero el script SQL exacto para PostgreSQL
2. Usar nombres de tablas y columnas en español, salvo restricción técnica explícita
3. Definir claves primarias, foráneas, índices y restricciones necesarias
4. Entregar después el comando `dotnet ef dbcontext scaffold ... --force`
5. Entregar luego el código C# necesario
6. Si aplica, proponer extensión parcial del `DbContext` para auditoría en `SaveChangesAsync`

## Convenciones de auditoría
Para tablas transaccionales incluir:
- `FechaCreacion` TIMESTAMP
- `UsuarioCreacion` VARCHAR(100)
- `FechaModificacion` TIMESTAMP NULL
- `UsuarioModificacion` VARCHAR(100) NULL
- `Activo` BOOLEAN

## Reglas del scaffold
- Usar el proveedor `Npgsql.EntityFrameworkCore.PostgreSQL`
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

## Ejemplo de comando scaffold
```bash
dotnet ef dbcontext scaffold "Host=localhost;Database=MiBase;Username=usuario;Password=clave" Npgsql.EntityFrameworkCore.PostgreSQL \
  --project src/X.Infraestructura \
  --startup-project src/X.Web.Mvc \
  --context AppDbContext \
  --context-dir Persistencia/Contexto \
  --output-dir Persistencia/EntidadesBaseDatos \
  --namespace X.Infraestructura.Persistencia.EntidadesBaseDatos \
  --context-namespace X.Infraestructura.Persistencia.Contexto \
  --no-onconfiguring \
  --force
```

## Reglas de arquitectura
- Mantener Clean Architecture pragmática
- Concentrar impacto del proveedor PostgreSQL en `Infraestructura`
- Mantener `Aplicacion` independiente del proveedor
- Mantener `Dominio` aislado de detalles relacionales
- Si el caso es simple y transaccional, no forzar un dominio rico artificial

## Reglas de rendimiento
- Aplicar filtros, ordenamiento y paginación sobre `EntidadesBaseDatos`
- Ejecutar `Where`, `OrderBy`, `Skip`, `Take` antes de proyectar
- Evitar traer colecciones completas a memoria
- Para lecturas complejas, preferir servicios de consulta específicos

## Reglas de seguridad
- No concatenar SQL con entrada del usuario
- Validar entrada del lado servidor
- Mantener protección anti-forgery en MVC
- Mantener autorización por políticas cuando corresponda

## Formato de respuesta esperado
1. Supuestos
2. Script SQL PostgreSQL
3. Índices, constraints y relaciones
4. Comando scaffold con Npgsql
5. Estructura de carpetas destino en `Infraestructura`
6. Código adicional necesario
7. Recomendaciones de rendimiento y seguridad