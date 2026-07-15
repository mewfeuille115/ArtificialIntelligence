using Anthropic;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
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
					options.MaxOutputTokens = 2000;
					options.Temperature = 0.7f;
				})
				//.Use(async (messages, options, next, cancellationToken) =>
				//{
				//	Console.WriteLine();
				//	Console.ForegroundColor = ConsoleColor.Green;
				//	Console.WriteLine("Before call to the model...");
				//	Console.ResetColor();

				//	await next(messages, options, cancellationToken);

				//	Console.WriteLine();
				//	Console.ForegroundColor = ConsoleColor.Green;
				//	Console.WriteLine("After call to the model...");
				//	Console.ResetColor();

				//})
				.Build(serviceProvider);
		});
	}
}
