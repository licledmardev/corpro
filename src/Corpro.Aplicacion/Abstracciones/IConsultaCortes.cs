using Corpro.Contratos.Cortes;
using Corpro.Dominio.Cortes;

namespace Corpro.Aplicacion.Abstracciones;

/// <summary>Servicio de lectura de cortes vigentes. Implementado en Infraestructura.</summary>
public interface IConsultaCortes
{
    /// <summary>
    /// Devuelve hasta <paramref name="maximo"/> cortes con fecha igual o posterior a
    /// <paramref name="desde"/> que coincidan con el criterio.
    /// </summary>
    Task<IReadOnlyList<CorteProgramadoDto>> BuscarAsync(
        CriterioConsulta criterio, DateOnly desde, int maximo, CancellationToken ct);

    /// <summary>Avisos activos con fecha igual o posterior a <paramref name="desde"/>, del más próximo al más lejano.</summary>
    Task<IReadOnlyList<AvisoVigenteDto>> AvisosVigentesAsync(DateOnly desde, int maximo, CancellationToken ct);
}
