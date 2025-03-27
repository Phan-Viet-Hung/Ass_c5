using C5.Data;
using C5.Models;
using C5.Models.Momo;
using C5.Service.Momo;
using C5.Service.VNPay;
using C5.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.Cookies;
using DotNetEnv;

var builder = WebApplication.CreateBuilder(args);
//MOMO API
builder.Services.Configure<MomoOptionModel>(builder.Configuration.GetSection("MomoAPI"));
builder.Services.AddScoped<IMomoService, MomoService>();
TimeZoneInfo timeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
TimeZoneInfo.ClearCachedData();
CultureInfo cultureInfo = new CultureInfo("vi-VN");

CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;
// Add services to the container.
// Thêm dịch vụ xác thực Google
Env.Load(); // Load biến từ file .env

var clientId = Env.GetString("CLIENT_ID");
var clientSecret = Env.GetString("CLIENT_SECRET");
var connectionString = Env.GetString("DATABASE_CONNECTION");

Console.WriteLine($"Client ID: {clientId}"); // Kiểm tra xem đã lấy đúng chưa

builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = GoogleDefaults.AuthenticationScheme;
})
.AddCookie()
.AddGoogle(options =>
{
    options.ClientId = Environment.GetEnvironmentVariable("CLIENT_ID");
    options.ClientSecret = Environment.GetEnvironmentVariable("CLIENT_SECRET");

    options.CallbackPath = new PathString("/signin-google");
    options.AuthorizationEndpoint = "https://accounts.google.com/o/oauth2/auth";
    options.TokenEndpoint = "https://accounts.google.com/o/oauth2/token";
    options.UserInformationEndpoint = "https://www.googleapis.com/oauth2/v2/userinfo";
    options.SaveTokens = true;
});

builder.Services.AddSignalR();
builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<FastFoodDbContext>(options =>
options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddIdentity<FastFoodUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 8;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireLowercase = true;

    //c?u h?nh lockout

    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;

    //C?u h?nh user
    options.User.RequireUniqueEmail = true;
    //options.SignIn.RequireConfirmedAccount = false;
})
            .AddEntityFrameworkStores<FastFoodDbContext>()
            .AddDefaultTokenProviders();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        policy => policy.AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader());
});
builder.Services.AddScoped<IProductService, ProductService>();


//VNPay API
builder.Services.AddHttpClient();
builder.Services.AddScoped<IVnPayService, VnPayService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
    app.UseSwagger();
}


app.UseCors("AllowAll");

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.UseEndpoints(endpoints =>
{
    endpoints.MapHub<NotificationHub>("/notificationHub"); // Định nghĩa route cho Hub
    endpoints.MapControllers();
});
app.Run();
