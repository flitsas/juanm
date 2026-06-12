namespace Gdc.Modules.Plantillas.Application;

public static class DerechoPeticionEstadoTransitions
{
    private static readonly Dictionary<string, string> NextStates = new(StringComparer.Ordinal)
    {
        [DerechoPeticionEstados.NoEnviado] = DerechoPeticionEstados.Enviado,
        [DerechoPeticionEstados.Enviado] = DerechoPeticionEstados.SinRespuesta,
        [DerechoPeticionEstados.SinRespuesta] = DerechoPeticionEstados.ConRespuesta,
    };

    public static bool IsValidState(string estado) =>
        NextStates.ContainsKey(estado) || estado == DerechoPeticionEstados.ConRespuesta;

    public static bool CanTransition(string currentEstado, string targetEstado) =>
        NextStates.TryGetValue(currentEstado, out var allowed) && allowed == targetEstado;
}
