using BachHoaXanh.Controllers;
using BachHoaXanh.Data;
using BachHoaXanh.Models;
using BachHoaXanh.Middleware;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

var builder = WebApplication.CreateBuilder(args);
<<<<<<< Updated upstream

=======
// Đặt Runtime.PythonDLL trước khi khởi tạo runtime
Runtime.PythonDLL = @"C:\Users\DELL\AppData\Local\Programs\Python\Python39\python39.dll";

// Cấu hình PythonHome và PythonPath
PythonEngine.PythonHome = @"C:\Users\DELL\AppData\Local\Programs\Python\Python39";
PythonEngine.PythonPath = string.Join(Path.PathSeparator, new[]
{
    @"C:\Users\DELL\AppData\Local\Programs\Python\Python39\Lib",
    @"C:\Users\DELL\AppData\Local\Programs\Python\Python39\Lib\site-packages",
    @"C:\Users\DELL\AppData\Local\Programs\Python\Python39\DLLs",
    Directory.GetCurrentDirectory()
});

try
{
    PythonEngine.Initialize();
    using (Py.GIL())
    {
        dynamic sys = Py.Import("sys");
        Console.WriteLine($"sys.executable: {sys.executable}");
        Console.WriteLine($"sys.prefix: {sys.prefix}");
        dynamic encodings = Py.Import("encodings");
        Console.WriteLine("Đã import encodings thành công");
        dynamic face_recognition = Py.Import("face_recognition");
        Console.WriteLine("Đã import face_recognition thành công");
    }
    Console.WriteLine("Python runtime khởi tạo thành công.");
}
catch (Exception ex)
{
    Console.WriteLine($"Lỗi khởi tạo Python runtime: {ex.Message}\nStackTrace: {ex.StackTrace}");
    throw;
}
>>>>>>> Stashed changes
builder.Services.AddControllers();
// Bind OtpSettings from appsettings.json
builder.Services.Configure<OtpSettings>(builder.Configuration.GetSection("OtpSettings"));

// Register EmailService
builder.Services.AddSingleton<IEmailService, EmailService>();

// Add services to the container
builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Adding logging service
builder.Services.AddLogging(logging =>
{
    logging.ClearProviders();
    logging.AddConsole();
    logging.AddDebug();
});

// Configure authentication (using only 1 Authentication Scheme - Cookies)
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.AccessDeniedPath = "/Home/Index"; // Redirect if access denied
        options.LoginPath = "/user/login"; // Redirect to login page if not authenticated
    });

// Add session
builder.Services.AddHttpContextAccessor();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Ensure middleware order is correct
app.UseSession();  // Enable session before authentication
app.UseMiddleware<SessionMiddleware>(); // Custom session middleware (make sure this is necessary)
app.UseMiddleware<CategoryMiddleware>();
// Authentication and authorization middleware
app.UseAuthentication(); // Ensure authentication is used before authorization
app.UseAuthorization();  // Ensure authorization comes after authentication


// Map default controller route
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
