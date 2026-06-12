namespace Gdc.Modules.Plantillas.Application.Templates;

public sealed record SystemVariableDefinition(
    string Key,
    string Label,
    string Source,
    string DataType);

public static class SystemVariableCatalog
{
    public static IReadOnlyList<SystemVariableDefinition> All { get; } =
        new List<SystemVariableDefinition>
        {
            new("tenant.nombre", "Nombre compañía", "tenant", PlantillaFieldTypes.Text),
            new("comparendo.numero", "Número comparendo", "comparendo", PlantillaFieldTypes.Text),
            new("comparendo.placa", "Placa", "comparendo", PlantillaFieldTypes.Text),
            new("comparendo.documento", "Documento infractor", "comparendo", PlantillaFieldTypes.Text),
            new("comparendo.infractor_nombre", "Nombre infractor", "comparendo", PlantillaFieldTypes.Text),
            new("comparendo.fecha_comparendo", "Fecha comparendo", "comparendo", PlantillaFieldTypes.Text),
            new("comparendo.fecha_notificacion", "Fecha notificación", "comparendo", PlantillaFieldTypes.Text),
            new("comparendo.total", "Valor total", "comparendo", PlantillaFieldTypes.Number),
            new("comparendo.estado", "Estado comparendo", "comparendo", PlantillaFieldTypes.Choice),
            new("contraventor.nombre", "Nombre contraventor", "contraventor", PlantillaFieldTypes.Text),
            new("contraventor.documento", "Documento contraventor", "contraventor", PlantillaFieldTypes.Text),
            new("contraventor.correo", "Correo contraventor", "contraventor", PlantillaFieldTypes.Text),
            new("derecho_peticion.secretaria_destino", "Secretaría destino", "derecho_peticion", PlantillaFieldTypes.Text),
        };

    public static bool IsKnown(string? key) =>
        !string.IsNullOrWhiteSpace(key) && All.Any(v => v.Key == key);

    public static SystemVariableDefinition? Find(string key) =>
        All.FirstOrDefault(v => v.Key == key);
}
