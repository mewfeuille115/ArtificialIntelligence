using OpenAI.Chat;
using System.Text;

namespace FirstChatBot;

internal static class ChatbotOpenAI
{
	internal static async Task RunAsync()
	{
		var model = "gpt-5.4-nano";
		var key = Environment.GetEnvironmentVariable("OPENAI_KEY");
		var client = new ChatClient(model, key);

		Console.WriteLine("AI: Hello! You can write your questions or press Enter to exit");
		Console.WriteLine();

		var messages = new List<ChatMessage>();

		var generalSystemPrompt = """
			You are an assistant that answers general questions.
			You must respond in the same language as the user's question. If you do not know the language, respond in English.
			Responses must be in plain text. Do not use Markdown formatting.
			""";

		var systemPromptCsharp = """
			You are an expert on C# and .NET.
			You must respond in the same language as the user's question. If you do not know the language, respond in English and always provide examples.
			Responses must be in plain text. Do not use Markdown formatting.
			""";

		var systemPromptPython = """
			You are an expert on Python.
			You must respond in the same language as the user's question. If you do not know the language, respond in English and always provide examples.
			Responses must be in plain text. Do not use Markdown formatting.
			""";

		messages.Add(new SystemChatMessage(systemPromptPython));

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

			messages.Add(new UserChatMessage(entry));

			Console.WriteLine();
			Console.Write("AI: ");

			var stream = client.CompleteChatStreamingAsync(messages);

			await foreach (var update in stream)
			{
				var text = string.Concat(update.ContentUpdate.Select(c => c.Text));
				stringBuilder.Append(text);
				Console.Write(text);
			}

			messages.Add(new AssistantChatMessage(stringBuilder.ToString()));

			Console.WriteLine();
			Console.WriteLine();
		}

	}
}
