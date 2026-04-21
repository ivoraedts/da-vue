using TheWeb.API.Data;
using Microsoft.EntityFrameworkCore;
using KoenZomers.Tado.Api.Controllers;

public class RunScheduledRetrievals : BackgroundService
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ILogger<RunScheduledRetrievals> _logger;

    public RunScheduledRetrievals(IServiceScopeFactory serviceScopeFactory, ILogger<RunScheduledRetrievals> logger)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Background Service is starting.");
         // Wait 5 seconds for the database/app to stabilize
        await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
        _logger.LogInformation("Background Service is started.");

        // Loop until the application is shut down
        while (!stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);

            try 
            {
                // Perform your background task here
                await DoWorkAsync(stoppingToken);

                // Wait for a specific interval before running again
                await Task.Delay(TimeSpan.FromSeconds(60), stoppingToken);
            }
            catch (OperationCanceledException)
            {
                // Expected when the application stops
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred in the background service.");
            }
        }

        _logger.LogInformation("Background Service is stopping.");
    }

    private async Task DoWorkAsync(CancellationToken stoppingToken)
    {
        using (var scope = _serviceScopeFactory.CreateScope())
        {
            var dataRetrievalService = scope.ServiceProvider.GetRequiredService<IDataRetrievalService>();
            await dataRetrievalService.RetrieveDataAsync(stoppingToken);
        }
    }
}
