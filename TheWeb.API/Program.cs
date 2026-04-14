using Microsoft.EntityFrameworkCore;
using TheWeb.API.Data;

var builder = WebApplication.CreateBuilder(args);

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
