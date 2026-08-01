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

var provider = "local";
var model = "qwythos-9b-claude-mythos-5-1m";

builder.Services.AddSingleton<IChatClient>(serviceProvider =>
{
	var configuration = serviceProvider.GetRequiredService<IConfiguration>();
	var openAIKey = configuration.GetValue<string>("OPENAI_KEY");
	var anthropicKey = configuration.GetValue<string>("ANTHROPIC_KEY");
	var localKey = "no-required";

	var client = provider switch
	{
		"openai" => new OpenAI.Chat.ChatClient(model, openAIKey).AsIChatClient(),
		"anthropic" => new AnthropicClient
		{
			ApiKey = anthropicKey,
		}.AsIChatClient()
			.AsBuilder()
			.ConfigureOptions(options => options.ModelId = model)
			.Build(),
		"local" => new OpenAIClient(
				new System.ClientModel.ApiKeyCredential(localKey),
				new OpenAIClientOptions { Endpoint = new Uri("http://localhost:1234/v1") })
			.GetChatClient(model)
			.AsIChatClient(),
		_ => throw new ArgumentException($"Unknown provider: {provider}"),
	};

	return client
		.AsBuilder()
		.UseFunctionInvocation(null, configuration =>
		{
			configuration.IncludeDetailedErrors = true;
		})
		.Build(serviceProvider);
});

builder.Services.AddTransient<ChatOptions>(serviceProvider => new ChatOptions
{
	Tools = [.. Tools.GetTools(serviceProvider)],
	ModelId = model,
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
