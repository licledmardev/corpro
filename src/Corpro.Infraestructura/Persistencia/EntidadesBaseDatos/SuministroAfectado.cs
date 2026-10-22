using System;
using System.Collections.Generic;

namespace Corpro.Infraestructura.Persistencia.EntidadesBaseDatos;

public partial class SuministroAfectado
{
    public long Id { get; set; }

    public long NroInterrupcion { get; set; }

    public string NroSuministro { get; set; } = null!;

    public string Direccion { get; set; } = null!;

    public string? Tarifa { get; set; }

    public string? TipoServicio { get; set; }

    public string? NivelTension { get; set; }

    public string? Cartera { get; set; }

    public string? SectorTipico { get; set; }

    public DateTime FechaCreacion { get; set; }

    public string UsuarioCreacion { get; set; } = null!;

    public DateTime? FechaModificacion { get; set; }

    public string? UsuarioModificacion { get; set; }

    public bool Activo { get; set; }

    public virtual InterrupcionProgramada NroInterrupcionNavigation { get; set; } = null!;
}
