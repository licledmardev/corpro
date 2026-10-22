using System.Text.RegularExpressions;

namespace Corpro.Dominio.Cortes;

/// <summary>
/// Texto que escribe el ciudadano para buscar: un número de suministro (coincidencia exacta)
/// o parte de una dirección (coincidencia parcial).
/// </summary>
public sealed partial record CriterioConsulta
{
    public const int LongitudMinimaDireccion = 4;
    public const int LongitudMaxima = 100;

    public string Valor { get; }

    /// <summary>True si el texto solo tiene dígitos: se trata como número de suministro.</summary>
    public bool EsSuministro { get; }

    private CriterioConsulta(string valor, bool esSuministro)
    {
        Valor = valor;
        EsSuministro = esSuministro;
    }

    public static bool TryCrear(string? texto, out CriterioConsulta? criterio, out string? error)
    {
        criterio = null;
        error = null;

        // Colapsa espacios múltiples: las direcciones del sistema comercial vienen con
        // espacios repetidos y se normalizan igual al importarlas.
        var normalizado = EspaciosMultiples().Replace(texto ?? string.Empty, " ").Trim();

        if (normalizado.Length == 0)
        {
            error = "Ingrese un número de suministro o una dirección.";
            return false;
        }

        if (normalizado.Length > LongitudMaxima)
        {
            error = $"El criterio no puede superar {LongitudMaxima} caracteres.";
            return false;
        }

        var esSuministro = normalizado.All(char.IsAsciiDigit);

        if (!esSuministro && normalizado.Length < LongitudMinimaDireccion)
        {
            error = $"Para buscar por dirección ingrese al menos {LongitudMinimaDireccion} caracteres.";
            return false;
        }

        criterio = new CriterioConsulta(normalizado, esSuministro);
        return true;
    }

    [GeneratedRegex(@"\s+")]
    private static partial Regex EspaciosMultiples();
}
