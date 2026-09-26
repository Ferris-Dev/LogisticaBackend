using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi;
using Tracking.Api.Middleware;
using Tracking.Api.Swagger;
using Tracking.Application;
using Tracking.Application.Dtos;
using Tracking.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Railway inyecta PORT; en local 8080 en todas las interfaces para que respondan
// el emulador (10.0.2.2) y un celular físico en la misma red.
var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

builder.Services
    .AddControllers()
    .AddJsonOptions(o =>
    {
        o.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    })
    .ConfigureApiBehaviorOptions(o =>
    {
        // JSON mal formado o cuerpo vacío → mismo formato de 400 que el resto del contrato.
        o.InvalidModelStateResponseFactory = _ => new BadRequestObjectResult(new ProblemDetails
        {
            Status = StatusCodes.Status400BadRequest,
            Title = GlobalExceptionHandler.TituloInvalida,
            Detail = "El cuerpo de la solicitud debe ser un JSON válido"
        })
        { ContentTypes = { "application/problem+json" } };
    });

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddCors(o => o.AddDefaultPolicy(p => p.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));

builder.Services.Configure<ForwardedHeadersOptions>(o =>
{
    // Railway termina TLS en su proxy; se confía en sus cabeceras X-Forwarded-*.
    o.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    o.KnownIPNetworks.Clear();
    o.KnownProxies.Clear();
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(o =>
{
    o.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Tracking Logístico API",
        Version = "v1",
        Description = "Consulta y actualización del estado de paquetes asignados a pilotos."
    });
    foreach (var assembly in new[] { typeof(Program).Assembly, typeof(PaqueteDetalle).Assembly })
    {
        var xml = Path.Combine(AppContext.BaseDirectory, $"{assembly.GetName().Name}.xml");
        if (File.Exists(xml))
            o.IncludeXmlComments(xml);
    }
    o.SupportNonNullableReferenceTypes();
    o.NonNullableReferenceTypesAsRequired();
    o.SchemaFilter<EstadoRequestSchemaFilter>();
});

var app = builder.Build();

app.UseForwardedHeaders();
app.UseExceptionHandler();
app.UseStatusCodePages();
app.UseCors();

if (LeerBool(app.Configuration, "ENABLE_SWAGGER", true))
{
    app.UseSwagger();
    app.UseSwaggerUI(o => o.DocumentTitle = "Tracking Logístico API");
}

app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = (context, report) =>
        context.Response.WriteAsJsonAsync(new { status = report.Status.ToString() })
});
app.MapControllers();

if (LeerBool(app.Configuration, "RUN_MIGRATIONS", true))
    await app.Services.MigrarBaseDeDatosAsync(app.Logger);

await app.RunAsync();

static bool LeerBool(IConfiguration config, string clave, bool porDefecto) =>
    bool.TryParse(config[clave], out var valor) ? valor : porDefecto;

public partial class Program;
