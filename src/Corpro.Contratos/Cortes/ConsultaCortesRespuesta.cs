namespace Corpro.Contratos.Cortes;

/// <summary>Respuesta de GET /api/cortes/consultar/{criterio}.</summary>
public sealed record ConsultaCortesRespuesta(
    string Criterio,
    bool BusquedaPorSuministro,
    bool Afectado,
    bool HayMasResultados,
    IReadOnlyList<CorteProgramadoDto> Cortes);

/// <summary>
/// Un suministro afectado por un corte. En búsquedas por dirección el número de
/// suministro llega enmascarado (ej. ****1665) para no exponer datos de terceros.
/// </summary>
public sealed record CorteProgramadoDto(
    long NroInterrupcion,
    string Suministro,
    string Direccion,
    DateOnly FechaCorte,
    TimeOnly HoraInicio,
    TimeOnly HoraFin,
    string TipoServicio,
    string Empresa,
    string? Motivo,
    string? SectoresAfectados,
    string? ImagenAviso);

/// <summary>Aviso de corte vigente (GET /api/cortes/avisos-vigentes).</summary>
public sealed record AvisoVigenteDto(
    long NroInterrupcion,
    string TipoServicio,
    string Empresa,
    DateOnly FechaCorte,
    TimeOnly HoraInicio,
    TimeOnly HoraFin,
    string? Motivo,
    string? SectoresAfectados,
    string? ImagenAviso);
