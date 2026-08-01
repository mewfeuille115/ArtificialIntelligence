using BlazorAI.Data;
using BlazorAI.Entities;
using Microsoft.EntityFrameworkCore;

namespace BlazorAI.Services;

public class PersonsService(
		IDbContextFactory<ApplicationDbContext> dbContextFactory
	) : IPersonsService
{
	public async Task<IEnumerable<Person>> GetAllPersonsAsync()
	{
		await using var context = await dbContextFactory.CreateDbContextAsync();
		return await context.Persons.ToListAsync();
	}
}
