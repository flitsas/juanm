namespace Gdc.Modules.Plantillas.Application.Templates;

/// <summary>
/// Heurísticas de tipado y opciones para tags AcroForm conocidos del spike vialix-dp-template.
/// </summary>
public static class PlantillaAcroformDefaults
{
    private static readonly Dictionary<string, string> FieldTypeOverrides =
        new()
        {
            ["comparendo_total"] = PlantillaFieldTypes.Number,
            ["comparendo_estado"] = PlantillaFieldTypes.Choice,
        };

    private static readonly Dictionary<string, IReadOnlyList<string>> ChoiceOptionsByAcroform =
        new()
        {
            ["comparendo_estado"] = ["Pendiente", "Notificado", "En trámite", "Pagado", "Cerrado"],
        };

    public static string ResolveFieldType(string acroformName, string detectedType) =>
        FieldTypeOverrides.TryGetValue(acroformName, out var overrideType)
            ? overrideType
            : detectedType;

    public static IReadOnlyList<string>? GetChoiceOptions(string acroformName) =>
        ChoiceOptionsByAcroform.TryGetValue(acroformName, out var options) ? options : null;
}
