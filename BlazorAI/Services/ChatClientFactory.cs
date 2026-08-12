using Anthropic;
using BlazorAI.Utilities;
using Microsoft.Extensions.AI;
using OpenAI;

namespace BlazorAI.Services;

public class ChatClientFactory(
		IConfiguration configuration,
		IServiceProvider serviceProvider
	) : IChatClientFactory
{
	public IChatClient Create(string model)
	{
		var openAIKey = configuration.GetValue<string>("OPENAI_KEY");
		var anthropicKey = configuration.GetValue<string>("ANTHROPIC_KEY");
		var localKey = "no-required";

		var provider = AIModels.GetProviderForModel(model);

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
			.UseFunctionInvocation(null, configuration =>
			{
				configuration.IncludeDetailedErrors = true;
			})
			.Build(serviceProvider);
	}
}
