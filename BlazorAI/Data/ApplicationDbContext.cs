using BlazorAI.Entities;
using Microsoft.EntityFrameworkCore;

namespace BlazorAI.Data;

public class ApplicationDbContext : DbContext
{
	public ApplicationDbContext(DbContextOptions options) : base(options)
	{
	}

	protected ApplicationDbContext()
	{
	}

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		base.OnModelCreating(modelBuilder);

		modelBuilder.Entity<Person>().HasData(
			new Person
			{
				Id = 1,
				Name = "Felipe Gavilán",
				Email = "felipe.gavilan@example.com",
				Salary = 45000m,
				IsActive = true
			},
			new Person
			{
				Id = 2,
				Name = "María López",
				Email = "maria.lopez@example.com",
				Salary = 52000m,
				IsActive = true
			},
			new Person
			{
				Id = 3,
				Name = "Carlos Rodríguez",
				Email = "carlos.rodriguez@example.com",
				Salary = 61000m,
				IsActive = false
			},
			new Person
			{
				Id = 4,
				Name = "Ana Martínez",
				Email = "ana.martinez@example.com",
				Salary = 48000m,
				IsActive = false
			},
			new Person
			{
				Id = 5,
				Name = "Luis Gómez",
				Email = "luis.gomez@example.com",
				Salary = 55000m,
				IsActive = true
			}
		);
	}

	public DbSet<Person> Persons { get; set; }
}
