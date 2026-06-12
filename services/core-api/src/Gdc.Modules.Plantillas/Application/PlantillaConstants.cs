namespace Gdc.Modules.Plantillas.Application;

public static class PlantillaFieldTypes
{
    public const string Text = "text";

    public const string Number = "number";

    public const string Choice = "choice";

    public static bool IsValid(string? value) =>
        value is Text or Number or Choice;
}

public static class DerechoPeticionEstados
{
    public const string NoEnviado = "NoEnviado";

    public const string Enviado = "Enviado";

    public const string SinRespuesta = "SinRespuesta";

    public const string ConRespuesta = "ConRespuesta";
}
