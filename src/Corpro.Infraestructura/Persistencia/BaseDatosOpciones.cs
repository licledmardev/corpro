namespace Corpro.Infraestructura.Persistencia;

/// <summary>Sección "BaseDatos" de appsettings.json.</summary>
public sealed class BaseDatosOpciones
{
    public const string Seccion = "BaseDatos";

    /// <summary>Nombre de la cadena en ConnectionStrings: "Local" o "Neon".</summary>
    public string ConexionActiva { get; set; } = "Local";

    public int ReintentosMaximos { get; set; } = 3;

    public int TiempoEsperaComandoSegundos { get; set; } = 30;
}
