using Anthropic;
using Anthropic.Models.Messages;
using System.Text;
using System.Text.Json;

namespace FirstChatBot;

internal static class ChatbotAnthropic
{
	internal static async Task RunAsync()
	{
		var model = "claude-haiku-4-5";
		var key = Environment.GetEnvironmentVariable("ANTHROPIC_KEY");
		var client = new AnthropicClient
		{
			ApiKey = key,
		};

		Console.WriteLine("AI: Hello! You can write your questions or press Enter to exit");
		Console.WriteLine();

		var messages = new List<MessageParam>();

		var systemPromptCsharp = """
			You are an expert on C# and .NET.
			You must respond in the same language as the user's question. If you do not know the language, respond in English and always provide examples.
			Responses must be in plain text. Do not use Markdown formatting.
			""";

		while (true)
		{
			var stringBuilder = new StringBuilder();
			Console.ForegroundColor = ConsoleColor.Blue;
			Console.Write("You: ");
			var entry = Console.ReadLine();
			Console.ResetColor();

			if (string.IsNullOrEmpty(entry))
			{
				break;
			}

			messages.Add(new MessageParam
			{
				Role = Role.User,
				Content = entry,
			});

			Console.WriteLine();
			Console.Write("AI: ");

			var parameters = new MessageCreateParams
			{
				Model = model,
				MaxTokens = 1024,
				System = systemPromptCsharp,
				Messages = messages,
			};

			await foreach (var update in client.Messages.CreateStreaming(parameters))
			{
				var text = ExtractDeltaText(update);

				if (!string.IsNullOrEmpty(text))
				{
					stringBuilder.Append(text);
					Console.Write(text);
				}
			}

			messages.Add(new MessageParam
			{
				Role = Role.Assistant,
				Content = stringBuilder.ToString(),
			});
		}
	}

	private static string? ExtractDeltaText(RawMessageStreamEvent update)
	{
		var json = update?.ToString();

		if (string.IsNullOrEmpty(json))
		{
			return null;
		}

		try
		{
			using var doc = JsonDocument.Parse(json);
			var root = doc.RootElement;

			if (!root.TryGetProperty("type", out var typeProp) ||
				typeProp.GetString() != "content_block_delta")
			{
				return null;
			}

			if (!root.TryGetProperty("delta", out var deltaProp))
			{
				return null;
			}

			if (!deltaProp.TryGetProperty("type", out var deltaTypeProp) ||
				deltaTypeProp.GetString() != "text_delta")
			{
				return null;
			}

			if (!deltaProp.TryGetProperty("text", out var textProp))
			{
				return null;
			}

			return textProp.GetString();
		}
		catch (JsonException)
		{
			return null;
		}

	}
}
