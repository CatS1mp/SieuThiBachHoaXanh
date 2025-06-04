namespace BachHoaXanh.Middleware
{
    public class SessionMiddleware
    {
        private readonly RequestDelegate _next;

        public SessionMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (context.User?.Identity?.IsAuthenticated == true)
            {
                context.Items["Username"] = context.User.Identity.Name;
                context.Items["Role"] = context.User.FindFirst("Role")?.Value;
                context.Items["UserID"] = context.User.FindFirst("UserID")?.Value;
            }

            await _next(context);
        }
    }
}