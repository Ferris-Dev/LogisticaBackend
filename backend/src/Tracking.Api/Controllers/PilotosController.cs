using Microsoft.AspNetCore.Mvc;
using Tracking.Application.Dtos;
using Tracking.Application.Services;

namespace Tracking.Api.Controllers;

[ApiController]
[Route("api/pilotos")]
public class PilotosController(IPilotoService pilotoService) : ControllerBase
{
    /// <summary>Lista los paquetes asignados a un piloto, ordenados por guía.</summary>
    /// <remarks>
    /// Devuelve 200 con [] si el piloto existe pero no tiene paquetes.
    ///
    ///     GET /api/pilotos/1/paquetes
    /// </remarks>
    /// <param name="pilotoId">Id numérico del piloto (1 = PIL-001, 2 = PIL-002).</param>
    /// <param name="ct"></param>
    /// <response code="200">Paquetes del piloto.</response>
    /// <response code="404">El piloto no existe.</response>
    [HttpGet("{pilotoId:int}/paquetes")]
    [ProducesResponseType<IReadOnlyList<PaqueteResumen>>(StatusCodes.Status200OK, "application/json")]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")]
    public async Task<ActionResult<IReadOnlyList<PaqueteResumen>>> ListarPaquetes(int pilotoId, CancellationToken ct) =>
        Ok(await pilotoService.ListarPaquetesAsync(pilotoId, ct));
}
