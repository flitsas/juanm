using Gdc.Modules.Dgc.Infrastructure.Persistence.Configurations;
using Microsoft.EntityFrameworkCore;

namespace Gdc.Modules.Dgc.Infrastructure.Persistence;

public static class DgcModelBuilderExtensions
{
    public static ModelBuilder ApplyDgcConfigurations(this ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("dgc");
        modelBuilder.ApplyConfiguration(new ComparendoConfiguration());
        modelBuilder.ApplyConfiguration(new ContraventorConfiguration());
        modelBuilder.ApplyConfiguration(new OcrLoteConfiguration());
        modelBuilder.ApplyConfiguration(new OcrItemConfiguration());
        modelBuilder.ApplyConfiguration(new EmailLogConfiguration());
        modelBuilder.ApplyConfiguration(new ContraventorJobConfigConfiguration());
        modelBuilder.ApplyConfiguration(new DescuentoMatrizConfiguration());
        return modelBuilder;
    }
}
