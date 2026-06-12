using Gdc.Modules.Plantillas.Application.Abstractions;
using PdfSharpCore.Pdf;
using PdfSharpCore.Pdf.AcroForms;
using PdfSharpCore.Pdf.IO;

namespace Gdc.Infrastructure.Plantillas;

public sealed class PdfSharpPdfCompiler : IPdfCompiler
{
    public byte[] FillAcroFormFields(Stream pdfContent, IReadOnlyDictionary<string, string> fieldValues)
    {
        using var document = PdfReader.Open(pdfContent, PdfDocumentOpenMode.Modify);
        var form = document.AcroForm;
        if (form?.Fields is null || !form.Fields.Any())
        {
            using var empty = new MemoryStream();
            document.Save(empty, false);
            return empty.ToArray();
        }

        SetNeedAppearances(form);

        foreach (var (name, value) in fieldValues)
        {
            if (form.Fields[name] is PdfTextField textField)
            {
                textField.Value = new PdfString(value);
            }
        }

        using var output = new MemoryStream();
        document.Save(output, false);
        return output.ToArray();
    }

    private static void SetNeedAppearances(PdfAcroForm form)
    {
        if (form.Elements.ContainsKey("/NeedAppearances"))
        {
            form.Elements["/NeedAppearances"] = new PdfBoolean(true);
        }
        else
        {
            form.Elements.Add("/NeedAppearances", new PdfBoolean(true));
        }
    }
}
