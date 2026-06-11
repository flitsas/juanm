using System.Text.Json;
using Gdc.Infrastructure.Auth;

namespace Gdc.Infrastructure.Tests;

/// <summary>
/// Uso de ejemplo: validar Auth:Invitation en appsettings.Development.json.
/// </summary>
public class InvitationSettingsTests
{
    [Fact]
    public void Development_appsettings_has_invitation_config_for_frontend_activate()
    {
        var devSettingsPath = Path.Combine(
            FindRepoRoot(),
            "services",
            "core-api",
            "src",
            "Gdc.Api",
            "appsettings.Development.json");
        Assert.True(File.Exists(devSettingsPath), $"No se encontró {devSettingsPath}");

        using var document = JsonDocument.Parse(File.ReadAllText(devSettingsPath));
        var invitation = document.RootElement.GetProperty("Auth").GetProperty("Invitation");

        Assert.Equal(48, invitation.GetProperty("ActivationTokenHours").GetInt32());
        Assert.Equal(
            "http://localhost:40103/activate",
            invitation.GetProperty("ActivationBaseUrl").GetString());
    }

    private static string FindRepoRoot()
    {
        var directory = new DirectoryInfo(Directory.GetCurrentDirectory());
        while (directory is not null)
        {
            var candidate = Path.Combine(directory.FullName, "services", "core-api");
            if (Directory.Exists(candidate))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        throw new InvalidOperationException("No se pudo resolver la raíz del repositorio.");
    }
}
