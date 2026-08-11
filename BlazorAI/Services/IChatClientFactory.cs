using Microsoft.Extensions.AI;

namespace BlazorAI.Services;

public interface IChatClientFactory
{
	IChatClient Create(string model);
}
