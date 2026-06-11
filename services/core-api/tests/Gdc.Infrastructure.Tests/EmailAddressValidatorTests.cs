using Gdc.Infrastructure.Auth;

namespace Gdc.Infrastructure.Tests;

public class EmailAddressValidatorTests
{
    [Theory]
    [InlineData("user@example.com")]
    [InlineData("user.name+tag@example.co")]
    public void IsValid_returns_true_for_valid_addresses(string email)
    {
        Assert.True(EmailAddressValidator.IsValid(email));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("not-an-email")]
    [InlineData("@example.com")]
    public void IsValid_returns_false_for_invalid_addresses(string? email)
    {
        Assert.False(EmailAddressValidator.IsValid(email));
    }
}
