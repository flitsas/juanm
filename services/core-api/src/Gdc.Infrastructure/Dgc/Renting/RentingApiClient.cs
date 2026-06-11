using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Gdc.Infrastructure.Dgc.Renting;

public sealed class RentingApiClient
{
    private const string TokenCacheKey = "dgc:renting_api:token";
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    private readonly HttpClient _httpClient;
    private readonly IMemoryCache _cache;
    private readonly DgcRentingApiOptions _options;
    private readonly ILogger<RentingApiClient> _logger;

    public RentingApiClient(
        HttpClient httpClient,
        IMemoryCache cache,
        IOptions<DgcRentingApiOptions> options,
        ILogger<RentingApiClient> logger)
    {
        _httpClient = httpClient;
        _cache = cache;
        _options = options.Value;
        _logger = logger;
    }

    public static HttpMessageHandler CreateHandler(
        IOptions<DgcRentingApiOptions> options,
        IHostEnvironment environment)
    {
        var configured = options.Value;
        var certPath = RentingCertificatePathResolver.Resolve(
            configured.CertificatePath,
            environment.ContentRootPath);

        var certificate = X509CertificateLoader.LoadPkcs12FromFile(
            certPath,
            configured.CertificatePassphrase);

        return new HttpClientHandler
        {
            ClientCertificates = { certificate },
        };
    }

    public async Task<RentingContractInfoItem[]?> GetContractInfoByPlateAsync(
        string placa,
        DateOnly fechaInfraccion,
        CancellationToken cancellationToken)
    {
        var token = await GetTokenAsync(cancellationToken);
        var normalizedPlate = placa.Trim().ToUpperInvariant();
        var query =
            $"plate={Uri.EscapeDataString(normalizedPlate)}&offenseDate={Uri.EscapeDataString(fechaInfraccion.ToString("yyyy-MM-dd"))}";

        using var request = new HttpRequestMessage(
            HttpMethod.Get,
            BuildUri(_options.ContractInfoByPlatePath, query));

        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        request.Headers.TryAddWithoutValidation(_options.ApiKeyHeaderName, _options.ApiKeyValue);

        using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        cts.CancelAfter(TimeSpan.FromSeconds(_options.ContractInfoTimeoutSeconds));

        using var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cts.Token);
        var body = await response.Content.ReadAsStringAsync(cts.Token);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning(
                "Renting contract lookup HTTP {Status} for plate prefix {PlatePrefix}",
                (int)response.StatusCode,
                MaskPlate(normalizedPlate));

            if ((int)response.StatusCode >= 500)
            {
                throw new HttpRequestException($"Renting API returned HTTP {(int)response.StatusCode}.");
            }

            return null;
        }

        return ParseContractResponse(body);
    }

    public static RentingContractInfoItem[]? ParseContractResponse(string body)
    {
        if (string.IsNullOrWhiteSpace(body))
        {
            return null;
        }

        using var document = JsonDocument.Parse(body);
        if (document.RootElement.ValueKind == JsonValueKind.Array)
        {
            var items = document.RootElement.Deserialize<RentingContractInfoItem[]>(JsonOptions);
            return items is { Length: > 0 } ? items : null;
        }

        if (document.RootElement.ValueKind == JsonValueKind.Object
            && document.RootElement.TryGetProperty("statusCode", out _))
        {
            return null;
        }

        return null;
    }

    private async Task<string> GetTokenAsync(CancellationToken cancellationToken)
    {
        if (_cache.TryGetValue(TokenCacheKey, out string? cached) && !string.IsNullOrEmpty(cached))
        {
            return cached;
        }

        if (string.IsNullOrWhiteSpace(_options.LoginSecretName) || string.IsNullOrWhiteSpace(_options.LoginSubject))
        {
            throw new InvalidOperationException(
                "Dgc:RentingApi LoginSecretName and LoginSubject must be configured.");
        }

        var loginRequest = new RentingLoginRequest(_options.LoginSecretName, _options.LoginSubject);

        using var request = new HttpRequestMessage(HttpMethod.Post, BuildUri(_options.LoginPath, null))
        {
            Content = JsonContent.Create(loginRequest),
        };
        request.Headers.TryAddWithoutValidation(_options.ApiKeyHeaderName, _options.ApiKeyValue);

        using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        cts.CancelAfter(TimeSpan.FromSeconds(_options.LoginTimeoutSeconds));

        using var response = await _httpClient.SendAsync(request, cts.Token);
        response.EnsureSuccessStatusCode();

        var login = await response.Content.ReadFromJsonAsync<RentingLoginResponse>(JsonOptions, cts.Token)
            ?? throw new InvalidOperationException("Renting login returned empty body.");

        if (string.IsNullOrWhiteSpace(login.Token))
        {
            throw new InvalidOperationException("Renting login returned empty token.");
        }

        _cache.Set(
            TokenCacheKey,
            login.Token,
            TimeSpan.FromSeconds(Math.Max(60, _options.LoginCacheSecondsTtl)));

        return login.Token;
    }

    private Uri BuildUri(string relativePath, string? query)
    {
        var baseUrl = _options.BaseUrl.TrimEnd('/') + "/";
        var path = relativePath.TrimStart('/');
        var uri = string.IsNullOrEmpty(query) ? $"{baseUrl}{path}" : $"{baseUrl}{path}?{query}";
        return new Uri(uri);
    }

    private static string MaskPlate(string plate) =>
        plate.Length <= 3 ? "***" : $"{plate[..3]}***";
}
