using System.Text.Json.Serialization;

namespace Gdc.Infrastructure.Dgc.Renting;

public sealed record RentingLoginRequest(
    [property: JsonPropertyName("secretName")] string SecretName,
    [property: JsonPropertyName("subject")] string Subject);

public sealed record RentingLoginResponse(
    [property: JsonPropertyName("token")] string Token,
    [property: JsonPropertyName("refresh")] string? Refresh);

public sealed record RentingContractInfoItem(
    [property: JsonPropertyName("plate")] string? Plate,
    [property: JsonPropertyName("names")] string? Names,
    [property: JsonPropertyName("firstSurname")] string? FirstSurname,
    [property: JsonPropertyName("customerEmail")] string? CustomerEmail,
    [property: JsonPropertyName("customerId")] string? CustomerId,
    [property: JsonPropertyName("customerTypeId")] string? CustomerTypeId);

public sealed record RentingContractInfoError(
    [property: JsonPropertyName("statusCode")] int StatusCode,
    [property: JsonPropertyName("message")] string? Message);
