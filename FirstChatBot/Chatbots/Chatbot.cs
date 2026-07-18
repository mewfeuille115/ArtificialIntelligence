using Microsoft.Extensions.AI;

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

			If a tool call fails:
			1. Read the exception message carefully.
			2. If you can identify a fix, first respond with a plain text message explaining what went wrong and what adjustment you will make.
			3. Only after explaining the adjustment, call the tool again with the corrected parameters.
			Do not call the tool again silently. The user must always see your explanation before the retry.
			""";

		var systemPromptCsharp = """
			You are an expert on C# and .NET.
			You must respond in the same language as the user's question. If you do not know the language, respond in English and always provide examples.
			Responses must be in plain text. Do not use Markdown formatting.
			""";

		messages.Add(new ChatMessage(role: ChatRole.System, content: generalSystemPrompt));

		while (true)
		{
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

			while (true)
			{
				var updates = new List<ChatResponseUpdate>();

				await foreach (var responseUpdate in client.GetStreamingResponseAsync(messages))
				{
					updates.Add(responseUpdate);

					foreach (var content in responseUpdate.Contents)
					{
						if (content is TextContent textContent)
						{
							Console.Write(textContent);
						}
					}
				}

				var response = updates.ToChatResponse();
				messages.AddMessages(response);

				var approvalRequest = response.Messages
					.SelectMany(m => m.Contents)
					.OfType<ToolApprovalRequestContent>()
					.FirstOrDefault();

				if (approvalRequest is not null)
				{
					Console.WriteLine();
					Console.WriteLine();
					Console.ForegroundColor = ConsoleColor.Yellow;
					Console.WriteLine($"AI is requesting to sensitive action.");

					if (approvalRequest.ToolCall is FunctionCallContent functionCall)
					{
						Console.WriteLine($"Tool: {ConvertFunctionName(functionCall.Name)}");

						if (functionCall.Arguments is not null)
						{
							foreach (var argument in functionCall.Arguments)
							{
								Console.WriteLine($"{argument.Key} = {argument.Value}");
							}
						}
					}

					Console.ResetColor();
					Console.Write("Do you approve? (y/n): ");
					var approval = Console.ReadLine()?.Trim().ToLower() == "y";
					var approvalResponse = approvalRequest.CreateResponse(approval);

					messages.Add(new ChatMessage(role: ChatRole.User, contents: [approvalResponse]));

					Console.WriteLine();
					Console.Write("AI: ");
					continue;
				}

				Console.WriteLine();
				Console.WriteLine();
				break;
			}
		}
	}

	private static string ConvertFunctionName(string name)
	{
		return name switch
		{
			"SendEmail" => "Send Email",
			_ => name,
		};
	}
}
