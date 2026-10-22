// Importa el Excel "Reportes de Interrupciones" (una fila por suministro afectado)
// junto con los datos del aviso PDF (fecha, horario, alimentador, sectores).
//
// Uso:
//   set CORPRO_DB=Host=...;Database=corpro;Username=...;Password=...
//   dotnet run --project tools/Corpro.Importador -- ^
//     --archivo "Reportes de Interrupciones 25-10-2026.xlsx" ^
//     --fecha 2026-10-25 --inicio 06:00 --fin 15:00 ^
//     --empresa "Electrocentro S.A." --alimentador A4906 --documento PDC15-R02 ^
//     --motivo "Mantenimiento preventivo y correctivo en red de MT" ^
//     --sectores "Distrito Yanacancha: Vista Alegre, ..." ^
//     --imagen-aviso avisos/9320261712.webp   (archivo en src/Corpro.Web/wwwroot/avisos/)
//
// Es idempotente: volver a ejecutarlo actualiza la cabecera y los suministros.
// La columna NombreNroServicio (titular) se ignora a propósito: la consulta es pública.

using System.Globalization;
using System.Text.RegularExpressions;
using ClosedXML.Excel;
using Npgsql;
using NpgsqlTypes;

var a = LeerArgumentos(args);
string Req(string k) => a.TryGetValue(k, out var v) && v.Length > 0 ? v : throw new ArgumentException($"Falta --{k}");
string? Opc(string k) => a.TryGetValue(k, out var v) && v.Length > 0 ? v : null;

