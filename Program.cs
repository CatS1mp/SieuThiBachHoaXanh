using BachHoaXanh.Controllers;
using BachHoaXanh.Data;
using BachHoaXanh.Models;
using BachHoaXanh.Middleware;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Python.Runtime;
using BachHoaXanh.Services;

var builder = WebApplication.CreateBuilder(args);
// Đặt Runtime.PythonDLL trước khi khởi tạo runtime
Runtime.PythonDLL = @"C:\Users\DELL\AppData\Local\Programs\Python\Python39\python39.dll";

// Register the background service

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
builder.Services.AddControllers();
builder.Services.AddControllersWithViews();

builder.Services.AddHostedService<StockCheckService>();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddLogging(logging =>
{
    logging.ClearProviders();
    logging.AddConsole();
    logging.AddDebug();
});

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.AccessDeniedPath = "/Home/Index";
        options.LoginPath = "/user/login";
    });

builder.Services.AddHttpClient();
builder.Services.AddHttpContextAccessor();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseMiddleware<SessionMiddleware>();
app.UseMiddleware<CategoryMiddleware>();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
