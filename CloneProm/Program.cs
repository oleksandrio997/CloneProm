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
            // create categories
            var electronics = new Category { Name = "Electronics" };
            var appliances = new Category { Name = "Home Appliances" };
            var books = new Category { Name = "Books" };
            var clothing = new Category { Name = "Clothing" };
            var toys = new Category { Name = "Toys" };

            ctx.Categories.AddRange(electronics, appliances, books, clothing, toys);

            // sellers
            var seller1 = new Seller { UserId = "system", ShopName = "Default Shop", Description = "Seeded seller" };
            var seller2 = new Seller { UserId = "shop2", ShopName = "HomeGoods", Description = "Household items" };
            var seller3 = new Seller { UserId = "bookseller", ShopName = "Books & Co.", Description = "Bookseller" };
            ctx.Sellers.AddRange(seller1, seller2, seller3);

            // products across categories
            ctx.Products.AddRange(
                // Electronics
                new Product { Name = "Notebook Alpha", Description = "Лёгкий ноутбук для повседневных задач.", Price = 149.99m, Quantity = 10, ImagePath = "https://via.placeholder.com/300x200?text=Notebook", Category = electronics, Seller = seller1 },
                new Product { Name = "Wireless Mouse Mini", Description = "Компактная эргономичная беспроводная мышь.", Price = 24.50m, Quantity = 50, ImagePath = "https://via.placeholder.com/300x200?text=Mouse", Category = electronics, Seller = seller1 },
                new Product { Name = "Mechanical Keyboard X", Description = "Механическая клавиатура с тактильными переключателями.", Price = 89.90m, Quantity = 20, ImagePath = "https://via.placeholder.com/300x200?text=Keyboard", Category = electronics, Seller = seller1 },

                // Home Appliances
                new Product { Name = "Air Purifier Pro", Description = "Компактный очиститель воздуха для дома.", Price = 129.00m, Quantity = 25, ImagePath = "https://via.placeholder.com/300x200?text=Air+Purifier", Category = appliances, Seller = seller2 },
                new Product { Name = "Blender 500W", Description = "Мощный блендер для смузи и супов.", Price = 59.99m, Quantity = 40, ImagePath = "https://via.placeholder.com/300x200?text=Blender", Category = appliances, Seller = seller2 },

                // Books
                new Product { Name = "C# in Depth", Description = "Практическое руководство по C#.", Price = 39.99m, Quantity = 100, ImagePath = "https://via.placeholder.com/300x200?text=Book", Category = books, Seller = seller3 },
                new Product { Name = "Cooking Basics", Description = "Книга рецептов для начинающих.", Price = 19.50m, Quantity = 60, ImagePath = "https://via.placeholder.com/300x200?text=Cookbook", Category = books, Seller = seller3 },

                // Clothing
                new Product { Name = "Classic T-Shirt", Description = "Удобная хлопковая футболка.", Price = 14.99m, Quantity = 200, ImagePath = "https://via.placeholder.com/300x200?text=T-Shirt", Category = clothing, Seller = seller1 },
                new Product { Name = "Denim Jeans", Description = "Классические джинсы строгого кроя.", Price = 49.99m, Quantity = 80, ImagePath = "https://via.placeholder.com/300x200?text=Jeans", Category = clothing, Seller = seller1 },

                // Toys
                new Product { Name = "Building Blocks Set", Description = "Набор конструктора для детей.", Price = 29.99m, Quantity = 150, ImagePath = "https://via.placeholder.com/300x200?text=Blocks", Category = toys, Seller = seller2 },
                new Product { Name = "Remote Car Racer", Description = "Машинка на радиоуправлении для детей.", Price = 49.99m, Quantity = 70, ImagePath = "https://via.placeholder.com/300x200?text=Car", Category = toys, Seller = seller2 }
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
