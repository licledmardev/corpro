using System;
using System.Collections.Generic;

namespace Corpro.Infraestructura.Persistencia.EntidadesBaseDatos;

public partial class InterrupcionProgramada
{
    public long NroInterrupcion { get; set; }

    public string TipoServicio { get; set; } = null!;

    public string Empresa { get; set; } = null!;

    public string? Alimentador { get; set; }

    public string? CodSistemaElectrico { get; set; }

    public string? NombreSistemaElectrico { get; set; }

    public DateOnly FechaCorte { get; set; }

    public TimeOnly HoraInicio { get; set; }

    public TimeOnly HoraFin { get; set; }

    public string? Motivo { get; set; }

    public string? SectoresAfectados { get; set; }

    public string? CodigoDocumento { get; set; }

    public DateTime FechaCreacion { get; set; }

    public string UsuarioCreacion { get; set; } = null!;

    public DateTime? FechaModificacion { get; set; }

    public string? UsuarioModificacion { get; set; }

    public bool Activo { get; set; }

    public string? ImagenAviso { get; set; }

    public virtual ICollection<SuministroAfectado> SuministroAfectado { get; set; } = new List<SuministroAfectado>();
}
