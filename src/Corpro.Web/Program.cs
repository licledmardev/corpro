using Corpro.Web;
using Corpro.Web.Servicios;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// La cultura es-PE ("domingo, 25 de octubre de 2026") se fija en wwwroot/index.html
// con Blazor.start({ applicationCulture }), que descarga solo los datos de ese idioma.

// URL pública de la API (wwwroot/appsettings.json). No es un secreto: el sitio es estático.
var apiBaseUrl = builder.Configuration["ApiBaseUrl"]
    ?? throw new InvalidOperationException("Falta 'ApiBaseUrl' en wwwroot/appsettings.json.");

builder.Services.AddSingleton(PortalOpciones.Desde(builder.Configuration));

builder.Services.AddScoped(_ => new CortesApiCliente(new HttpClient
{
    BaseAddress = new Uri(apiBaseUrl.TrimEnd('/') + "/"),
    // El plan gratuito de Render duerme la API tras 15 min sin uso; despertar toma hasta ~1 min.
    Timeout = TimeSpan.FromSeconds(90),
}));

await builder.Build().RunAsync();
