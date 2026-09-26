using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace Tracking.Tests.Api;

/// <summary>Verifica la forma de las respuestas HTTP que consume la app Android.</summary>
public sealed class ContratoApiTests : IDisposable
{
    private readonly ApiFactory _factory = new();
    private readonly HttpClient _client;

    public ContratoApiTests() => _client = _factory.CreateClient();

    public void Dispose() => _factory.Dispose();

    [Fact]
    public async Task Detalle_UsaCamelCaseEnumsComoStringYFechasUtc()
    {
        var respuesta = await _client.GetAsync("/api/tracking/GUA-10001");

        Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);
        using var json = JsonDocument.Parse(await respuesta.Content.ReadAsStringAsync());
        var raiz = json.RootElement;
        Assert.Equal("GUA-10001", raiz.GetProperty("guia").GetString());
        Assert.Equal("PENDIENTE", raiz.GetProperty("estado").GetString());
        Assert.Equal("2026-09-26T14:00:00Z", raiz.GetProperty("fechaAsignacion").GetString());
        Assert.Equal(JsonValueKind.Null, raiz.GetProperty("observaciones").ValueKind);
        Assert.Equal(1, raiz.GetProperty("pilotoId").GetInt32());
    }

    [Fact]
    public async Task Listado_DevuelveResumenes()
    {
        var lista = await _client.GetFromJsonAsync<JsonElement>("/api/pilotos/2/paquetes");

        Assert.Equal(4, lista.GetArrayLength());
        Assert.Equal(["guia", "cliente", "zona", "estado"],
            lista[0].EnumerateObject().Select(p => p.Name));
    }

    [Theory]
    [InlineData("/api/tracking/GUA-99999", 404, "Guía no encontrada", "La guía GUA-99999 no existe")]
    [InlineData("/api/tracking/ABC", 400, "Solicitud inválida", "El formato de guía debe ser GUA-00000")]
    [InlineData("/api/pilotos/99/paquetes", 404, "Piloto no encontrado", "El piloto 99 no existe")]
    public async Task Errores_SonProblemDetails(string url, int status, string titulo, string detalle)
    {
        var respuesta = await _client.GetAsync(url);

        await AssertProblem(respuesta, status, titulo, detalle);
    }

    [Fact]
    public async Task CambiarEstado_EstadoInvalido_Devuelve400ConErrores()
    {
        var respuesta = await _client.PutAsJsonAsync("/api/tracking/GUA-10001/estado", new { estado = "PERDIDO" });

        var raiz = await AssertProblem(respuesta, 400, "Solicitud inválida");
        Assert.Equal("Valor no permitido", raiz.GetProperty("errors").GetProperty("estado")[0].GetString());
    }

    [Fact]
    public async Task CambiarEstado_JsonMalFormado_Devuelve400ProblemDetails()
    {
        var contenido = new StringContent("{estado", System.Text.Encoding.UTF8, "application/json");

        var respuesta = await _client.PutAsync("/api/tracking/GUA-10001/estado", contenido);

        await AssertProblem(respuesta, 400, "Solicitud inválida");
    }

    [Fact]
    public async Task CambiarEstado_Valido_Devuelve200ConDetalleActualizado()
    {
        var respuesta = await _client.PutAsJsonAsync("/api/tracking/GUA-10001/estado",
            new { estado = "ENTREGADO", fechaCambio = "2026-09-26T16:00:00Z" });

        Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);
        var raiz = await respuesta.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("ENTREGADO", raiz.GetProperty("estado").GetString());
        Assert.Equal("2026-09-26T16:00:00Z", raiz.GetProperty("ultimaActualizacion").GetString());
    }

    private static async Task<JsonElement> AssertProblem(HttpResponseMessage respuesta, int status, string titulo,
        string? detalle = null)
    {
        Assert.Equal(status, (int)respuesta.StatusCode);
        Assert.Equal("application/problem+json", respuesta.Content.Headers.ContentType?.MediaType);

        var raiz = await respuesta.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(titulo, raiz.GetProperty("title").GetString());
        Assert.Equal(status, raiz.GetProperty("status").GetInt32());
        if (detalle is not null)
            Assert.Equal(detalle, raiz.GetProperty("detail").GetString());
        return raiz;
    }
}
