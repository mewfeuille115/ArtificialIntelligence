using BlazorAI.Services;
using Microsoft.Extensions.AI;

namespace BlazorAI.Utilities;

internal static class Tools
{
	internal static IEnumerable<AITool> GetTools(this IServiceProvider serviceProvider)
	{
		var weatherService = serviceProvider.GetRequiredService<IWeatherService>();

		yield return AIFunctionFactory.Create(
			weatherService.GetWeatherAsync,
			new AIFunctionFactoryOptions
			{
				Name = "get_weather",
				Description = "Get the current weather for a given location.",
			});


		var evaluateConditionsService = serviceProvider.GetRequiredService<EvaluateConditionsService>();

		yield return AIFunctionFactory.Create(
			evaluateConditionsService.EvaluateConditions,
			new AIFunctionFactoryOptions
			{
				Name = "evaluate_conditions",
				Description = "Evaluates a weather condition (for example, 'sunny', 'rainy', 'cloudy') and determines if it's a good" +
				" time for outdoor activities.",
			});

		var fakeGetEmailService = serviceProvider.GetRequiredService<FakeGetEmailService>();
		yield return AIFunctionFactory.Create(fakeGetEmailService.GetEmail);

		var fakeSendEmailService = serviceProvider.GetRequiredService<FakeSendEmailService>();
		var functionSendEmails = AIFunctionFactory.Create(fakeSendEmailService.SendEmail);
		yield return new ApprovalRequiredAIFunction(functionSendEmails);

		var personsService = serviceProvider.GetRequiredService<IPersonsService>();
		yield return AIFunctionFactory.Create(personsService.GetAllPersonsAsync);
	}
}
