namespace Gdc.Modules.Plantillas.Application.Templates;

public sealed record SystemVariableDto(string Key, string Label, string Source, string DataType);

public sealed record SystemVariableListResponse(IReadOnlyList<SystemVariableDto> Items);

public sealed record UpdateFieldMappingRequest(
    Guid FieldId,
    string FieldType,
    string SystemVariable,
    IReadOnlyList<string>? ChoiceOptions);

public sealed record UpdateFieldMappingsRequest(IReadOnlyList<UpdateFieldMappingRequest> Fields);

public sealed record ActivateTemplateResponse(Guid Id, bool IsActive, int MappedFieldCount);
