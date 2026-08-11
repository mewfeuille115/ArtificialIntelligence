using BlazorAI.DTOs;
using BlazorAI.Utilities;
using Microsoft.Extensions.AI;
using System.Text;

namespace BlazorAI.Services.Chatbots;

public class Chatbot : IChatbot
{
	private string model;
	private readonly IChatClientFactory chatClientFactory;
	private readonly ChatOptions chatOptions;
	private readonly List<ChatMessage> messages = [];
	private readonly Queue<ToolApprovalRequestContent> pendingApprovals = new();
	private CancellationTokenSource? actualCts;

	public List<ChatMessageUI> Conversation { get; } = [];
	public bool IsProcessing { get; private set; }
	public ApprovalRequestUI? PendingApproval { get; private set; }

	public event Action? OnChange;

	public Chatbot(IChatClientFactory chatClientFactory, ChatOptions chatOptions)
	{
		model = AIModels.GetDefaultModel;
		this.chatClientFactory = chatClientFactory;
		this.chatOptions = chatOptions;
		var generalSystemPrompt = """
			You are an assistant that answers general questions.
			You must respond in the same language as the user's question. If you do not know the language, respond in English.
			Responses must be in plain text. Do not use Markdown formatting.
			Responses should be concise unless instructed otherwise.

			If a tool call fails:
			1. Read the exception message carefully.
			2. If you can identify a fix, first respond with a plain text message explaining what went wrong and what adjustment you will make.
			3. Only after explaining the adjustment, call the tool again with the corrected parameters.
			Do not call the tool again silently. The user must always see your explanation before the retry.
			""";

		messages.Add(new ChatMessage(role: ChatRole.System, content: generalSystemPrompt));
	}

	private void NotifyChange() => OnChange?.Invoke();

	public void CancelActualResponse()
	{
		if (IsProcessing)
		{
			actualCts?.Cancel();
		}
	}

	public async Task SendMessageAsync(string userText, CancellationToken cancellationToken = default)
	{
		if (string.IsNullOrEmpty(userText))
		{
			return;
		}

		if (IsProcessing || PendingApproval is not null)
		{
			return;
		}

		try
		{
			IsProcessing = true;
			actualCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

			Conversation.Add(new ChatMessageUI
			{
				Role = MessageRole.User,
				Text = userText,
			});

			messages.Add(new ChatMessage(role: ChatRole.User, content: userText));

			Conversation.Add(new ChatMessageUI
			{
				Role = MessageRole.AI,
				Text = string.Empty,
			});

			NotifyChange();
			await ProcessResponse(actualCts.Token);
		}
		catch (OperationCanceledException)
		{
			HandleCancelledOperation();
		}
		finally
		{
			HandleFinally();
		}
	}

	private void HandleCancelledOperation()
	{
		if (Conversation.Count > 0 && Conversation[^1].Role == MessageRole.AI)
		{
			if (string.IsNullOrWhiteSpace(Conversation[^1].Text))
			{
				Conversation[^1].Text = "[The response was cancelled]";
			}
			else
			{
				Conversation[^1].Text += " [Cancelled]";
			}
		}
	}

	private void HandleFinally()
	{
		actualCts?.Dispose();
		actualCts = null;
		IsProcessing = false;
		NotifyChange();
	}

	private async Task ProcessResponse(CancellationToken cancellationToken = default)
	{
		var updates = new List<ChatResponseUpdate>();
		var builder = new StringBuilder();

		var client = chatClientFactory.Create(model);

		await foreach (var update in client.GetStreamingResponseAsync(
			messages,
			chatOptions,
			cancellationToken: cancellationToken))
		{
			updates.Add(update);

			foreach (var content in update.Contents)
			{
				if (content is TextContent textContent)
				{
					builder.Append(textContent.Text);

					Conversation[^1].Text = builder.ToString();
					NotifyChange();
				}
			}
		}

		var response = updates.ToChatResponse();
		messages.AddMessages(response);

		var approvalRequests = response.Messages
			.SelectMany(message => message.Contents)
			.OfType<ToolApprovalRequestContent>()
			.ToList();

		if (approvalRequests.Count > 0)
		{
			foreach (var request in approvalRequests)
			{
				pendingApprovals.Enqueue(request);
			}

			if (string.IsNullOrWhiteSpace(Conversation[^1].Text))
			{
				Conversation.RemoveAt(Conversation.Count - 1);
			}

			ShowNextPendingApproval();
			NotifyChange();
			return;
		}
	}

	private void ShowNextPendingApproval()
	{
		if (pendingApprovals.Count == 0)
		{
			PendingApproval = null;
			return;
		}

		var approvalRequest = pendingApprovals.Dequeue();


		if (approvalRequest.ToolCall is FunctionCallContent functionCall)
		{
			PendingApproval = new ApprovalRequestUI
			{
				ApprovalRequest = approvalRequest,
				ToolName = ConvertFunctionName(functionCall.Name),
				Arguments = functionCall.Arguments?.ToDictionary(x => x.Key, x => x.Value) ?? [],
			};
		}
	}

	public async Task ResolveApprovalAsync(bool approved, CancellationToken cancellationToken = default)
	{
		if (PendingApproval is null || IsProcessing)
		{
			return;
		}

		try
		{
			IsProcessing = true;
			actualCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
			var approvalResponse = PendingApproval.ApprovalRequest.CreateResponse(approved);
			messages.Add(new ChatMessage(ChatRole.User, [approvalResponse]));
			PendingApproval = null;

			Conversation.Add(new ChatMessageUI
			{
				Role = MessageRole.System,
				Text = approved
					? "Action approved by the user."
					: "Action denied by the user.",
			});

			PendingApproval = null;
			ShowNextPendingApproval();

			if (PendingApproval is not null)
			{
				IsProcessing = false;
				NotifyChange();
				return;
			}

			Conversation.Add(new ChatMessageUI
			{
				Role = MessageRole.AI,
				Text = string.Empty,
			});

			NotifyChange();
			await ProcessResponse(actualCts.Token);
		}
		catch (OperationCanceledException)
		{
			HandleCancelledOperation();
		}
		finally
		{
			HandleFinally();
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

	public void SetModel(string model)
	{
		this.model = model;
	}
}
