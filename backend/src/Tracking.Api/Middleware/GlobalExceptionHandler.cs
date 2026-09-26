using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Tracking.Application.Exceptions;
using Tracking.Domain.Exceptions;

namespace Tracking.Api.Middleware;

/// <summary>
/// Convierte cualquier excepción en ProblemDetails (application/problem+json).
/// Nunca expone stack traces: los errores no controlados se registran en el log y
/// el cliente solo recibe un 500 genérico.
/// </summary>
public class GlobalExceptionHandler(IProblemDetailsService problemDetails, ILogger<GlobalExceptionHandler> logger)
    : IExceptionHandler
{
    public const string TituloInvalida = "Solicitud inválida";

    public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception exception, CancellationToken ct)
    {
        ProblemDetails problema = exception switch
        {
            GuiaNoEncontradaException e => Crear(StatusCodes.Status404NotFound, "Guía no encontrada", e.Message),
            PilotoNoEncontradoException e => Crear(StatusCodes.Status404NotFound, "Piloto no encontrado", e.Message),
            FormatoGuiaInvalidoException e => Crear(StatusCodes.Status400BadRequest, TituloInvalida, e.Message),
            ValidacionException e => new HttpValidationProblemDetails(e.Errores)
            {
                Status = StatusCodes.Status400BadRequest,
                Title = TituloInvalida,
                Detail = e.Message
            },
            BadHttpRequestException e => Crear(e.StatusCode, TituloInvalida, "La solicitud no es válida"),
            _ => Crear(StatusCodes.Status500InternalServerError, "Error interno", "Ocurrió un error inesperado")
        };

        if (problema.Status >= 500)
            logger.LogError(exception, "Error no controlado en {Metodo} {Ruta}", context.Request.Method, context.Request.Path);

        context.Response.StatusCode = problema.Status!.Value;
        return await problemDetails.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = context,
            ProblemDetails = problema,
            Exception = exception
        });
    }

    private static ProblemDetails Crear(int status, string titulo, string detalle) =>
        new() { Status = status, Title = titulo, Detail = detalle };
}
