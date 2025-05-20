using DocumentsProject.Core.Domain.Models;
using DocumentsProject.Core.Domain.Services;
using DocumentsProject.Core.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;

try
{
    var builder = WebApplication.CreateBuilder(args);

    ConfigureServices(builder);

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

    app.UseCors();

    // Rate Limiting Middleware
    app.UseRateLimiter();

    // Authentication and Authorization
    app.UseAuthentication();
    app.UseAuthorization();

    // Session Middleware
    app.UseSession();

    // Map routes
    app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}");

    app.Run();
}
catch (Exception ex)
{
    Console.WriteLine(ex.ToString());
}
finally
{

}

void ConfigureServices(WebApplicationBuilder builder)
{
    builder.Services.AddHttpClient();

    builder.Services.Configure<ForwardedHeadersOptions>(options =>
    {
        options.ForwardedHeaders =
            ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
        // Only loopback proxies are allowed by default.
        // Clear that restriction because forwarders are enabled by explicit 
        // configuration.
        options.KnownNetworks.Clear();
        options.KnownProxies.Clear();
    });

    builder.Services.AddCors();

    // Add services to the container.

    builder.Services.AddRateLimiter(options =>
    {
        options.AddFixedWindowLimiter(policyName: "fixed", opt =>
        {
            opt.PermitLimit = 5;
            opt.Window = TimeSpan.FromSeconds(10);
            opt.QueueLimit = 5;
        });
        options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    });

    builder.Services.AddControllersWithViews();

    

    builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
        .AddCookie(options =>
        {
            options.LoginPath = "/Login/Index"; // Redirect path if not authenticated
            options.LogoutPath = "/Logout/Index";
            options.AccessDeniedPath = "/AccessDenied";  // Path to redirect when access is denied
            options.ExpireTimeSpan = TimeSpan.FromMinutes(10);
            options.SlidingExpiration = true;
        });

    builder.Services.AddAuthorization();

    builder.Services.AddDbContext<DocumentdbContext>(options =>
        options.UseMySql(builder.Configuration.GetConnectionString("MVCConnection"), new MySqlServerVersion("8.0.12-mysql")));

    builder.Services.AddScoped<ILoginService, LoginService>();
    builder.Services.AddScoped<IUserService, UserService>();
    builder.Services.AddScoped<IDocumentService, DocumentService>();
    builder.Services.AddScoped<IMailService, MailService>();

    builder.Services.AddSession(options =>
    {
        options.IdleTimeout = TimeSpan.FromMinutes(30); // Set the timeout for sessions
        options.Cookie.HttpOnly = true;
        options.Cookie.IsEssential = true; // Mark the session cookie as essential
    });

    builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
    {
        var configuration = builder.Configuration.GetSection("Redis")["ConnectionString"];
        return ConnectionMultiplexer.Connect(configuration);
    });

    builder.Services.AddScoped<IRedisCacheService, RedisCacheService>();


}
