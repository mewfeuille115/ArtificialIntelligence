using FirstChatBot;
using FirstChatBot.Chatbots;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

Utilities.LoadEnvVariables();

// Example usage: dotnet run -- openai gpt-5.4-nano
// Example usage: dotnet run -- anthropic claude-haiku-4-5
// Example usage: dotnet run -- local qwopus3.6-27b-v2-mtp
var provider = args.Length > 0 ? args[0].ToLowerInvariant() : "local";
var model = args.Length > 1
	? args[1].ToLowerInvariant()
	: provider switch
	{
		"local" => "qwopus3.6-27b-v2-mtp",
		"openai" => "gpt-5.4-nano",
		"anthropic" => "claude-haiku-4-5",
		_ => throw new ArgumentException($"Proveedor desconocido: {provider}")
	};

Console.WriteLine($"Using provider: {provider}, model: {model}");

var builder = Host.CreateApplicationBuilder(args);
Startup.ConfigureServices(builder, provider, model);
var host = builder.Build();

var client = host.Services.GetRequiredService<IChatClient>();

await Chatbot.RunAsync(client);
