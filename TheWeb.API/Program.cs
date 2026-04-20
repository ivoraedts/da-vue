using Microsoft.EntityFrameworkCore;
using TheWeb.API.Data;
using KoenZomers.Tado.Api.Extensions;

var builder = WebApplication.CreateBuilder(args);

/*
var config = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddEnvironmentVariables()
    .Build();

builder.Services.AddSingleton<IConfiguration>(config);
*/
builder.Services.AddTadoServices();


builder.Services.AddDaVueDbContext(builder.Configuration);
// Add services for Controllers
builder.Services.AddControllers(); 

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<DaVueDbContext>();
    dbContext.Database.Migrate();
}

// Enable routing for Controllers
app.MapControllers(); 

app.Run();
