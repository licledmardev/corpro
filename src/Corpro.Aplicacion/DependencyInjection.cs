using Corpro.Aplicacion.Cortes;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Corpro.Aplicacion;

public static class DependencyInjection
{
    public static IServiceCollection AddAplicacion(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<CortesOpciones>()
            .Bind(configuration.GetSection(CortesOpciones.Seccion))
            .Validate(o => o.MaximoResultados is > 0 and <= 500, "Cortes:MaximoResultados debe estar entre 1 y 500.")
            .ValidateOnStart();

        services.AddSingleton(TimeProvider.System);
        services.AddScoped<CortesServicio>();
        return services;
    }
}
