using PdfSharpCore.Pdf;
using PdfSharpCore.Pdf.AcroForms;
using PdfSharpCore.Pdf.IO;

namespace Gdc.Infrastructure.Tests;

/// <summary>
/// Spike Fase 0 #9564 — PdfSharpCore + plantilla vialix-dp-template.pdf.
/// </summary>
public sealed class PdfAcroFormSpikeTests
{
    private static string FixturesDirectory =>
        Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "fixtures", "pdf"));

    private static string VialixTemplatePath =>
        Path.Combine(FixturesDirectory, "vialix-dp-template.pdf");

    private static readonly string[] ExpectedTextFieldNames =
    [
        "tenant_nombre",
        "comparendo_numero",
        "comparendo_placa",
        "comparendo_documento",
        "comparendo_infractor_nombre",
        "comparendo_fecha_comparendo",
        "comparendo_fecha_notificacion",
        "comparendo_total",
        "contraventor_nombre",
        "contraventor_documento",
        "contraventor_correo",
        "secretaria_destino",
    ];

    private const string ChoiceFieldName = "comparendo_estado";

    [Fact]
    public void Spike_PdfSharpCore_creates_and_reopens_blank_document()
    {
        using var created = new PdfDocument();
        created.AddPage();

        using var stream = new MemoryStream();
        created.Save(stream);
        stream.Position = 0;

        using var document = PdfReader.Open(stream, PdfDocumentOpenMode.Import);
        Assert.Equal(1, document.PageCount);
    }

    [Fact]
    public void Spike_vialix_dp_template_fixture_exists()
    {
        Assert.True(File.Exists(VialixTemplatePath), $"Missing fixture: {VialixTemplatePath}");
        Assert.True(new FileInfo(VialixTemplatePath).Length > 1000);
    }

    [Fact]
    public void Spike_vialix_dp_template_lists_expected_acroform_fields()
    {
        var fieldNames = ExtractFieldNames(VialixTemplatePath);

        Assert.Contains(ChoiceFieldName, fieldNames);
        foreach (var expected in ExpectedTextFieldNames)
        {
            Assert.Contains(expected, fieldNames);
        }

        Assert.True(fieldNames.Count >= ExpectedTextFieldNames.Length + 1);
    }

    [Fact]
    public void Spike_vialix_dp_template_fills_all_text_fields()
    {
        var filled = FillTextFields(
            VialixTemplatePath,
            ExpectedTextFieldNames.ToDictionary(n => n, n => $"SPIKE_{n}"));

        Assert.True(filled.Length > 5000);

        using var verifyStream = new MemoryStream(filled);
        using var document = PdfReader.Open(verifyStream, PdfDocumentOpenMode.Import);
        var form = document.AcroForm;
        Assert.NotNull(form?.Fields);

        var numero = (PdfTextField)form.Fields["comparendo_numero"]!;
        Assert.Contains("SPIKE_comparendo_numero", numero.Value?.ToString() ?? string.Empty);
    }

    [Fact]
    public void Spike_extracts_and_fills_acroform_for_all_fixtures()
    {
        var pdfFiles = Directory.GetFiles(FixturesDirectory, "*.pdf", SearchOption.TopDirectoryOnly);
        Assert.NotEmpty(pdfFiles);

        foreach (var path in pdfFiles)
        {
            var fieldNames = ExtractFieldNames(path);
            Assert.NotEmpty(fieldNames);

            var filled = FillTextFields(
                path,
                fieldNames
                    .Where(n => IsTextField(path, n))
                    .ToDictionary(n => n, n => "SPIKE_TEST_VALUE"));

            Assert.True(filled.Length > 0);
        }
    }

    private static bool IsTextField(string pdfPath, string name)
    {
        using var document = PdfReader.Open(pdfPath, PdfDocumentOpenMode.Import);
        var field = document.AcroForm?.Fields[name];
        return field is PdfTextField;
    }

    private static List<string> ExtractFieldNames(string pdfPath)
    {
        using var document = PdfReader.Open(pdfPath, PdfDocumentOpenMode.Import);
        var form = document.AcroForm;
        if (form?.Fields is null)
        {
            return [];
        }

        return form.Fields.Names.ToList();
    }

    private static byte[] FillTextFields(string pdfPath, Dictionary<string, string> values)
    {
        using var document = PdfReader.Open(pdfPath, PdfDocumentOpenMode.Modify);
        var form = document.AcroForm;
        if (form?.Fields is null || !form.Fields.Any())
        {
            using var empty = new MemoryStream();
            document.Save(empty, false);
            return empty.ToArray();
        }

        SetNeedAppearances(form);

        foreach (var (name, value) in values)
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
