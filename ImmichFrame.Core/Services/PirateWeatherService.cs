using System.Text.Json;
using ImmichFrame.Core.Models;
using ImmichFrame.Core.Interfaces;
using ImmichFrame.Core.Helpers;


namespace ImmichFrame.Core.Services
{
    public class PirateWeatherService : IWeatherService
    {
        private readonly IGeneralSettings _settings;
        private readonly HttpClient _httpClient;
        private readonly IApiCache _weatherCache = new ApiCache(TimeSpan.FromMinutes(5));

        public PirateWeatherService(IGeneralSettings settings, HttpClient httpClient)
        {
            _settings = settings;
            _httpClient = httpClient;
        }

        public async Task<IWeather?> GetWeather()
        {
            return await _weatherCache.GetOrAddAsync("weather", async () =>
            {
                var weatherLatLong = _settings.WeatherLatLong;
                var weatherLat = !string.IsNullOrWhiteSpace(weatherLatLong) ? float.Parse(weatherLatLong!.Split(',')[0]) : 0f;
                var weatherLong = !string.IsNullOrWhiteSpace(weatherLatLong) ? float.Parse(weatherLatLong!.Split(',')[1]) : 0f;

                return await GetWeather(weatherLat, weatherLong);
            });
        }

        public async Task<IWeather?> GetWeather(double latitude, double longitude)
        {
            try
            {
                var units = _settings.UnitSystem?.ToLower() == "metric" ? "si" : "us";
                var url = $"https://api.pirateweather.net/forecast/{_settings.WeatherApiKey}/{latitude},{longitude}?units={units}";

                var response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                var data = JsonSerializer.Deserialize<PirateWeatherResponse>(json);

                var forecast = "";
                if (data?.Daily?.Data?.Count > 0)
                {
                    var tomorrow = data.Daily.Data[0];
                    forecast = $"Tomorrow: {tomorrow.Summary} High: {tomorrow.TemperatureHigh:F0}{(units == "si" ? "°C" : "°F")} Low: {tomorrow.TemperatureLow:F0}{(units == "si" ? "°C" : "°F")}";
                }

                return new Weather
                {
                    //Location = data?.timezone ?? "",
                    //Unit = units == "si" ? "°C" : "°F",
                    //TemperatureUnit = $"{data?.Currently?.temperature ?? 0}{(units == "si" ? "°C" : "°F")}",
                    //Description = data?.Currently?.summary ?? "",
                    //Forecast = forecast,
                    IconId = data?.Currently?.Icon ?? "",
                    Location = "Pirate Weather",
                    Temperature = data?.Currently?.Temperature ?? 0,
                    Unit = units == "si" ? "°C" : "°F", 
                    Description = data?.Currently?.Summary ?? "",
                };
            }
            catch
            {
                return null;
            }
        }
    }
}