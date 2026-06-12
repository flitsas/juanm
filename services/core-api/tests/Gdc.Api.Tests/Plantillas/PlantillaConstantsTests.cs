using Gdc.Modules.Plantillas.Application;

namespace Gdc.Api.Tests.Plantillas;

public sealed class PlantillaConstantsTests
{
    [Fact]
    public void DerechoPeticionEstados_defines_rf05_values()
    {
        var estados = new[]
        {
            DerechoPeticionEstados.NoEnviado,
            DerechoPeticionEstados.Enviado,
            DerechoPeticionEstados.SinRespuesta,
            DerechoPeticionEstados.ConRespuesta,
        };

        Assert.Equal(4, estados.Distinct().Count());
        Assert.Contains("NoEnviado", estados);
        Assert.Contains("ConRespuesta", estados);
    }
}
