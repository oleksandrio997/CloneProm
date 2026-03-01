using CloneProm.Data;
using CloneProm.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using System;
using CloneProm.Models;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;

var builder = WebApplication.CreateBuilder(args);

//builder.Services.AddDbContext<ClonePromDbContext>(options =>
//options.UseInMemoryDatabase("TestClonePromDB"));

builder.Services.AddDbContext<ClonePromDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
        options.SignIn.RequireConfirmedAccount = false)
    .AddDefaultTokenProviders()
    .AddEntityFrameworkStores<ClonePromDbContext>();

builder.Services.AddRazorPages();

builder.Services.AddControllersWithViews()
    .AddRazorRuntimeCompilation();

builder.Services.AddSession();

// Add session support for cart and favorites (session-based storage)
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(6);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

using (var scope = app.Services.CreateScope())
{
    // seed roles/users first, then seed data that relies on them.
    try
    {
        // InitializeAdminAsync sets up roles and an admin user.
    }
    catch (Exception ex)
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("StartupSeed");
        logger.LogError(ex, "Error initializing admin/roles");
    }

    try
    {
        // Use new async initialize to seed categories/sellers/products
        await scope.ServiceProvider.InitializeAsync();
    }
    catch (Exception ex)
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("StartupSeed");
        logger.LogError(ex, "Error seeding initial data");
    }
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.UseSession();

// Session must be enabled after routing and before endpoints so it's available in controllers
app.UseSession();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

app.Run();
