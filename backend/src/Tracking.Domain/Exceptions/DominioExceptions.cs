namespace Tracking.Domain.Exceptions;

public abstract class DominioException(string message) : Exception(message);

public sealed class GuiaNoEncontradaException(string guia)
    : DominioException($"La guía {guia} no existe")
{
    public string Guia { get; } = guia;
}

public sealed class PilotoNoEncontradoException(int pilotoId)
    : DominioException($"El piloto {pilotoId} no existe")
{
    public int PilotoId { get; } = pilotoId;
}

public sealed class FormatoGuiaInvalidoException(string? valor)
    : DominioException("El formato de guía debe ser GUA-00000")
{
    public string? Valor { get; } = valor;
}
