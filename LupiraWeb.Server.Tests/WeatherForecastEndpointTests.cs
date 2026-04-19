using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace LupiraWeb.Server.Tests;

public class WeatherForecastEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public WeatherForecastEndpointTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Get_returns_five_forecasts()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/weatherforecast");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var forecasts = await response.Content.ReadFromJsonAsync<WeatherForecast[]>();
        Assert.NotNull(forecasts);
        Assert.Equal(5, forecasts!.Length);
    }
}
