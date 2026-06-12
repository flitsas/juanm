using Gdc.Modules.Notif.Application.Abstractions;

namespace Gdc.Modules.Reglas.Application.Orchestration;

public static class ReglasTagComposer
{
    public static IReadOnlyDictionary<string, string> BuildComparendoTags(
        DgcComparendoSnapshot? comparendo,
        string ruleName) =>
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["infractor"] = comparendo?.InfractorNombre ?? string.Empty,
            ["numero_comparendo"] = comparendo?.NumeroComparendo ?? string.Empty,
            ["placa"] = comparendo?.Placa ?? string.Empty,
            ["estado"] = comparendo?.Estado ?? string.Empty,
            ["fecha_comparendo"] = comparendo?.FechaComparendo?.ToString("yyyy-MM-dd") ?? string.Empty,
            ["fecha_notificacion"] = comparendo?.FechaNotificacion?.ToString("yyyy-MM-dd") ?? string.Empty,
            ["regla"] = ruleName,
        };

    public static string Merge(string template, IReadOnlyDictionary<string, string> tags)
    {
        var result = template;
        foreach (var (key, value) in tags)
        {
            result = result.Replace($"{{{{{key}}}}}", value, StringComparison.OrdinalIgnoreCase);
        }

        return result;
    }
}
