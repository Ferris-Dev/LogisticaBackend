using Microsoft.AspNetCore.Mvc;
using Tracking.Application.Dtos;
using Tracking.Application.Services;

namespace Tracking.Api.Controllers;

[ApiController]
[Route("api/tracking")]
public class TrackingController(ITrackingService trackingService) : ControllerBase
{
    /// <summary>Consulta el detalle de un paquete por su guía.</summary>
    /// <remarks>
    /// La guía se normaliza a mayúsculas (gua-10001 → GUA-10001).
    ///
    ///     GET /api/tracking/GUA-10001
    /// </remarks>
    /// <param name="guia">Formato GUA-00000.</param>
    /// <param name="ct"></param>
    /// <response code="200">Detalle del paquete.</response>
    /// <response code="400">La guía no cumple el formato GUA-00000.</response>
    /// <response code="404">La guía no existe.</response>
    [HttpGet("{guia}")]
    [ProducesResponseType<PaqueteDetalle>(StatusCodes.Status200OK, "application/json")]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")]
    public async Task<ActionResult<PaqueteDetalle>> Obtener(string guia, CancellationToken ct) =>
        Ok(await trackingService.ObtenerAsync(guia, ct));

    /// <summary>Cambia el estado de un paquete (last-write-wins).</summary>
    /// <remarks>
    /// Si fechaCambio es anterior a ultimaActualizacion, el cambio se ignora y se devuelve 200
    /// con el estado vigente: la app offline puede reenviar cambios viejos sin pisar los nuevos.
    /// Cada cambio efectivo queda registrado en historial_estados.
    ///
    ///     PUT /api/tracking/GUA-10001/estado
    ///     { "estado": "ENTREGADO", "fechaCambio": "2026-09-26T16:00:00Z" }
    /// </remarks>
    /// <param name="guia">Formato GUA-00000.</param>
    /// <param name="request">Nuevo estado y momento del cambio.</param>
    /// <param name="ct"></param>
    /// <response code="200">Estado vigente del paquete (aplicado o no).</response>
    /// <response code="400">Guía con formato inválido, estado no permitido, fecha inválida o JSON mal formado.</response>
    /// <response code="404">La guía no existe.</response>
    [HttpPut("{guia}/estado")]
    [Consumes("application/json")]
    [ProducesResponseType<PaqueteDetalle>(StatusCodes.Status200OK, "application/json")]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")]
    public async Task<ActionResult<PaqueteDetalle>> CambiarEstado(string guia, CambioEstadoRequest request, CancellationToken ct) =>
        Ok(await trackingService.CambiarEstadoAsync(guia, request, ct));
}