try
{
    var archivo = Req("archivo");
    var fecha = DateOnly.ParseExact(Req("fecha"), "yyyy-MM-dd", CultureInfo.InvariantCulture);
    var inicio = TimeOnly.ParseExact(Req("inicio"), "HH:mm", CultureInfo.InvariantCulture);
    var fin = TimeOnly.ParseExact(Req("fin"), "HH:mm", CultureInfo.InvariantCulture);
    const string usuario = "importador";

    var filas = LeerExcel(archivo);
    var interrupciones = filas.Select(f => f.NroInterrupcion).Distinct().ToList();
    if (interrupciones.Count != 1)
        throw new InvalidOperationException($"El archivo contiene {interrupciones.Count} interrupciones; se espera una por aviso.");

    var nroInterrupcion = interrupciones[0];
    var sistema = filas[0];
    Console.WriteLine($"Interrupción {nroInterrupcion}: {filas.Count} suministros leídos ({filas.Select(f => f.NroSuministro).Distinct().Count()} únicos).");

    if (a.ContainsKey("validar"))
    {
        Console.WriteLine("Modo --validar: no se escribió en la base de datos.");
        return 0;
    }

    var conexion = Opc("conexion") ?? Environment.GetEnvironmentVariable("CORPRO_DB")
        ?? throw new ArgumentException("Defina la variable de entorno CORPRO_DB o use --conexion.");

    await using var ds = NpgsqlDataSource.Create(conexion);
    await using var cn = await ds.OpenConnectionAsync();
    await using var tx = await cn.BeginTransactionAsync();

    // 1) Cabecera
    await using (var cmd = new NpgsqlCommand("""
        INSERT INTO interrupcion_programada
            (nro_interrupcion, tipo_servicio, empresa, alimentador, cod_sistema_electrico, nombre_sistema_electrico,
             fecha_corte, hora_inicio, hora_fin, motivo, sectores_afectados, codigo_documento, imagen_aviso, usuario_creacion)
        VALUES (@nro, @tipo, @empresa, @alim, @codsis, @nomsis, @fecha, @ini, @fin, @motivo, @sectores, @doc, @img, @usr)
        ON CONFLICT (nro_interrupcion) DO UPDATE SET
            tipo_servicio = EXCLUDED.tipo_servicio, empresa = EXCLUDED.empresa, alimentador = EXCLUDED.alimentador,
            cod_sistema_electrico = EXCLUDED.cod_sistema_electrico, nombre_sistema_electrico = EXCLUDED.nombre_sistema_electrico,
            fecha_corte = EXCLUDED.fecha_corte, hora_inicio = EXCLUDED.hora_inicio, hora_fin = EXCLUDED.hora_fin,
            motivo = EXCLUDED.motivo, sectores_afectados = EXCLUDED.sectores_afectados, codigo_documento = EXCLUDED.codigo_documento,
            imagen_aviso = COALESCE(EXCLUDED.imagen_aviso, interrupcion_programada.imagen_aviso),
            fecha_modificacion = now(), usuario_modificacion = EXCLUDED.usuario_creacion, activo = TRUE
        """, cn, tx))
    {
        cmd.Parameters.AddWithValue("nro", nroInterrupcion);
        cmd.Parameters.AddWithValue("tipo", (Opc("tipo") ?? "ENERGIA").ToUpperInvariant());
        cmd.Parameters.AddWithValue("empresa", Req("empresa"));
        cmd.Parameters.AddWithValue("alim", (object?)Opc("alimentador") ?? DBNull.Value);
        cmd.Parameters.AddWithValue("codsis", (object?)sistema.CodSistema ?? DBNull.Value);
        cmd.Parameters.AddWithValue("nomsis", (object?)sistema.NombreSistema ?? DBNull.Value);
        cmd.Parameters.AddWithValue("fecha", fecha);
        cmd.Parameters.AddWithValue("ini", inicio);
        cmd.Parameters.AddWithValue("fin", fin);
        cmd.Parameters.AddWithValue("motivo", (object?)Opc("motivo") ?? DBNull.Value);
        cmd.Parameters.AddWithValue("sectores", (object?)Opc("sectores") ?? DBNull.Value);
        cmd.Parameters.AddWithValue("doc", (object?)Opc("documento") ?? DBNull.Value);
        cmd.Parameters.AddWithValue("img", (object?)Opc("imagen-aviso") ?? DBNull.Value);
        cmd.Parameters.AddWithValue("usr", usuario);
        await cmd.ExecuteNonQueryAsync();
    }

    // 2) Suministros: COPY binario a tabla temporal + upsert (miles de filas en segundos)
    await using (var cmd = new NpgsqlCommand("""
        CREATE TEMP TABLE tmp_suministro (
            nro_suministro VARCHAR(20), direccion VARCHAR(200), tarifa VARCHAR(10), tipo_servicio VARCHAR(20),
            nivel_tension VARCHAR(5), cartera VARCHAR(5), sector_tipico VARCHAR(5)) ON COMMIT DROP
        """, cn, tx))
        await cmd.ExecuteNonQueryAsync();

    await using (var copy = await cn.BeginBinaryImportAsync(
        "COPY tmp_suministro (nro_suministro, direccion, tarifa, tipo_servicio, nivel_tension, cartera, sector_tipico) FROM STDIN (FORMAT BINARY)"))
    {
        foreach (var f in filas)
        {
            await copy.StartRowAsync();
            await copy.WriteAsync(f.NroSuministro, NpgsqlDbType.Varchar);
            await copy.WriteAsync(f.Direccion, NpgsqlDbType.Varchar);
            await EscribirNullable(copy, f.Tarifa);
            await EscribirNullable(copy, f.TipoServicio);
            await EscribirNullable(copy, f.NivelTension);
            await EscribirNullable(copy, f.Cartera);
            await EscribirNullable(copy, f.SectorTipico);
        }
        await copy.CompleteAsync();
    }

    int afectadas;
    await using (var cmd = new NpgsqlCommand("""
        INSERT INTO suministro_afectado
            (nro_interrupcion, nro_suministro, direccion, tarifa, tipo_servicio, nivel_tension, cartera, sector_tipico, usuario_creacion)
        SELECT @nro, nro_suministro, direccion, tarifa, tipo_servicio, nivel_tension, cartera, sector_tipico, @usr
        FROM tmp_suministro
        ON CONFLICT (nro_interrupcion, nro_suministro) DO UPDATE SET
            direccion = EXCLUDED.direccion, tarifa = EXCLUDED.tarifa, tipo_servicio = EXCLUDED.tipo_servicio,
            nivel_tension = EXCLUDED.nivel_tension, cartera = EXCLUDED.cartera, sector_tipico = EXCLUDED.sector_tipico,
            fecha_modificacion = now(), usuario_modificacion = EXCLUDED.usuario_creacion, activo = TRUE
        """, cn, tx))
    {
        cmd.Parameters.AddWithValue("nro", nroInterrupcion);
        cmd.Parameters.AddWithValue("usr", usuario);
        afectadas = await cmd.ExecuteNonQueryAsync();
    }

    await tx.CommitAsync();
    Console.WriteLine($"Listo: {afectadas} suministros insertados/actualizados.");
    return 0;
}
catch (Exception ex) when (ex is ArgumentException or FormatException or InvalidOperationException or NpgsqlException or IOException)
{
    Console.Error.WriteLine($"Error: {ex.Message}");
    return 1;
}

