using Anthropic;
using FirstChatBot.Services;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenAI;

namespace FirstChatBot;

internal static class Startup
{
	public static void ConfigureServices(
		HostApplicationBuilder builder,
		string provider,
		string? model)
	{
		var openAIKey = Environment.GetEnvironmentVariable("OPENAI_KEY");
		var anthropicKey = Environment.GetEnvironmentVariable("ANTHROPIC_KEY");
		var localKey = "no-required";

		builder.Services.AddTransient<IWeatherService, OpenWeatherService>();
		builder.Services.AddTransient<EvaluateConditionsService>();
		builder.Logging.AddFilter("System.Net.Http.HttpClient", LogLevel.None);
		builder.Services.AddHttpClient();

		builder.Services.AddTransient<FakeGetEmailService>();
		builder.Services.AddTransient<FakeSendEmailService>();

		builder.Services.AddSingleton<IChatClient>(serviceProvider =>
		{
			var client = provider switch
			{
				"openai" => new OpenAI.Chat.ChatClient(model, openAIKey).AsIChatClient(),
				"anthropic" => new AnthropicClient
				{
					ApiKey = anthropicKey,
				}.AsIChatClient()
					.AsBuilder()
					.ConfigureOptions(options => options.ModelId = model)
					.Build(),
				"local" => new OpenAIClient(
						new System.ClientModel.ApiKeyCredential(localKey),
						new OpenAIClientOptions { Endpoint = new Uri("http://localhost:1234/v1") })
					.GetChatClient(model)
					.AsIChatClient(),
				_ => throw new ArgumentException($"Unknown provider: {provider}"),
			};

			return client
				.AsBuilder()
				.ConfigureOptions(options =>
				{
					options.MaxOutputTokens = 5000;
					options.Temperature = 0.7f;
					options.Tools = [.. Tools.Tools.GetTools(serviceProvider)];
				})
				.UseFunctionInvocation(null, configuration =>
				{
					configuration.IncludeDetailedErrors = true;
				})
				.Use(async (messages, options, next, cancellationToken) =>
				{
					await next(messages, options, cancellationToken);
				})
				.Build(serviceProvider);
		});
	}
}
