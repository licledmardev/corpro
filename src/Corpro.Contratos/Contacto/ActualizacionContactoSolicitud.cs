using System.ComponentModel.DataAnnotations;

namespace Corpro.Contratos.Contacto;

/// <summary>
/// Cuerpo de POST /api/cortes/actualizar-datos (opcional para el cliente).
/// Las anotaciones dan validación inmediata en Blazor; la API vuelve a validar en el servidor.
/// </summary>
public sealed class ActualizacionContactoSolicitud
{
    [Required(ErrorMessage = "Ingrese su número de suministro.")]
    [RegularExpression(@"^\d{1,20}$", ErrorMessage = "El suministro solo lleva dígitos.")]
    public string Suministro { get; set; } = string.Empty;

    [RegularExpression(@"^(\+?51)?\s*9[\d\s-]{8,10}$", ErrorMessage = "Ingrese un celular de 9 dígitos que empiece con 9.")]
    public string? Celular { get; set; }

    [EmailAddress(ErrorMessage = "Ingrese un correo electrónico válido.")]
    [StringLength(150)]
    public string? Correo { get; set; }
}

/// <summary>Respuesta 201 del registro.</summary>
public sealed record ActualizacionContactoRespuesta(long Id, DateTime FechaRegistroUtc);
