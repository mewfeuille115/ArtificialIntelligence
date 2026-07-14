namespace FirstChatBot;

internal static class Utilities
{
	internal static void LoadEnvVariables()
	{
		string envFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ".env");
		if (!File.Exists(envFilePath))
		{
			return;
		}

		var envVars = File.ReadAllLines(envFilePath)
			.Where(line => !string.IsNullOrWhiteSpace(line))
			.Select(line => line.Split("=", 2))
			.Where(parts => parts.Length == 2);

		foreach (var parts in envVars)
		{
			string key = parts[0].Trim();
			string value = parts[1].Trim();
			Environment.SetEnvironmentVariable(key, value);
		}
	}
}
