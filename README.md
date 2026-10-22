# Corpro — Avisos de cortes programados

Portal público donde el cliente consulta si su suministro será afectado por un corte programado
(por **número de suministro** o **dirección**), ve el **aviso oficial** y, de forma **opcional**, deja
su **celular y/o correo** para recibir avisos.

- **Frontend:** Blazor WebAssembly standalone → GitHub Pages (estático, gratis).
- **Backend:** ASP.NET Core 10 Minimal API → cualquier hosting con .NET (Render, Azure App Service F1, Fly.io…).
- **Base de datos:** PostgreSQL con EF Core (Npgsql), **Database-First**. Local en desarrollo, Neon en la nube.

## Estructura

```
corpro_solucion/
├── Corpro.slnx
├── db/scripts/
│   ├── 001_esquema_inicial.sql      ← fuente de verdad del modelo (Database-First)
│   ├── 002_datos_prueba.sql         ← datos FICTICIOS para desarrollo
│   └── 003_imagen_aviso_y_contacto_opcional.sql ← incremental para bases creadas antes
├── src/
│   ├── Corpro.Dominio/              ← reglas de negocio, sin EF Core
│   │   ├── Cortes/CriterioConsulta.cs
│   │   └── Contacto/ActualizacionContacto.cs
│   ├── Corpro.Contratos/            ← DTOs compartidos API ↔ Blazor (sin dependencias)
│   ├── Corpro.Aplicacion/           ← casos de uso + interfaces (no conoce DbContext)
│   │   ├── Abstracciones/  IConsultaCortes, IActualizacionContactoRepositorio
│   │   └── Cortes/         CortesServicio, CortesOpciones
│   ├── Corpro.Infraestructura/      ← EF Core + Npgsql
│   │   ├── Persistencia/Contexto/CorproDbContext.cs            (scaffold)
│   │   ├── Persistencia/Contexto/Extensiones/*.extended.cs     (auditoría, sobrevive al scaffold)
│   │   ├── Persistencia/EntidadesBaseDatos/*.cs                (scaffold)
│   │   ├── Consultas/ConsultaCortes.cs
│   │   └── Repositorios/ActualizacionContactoRepositorio.cs
│   ├── Corpro.Api/                  ← Minimal API: endpoints, CORS, rate limiting
│   └── Corpro.Web/                  ← Blazor WASM
│       ├── Pages/Home.razor          (buscador, avisos y contacto)
│       ├── Componentes/              (ResultadoConsulta, AvisosVigentes, ActualizarContacto, AyudaSuministro, Modal, Icono)
│       ├── wwwroot/css/app.css       (tema corporativo Distriluz)
│       ├── wwwroot/avisos/           (imágenes de los avisos oficiales)
│       └── wwwroot/img/              (ilustraciones de ayuda)
├── tools/Corpro.Importador/         ← carga el Excel de interrupciones con COPY
└── .github/workflows/frontend-github-pages.yml
```

Dependencias: `Dominio` ← `Aplicacion` ← `Infraestructura` ← `Api`. `Web` solo conoce `Contratos`.

## Modelo de datos

El aviso PDF es la **cabecera** (fecha, horario, alimentador, sectores) y el Excel trae **un suministro
por fila** (8 404 filas para la interrupción 9320261712). Por eso se normaliza en dos tablas, y
`corte_programado` es una **vista** que entrega el modelo plano pedido
(`Suministro, Direccion, FechaCorte, HoraInicio, HoraFin`):

| Objeto | Rol |
|---|---|
| `interrupcion_programada` | Una fila por aviso (PK = `nro_interrupcion` del sistema comercial). `imagen_aviso` guarda la ruta de la imagen del aviso |
| `suministro_afectado` | Suministros de cada interrupción. Índice B-tree por suministro y GIN `pg_trgm` por dirección |
| `corte_programado` (vista) | Une ambas; la consulta pública lee de aquí |
| `datos_contacto_actualizados` | Celular y/o correo que deja el cliente (opcional) |

El nombre del titular (`NombreNroServicio`) **no se importa**: la consulta es pública y no lo necesita.

## Endpoints

