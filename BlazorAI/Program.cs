using Anthropic;
using BlazorAI.Components;
using BlazorAI.Data;
using BlazorAI.Services;
using BlazorAI.Services.Chatbots;
using BlazorAI.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.AI;
using OpenAI;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
	.AddInteractiveServerComponents();

builder.Services.AddDbContextFactory<ApplicationDbContext>(options =>
	options.UseSqlite("Data Source=mydb.db"));

builder.Services.AddScoped<IPersonsService, PersonsService>();
builder.Services.AddScoped<IChatbot, Chatbot>();

builder.Services.AddTransient<IWeatherService, OpenWeatherService>();
builder.Services.AddTransient<EvaluateConditionsService>();
builder.Services.AddTransient<FakeGetEmailService>();
builder.Services.AddTransient<FakeSendEmailService>();
builder.Services.AddHttpClient();

builder.Services.AddTransient<IChatClientFactory, ChatClientFactory>();

builder.Services.AddTransient<ChatOptions>(serviceProvider => new ChatOptions
{
	Tools = [.. Tools.GetTools(serviceProvider)],
	Temperature = 0.7f,
	MaxOutputTokens = 5000,
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
	app.UseExceptionHandler("/Error", createScopeForErrors: true);
	// The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
	app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
	.AddInteractiveServerRenderMode();

await app.RunAsync();
