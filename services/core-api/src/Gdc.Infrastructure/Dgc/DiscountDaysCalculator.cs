using Gdc.Modules.Dgc.Domain.Entities;

namespace Gdc.Infrastructure.Dgc;

public static class DiscountDaysCalculator
{
    public static int? Calculate(
        DateOnly? fechaNotificacion,
        IReadOnlyList<DescuentoMatriz> matrices,
        Guid? secretariaId,
        DateOnly? referenceDate = null)
    {
        if (fechaNotificacion is null)
        {
            return null;
        }

        var matrix = ResolveMatrix(matrices, secretariaId);
        if (matrix is null)
        {
            return null;
        }

        var today = referenceDate ?? DateOnly.FromDateTime(DateTime.UtcNow);
        var elapsed = today.DayNumber - fechaNotificacion.Value.DayNumber;
        return Math.Max(0, matrix.DiasDescuento - elapsed);
    }

    public static DescuentoMatriz? ResolveMatrix(
        IReadOnlyList<DescuentoMatriz> matrices,
        Guid? secretariaId)
    {
        if (secretariaId is not null)
        {
            var bySecretaria = matrices.FirstOrDefault(m =>
                m.IsActive && m.SecretariaId == secretariaId);
            if (bySecretaria is not null)
            {
                return bySecretaria;
            }
        }

        return matrices.FirstOrDefault(m => m.IsActive && m.SecretariaId is null);
    }
}
