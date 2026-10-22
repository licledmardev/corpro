using Corpro.Aplicacion.Abstracciones;
using Corpro.Dominio.Contacto;
using Corpro.Infraestructura.Persistencia.Contexto;
using Corpro.Infraestructura.Persistencia.EntidadesBaseDatos;

namespace Corpro.Infraestructura.Repositorios;

public sealed class ActualizacionContactoRepositorio(CorproDbContext db) : IActualizacionContactoRepositorio
{
    public async Task<long> AgregarAsync(ActualizacionContacto actualizacion, CancellationToken ct)
    {
        // Mapeo manual Dominio → EntidadBaseDatos. Parámetros siempre vía EF (sin SQL concatenado).
        var fila = new DatosContactoActualizados
        {
            Suministro = actualizacion.Suministro,
            TelefonoNuevo = actualizacion.Celular,
            CorreoNuevo = actualizacion.Correo,
            FechaRegistro = actualizacion.FechaRegistroUtc,
        };

        db.DatosContactoActualizados.Add(fila);
        await db.SaveChangesAsync(ct);
        return fila.Id;
    }
}
