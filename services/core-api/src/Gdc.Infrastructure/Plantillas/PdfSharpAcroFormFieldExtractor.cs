using Gdc.Modules.Plantillas.Application;
using Gdc.Modules.Plantillas.Application.Abstractions;
using PdfSharpCore.Pdf.AcroForms;
using PdfSharpCore.Pdf.IO;

namespace Gdc.Infrastructure.Plantillas;

public sealed class PdfSharpAcroFormFieldExtractor : IAcroFormFieldExtractor
{
    public IReadOnlyList<AcroFormFieldDescriptor> ExtractFields(Stream pdfContent)
    {
        using var document = PdfReader.Open(pdfContent, PdfDocumentOpenMode.Import);
        var form = document.AcroForm;
        if (form?.Fields is null || !form.Fields.Any())
        {
            return [];
        }

        var fields = new List<AcroFormFieldDescriptor>();
        foreach (var name in form.Fields.Names)
        {
            var field = form.Fields[name];
            var fieldType = InferFieldType(field);
            fields.Add(new AcroFormFieldDescriptor(name, fieldType));
        }

        return fields;
    }

    private static string InferFieldType(PdfAcroField field) =>
        field switch
        {
            PdfTextField => PlantillaFieldTypes.Text,
            _ when field.GetType().Name.Contains("Check", StringComparison.OrdinalIgnoreCase) =>
                PlantillaFieldTypes.Choice,
            _ => PlantillaFieldTypes.Text,
        };
}
