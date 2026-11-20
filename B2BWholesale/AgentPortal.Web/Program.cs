
using B2BWholesale.Data;
using B2BWholesale.Data.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

// Database của Quý - GIỮ NGUYÊN
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")
    )
);

// AN - TASK 3.2: catalogue + transactional order placement
builder.Services.AddScoped<ICatalogService, CatalogService>();
builder.Services.AddScoped<IOrderPlacementService, OrderPlacementService>();

// MVC
builder.Services.AddControllersWithViews();

// PHÚC - TASK 3.1: Cookie Authentication
builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/AccessDenied";

        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;

        options.ExpireTimeSpan = TimeSpan.FromHours(2);
        options.SlidingExpiration = true;
    });

// PHÚC - TASK 3.1: Authorization
builder.Services.AddAuthorization();

var app = builder.Build();

// Error handling
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseRouting();

// PHÚC - TASK 3.1: Middleware xác thực
app.UseAuthentication();
app.UseAuthorization();

// Static assets - GIỮ NGUYÊN
app.MapStaticAssets();

// MVC routing - GIỮ NGUYÊN
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
