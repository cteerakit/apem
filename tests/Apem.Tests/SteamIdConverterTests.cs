using Apem.Models;
using Xunit;

namespace Apem.Tests;

public sealed class SteamIdConverterTests
{
    [Fact]
    public void ToAccountId_ConvertsSteamId64()
    {
        var accountId = SteamIdConverter.ToAccountId("76561198000000001");
        Assert.Equal(39734273UL, accountId);
    }

    [Fact]
    public void ToAccountId_AcceptsGsiAccountId()
    {
        Assert.Equal(87278757UL, SteamIdConverter.ToAccountId("87278757"));
    }

    [Fact]
    public void ToSteamId64_RoundTripsAccountId()
    {
        Assert.Equal(76561198000000001UL, SteamIdConverter.ToSteamId64("39734273"));
        Assert.Equal(76561198000000001UL, SteamIdConverter.ToSteamId64("76561198000000001"));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("not-a-number")]
    public void ToAccountId_RejectsInvalidValues(string? steamId)
    {
        Assert.Null(SteamIdConverter.ToAccountId(steamId));
        Assert.Null(SteamIdConverter.ToSteamId64(steamId));
    }
}