| Método | Ruta | Descripción |
|---|---|---|
| GET | `/api/cortes/consultar/{criterio}` | Solo dígitos → coincidencia **exacta** por suministro. Texto → coincidencia **parcial** por dirección (ILIKE, mín. 4 caracteres). Solo cortes de hoy en adelante, máx. 50. En búsquedas por dirección el suministro se devuelve enmascarado (`****1665`). |
| GET | `/api/cortes/avisos-vigentes` | Avisos de hoy en adelante con su imagen. Caché de 2 minutos en el servidor. |
| POST | `/api/cortes/actualizar-datos` | Recibe `suministro`, `celular` (9 dígitos, empieza en 9) y/o `correo`. Valida en servidor; responde 201 o 400 con errores por campo. Límite: 5 por IP cada 10 min. |
| GET | `/salud` | Comprobación de vida para el hosting. |

## Puesta en marcha (desarrollo)

1. Crear la base y ejecutar los scripts (psql, pgAdmin o DBeaver):

```bash
psql -h localhost -U postgres -c "CREATE DATABASE corpro;"
```

```bash
psql -h localhost -U postgres -d corpro -f db/scripts/001_esquema_inicial.sql -f db/scripts/002_datos_prueba.sql
```

2. Guardar la cadena local como secreto (no queda en el repositorio):

```bash
dotnet user-secrets --project src/Corpro.Api set "ConnectionStrings:Local" "Host=localhost;Port=5432;Database=corpro;Username=postgres;Password=TU_CLAVE"
```

