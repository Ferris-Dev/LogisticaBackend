using System.Text.RegularExpressions;
using Tracking.Domain.Exceptions;

namespace Tracking.Domain;

public static partial class Guia
{
    [GeneratedRegex(@"^GUA-\d{5}$")]
    private static partial Regex Formato();

    /// <summary>
    /// Normaliza la guía (trim + mayúsculas) y valida el formato GUA-00000.
    /// </summary>
    /// <exception cref="FormatoGuiaInvalidoException">Si no cumple el formato.</exception>
    public static string Normalizar(string? valor)
    {
        var guia = (valor ?? string.Empty).Trim().ToUpperInvariant();
        return Formato().IsMatch(guia) ? guia : throw new FormatoGuiaInvalidoException(valor);
    }
}
