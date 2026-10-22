namespace Corpro.Aplicacion.Cortes;

/// <summary>Sección "Cortes" de appsettings.json.</summary>
public sealed class CortesOpciones
{
    public const string Seccion = "Cortes";

    /// <summary>Zona IANA para decidir qué cortes siguen vigentes "hoy".</summary>
    public string ZonaHoraria { get; set; } = "America/Lima";

    /// <summary>Tope de filas por consulta (protege a la base ante búsquedas amplias).</summary>
    public int MaximoResultados { get; set; } = 50;
}
