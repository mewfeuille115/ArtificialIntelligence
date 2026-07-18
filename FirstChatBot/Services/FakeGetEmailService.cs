using System.ComponentModel;

namespace FirstChatBot.Services;

internal class FakeGetEmailService
{

	[Description("Get email address of a person")]
	public string GetEmail(string name) =>
		$"{name.ToLowerInvariant()}@example.com";
}