static List<FilaExcel> LeerExcel(string ruta)
{
    using var libro = new XLWorkbook(ruta);
    var hoja = libro.Worksheet(1);
    var encabezado = hoja.Row(1).CellsUsed().ToDictionary(c => c.GetString().Trim(), c => c.Address.ColumnNumber, StringComparer.OrdinalIgnoreCase);

    int Col(string nombre) => encabezado.TryGetValue(nombre, out var n) ? n : throw new InvalidOperationException($"Falta la columna '{nombre}'.");
    string? Texto(IXLRow r, string nombre) => encabezado.ContainsKey(nombre) ? Normalizar(r.Cell(Col(nombre)).GetString()) : null;

    var lista = new List<FilaExcel>();
    foreach (var r in hoja.RowsUsed().Skip(1))
    {
        var nroSum = Normalizar(r.Cell(Col("IDNroServicio")).GetString());
        if (nroSum is null) continue;

        lista.Add(new FilaExcel(
            NroInterrupcion: long.Parse(r.Cell(Col("NroInterrupcion")).GetString(), CultureInfo.InvariantCulture),
            NroSuministro: nroSum,
            Direccion: Normalizar(r.Cell(Col("Direccion")).GetString()) ?? "(sin dirección)",
            Tarifa: Texto(r, "Tarifa"),
            TipoServicio: Texto(r, "TipoServicio"),
            NivelTension: Texto(r, "NivelTension"),
            Cartera: Texto(r, "Cartera"),
            SectorTipico: Texto(r, "SectorTipico"),
            CodSistema: Texto(r, "CodSistemaElectrico"),
            NombreSistema: Texto(r, "NombreSistemaElectrico")));
    }
    return lista;
}

// Colapsa espacios múltiples: así la búsqueda "Jr. Los Pinos 120" coincide con "Jr. LOS PINOS  120".
static string? Normalizar(string? s)
{
    if (string.IsNullOrWhiteSpace(s)) return null;
    return Regex.Replace(s, @"\s+", " ").Trim();
}

static Task EscribirNullable(NpgsqlBinaryImporter copy, string? valor) =>
    valor is null ? copy.WriteNullAsync() : copy.WriteAsync(valor, NpgsqlDbType.Varchar);

static Dictionary<string, string> LeerArgumentos(string[] args)
{
    var d = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
    for (var i = 0; i < args.Length; i++)
        if (args[i].StartsWith("--"))
            d[args[i][2..]] = i + 1 < args.Length && !args[i + 1].StartsWith("--") ? args[++i] : "";
    return d;
}

record FilaExcel(
    long NroInterrupcion, string NroSuministro, string Direccion,
    string? Tarifa, string? TipoServicio, string? NivelTension, string? Cartera, string? SectorTipico,
    string? CodSistema, string? NombreSistema);
