using Corpro.Aplicacion.Abstracciones;
using Corpro.Infraestructura.Consultas;
using Corpro.Infraestructura.Persistencia;
using Corpro.Infraestructura.Persistencia.Contexto;
using Corpro.Infraestructura.Repositorios;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Corpro.Infraestructura;

public static class DependencyInjection
{
    public static IServiceCollection AddInfraestructura(this IServiceCollection services, IConfiguration configuration)
    {
        var opciones = configuration.GetSection(BaseDatosOpciones.Seccion).Get<BaseDatosOpciones>() ?? new();

        // "BaseDatos:ConexionActiva" elige qué entrada de ConnectionStrings usar (Local, Neon, ...).
        var cadena = configuration.GetConnectionString(opciones.ConexionActiva);
        if (string.IsNullOrWhiteSpace(cadena))
            throw new InvalidOperationException(
                $"Falta ConnectionStrings:{opciones.ConexionActiva}. Configúrela en appsettings, user-secrets o la variable de entorno ConnectionStrings__{opciones.ConexionActiva}.");

        // Pool de DbContext: reutiliza instancias y reduce asignaciones bajo carga.
        services.AddDbContextPool<CorproDbContext>(o =>
            o.UseNpgsql(cadena, npgsql =>
            {
                // Neon suspende el cómputo inactivo: los reintentos absorben el arranque en frío.
                if (opciones.ReintentosMaximos > 0)
                    npgsql.EnableRetryOnFailure(opciones.ReintentosMaximos);
                npgsql.CommandTimeout(opciones.TiempoEsperaComandoSegundos);
            }));

        services.AddScoped<IConsultaCortes, ConsultaCortes>();
        services.AddScoped<IActualizacionContactoRepositorio, ActualizacionContactoRepositorio>();
        return services;
    }
}
