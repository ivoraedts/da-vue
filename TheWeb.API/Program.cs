using Microsoft.EntityFrameworkCore;
using TheWeb.API.Data;
using TheWeb.API.Services;
using TheWeb.API.BackgroundTasks;
using KoenZomers.Tado.Api.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddTadoServices();
builder.Services.AddDaVueDbContext(builder.Configuration);
builder.Services.AddControllers();
builder.Services.AddTransient<IDataRetrievalService, DataRetrievalService>();
builder.Services.AddTransient<IHourlyDataAggregationService, HourlyDataAggregationService>();
builder.Services.AddTransient<IDailyDataAggregationService, DailyDataAggregationService>();
builder.Services.AddHostedService<RunScheduledRetrievals>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<DaVueDbContext>();
    dbContext.Database.Migrate();
}

app.MapControllers(); 

app.Run();
