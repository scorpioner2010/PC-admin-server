using AdminPanel.Services;

var builder = WebApplication.CreateBuilder(args);

// MVC/Razor
builder.Services.AddRazorPages();
builder.Services.AddControllers();

// Stores
builder.Services.AddSingleton<IAgentStore, AgentStore>();
builder.Services.AddSingleton<IPolicyStore, PolicyStore>();
builder.Services.AddSingleton<ISettingsStore, SettingsStore>();

// NEW: командна черга для миттєвих команд (Shutdown, SetVolume)
builder.Services.AddSingleton<ICommandsQueue, CommandsQueue>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}

app.UseStaticFiles();
app.UseRouting();

app.MapRazorPages();
app.MapControllers();

app.Run();