using OMA.Core.Data;

var builder = WebApplication.CreateBuilder(new WebApplicationOptions()
{
    WebRootPath = System.AppDomain.CurrentDomain.BaseDirectory + "wwwroot",
});

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddScoped<OMAContext>();
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
