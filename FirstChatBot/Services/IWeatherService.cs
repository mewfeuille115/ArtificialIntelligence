namespace FirstChatBot.Services;

internal interface IWeatherService
{
	Task<string> GetWeatherAsync(string location);
}