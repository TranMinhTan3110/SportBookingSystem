using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using SportBookingSystem.Middlewares;
using SportBookingSystem.Helper;
using SportBookingSystem.Models.EF;
using SportBookingSystem.Models.Entities;
using SportBookingSystem.Services;

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
builder.Services.AddScoped<IEmailService, SmtpEmailService>();
builder.Services.AddScoped<ILoginServices, LoginServices>();
builder.Services.AddScoped<ICloudinaryService, CloudinaryService>();
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
builder.Services.AddSingleton<RabbitMQProducer>();

// Add services to the container.
builder.Services.AddControllersWithViews()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
    });
builder.Services.AddMemoryCache();

// Đăng ký Session cho giỏ hàng (CartService)
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.Name = ".SportBooking.Session";
});
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICartService, CartService>();

// Đăng ký Redis Cache
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("Redis") ?? "localhost:6379";
    options.InstanceName = "SportBooking_";
});
builder.Services.Configure<MailSettings>(builder.Configuration.GetSection("MailSettings"));
var app = builder.Build();

// Đăng ký Middleware chống lộ lọt ex.Message ra ngoài
app.UseMiddleware<GlobalExceptionMiddleware>();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseSession();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=TrangChu}/{action=Index}/{id?}");

// Tự động Migrate Database & Khởi tạo dữ liệu mẫu nếu Database trống
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.Migrate();

    // 1. Khởi tạo Roles nếu chưa có
    if (!db.Roles.Any())
    {
        db.Database.ExecuteSqlRaw("SET IDENTITY_INSERT Roles ON; INSERT INTO Roles (RoleId, RoleName) VALUES (1, 'Admin'), (2, 'User'); SET IDENTITY_INSERT Roles OFF;");
    }

    // 2. Khởi tạo tài khoản Admin mặc định nếu chưa có
    if (!db.Users.Any(u => u.RoleId == 1))
    {
        db.Users.Add(new Users
        {
            Username = "admin",
            Phone = "0900000000",
            Email = "admin@system.com",
            FullName = "Quản trị viên",
            Password = "$2a$11$GMFpp2eQHSfzhMhVoPJhH./vaJxC3wgA.Avl/I49EumGdOoq5gTUy", // Mật khẩu: 123456
            WalletBalance = 0,
            RewardPoints = 0,
            IsActive = true,
            RoleId = 1
        });
        db.SaveChanges();
    }

    // 3. Khởi tạo Danh mục & Sân bóng mẫu nếu chưa có
    if (!db.Categories.Any())
    {
        var catBongDa = new Categories { CategoryName = "Sân bóng đá", Type = "Pitch" };
        var catCauLong = new Categories { CategoryName = "Sân cầu lông", Type = "Pitch" };
        db.Categories.AddRange(catBongDa, catCauLong);
        db.SaveChanges();

        if (!db.Pitches.Any())
        {
            db.Pitches.AddRange(
                new Pitches
                {
                    PitchName = "Sân A2 (Bóng đá 5-7 người)",
                    CategoryId = catBongDa.CategoryId,
                    Capacity = 7,
                    PricePerHour = 200000,
                    Status = "Sẵn sàng",
                    Description = "Sân cỏ nhân tạo cao cấp, hệ thống đèn chiếu sáng ban đêm",
                    ImageUrl = "/asset/img/pitch_c739c6ec-9480-4695-8052-1100cdb4d60d.webp"
                },
                new Pitches
                {
                    PitchName = "Sân Cầu lông B1",
                    CategoryId = catCauLong.CategoryId,
                    Capacity = 4,
                    PricePerHour = 100000,
                    Status = "Sẵn sàng",
                    Description = "Sân thảm tiêu chuẩn thi đấu, thoáng mát",
                    ImageUrl = "/asset/img/pitch_d11ae77a-cda1-4992-bb20-5b9a4b3dc6d1.jpg"
                }
            );
            db.SaveChanges();
        }
    }
}

app.Run();