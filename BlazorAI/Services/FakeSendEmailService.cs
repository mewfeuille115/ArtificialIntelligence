using System.ComponentModel;

namespace BlazorAI.Services;

internal class FakeSendEmailService
{
	[Description("Send an email to a person")]
	public Task SendEmail(
		[Description("Body of the email")] string body,
		[Description("Subject of the email")] string subject,
		[Description("Recipient of the email")] string recipient)
	{
		if (!string.IsNullOrWhiteSpace(subject) && subject.Length > 0)
		{
			var fisrtChar = subject[0].ToString();

			if (!fisrtChar.Equals(fisrtChar.ToUpper()))
			{
				throw new ArgumentException("The subject must start with an uppercase letter.");
			}
		}

		Console.WriteLine("Sending email...");

		Console.WriteLine($""""

			Recipient: {recipient}
			Subject: {subject}

			Body: {body}

			"""");

		Console.WriteLine("Email sent successfully!");

		return Task.CompletedTask;
	}
}
