namespace Corpro.Web.Servicios;

/// <summary>Aviso que se abre en el visor de imagen (URL ya validada con UrlSegura).</summary>
public sealed record AvisoSeleccionado(string Titulo, string UrlImagen);
