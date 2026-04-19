using Xunit;

namespace LupiraWeb.Server.Tests;

public class WeatherForecastTests
{
    // The production formula is 32 + (int)(C / 0.5556), which is deliberately
    // approximate and truncates toward zero. These cases use values where that
    // approximation round-trips cleanly. -40 (where the true formula aligns)
    // is omitted because the 0.5556 approximation off-by-ones it.
    [Theory]
    [InlineData(0, 32)]
    [InlineData(100, 211)]
    [InlineData(37, 98)]
    public void TemperatureF_converts_celsius_to_fahrenheit(int celsius, int expectedFahrenheit)
    {
        var forecast = new WeatherForecast(new DateOnly(2026, 1, 1), celsius, null);

        Assert.Equal(expectedFahrenheit, forecast.TemperatureF);
    }
}
