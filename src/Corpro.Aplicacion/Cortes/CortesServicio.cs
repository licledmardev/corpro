using Corpro.Aplicacion.Abstracciones;
using Corpro.Aplicacion.Comun;
using Corpro.Contratos.Contacto;
using Corpro.Contratos.Cortes;
using Corpro.Dominio.Contacto;
using Corpro.Dominio.Cortes;
using Microsoft.Extensions.Options;

namespace Corpro.Aplicacion.Cortes;

public sealed class CortesServicio(
    IConsultaCortes consultaCortes,
    IActualizacionContactoRepositorio contactos,
    TimeProvider reloj,
    IOptions<CortesOpciones> opciones)
{
    private const int MaximoAvisos = 20;

    private readonly CortesOpciones _opciones = opciones.Value;
    private readonly TimeZoneInfo _zona = ObtenerZona(opciones.Value.ZonaHoraria);

    // Si el sistema no trae la base de zonas horarias (contenedores mínimos), se usa UTC-5:
    // Perú no aplica horario de verano.
    private static TimeZoneInfo ObtenerZona(string id) =>
        TimeZoneInfo.TryFindSystemTimeZoneById(id, out var zona)
            ? zona
            : TimeZoneInfo.CreateCustomTimeZone(id, TimeSpan.FromHours(-5), id, id);

    public async Task<Resultado<ConsultaCortesRespuesta>> ConsultarAsync(string? texto, CancellationToken ct)
    {
        if (!CriterioConsulta.TryCrear(texto, out var criterio, out var error))
            return Resultado<ConsultaCortesRespuesta>.Fallo("criterio", error!);

        // Se pide uno más del máximo para saber si hay más resultados sin hacer COUNT(*).
        var cortes = await consultaCortes.BuscarAsync(criterio!, Hoy(), _opciones.MaximoResultados + 1, ct);
        var hayMas = cortes.Count > _opciones.MaximoResultados;
        var visibles = cortes.Take(_opciones.MaximoResultados);

        // Por dirección se enmascara el suministro: quien busca por dirección no
        // necesariamente es el titular de los suministros vecinos.
        if (!criterio!.EsSuministro)
            visibles = visibles.Select(c => c with { Suministro = Enmascarar(c.Suministro) });

        var lista = visibles.ToList();
        return Resultado<ConsultaCortesRespuesta>.Exito(
            new ConsultaCortesRespuesta(criterio.Valor, criterio.EsSuministro, lista.Count > 0, hayMas, lista));
    }

    public Task<IReadOnlyList<AvisoVigenteDto>> AvisosVigentesAsync(CancellationToken ct) =>
        consultaCortes.AvisosVigentesAsync(Hoy(), MaximoAvisos, ct);

    public async Task<Resultado<ActualizacionContactoRespuesta>> ActualizarContactoAsync(
        ActualizacionContactoSolicitud solicitud, CancellationToken ct)
    {
        var errores = ActualizacionContacto.TryCrear(
            solicitud.Suministro, solicitud.Celular, solicitud.Correo,
            reloj.GetUtcNow().UtcDateTime, out var actualizacion);

        if (errores.Count > 0)
            return Resultado<ActualizacionContactoRespuesta>.Fallo(errores);

        var id = await contactos.AgregarAsync(actualizacion!, ct);
        return Resultado<ActualizacionContactoRespuesta>.Exito(new ActualizacionContactoRespuesta(id, actualizacion!.FechaRegistroUtc));
    }

    private DateOnly Hoy() => DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(reloj.GetUtcNow(), _zona).DateTime);

    private static string Enmascarar(string suministro) =>
        suministro.Length <= 4 ? new string('*', suministro.Length) : $"****{suministro[^4..]}";
}
