using Microsoft.Extensions.Configuration;

namespace Corpro.Web.Servicios;

/// <summary>
/// Datos de marca de la sección "Portal" de wwwroot/appsettings.json.
/// Permite usar el mismo portal para otra empresa del grupo sin tocar código.
/// </summary>
public sealed class PortalOpciones
{
    public string Empresa { get; init; } = "Electrocentro";
    public string Grupo { get; init; } = "Grupo Distriluz";

    /// <summary>Ruta del logo dentro de wwwroot (ej. img/logo.svg). Vacío = logotipo de texto.</summary>
    public string? LogoUrl { get; init; }

    /// <summary>Texto libre del centro de atención (ej. teléfono de emergencias). Vacío = no se muestra.</summary>
    public string? CentroAtencion { get; init; }

    public string? SitioWeb { get; init; }

    public static PortalOpciones Desde(IConfiguration config)
    {
        var s = config.GetSection("Portal");
        static string? Valor(string? v) => string.IsNullOrWhiteSpace(v) ? null : v.Trim();
        return new PortalOpciones
        {
            Empresa = Valor(s["Empresa"]) ?? "Electrocentro",
            Grupo = Valor(s["Grupo"]) ?? "Grupo Distriluz",
            LogoUrl = UrlSegura.Normalizar(s["LogoUrl"]),
            CentroAtencion = Valor(s["CentroAtencion"]),
            SitioWeb = UrlSegura.Normalizar(s["SitioWeb"]),
        };
    }
}
