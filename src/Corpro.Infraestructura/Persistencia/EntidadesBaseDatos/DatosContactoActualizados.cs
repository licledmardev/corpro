using System;
using System.Collections.Generic;

namespace Corpro.Infraestructura.Persistencia.EntidadesBaseDatos;

public partial class DatosContactoActualizados
{
    public long Id { get; set; }

    public string Suministro { get; set; } = null!;

    public string? TelefonoNuevo { get; set; }

    public string? CorreoNuevo { get; set; }

    public DateTime FechaRegistro { get; set; }

    public DateTime FechaCreacion { get; set; }

    public string UsuarioCreacion { get; set; } = null!;

    public DateTime? FechaModificacion { get; set; }

    public string? UsuarioModificacion { get; set; }

    public bool Activo { get; set; }
}
