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
            // Check if the user is authenticated and not null
            if (context.User?.Identity?.IsAuthenticated == true)
            {
                context.Items["Username"] = context.User.Identity.Name;
                context.Items["Role"] = context.User.FindFirst("Role")?.Value;
                context.Items["UserID"] = context.User.FindFirst("UserID")?.Value;
            }

            // Proceed to the next middleware
            await _next(context);
        }

    }
}
