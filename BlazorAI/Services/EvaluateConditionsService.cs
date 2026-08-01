namespace BlazorAI.Services;

internal class EvaluateConditionsService
{
	public string EvaluateConditions(string weatherCondition)
	{
		weatherCondition = weatherCondition.ToLower();

		// Rain / drizzle / precipitation
		if (weatherCondition.Contains("rain") ||
			weatherCondition.Contains("drizzle") ||
			weatherCondition.Contains("precipitation"))
		{
			return "It's not a good time for outdoor activities.";
		}

		// Storm / stormy
		if (weatherCondition.Contains("storm") ||
			weatherCondition.Contains("stormy"))
		{
			return "Avoid going outside, dangerous conditions.";
		}

		// Snow / snowfall / blizzard
		if (weatherCondition.Contains("snow") ||
			weatherCondition.Contains("snowfall") ||
			weatherCondition.Contains("blizzard"))
		{
			return "Cold and potentially dangerous conditions. Only go outside if necessary.";
		}

		// Mist / fog
		if (weatherCondition.Contains("mist") ||
			weatherCondition.Contains("fog"))
		{
			return "Visibility is low, be cautious if you go outside.";
		}

		// sunny
		if (weatherCondition.Contains("sunny"))
		{
			return "Great weather for outdoor activities!";
		}

		return "Weather conditions are normal.";
	}
}
