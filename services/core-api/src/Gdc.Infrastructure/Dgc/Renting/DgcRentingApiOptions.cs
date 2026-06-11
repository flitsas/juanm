namespace Gdc.Infrastructure.Dgc.Renting;

/// <summary>
/// Paridad con FLIT 1 <c>rentingApiConfig.ts</c> / variables RENTING_API_*.
/// Secretos vía env: Dgc__RentingApi__ApiKeyValue, Passphrase, LoginSecretName, LoginSubject.
/// </summary>
public sealed class DgcRentingApiOptions
{
    public const string SectionName = "Dgc:RentingApi";

    public string BaseUrl { get; set; } = "https://rcprod-apimanageusados.azure-api.net/";

    public string ApiKeyHeaderName { get; set; } = "Ocp-Apim-Subscription-Key";

    public string ApiKeyValue { get; set; } = string.Empty;

    /// <summary>Ruta relativa al repo (infra/secrets/...) o absoluta.</summary>
    public string CertificatePath { get; set; } = "infra/secrets/renting.rc.prod.pfx";

    public string CertificatePassphrase { get; set; } = string.Empty;

    public int TimeoutSeconds { get; set; } = 15;

    public string LoginPath { get; set; } = "Authorization/TokenForThird";

    public int LoginTimeoutSeconds { get; set; } = 10;

    public int LoginCacheSecondsTtl { get; set; } = 600;

    public string LoginSecretName { get; set; } = string.Empty;

    public string LoginSubject { get; set; } = string.Empty;

    public string ContractInfoByPlatePath { get; set; } =
        "RentingMaster/ContractPlate/GetContractPlatesInfoByPlate";

    public int ContractInfoTimeoutSeconds { get; set; } = 20;
}
