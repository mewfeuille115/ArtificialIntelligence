using BlazorAI.DTOs;

namespace BlazorAI.Services.Chatbots;

public interface IChatbot
{
	List<ChatMessageUI> Conversation { get; }
	bool IsProcessing { get; }
	ApprovalRequestUI? PendingApproval { get; }

	event Action? OnChange;

	void CancelActualResponse();
	Task SendMessageAsync(string userText, CancellationToken cancellationToken = default);
	Task ResolveApprovalAsync(bool approved, CancellationToken cancellationToken = default);
}
