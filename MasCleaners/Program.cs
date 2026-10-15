using MasCleaners.DataConnection;
using MasCleaners.Interfaces;
using MasCleaners.Models;
using MasCleaners.Repositories;
using MasCleaners.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);


// =========================================================
// DATABASE
// =========================================================

builder.Services.AddDbContext<SqlDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString(
            "DefaultConnection")));


// =========================================================
// IDENTITY
// =========================================================

builder.Services
    .AddIdentity<ApplicationUser, IdentityRole>(options =>
    {
        // Password settings
        options.Password.RequireDigit = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequiredLength = 8;

        // User settings
        options.User.RequireUniqueEmail = true;

        // Sign-in settings
        options.SignIn.RequireConfirmedAccount = false;
    })
    .AddEntityFrameworkStores<SqlDbContext>()
    .AddDefaultTokenProviders();

// ======================================================
// AUTHENTICATION COOKIE
// ======================================================

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";

    options.Cookie.Name = "MASCleaners.Auth";
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.SameSite = SameSiteMode.Lax;

    options.ExpireTimeSpan = TimeSpan.FromHours(8);
    options.SlidingExpiration = true;
});
// =========================================================
// MVC
// =========================================================

builder.Services.AddControllersWithViews();


// =========================================================
// UNIT OF WORK
// =========================================================

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<CartService>();

var app = builder.Build();


// =========================================================
// HTTP REQUEST PIPELINE
// =========================================================

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");

    app.UseHsts();
}


app.UseHttpsRedirection();

app.UseRouting();


// =========================================================
// AUTHENTICATION
// =========================================================

app.UseAuthentication();


// =========================================================
// AUTHORIZATION
// =========================================================

app.UseAuthorization();


// =========================================================
// STATIC FILES
// =========================================================

app.MapStaticAssets();


// =========================================================
// DEFAULT ROUTE
// =========================================================

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();
// ======================================================
// SEED IDENTITY ROLES
// ======================================================

var startupLogger = app.Services
    .GetRequiredService<ILogger<Program>>();

await IdentitySeeder.SeedRolesAsync(
    app.Services,
    startupLogger);

// ======================================================
// SEED INITIAL SUPER USER
// ======================================================

await IdentitySeeder.SeedSuperUserAsync(
    app.Services,
    startupLogger);

app.Run();