using System.Globalization;
using Gdc.Modules.Plantillas.Application.Abstractions;

namespace Gdc.Modules.Plantillas.Application.Templates;

public static class ComparendoVariableValuesBuilder
{
    public static IReadOnlyDictionary<string, string> Build(ComparendoCompilationSnapshot snapshot)
    {
        var values = new Dictionary<string, string>
        {
            ["tenant.nombre"] = snapshot.TenantNombre,
            ["comparendo.numero"] = snapshot.NumeroComparendo,
            ["comparendo.placa"] = snapshot.Placa ?? string.Empty,
            ["comparendo.documento"] = snapshot.Documento ?? string.Empty,
            ["comparendo.infractor_nombre"] = snapshot.InfractorNombre ?? string.Empty,
            ["comparendo.fecha_comparendo"] = FormatDate(snapshot.FechaComparendo),
            ["comparendo.fecha_notificacion"] = FormatDate(snapshot.FechaNotificacion),
            ["comparendo.total"] = snapshot.TotalValor.ToString(CultureInfo.InvariantCulture),
            ["comparendo.estado"] = snapshot.Estado,
            ["derecho_peticion.secretaria_destino"] = snapshot.SecretariaDestino ?? string.Empty,
        };

        if (snapshot.Contraventor is not null)
        {
            values["contraventor.nombre"] = snapshot.Contraventor.Nombre;
            values["contraventor.documento"] = snapshot.Contraventor.Documento;
            values["contraventor.correo"] = snapshot.Contraventor.Correo ?? string.Empty;
        }

        return values;
    }

    private static string FormatDate(DateOnly? date) =>
        date?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) ?? string.Empty;
}
