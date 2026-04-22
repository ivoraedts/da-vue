using TheWeb.API.Services;

namespace TheWeb.API.BackgroundTasks;

public class RunScheduledRetrievals : BackgroundService
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ILogger<RunScheduledRetrievals> _logger;
    const int DelayInSeconds = 5;
    const int AfterErrorDelayInSeconds = 60;

    public RunScheduledRetrievals(IServiceScopeFactory serviceScopeFactory, ILogger<RunScheduledRetrievals> logger)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation($"Background Service is starting in {DelayInSeconds} seconds.");
        await Task.Delay(TimeSpan.FromSeconds(DelayInSeconds), stoppingToken);
        _logger.LogInformation("Background Service is started.");

        // Loop until the application is shut down
        while (!stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);

            try
            {
                // Perform your background task here
                var nextRetrievalTime = await DoWorkAsync(stoppingToken);

                _logger.LogInformation($"Next retrieval time is at: {nextRetrievalTime:O}, so waiting for {nextRetrievalTime - DateTime.UtcNow}.");
                await Task.Delay(nextRetrievalTime - DateTime.UtcNow, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                // Expected when the application stops
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError($"An unexpected error occurred in the background service: {ex}");
                await Task.Delay(TimeSpan.FromSeconds(AfterErrorDelayInSeconds), stoppingToken);
            }
        }

        _logger.LogInformation("Background Service is stopping.");
    }

    private async Task<DateTime> DoWorkAsync(CancellationToken stoppingToken)
    {
        using (var scope = _serviceScopeFactory.CreateScope())
        {
            var dataRetrievalService = scope.ServiceProvider.GetRequiredService<IDataRetrievalService>();

            var nextRetrievalTime = await dataRetrievalService.RetrieveDataAsync(stoppingToken);
            return nextRetrievalTime;
        }
    }
}
