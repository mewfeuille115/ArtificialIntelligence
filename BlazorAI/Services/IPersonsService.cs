using BlazorAI.Entities;
using System.ComponentModel;

namespace BlazorAI.Services;

[Description("Service for managing persons.")]
public interface IPersonsService
{
	[Description("Retrieves all persons.")]
	Task<IEnumerable<Person>> GetAllPersonsAsync();
}
