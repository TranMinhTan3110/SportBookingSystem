using SportBookingSystem.Services;

namespace SportBookingSystem.Services
{
    public class OrderCancellationBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly TimeSpan _checkInterval = TimeSpan.FromMinutes(1);

        public OrderCancellationBackgroundService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var transactionService = scope.ServiceProvider.GetRequiredService<ITransactionService>();
                        await transactionService.CancelExpiredOrdersAsync();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error in background service: {ex.Message}");
                }

                await Task.Delay(_checkInterval, stoppingToken);
            }
        }
    }
}
