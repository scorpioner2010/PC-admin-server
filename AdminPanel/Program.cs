using AdminPanel.Services;

var builder = WebApplication.CreateBuilder(args);

// MVC/Razor
builder.Services.AddRazorPages();
builder.Services.AddControllers();

// In-memory stores
builder.Services.AddSingleton<IAgentStore, AgentStore>();
builder.Services.AddSingleton<IPolicyStore, PolicyStore>();

// NEW: global settings (unlock password)
builder.Services.AddSingleton<ISettingsStore, SettingsStore>(); // <-- added

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