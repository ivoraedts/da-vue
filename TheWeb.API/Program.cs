using Microsoft.EntityFrameworkCore;
using TheWeb.API.Data;
using KoenZomers.Tado.Api.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddTadoServices();
builder.Services.AddDaVueDbContext(builder.Configuration);
builder.Services.AddControllers(); 

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<DaVueDbContext>();
    dbContext.Database.Migrate();
}

app.MapControllers(); 

app.Run();
