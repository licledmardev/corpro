using System.Threading.RateLimiting;
using Corpro.Api;
using Corpro.Api.Endpoints;
using Corpro.Aplicacion;
using Corpro.Infraestructura;
using Microsoft.AspNetCore.HttpOverrides;

var builder = WebApplication.CreateBuilder(args);

// ---------- Capas (inyección de dependencias) ----------
builder.Services.AddAplicacion(builder.Configuration);
builder.Services.AddInfraestructura(builder.Configuration);

builder.Services.AddProblemDetails();

// Los avisos vigentes son iguales para todos: se guardan 2 min en memoria y no golpean la BD.
builder.Services.AddOutputCache(o =>
    o.AddPolicy(Politicas.CacheAvisos, p => p.Expire(TimeSpan.FromMinutes(2))));
builder.Services.AddOpenApi();

// La API solo recibe JSON pequeño: se limita el cuerpo a 16 KB.
builder.WebHost.ConfigureKestrel(k => k.Limits.MaxRequestBodySize = 16 * 1024);

// ---------- CORS: solo el dominio de GitHub Pages (y localhost en desarrollo) ----------
var origenes = builder.Configuration.GetSection("Cors:OrigenesPermitidos").Get<string[]>() ?? [];
builder.Services.AddCors(o => o.AddPolicy(Politicas.CorsFrontend, p => p
    .WithOrigins(origenes)
    .WithMethods("GET", "POST")
    .WithHeaders("Content-Type")
    .SetPreflightMaxAge(TimeSpan.FromHours(1))));

// ---------- Límite de peticiones por IP ----------
builder.Services.AddRateLimiter(o =>
{
    o.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    o.AddPolicy(Politicas.LimiteConsulta, ctx => RateLimitPartition.GetFixedWindowLimiter(
        ctx.Connection.RemoteIpAddress?.ToString() ?? "anonimo",
        _ => new FixedWindowRateLimiterOptions { PermitLimit = 60, Window = TimeSpan.FromMinutes(1) }));
    o.AddPolicy(Politicas.LimiteRegistro, ctx => RateLimitPartition.GetFixedWindowLimiter(
        ctx.Connection.RemoteIpAddress?.ToString() ?? "anonimo",
        _ => new FixedWindowRateLimiterOptions { PermitLimit = 5, Window = TimeSpan.FromMinutes(10) }));
});

// Detrás de un proxy (Render, Azure, Fly...) la IP real llega en X-Forwarded-For.
// Activar solo si el hosting lo garantiza; si no, el encabezado podría falsificarse.
var confiarEnProxy = builder.Configuration.GetValue<bool>("Proxy:ConfiarEnEncabezadosReenviados");
if (confiarEnProxy)
{
    builder.Services.Configure<ForwardedHeadersOptions>(o =>
    {
        o.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
        o.KnownIPNetworks.Clear();
        o.KnownProxies.Clear();
    });
}

var app = builder.Build();

if (confiarEnProxy)
    app.UseForwardedHeaders();

app.UseExceptionHandler();
app.UseStatusCodePages();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}
else
{
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseCors();
app.UseRateLimiter();
app.UseOutputCache();

app.MapGet("/salud", () => TypedResults.Ok(new { estado = "ok" })).ExcludeFromDescription();
app.MapCortesEndpoints();

app.Run();
