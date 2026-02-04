using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using SportBookingSystem.Helper;
using SportBookingSystem.Models.EF;
using SportBookingSystem.Services; 
using SportBookingSystem.Helper;

var builder = WebApplication.CreateBuilder(args);

// Đăng ký DbContext 
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

// Đăng ký Authentication với Cookie 
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Login/SignIn"; 
        options.AccessDeniedPath = "/Login/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
    });

// Đăng ký Dependency Injection cho LoginServices 
builder.Services.AddScoped<IEmailService, SendGridEmailService>();
builder.Services.AddScoped<ILoginServices, LoginServices>();
builder.Services.AddScoped<IPitchService, PitchService>();
builder.Services.AddScoped<IBookingService, BookingService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IFoodADService, FoodADService>();
builder.Services.AddScoped<IFoodService, FoodService>();
builder.Services.AddScoped<ITransactionService, TransactionService>();
builder.Services.AddScoped<ITransactionUserService, TransactionUserService>();
builder.Services.AddScoped<IReportService, ReportService>();
builder.Services.AddScoped<ISportProductService, SportProductService>();
builder.Services.AddScoped<IProfileService, ProfileService>();
builder.Services.AddScoped<IQrService, QrService>();
builder.Services.AddScoped<IPurchaseService, PurchaseService>();
builder.Services.AddScoped<IWalletService, WalletService>();
builder.Services.AddScoped<IVnPayService, VnPayService>();
builder.Services.AddHostedService<OrderCancellationBackgroundService>();
builder.Services.AddHostedService<BookingAutoCancellationService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.Configure<MailSettings>(builder.Configuration.GetSection("MailSettings"));
// Add services to the container.
builder.Services.AddControllersWithViews()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
    });
builder.Services.AddMemoryCache();

builder.Services.Configure<MailSettings>(builder.Configuration.GetSection("MailSettings"));

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=TrangChu}/{action=Index}/{id?}");

app.Run();