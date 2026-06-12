namespace Gdc.Modules.Plantillas.Application.Abstractions;

public interface IPdfCompiler
{
    byte[] FillAcroFormFields(Stream pdfContent, IReadOnlyDictionary<string, string> fieldValues);
}
