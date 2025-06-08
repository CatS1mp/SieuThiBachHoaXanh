using System;
using BachHoaXanh.Data;

namespace BachHoaXanh.Services
{
    public class OrderExpirationService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly TimeSpan _interval = TimeSpan.FromMinutes(5); // kiểm tra mỗi 5 phút

        public OrderExpirationService(IServiceProvider serviceProvider)
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
                        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                        var expiredOrders = context.OrderList
                            .Where(o => o.OrderStatus != null && o.OrderStatus == "Pending" && o.CreatedAt <= DateTime.Now.AddHours(-1))
                            .ToList();

                        foreach (var order in expiredOrders)
                        {
                            if (order != null)
                            {
                                order.CanCancel = false;
                                order.UpdatedAt = DateTime.Now;
                            }
                        }

                        await context.SaveChangesAsync();
                    }
                }
                catch (Exception ex)
                {
                    // Log error but continue running
                    Console.WriteLine($"Error in OrderExpirationService: {ex.Message}");
                }

                await Task.Delay(_interval, stoppingToken);
            }
        }
    }

}
