using Gdc.Infrastructure.Dgc.Renting;

namespace Gdc.Api.Tests.Dgc;

public sealed class RentingApiClientTests
{
    [Fact]
    public void ParseContractResponse_returns_items_for_success_array()
    {
        const string json = """
            [
              {
                "plate": "ABC123",
                "names": "Maria",
                "firstSurname": "Lopez",
                "customerEmail": "maria@test.com",
                "customerId": "55443322"
              }
            ]
            """;

        var items = RentingApiClient.ParseContractResponse(json);

        Assert.NotNull(items);
        Assert.Single(items!);
        Assert.Equal("Maria", items![0].Names);
        Assert.Equal("Lopez", items[0].FirstSurname);
    }

    [Fact]
    public void ParseContractResponse_returns_null_for_error_object()
    {
        const string json = """{"statusCode":404,"message":"Not found"}""";

        Assert.Null(RentingApiClient.ParseContractResponse(json));
    }

    [Fact]
    public void ParseContractResponse_returns_null_for_empty_array()
    {
        Assert.Null(RentingApiClient.ParseContractResponse("[]"));
    }
}
