using System;
using System.Collections.Generic;

namespace Corpro.Infraestructura.Persistencia.EntidadesBaseDatos;

public partial class CorteProgramado
{
    public string? Suministro { get; set; }

    public string? Direccion { get; set; }

    public DateOnly? FechaCorte { get; set; }

    public TimeOnly? HoraInicio { get; set; }

    public TimeOnly? HoraFin { get; set; }

    public long? NroInterrupcion { get; set; }

    public string? TipoServicio { get; set; }

    public string? Empresa { get; set; }

    public string? Alimentador { get; set; }

    public string? Motivo { get; set; }

    public string? SectoresAfectados { get; set; }

    public string? ImagenAviso { get; set; }
}
