using System.Text.Json.Nodes;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;
using Tracking.Application.Dtos;
using Tracking.Domain.Entities;

namespace Tracking.Api.Swagger;

/// <summary>
/// CambioEstadoRequest.estado es string (para validar con mensajes propios),
/// pero en Swagger se documenta con los valores permitidos del enum.
/// </summary>
public class EstadoRequestSchemaFilter : ISchemaFilter
{
    public void Apply(IOpenApiSchema schema, SchemaFilterContext context)
    {
        if (context.Type != typeof(CambioEstadoRequest)
            || schema.Properties?.TryGetValue("estado", out var estado) != true
            || estado is not OpenApiSchema estadoSchema)
            return;

        estadoSchema.Enum = Enum.GetNames<EstadoPaquete>().Select(n => (JsonNode)JsonValue.Create(n)).ToList();
    }
}
