namespace BlazorAI.Services;

internal class FakeWeatherService : IWeatherService
{
	public async Task<string> GetWeatherAsync(string location)
	{
		return location.ToLowerInvariant() switch
		{
			"new york" => "The weather in New York is sunny with a high of 75°F (24°C).",
			"london" => "The weather in London is cloudy with a chance of rain and a high of 60°F (16°C).",
			"tokyo" => "The weather in Tokyo is partly cloudy with a high of 80°F (27°C).",
			"mexico city" => "The weather in Mexico City is sunny with a high of 85°F (29°C).",
			_ => $"Sorry, I don't have weather information for {location}.",
		};
	}
}
