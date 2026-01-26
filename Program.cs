using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using SportBookingSystem.Models.EF;
using SportBookingSystem.Services; 

var builder = WebApplication.CreateBuilder(args);

// 1. Đăng ký DbContext 
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

// 2. Đăng ký Authentication với Cookie 
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Login/SignIn"; 
        options.AccessDeniedPath = "/Login/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
    });

// 3. Đăng ký Dependency Injection cho LoginServices 
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
// Add services to the container.
builder.Services.AddControllersWithViews()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
    });
builder.Services.AddMemoryCache();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// 4. Bật Authentication trước Authorization 
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Login}/{action=SignIn}/{id?}");

app.Run();