using System.Net.Mail;

namespace Corpro.Dominio.Contacto;

/// <summary>
/// Datos de contacto que el cliente nos deja (opcional) para avisarle de cortes.
/// Solo se aceptan celular y/o correo electrónico.
/// </summary>
public sealed class ActualizacionContacto
{
    public string Suministro { get; }
    public string? Celular { get; }
    public string? Correo { get; }
    public DateTime FechaRegistroUtc { get; }

    private ActualizacionContacto(string suministro, string? celular, string? correo, DateTime fechaRegistroUtc)
    {
        Suministro = suministro;
        Celular = celular;
        Correo = correo;
        FechaRegistroUtc = fechaRegistroUtc;
    }

    /// <summary>Crea la actualización si los datos son válidos; si no, devuelve los errores por campo.</summary>
    public static IReadOnlyDictionary<string, string[]> TryCrear(
        string? suministro, string? celular, string? correo, DateTime fechaRegistroUtc,
        out ActualizacionContacto? actualizacion)
    {
        actualizacion = null;
        var errores = new Dictionary<string, string[]>();

        var sum = suministro?.Trim() ?? string.Empty;
        var cel = Limpiar(celular);
        var mail = Limpiar(correo)?.ToLowerInvariant();

        if (sum.Length is < 1 or > 20 || !sum.All(char.IsAsciiDigit))
            errores["Suministro"] = ["Ingrese su número de suministro (solo dígitos)."];

        if (cel is null && mail is null)
            errores["Contacto"] = ["Ingrese su celular, su correo o ambos."];

        // Celular Perú: 9 dígitos que empiezan en 9. Se acepta el prefijo +51 o 51.
        if (cel is not null)
        {
            if (cel.StartsWith("+51")) cel = cel[3..];
            else if (cel.Length == 11 && cel.StartsWith("51")) cel = cel[2..];

            if (cel.Length != 9 || cel[0] != '9' || !cel.All(char.IsAsciiDigit))
                errores["Celular"] = ["Ingrese un celular de 9 dígitos que empiece con 9."];
        }

        if (mail is not null && (mail.Length > 150 || !MailAddress.TryCreate(mail, out var parsed) || parsed.Address != mail))
            errores["Correo"] = ["Ingrese un correo electrónico válido."];

        if (errores.Count == 0)
            actualizacion = new ActualizacionContacto(sum, cel, mail, fechaRegistroUtc);

        return errores;
    }

    private static string? Limpiar(string? valor)
    {
        if (string.IsNullOrWhiteSpace(valor)) return null;
        return new string(valor.Where(c => !char.IsWhiteSpace(c) && c != '-').ToArray());
    }
}
