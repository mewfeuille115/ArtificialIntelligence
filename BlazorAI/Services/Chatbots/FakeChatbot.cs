using BlazorAI.DTOs;

namespace BlazorAI.Services.Chatbots;

public class FakeChatbot : IChatbot
{
	public List<ChatMessageUI> Conversation { get; } = [];
	public bool IsProcessing => false;
	public ApprovalRequestUI? PendingApproval => throw new NotImplementedException();

	public event Action? OnChange;

	private void NotifyChange() => OnChange?.Invoke();

	public void CancelActualResponse()
	{

	}

	public async Task SendMessageAsync(string userText, CancellationToken cancellationToken = default)
	{
		Conversation.Add(new ChatMessageUI
		{
			Role = MessageRole.User,
			Text = userText,
		});
		NotifyChange();

		await Task.Delay(500, cancellationToken);

		Conversation.Add(new ChatMessageUI
		{
			Role = MessageRole.AI,
			Text = $"This is a test response. Not connected to any AI service.",
		});
		NotifyChange();
	}

	public Task ResolveApprovalAsync(bool approved, CancellationToken cancellationToken = default)
	{
		return Task.CompletedTask;
	}

	public void SetModel(string model)
	{
	}
}
