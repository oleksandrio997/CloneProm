using CloneProm.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using System;
using CloneProm.Models;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;

var builder = WebApplication.CreateBuilder(args);

// Use in-memory database in Development to avoid local SQL setup issues
if (builder.Environment.IsDevelopment())
{
    builder.Services.AddDbContext<ClonePromDbContext>(options =>
        options.UseInMemoryDatabase("TestClonePromDB"));
}
else
{
    builder.Services.AddDbContext<ClonePromDbContext>(options =>
        options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
}

builder.Services.AddDefaultIdentity<IdentityUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
})
.AddEntityFrameworkStores<ClonePromDbContext>();

builder.Services.AddControllersWithViews()
    .AddRazorRuntimeCompilation();

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

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// Session must be enabled after routing and before endpoints so it's available in controllers
app.UseSession();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapControllers();
app.MapRazorPages();

// Seed sample data when using InMemory DB (development)
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var ctx = services.GetRequiredService<ClonePromDbContext>();
        // Ensure database created
        ctx.Database.EnsureCreated();

        if (!ctx.Categories.Any())
        {
            var cat = new Category { Name = "Electronics" };
            ctx.Categories.Add(cat);

            var seller = new Seller { UserId = "system", ShopName = "Default Shop", Description = "Seeded seller" };
            ctx.Sellers.Add(seller);

            ctx.Products.AddRange(
                new Product { Name = "Notebook Alpha", Description = "Лёгкий ноутбук для повседневных задач.", Price = 149.99m, Quantity = 10, ImagePath = "https://via.placeholder.com/300x200", Category = cat, Seller = seller },
                new Product { Name = "Wireless Mouse Mini", Description = "Компактная эргономичная беспроводная мышь.", Price = 24.50m, Quantity = 50, ImagePath = "https://via.placeholder.com/300x200", Category = cat, Seller = seller },
                new Product { Name = "Mechanical Keyboard X", Description = "Механическая клавиатура с тактильными переключателями.", Price = 89.90m, Quantity = 20, ImagePath = "https://via.placeholder.com/300x200", Category = cat, Seller = seller },
                new Product { Name = "USB-C Charger 30W", Description = "Быстрое зарядное устройство для телефонов и планшетов.", Price = 19.99m, Quantity = 100, ImagePath = "https://via.placeholder.com/300x200", Category = cat, Seller = seller },
                new Product { Name = "Noise-Cancel Headphones", Description = "Комфортные наушники с активным шумоподавлением.", Price = 129.00m, Quantity = 15, ImagePath = "https://via.placeholder.com/300x200", Category = cat, Seller = seller }
            );

            ctx.SaveChanges();
        }
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Error seeding the database.");
    }
}

app.Run();
