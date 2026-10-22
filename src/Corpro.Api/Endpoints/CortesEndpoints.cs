using Corpro.Aplicacion.Cortes;
using Corpro.Contratos.Contacto;
using Corpro.Contratos.Cortes;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Corpro.Api.Endpoints;

public static class CortesEndpoints
{
    public static IEndpointRouteBuilder MapCortesEndpoints(this IEndpointRouteBuilder app)
    {
        var grupo = app.MapGroup("/api/cortes")
            .WithTags("Cortes")
            .RequireCors(Politicas.CorsFrontend);

        grupo.MapGet("/consultar/{criterio}", ConsultarAsync)
            .WithName("ConsultarCortes")
            .WithSummary("Busca cortes vigentes por número de suministro (exacto) o dirección (parcial).")
            .RequireRateLimiting(Politicas.LimiteConsulta);

        grupo.MapGet("/avisos-vigentes", AvisosVigentesAsync)
            .WithName("AvisosVigentes")
            .WithSummary("Lista los avisos de corte programados desde hoy, con su imagen.")
            .CacheOutput(Politicas.CacheAvisos)
            .RequireRateLimiting(Politicas.LimiteConsulta);

        grupo.MapPost("/actualizar-datos", ActualizarDatosAsync)
            .WithName("ActualizarDatosContacto")
            .WithSummary("Registra el celular y/o correo del cliente para avisarle de cortes.")
            .RequireRateLimiting(Politicas.LimiteRegistro);

        return app;
    }

    private static async Task<Results<Ok<ConsultaCortesRespuesta>, ValidationProblem>> ConsultarAsync(
        string criterio, CortesServicio servicio, CancellationToken ct)
    {
        // Kestrel no decodifica %2F en la ruta; direcciones como "Mz. C/Lote 4" lo necesitan.
        criterio = criterio.Replace("%2F", "/", StringComparison.OrdinalIgnoreCase);

        var resultado = await servicio.ConsultarAsync(criterio, ct);
        return resultado.EsExitoso
            ? TypedResults.Ok(resultado.Valor!)
            : TypedResults.ValidationProblem(resultado.Errores);
    }

    private static async Task<Ok<IReadOnlyList<AvisoVigenteDto>>> AvisosVigentesAsync(
        CortesServicio servicio, CancellationToken ct) =>
        TypedResults.Ok(await servicio.AvisosVigentesAsync(ct));

    private static async Task<Results<Created<ActualizacionContactoRespuesta>, ValidationProblem>> ActualizarDatosAsync(
        ActualizacionContactoSolicitud solicitud, CortesServicio servicio, CancellationToken ct)
    {
        var resultado = await servicio.ActualizarContactoAsync(solicitud, ct);
        return resultado.EsExitoso
            ? TypedResults.Created((string?)null, resultado.Valor!)
            : TypedResults.ValidationProblem(resultado.Errores);
    }
}
