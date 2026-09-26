namespace Tracking.Application.Exceptions;

/// <summary>Uno o más campos de la solicitud no son válidos (se responde 400 con "errors").</summary>
public sealed class ValidacionException(IDictionary<string, string[]> errores)
    : Exception("Uno o más campos no son válidos")
{
    public IDictionary<string, string[]> Errores { get; } = errores;
}
