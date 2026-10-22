namespace Corpro.Web.Servicios;

/// <summary>Acepta solo rutas relativas al sitio o URLs http(s). Descarta "javascript:", "data:", etc.</summary>
public static class UrlSegura
{
    public static string? Normalizar(string? url)
    {
        if (string.IsNullOrWhiteSpace(url)) return null;
        url = url.Trim();

        if (Uri.TryCreate(url, UriKind.Absolute, out var abs))
            return abs.Scheme is "http" or "https" ? abs.ToString() : null;

        // Relativa: sin "//" inicial (sería otro dominio) y sin ":" (esquemas).
        return url.StartsWith("//") || url.Contains(':') ? null : url.TrimStart('/');
    }
}
