using OMA.Core.Data;

var builder = WebApplication.CreateSlimBuilder(new WebApplicationOptions
{
    //WebRootPath = Directory.GetCurrentDirectory() + "\\wwwroot",
});

builder.WebHost.UseIIS();
builder.WebHost.UseIISIntegration();

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddScoped<OmaContext>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.MapControllers();

app.Run();
