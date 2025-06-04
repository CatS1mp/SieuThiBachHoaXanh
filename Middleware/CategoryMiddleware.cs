using BachHoaXanh.Data;
using Microsoft.EntityFrameworkCore;

namespace BachHoaXanh.Middleware
{
    public class CategoryMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IServiceScopeFactory _scopeFactory;

        public CategoryMiddleware(RequestDelegate next, IServiceScopeFactory scopeFactory)
        {
            _next = next;
            _scopeFactory = scopeFactory;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (context.Items["Categories"] == null)
            {
                using (var scope = _scopeFactory.CreateScope())
                {
                    var _context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                    var categories = _context.CategoryList.Include(c => c.SubCategories).ToList();
                    context.Items["Categories"] = categories;
                }
            }

            await _next(context);
        }
    }
}