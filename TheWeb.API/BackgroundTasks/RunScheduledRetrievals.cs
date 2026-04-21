using TheWeb.API.Exceptions;

public class RunScheduledRetrievals : BackgroundService
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ILogger<RunScheduledRetrievals> _logger;
    const int DelayInSeconds = 5;

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

                _logger.LogInformation($"Next retrieval time is at: {nextRetrievalTime:O}, so waiting for {nextRetrievalTime - DateTime.UtcNow}." );
                await Task.Delay(nextRetrievalTime - DateTime.UtcNow, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                // Expected when the application stops
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError($"An error occurred in the background service: {ex}");
                await Task.Delay(TimeSpan.FromSeconds(DelayInSeconds), stoppingToken);
            }
        }

        _logger.LogInformation("Background Service is stopping.");
    }

    private async Task<DateTime> DoWorkAsync(CancellationToken stoppingToken)
    {
        using (var scope = _serviceScopeFactory.CreateScope())
        {
            var dataRetrievalService = scope.ServiceProvider.GetRequiredService<IDataRetrievalService>();
            try
            {
                var nextRetrievalTime = await dataRetrievalService.RetrieveDataAsync(stoppingToken);
                return nextRetrievalTime;
            }
            catch (ToEarlyException ex)
            {
                _logger.LogInformation($"ToEarlyException occurred: {ex.Message}. Next retrieval time is at: {ex.NextRetrievalTime:O}, so waiting for {ex.NextRetrievalTime - DateTime.UtcNow}." );
                return ex.NextRetrievalTime;
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving data.");
                return DateTime.UtcNow.AddMinutes(5); // In case something goes wrong, try again in 5 minutes
            }
        }
    }
}