3. Ejecutar API (http://localhost:5172) y frontend (http://localhost:5032) en dos terminales:

```bash
dotnet run --project src/Corpro.Api
```

```bash
dotnet run --project src/Corpro.Web
```

Pruebe con el suministro `10000001` o la dirección `retamas` (datos de prueba), o con cualquier suministro del Excel real que haya cargado.
Si su base se creó con una versión anterior del script 001, ejecute también `003_imagen_aviso_y_contacto_opcional.sql`.

## Alternar Local ↔ Neon

`appsettings.json` define las dos cadenas y `BaseDatos:ConexionActiva` elige cuál usar:

```json
"BaseDatos": { "ConexionActiva": "Neon" },
"ConnectionStrings": {
  "Local": "Host=localhost;Port=5432;Database=corpro;Username=postgres;Password=...",
  "Neon":  "Host=ep-xxxx-pooler.us-east-2.aws.neon.tech;Database=corpro;Username=corpro_owner;Password=...;SSL Mode=Require;Channel Binding=Require;Maximum Pool Size=20"
}
```

- `appsettings.Development.json` fija `Local`; en producción queda `Neon`.
- La clave real de Neon va en una variable de entorno del hosting: `ConnectionStrings__Neon=Host=...`.
- Use el host **`-pooler`** de Neon (PgBouncer) para soportar muchas conexiones concurrentes.
- `EnableRetryOnFailure` absorbe el arranque en frío de Neon cuando el cómputo estaba suspendido.
- Para cambiar sin redeploy: variable `BaseDatos__ConexionActiva=Local|Neon`.

## Regenerar el modelo (Database-First)

Tras cambiar el SQL, regenerar con el scaffold (sobrescribe solo los archivos generados; las
extensiones `*.extended.cs` se conservan):

```bash
dotnet ef dbcontext scaffold "Name=ConnectionStrings:Local" Npgsql.EntityFrameworkCore.PostgreSQL --project src/Corpro.Infraestructura --startup-project src/Corpro.Api --context CorproDbContext --context-dir Persistencia/Contexto --output-dir Persistencia/EntidadesBaseDatos --namespace Corpro.Infraestructura.Persistencia.EntidadesBaseDatos --context-namespace Corpro.Infraestructura.Persistencia.Contexto --no-onconfiguring --no-pluralize --force
```

## Cargar un aviso real (PDF + Excel)

Los archivos reales **no se versionan** (`.gitignore` excluye `*.xlsx` y `*.pdf`).

1. Convierta la primera página del PDF en imagen y guárdela en `src/Corpro.Web/wwwroot/avisos/<NroInterrupcion>.webp`
   (por ejemplo con `pdftoppm -r 150 -png -singlefile aviso.pdf aviso` y luego a WebP). El aviso es público, así que la imagen sí se publica con el sitio.
2. Cargue el Excel indicando esa ruta con `--imagen-aviso avisos/<NroInterrupcion>.webp`.
Primero valide la lectura sin tocar la base:

```bash
dotnet run --project tools/Corpro.Importador -- --archivo "Reportes de Interrupciones 25-10-2026.xlsx" --fecha 2026-10-25 --inicio 06:00 --fin 15:00 --empresa "Electrocentro S.A." --validar
```

Luego cárguelo (la cadena va en la variable `CORPRO_DB`); es idempotente:

```bash
dotnet run --project tools/Corpro.Importador -- --archivo "Reportes de Interrupciones 25-10-2026.xlsx" --fecha 2026-10-25 --inicio 06:00 --fin 15:00 --empresa "Electrocentro S.A." --alimentador A4906 --documento PDC15-R02 --motivo "Mantenimiento preventivo y correctivo en la red de media tensión" --sectores "Distrito Yanacancha: Vista Alegre, 27 de Noviembre, Yanacancha Antigua, San Juan. Distrito de Chaupimarca: Miguel Bravo Quipe, Centro, Real de Minas, Relleno de Patarcocha, Túpac Amaru, Uliachín, La Esperanza, Noruega Alta y Baja, Misti, Santa Rosa. Distrito Simón Bolívar: Buenos Aires, Champamarca." --imagen-aviso avisos/9320261712.webp
```

## Marca y apariencia

- **Colores y fuentes:** variables al inicio de `src/Corpro.Web/wwwroot/css/app.css` (`--distriluz-azul-oscuro`, `--distriluz-azul-claro`, `--distriluz-verde`, `--distriluz-gris`, `--font-principal`, `--font-secundaria`). Los estilos globales PAMF Distriluz van debajo, sin cambios.
- **Empresa, logo y centro de atención:** sección `Portal` de `src/Corpro.Web/wwwroot/appsettings.json`. El logo está en `wwwroot/img/logo-electrocentro.png` (205×43 px); una versión SVG o PNG de mayor resolución se verá más nítida en pantallas de alta densidad.
- **Ayuda "¿Dónde encuentro mi número de suministro?":** `wwwroot/img/ayuda-recibo.svg` es una ilustración que reproduce la disposición del recibo real y resalta el número grande de la parte superior (“Para consultas, su código”), con un suministro ficticio. `ayuda-caja-portamedidor.svg` es una ilustración genérica. No publique imágenes de recibos reales: el repositorio de GitHub Pages es público.
- **Portada:** en escritorio el buscador va a la izquierda y el aviso oficial vigente en un panel a la derecha (se amplía en modal); en móvil el panel baja debajo del buscador.

## Publicación

**Frontend (GitHub Pages):** en el repositorio, *Settings → Pages → Source: GitHub Actions* y cree la
variable de repositorio `API_BASE_URL` (ej. `https://corpro-api.onrender.com`). Cada push a `main`
publica en `https://<usuario>.github.io/<repositorio>/`.

**Backend:** publique `src/Corpro.Api` y configure en el hosting:

| Variable | Valor |
|---|---|
| `ConnectionStrings__Neon` | cadena completa de Neon |
| `Cors__OrigenesPermitidos__0` | `https://<usuario>.github.io` (solo el origen, sin la ruta del repo) |
| `Proxy__ConfiarEnEncabezadosReenviados` | `true` si el hosting está detrás de un proxy (Render, Fly, Azure) |

## Seguridad aplicada

- CORS restringido al origen de GitHub Pages, solo `GET`/`POST` y `Content-Type`.
- Límite de peticiones por IP (consulta 60/min, registro 5/10 min) y cuerpo máximo de 16 KB.
- Validación en servidor (`ActualizacionContacto` en Dominio); las anotaciones de los DTOs solo mejoran la UX.
- Las rutas de imágenes (avisos, logo) solo aceptan rutas del propio sitio o `http(s)` (`UrlSegura`).
- Consultas parametrizadas por EF Core; los comodines `%`/`_` del usuario se escapan.
- Mínimos datos personales: sin nombres de titulares, suministros enmascarados al buscar por dirección.
- Cadenas de conexión fuera del repositorio (user-secrets / variables de entorno), SSL obligatorio con Neon.
