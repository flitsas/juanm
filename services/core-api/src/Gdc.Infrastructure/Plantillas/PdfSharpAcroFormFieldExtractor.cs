using Gdc.Modules.Plantillas.Application;
using Gdc.Modules.Plantillas.Application.Abstractions;
using Gdc.Modules.Plantillas.Application.Templates;
using PdfSharpCore.Pdf;
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
            var detectedType = InferFieldType(field);
            var fieldType = PlantillaAcroformDefaults.ResolveFieldType(name, detectedType);
            fields.Add(new AcroFormFieldDescriptor(name, fieldType));
        }

        return fields;
    }

    private static string InferFieldType(PdfAcroField field)
    {
        if (IsChoiceField(field))
        {
            return PlantillaFieldTypes.Choice;
        }

        return field switch
        {
            PdfTextField => PlantillaFieldTypes.Text,
            _ when field.GetType().Name.Contains("Check", StringComparison.OrdinalIgnoreCase) =>
                PlantillaFieldTypes.Choice,
            _ => PlantillaFieldTypes.Text,
        };
    }

    private static bool IsChoiceField(PdfAcroField field)
    {
        if (!field.Elements.ContainsKey("/FT"))
        {
            return false;
        }

        var fieldType = field.Elements["/FT"].ToString();
        return fieldType is "/Ch" or "Ch";
    }
}
