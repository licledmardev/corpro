using Corpro.Dominio.Contacto;

namespace Corpro.Aplicacion.Abstracciones;

public interface IActualizacionContactoRepositorio
{
    /// <summary>Guarda los datos de contacto y devuelve su Id.</summary>
    Task<long> AgregarAsync(ActualizacionContacto actualizacion, CancellationToken ct);
}
