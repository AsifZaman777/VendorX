using VendorX.Services;

namespace VendorX.BackgroundServices
{
    public class FixedExpenseBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<FixedExpenseBackgroundService> _logger;
        private readonly TimeSpan _period = TimeSpan.FromHours(1); // Run every hour

        public FixedExpenseBackgroundService(
            IServiceProvider serviceProvider,
            ILogger<FixedExpenseBackgroundService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Fixed Expense Background Service is starting");

            using PeriodicTimer timer = new PeriodicTimer(_period);

            while (!stoppingToken.IsCancellationRequested && await timer.WaitForNextTickAsync(stoppingToken))
            {
                try
                {
                    _logger.LogInformation("Fixed Expense Background Service is running at: {time}", DateTimeOffset.Now);

                    using var scope = _serviceProvider.CreateScope();
                    var fixedExpenseService = scope.ServiceProvider.GetRequiredService<IFixedExpenseService>();
                    
                    await fixedExpenseService.GenerateDueExpensesAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error in Fixed Expense Background Service: {ex.Message}");
                }
            }

            _logger.LogInformation("Fixed Expense Background Service is stopping");
        }
    }
}
