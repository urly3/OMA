using OMA.Core.Data;

var builder = WebApplication.CreateSlimBuilder();

builder.WebHost.UseIIS();
builder.WebHost.UseIISIntegration();

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddScoped<OmaContext>();
builder.Services.AddMemoryCache();

var app = builder.Build();

app.UseStaticFiles();
app.MapControllers();

app.Run();
