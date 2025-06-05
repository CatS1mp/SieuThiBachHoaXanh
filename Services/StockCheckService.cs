using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BachHoaXanh.Models;
using Microsoft.EntityFrameworkCore;
using BachHoaXanh.Data;

namespace BachHoaXanh.Services
{
    public class StockCheckService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<StockCheckService> _logger;

        public StockCheckService(IServiceProvider serviceProvider, ILogger<StockCheckService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    _logger.LogInformation("StockCheckService running at: {time}", DateTimeOffset.Now);

                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                        var products = await context.ProductList
                            .Include(p => p.Stocks)
                            .ToListAsync(stoppingToken);

                        foreach (var product in products)
                        {
                            int stockQuantity = product.StockQuantity;

                            if (stockQuantity == 0 && product.Status != ProductStatus.TamHetHang)
                            {
                                product.Status = ProductStatus.TamHetHang;
                                product.UpdatedAt = DateTime.Now;
                                Console.WriteLine("Product {0} ({1}) set to TamHetHang due to zero stock.",
                                    product.ProductID, product.ProductName ?? "Unnamed Product");
                            }
                            else if (stockQuantity > 0 && product.Status == ProductStatus.TamHetHang)
                            {
                                product.Status = ProductStatus.KinhDoanh;
                                product.UpdatedAt = DateTime.Now;
                                Console.WriteLine("Product {0} ({1}) set to KinhDoanh due to available stock.",
                                    product.ProductID, product.ProductName ?? "Unnamed Product");
                            }
                        }

                        await context.SaveChangesAsync(stoppingToken);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An error occurred while checking stock quantities.");
                }

                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
        }
    }
}