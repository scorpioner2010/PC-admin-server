using AdminPanel.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();
builder.Services.AddControllers();

// In-memory stores
builder.Services.AddSingleton<IAgentStore, AgentStore>();
builder.Services.AddSingleton<IPolicyStore, PolicyStore>();

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