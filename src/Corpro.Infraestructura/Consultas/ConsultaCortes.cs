using Corpro.Aplicacion.Abstracciones;
using Corpro.Contratos.Cortes;
using Corpro.Dominio.Cortes;
using Corpro.Infraestructura.Persistencia.Contexto;
using Microsoft.EntityFrameworkCore;

namespace Corpro.Infraestructura.Consultas;

/// <summary>Lectura sobre la vista corte_programado. Filtra y limita en SQL antes de proyectar.</summary>
public sealed class ConsultaCortes(CorproDbContext db) : IConsultaCortes
{
    public async Task<IReadOnlyList<CorteProgramadoDto>> BuscarAsync(
        CriterioConsulta criterio, DateOnly desde, int maximo, CancellationToken ct)
    {
        var query = db.CorteProgramado
            .AsNoTracking()
            .Where(c => c.FechaCorte >= desde);

        if (criterio.EsSuministro)
        {
            // Coincidencia exacta → usa ix_suministro_afectado_nro_suministro
            query = query.Where(c => c.Suministro == criterio.Valor);
        }
        else
        {
            // Coincidencia parcial, sin distinguir mayúsculas → usa el índice GIN pg_trgm.
            // Se escapan % y _ para que el texto del usuario sea literal.
            var patron = $"%{EscaparLike(criterio.Valor)}%";
            query = query.Where(c => EF.Functions.ILike(c.Direccion!, patron, @"\"));
        }

        var filas = await query
            .OrderBy(c => c.FechaCorte)
            .ThenBy(c => c.HoraInicio)
            .ThenBy(c => c.Direccion)
            .Take(maximo)
            .ToListAsync(ct);

        // Mapeo manual EntidadBaseDatos → DTO (sin AutoMapper).
        return filas.Select(c => new CorteProgramadoDto(
                NroInterrupcion: c.NroInterrupcion ?? 0,
                Suministro: c.Suministro ?? string.Empty,
                Direccion: c.Direccion ?? string.Empty,
                FechaCorte: c.FechaCorte!.Value,
                HoraInicio: c.HoraInicio!.Value,
                HoraFin: c.HoraFin!.Value,
                TipoServicio: c.TipoServicio ?? "ENERGIA",
                Empresa: c.Empresa ?? string.Empty,
                Motivo: c.Motivo,
                SectoresAfectados: c.SectoresAfectados,
                ImagenAviso: c.ImagenAviso))
            .ToList();
    }

    public async Task<IReadOnlyList<AvisoVigenteDto>> AvisosVigentesAsync(DateOnly desde, int maximo, CancellationToken ct)
    {
        // Usa ix_interrupcion_fecha_activo (índice parcial WHERE activo). Proyección directa en SQL.
        return await db.InterrupcionProgramada
            .AsNoTracking()
            .Where(i => i.Activo && i.FechaCorte >= desde)
            .OrderBy(i => i.FechaCorte)
            .ThenBy(i => i.HoraInicio)
            .Take(maximo)
            .Select(i => new AvisoVigenteDto(
                i.NroInterrupcion, i.TipoServicio, i.Empresa, i.FechaCorte, i.HoraInicio, i.HoraFin,
                i.Motivo, i.SectoresAfectados, i.ImagenAviso))
            .ToListAsync(ct);
    }

    private static string EscaparLike(string texto) =>
        texto.Replace(@"\", @"\\").Replace("%", @"\%").Replace("_", @"\_");
}
