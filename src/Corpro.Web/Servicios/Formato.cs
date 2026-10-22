using System.Globalization;

namespace Corpro.Web.Servicios;

/// <summary>Formatos de fecha y hora en español (la cultura es-PE se fija en index.html).</summary>
public static class Formato
{
    /// <summary>"Domingo, 25 de octubre de 2026" (o en minúscula para usar dentro de una frase).</summary>
    public static string FechaLarga(DateOnly fecha, bool mayuscula = true)
    {
        var texto = fecha.ToString("dddd, d 'de' MMMM 'de' yyyy", CultureInfo.CurrentCulture);
        return mayuscula ? char.ToUpper(texto[0], CultureInfo.CurrentCulture) + texto[1..] : texto;
    }

    public static string Dia(DateOnly fecha) => fecha.Day.ToString(CultureInfo.CurrentCulture);

    /// <summary>"oct"</summary>
    public static string MesCorto(DateOnly fecha) =>
        fecha.ToString("MMM", CultureInfo.CurrentCulture).TrimEnd('.');

    /// <summary>"06:00 a 15:00 h"</summary>
    public static string Horario(TimeOnly inicio, TimeOnly fin) => $"{inicio:HH\\:mm} a {fin:HH\\:mm} h";

    public static string Servicio(string tipoServicio) =>
        tipoServicio == "AGUA" ? "agua potable" : "energía eléctrica";
}
