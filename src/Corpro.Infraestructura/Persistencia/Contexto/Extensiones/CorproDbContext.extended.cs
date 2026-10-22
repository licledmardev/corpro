using Corpro.Infraestructura.Persistencia.EntidadesBaseDatos;
using Microsoft.EntityFrameworkCore;

namespace Corpro.Infraestructura.Persistencia.Contexto;

/// <summary>
/// Extensión del contexto generado. Sobrevive a `dotnet ef dbcontext scaffold --force`.
/// Completa las columnas de auditoría antes de guardar.
/// </summary>
public partial class CorproDbContext
{
    /// <summary>Usuario que se registra en auditoría. El portal es público y anónimo.</summary>
    public const string UsuarioPortalPublico = "portal-publico";

    public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        AplicarAuditoria();
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    private void AplicarAuditoria()
    {
        var ahora = DateTime.UtcNow; // timestamptz exige DateTime en UTC con Npgsql

        foreach (var entrada in ChangeTracker.Entries())
        {
            switch (entrada.Entity)
            {
                case DatosContactoActualizados d when entrada.State == EntityState.Added:
                    d.FechaCreacion = ahora;
                    d.UsuarioCreacion = UsuarioPortalPublico;
                    d.Activo = true;
                    break;
                case DatosContactoActualizados d when entrada.State == EntityState.Modified:
                    d.FechaModificacion = ahora;
                    d.UsuarioModificacion = UsuarioPortalPublico;
                    break;
            }
        }
    }
}
