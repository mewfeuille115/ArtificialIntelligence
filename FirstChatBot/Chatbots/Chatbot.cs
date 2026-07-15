using Microsoft.Extensions.AI;
using System.Text;

namespace FirstChatBot.Chatbots;

internal static class Chatbot
{
	internal static async Task RunAsync(IChatClient client)
	{
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

		messages.Add(new ChatMessage(role: ChatRole.System, content: systemPromptCsharp));

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

			messages.Add(new ChatMessage(role: ChatRole.User, content: entry));

			Console.WriteLine();
			Console.Write("AI: ");

			await foreach (var fragment in client.GetStreamingResponseAsync(messages))
			{
				stringBuilder.Append(fragment);
				Console.Write(fragment);
			}

			messages.Add(new ChatMessage(role: ChatRole.Assistant, content: stringBuilder.ToString()));

			Console.WriteLine();
			Console.WriteLine();
		}
	}
}
