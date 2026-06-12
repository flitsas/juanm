using System.Text.Json;

namespace Gdc.Modules.Plantillas.Application.Templates;

public static class PlantillaFieldMappingValidator
{
    public static void ValidateMapping(
        string fieldType,
        string systemVariable,
        IReadOnlyList<string>? choiceOptions,
        out string? choiceOptionsJson)
    {
        choiceOptionsJson = null;

        if (!PlantillaFieldTypes.IsValid(fieldType))
        {
            throw new ArgumentException($"Invalid field type '{fieldType}'.");
        }

        if (!SystemVariableCatalog.IsKnown(systemVariable))
        {
            throw new ArgumentException($"Unknown system variable '{systemVariable}'.");
        }

        var definition = SystemVariableCatalog.Find(systemVariable)!;
        if (!IsCompatibleType(fieldType, definition.DataType))
        {
            throw new ArgumentException(
                $"Field type '{fieldType}' is not compatible with system variable '{systemVariable}' ({definition.DataType}).");
        }

        if (fieldType == PlantillaFieldTypes.Choice)
        {
            if (choiceOptions is null || choiceOptions.Count == 0)
            {
                throw new ArgumentException("Choice fields require at least one option.");
            }

            choiceOptionsJson = JsonSerializer.Serialize(choiceOptions);
        }
        else if (choiceOptions is { Count: > 0 })
        {
            throw new ArgumentException("Choice options are only allowed for choice field type.");
        }
    }

    private static bool IsCompatibleType(string fieldType, string variableDataType) =>
        fieldType == variableDataType
        || (fieldType == PlantillaFieldTypes.Text && variableDataType == PlantillaFieldTypes.Text);
}
