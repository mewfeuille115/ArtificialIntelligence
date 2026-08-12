namespace BlazorAI.Utilities;

public static class AIModels
{
	private static readonly Dictionary<string, string> Models = new()
	{
		["gpt-5.4-nano"] = "openai",
		["claude-haiku-4-5"] = "anthropic",
		["qwythos-9b-claude-mythos-5-1m"] = "local"
	};

	public static string GetProviderForModel(string model)
	{
		if (Models.TryGetValue(model, out var provider))
		{
			return provider;
		}

		throw new ArgumentException($"Not supported model: {model}");
	}

	public static IEnumerable<string> GetAvailableModels() => Models.Keys;

	public static string GetDefaultModel => "qwythos-9b-claude-mythos-5-1m";
}
