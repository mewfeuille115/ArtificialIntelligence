using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace BlazorAI.Services;

internal class OpenWeatherService(
		HttpClient httpClient,
		IConfiguration configuration
	) : IWeatherService
{
	public async Task<string> GetWeatherAsync(string location)
	{
		var apikey = configuration.GetValue<string>("WEATHER_API_KEY");
		var locationURL = Uri.EscapeDataString(location);
		var url = $"http://api.weatherapi.com/v1/current.json?key={apikey}&q={locationURL}&aqi=no";
		var weatherResponse = await httpClient.GetFromJsonAsync<WeatherResponse>(url);

		return weatherResponse!.Current.Condition.Text;
	}

	public class WeatherResponse
	{
		[JsonPropertyName("current")]
		public Current Current { get; set; } = default!;
	}

	public class Current
	{
		[JsonPropertyName("condition")]
		public Condition Condition { get; set; } = default!;
	}

	public class Condition
	{
		[JsonPropertyName("text")]
		public string Text { get; set; } = default!;
	}
}
