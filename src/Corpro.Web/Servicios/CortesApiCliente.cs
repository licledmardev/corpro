using System.Net;
using System.Net.Http.Json;
using Corpro.Contratos.Contacto;
using Corpro.Contratos.Cortes;

namespace Corpro.Web.Servicios;

/// <summary>Cliente tipado de la Web API. La URL base sale de wwwroot/appsettings.json ("ApiBaseUrl").</summary>
public sealed class CortesApiCliente(HttpClient http)
{
    public async Task<RespuestaApi<ConsultaCortesRespuesta>> ConsultarAsync(string criterio, CancellationToken ct = default)
    {
        // EscapeDataString codifica espacios, tildes y "/" para que la dirección viaje en la ruta.
        var url = $"api/cortes/consultar/{Uri.EscapeDataString(criterio.Trim())}";
        using var resp = await http.GetAsync(url, ct);
        return await LeerAsync<ConsultaCortesRespuesta>(resp, ct);
    }

    public async Task<RespuestaApi<IReadOnlyList<AvisoVigenteDto>>> AvisosVigentesAsync(CancellationToken ct = default)
    {
        using var resp = await http.GetAsync("api/cortes/avisos-vigentes", ct);
        return await LeerAsync<IReadOnlyList<AvisoVigenteDto>>(resp, ct);
    }

    public async Task<RespuestaApi<ActualizacionContactoRespuesta>> ActualizarContactoAsync(
        ActualizacionContactoSolicitud solicitud, CancellationToken ct = default)
    {
        using var resp = await http.PostAsJsonAsync("api/cortes/actualizar-datos", solicitud, ct);
        return await LeerAsync<ActualizacionContactoRespuesta>(resp, ct);
    }

    private static async Task<RespuestaApi<T>> LeerAsync<T>(HttpResponseMessage resp, CancellationToken ct)
    {
        if (resp.IsSuccessStatusCode)
            return RespuestaApi<T>.Ok((await resp.Content.ReadFromJsonAsync<T>(ct))!);

        if (resp.StatusCode == HttpStatusCode.TooManyRequests)
            return RespuestaApi<T>.Error("Demasiadas solicitudes. Espere unos minutos e intente de nuevo.");

        if (resp.StatusCode == HttpStatusCode.BadRequest)
        {
            var problema = await resp.Content.ReadFromJsonAsync<ProblemaValidacion>(ct);
            return RespuestaApi<T>.Error("Revise los datos ingresados.", problema?.Errors);
        }

        return RespuestaApi<T>.Error("El servicio no está disponible en este momento. Intente más tarde.");
    }

    private sealed record ProblemaValidacion(string? Title, Dictionary<string, string[]>? Errors);
}

public sealed record RespuestaApi<T>(T? Valor, string? Mensaje, IReadOnlyDictionary<string, string[]>? Errores)
{
    public bool EsExitosa => Mensaje is null;
    public static RespuestaApi<T> Ok(T valor) => new(valor, null, null);
    public static RespuestaApi<T> Error(string mensaje, IReadOnlyDictionary<string, string[]>? errores = null) => new(default, mensaje, errores);
}
