namespace Gdc.Infrastructure.Dgc;

public sealed class ContraventorWindowEvaluator
{
    // Servicio registrado e inyectado por DI (singleton); el metodo es de instancia por
    // diseno (mockeable/consistente con el resto de servicios), aunque no use estado.
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Performance", "CA1822:Mark members as static",
        Justification = "Servicio inyectable por DI; la instancia es intencional.")]
    public bool IsWindowActive(string cronExpression, DateTimeOffset now)
    {
        if (!TryParseDailyWindow(cronExpression, out var hour, out var minute))
        {
            return false;
        }

        return now.Hour == hour && now.Minute == minute;
    }

    internal static bool TryParseDailyWindow(string cronExpression, out int hour, out int minute)
    {
        hour = 0;
        minute = 0;

        var parts = cronExpression.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length < 2)
        {
            return false;
        }

        if (!int.TryParse(parts[0], out minute) || minute is < 0 or > 59)
        {
            return false;
        }

        if (!int.TryParse(parts[1], out hour) || hour is < 0 or > 23)
        {
            return false;
        }

        return parts.Length < 3 || parts[2] == "*";
    }
}
