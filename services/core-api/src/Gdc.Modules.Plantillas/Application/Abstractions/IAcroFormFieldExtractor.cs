namespace Gdc.Modules.Plantillas.Application.Abstractions;

public sealed record AcroFormFieldDescriptor(string AcroformName, string FieldType);

public interface IAcroFormFieldExtractor
{
    IReadOnlyList<AcroFormFieldDescriptor> ExtractFields(Stream pdfContent);
}
