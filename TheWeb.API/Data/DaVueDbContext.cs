using Microsoft.EntityFrameworkCore;

namespace TheWeb.API.Data
{
    public class DaVueDbContext : DbContext
    {
        protected readonly IConfiguration Configuration;

        public DaVueDbContext(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            var connectionString = Configuration.GetConnectionString("DaVueDb");
            options.UseNpgsql(connectionString);
        }

        public DbSet<TodoItem> TodoItems { get; set; }
        public DbSet<TadoDeviceAuthentication> TadoDeviceAuthentications {get; set;}
        public DbSet<TadoToken> TadoTokens { get; set; }
    }

    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddDaVueDbContext(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<DaVueDbContext>(options =>
                options.UseNpgsql(configuration.GetConnectionString("DaVueDb"),
                npgsqlOptions => npgsqlOptions.EnableRetryOnFailure()
                )
            );
            return services;
        }
    }
}