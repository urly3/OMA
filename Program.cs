using Microsoft.AspNetCore.Mvc.Razor;
using OMA.Data;

var builder = WebApplication.CreateBuilder(new WebApplicationOptions()
{
    WebRootPath = Directory.GetCurrentDirectory() + "/oma.mvc/wwwroot",
});

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddScoped<OMADataService>();
builder.Services.Configure<RazorViewEngineOptions>(o =>
{
    o.ViewLocationFormats.Clear();
    o.ViewLocationFormats.Add("/oma.mvc/Views/{1}/{0}" + RazorViewEngine.ViewExtension);
    o.ViewLocationFormats.Add("/oma.mvc/views/Shared/{0}" + RazorViewEngine.ViewExtension);
});
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
app.UseRouting();
app.UseAuthorization();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
