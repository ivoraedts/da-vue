var builder = WebApplication.CreateBuilder(args);

// Add services for Controllers
builder.Services.AddControllers(); 

var app = builder.Build();

// Enable routing for Controllers
app.MapControllers(); 

app.Run();
