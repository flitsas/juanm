namespace Gdc.Infrastructure.Dgc.Renting;

public sealed class DgcVehicleRegistryOptions
{
    public const string SectionName = "Dgc:VehicleRegistry";

    /// <summary>Renting (prod API) o Stub (tests sin red).</summary>
    public string Provider { get; set; } = "Renting";
}
