using Gdc.Modules.Plantillas.Infrastructure.Persistence.Configurations;
using Microsoft.EntityFrameworkCore;

namespace Gdc.Modules.Plantillas.Infrastructure.Persistence;

public static class PlantillasModelBuilderExtensions
{
    public static ModelBuilder ApplyPlantillasConfigurations(this ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new PdfTemplateConfiguration());
        modelBuilder.ApplyConfiguration(new PdfTemplateFieldConfiguration());
        modelBuilder.ApplyConfiguration(new DerechoPeticionConfiguration());
        return modelBuilder;
    }
}
