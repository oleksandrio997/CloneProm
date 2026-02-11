using CloneProm.Models;
using Microsoft.AspNetCore.Identity;
using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CloneProm.Data
{
    public static class SeedData
    {
        public static void Initialize(IServiceProvider services)
        {
            using var scope = services.CreateScope();
            var provider = scope.ServiceProvider;
            try
            {
                var ctx = provider.GetRequiredService<ClonePromDbContext>();
                ctx.Database.EnsureCreated();

                // if any products exist, assume DB already seeded
                if (ctx.Products.Any()) return;

                // ensure categories exist (idempotent)
                string[] catNames = { "Electronics", "Home Appliances", "Books", "Clothing", "Toys" };
                foreach (var name in catNames)
                {
                    if (!ctx.Categories.Any(c => c.Name == name))
                        ctx.Categories.Add(new Models.Category { Name = name });
                }
                ctx.SaveChanges();

                // ensure sellers
                (string id, string shop, string desc)[] sellers = {
                    ("system","Default Shop","Seeded seller"),
                    ("shop2","HomeGoods","Household items"),
                    ("bookseller","Books & Co.","Bookseller")
                };
                foreach (var s in sellers)
                {
                    if (!ctx.Sellers.Any(x => x.ShopName == s.shop))
                        ctx.Sellers.Add(new Models.Seller { UserId = s.id, ShopName = s.shop, Description = s.desc });
                }
                ctx.SaveChanges();

                // get references
                var electronics = ctx.Categories.First(c => c.Name == "Electronics");
                var appliances = ctx.Categories.First(c => c.Name == "Home Appliances");
                var books = ctx.Categories.First(c => c.Name == "Books");
                var clothing = ctx.Categories.First(c => c.Name == "Clothing");
                var toys = ctx.Categories.First(c => c.Name == "Toys");

                var seller1 = ctx.Sellers.First(s => s.ShopName == "Default Shop");
                var seller2 = ctx.Sellers.First(s => s.ShopName == "HomeGoods");
                var seller3 = ctx.Sellers.First(s => s.ShopName == "Books & Co.");

                // idempotent add products by name
                void AddIfMissing(string name, Models.Product product)
                {
                    if (!ctx.Products.Any(p => p.Name == name)) ctx.Products.Add(product);
                }

                AddIfMissing("Notebook Alpha", new Models.Product { Name = "Notebook Alpha", Description = "Лёгкий ноутбук для повседневных задач.", Price = 149.99m, Quantity = 10, ImagePath = "https://via.placeholder.com/300x200?text=Notebook", Category = electronics, Seller = seller1 });
                AddIfMissing("Wireless Mouse Mini", new Models.Product { Name = "Wireless Mouse Mini", Description = "Компактная эргономичная беспроводная мышь.", Price = 24.50m, Quantity = 50, ImagePath = "https://via.placeholder.com/300x200?text=Mouse", Category = electronics, Seller = seller1 });
                AddIfMissing("Mechanical Keyboard X", new Models.Product { Name = "Mechanical Keyboard X", Description = "Механическая клавиатура с тактильными переключателями.", Price = 89.90m, Quantity = 20, ImagePath = "https://via.placeholder.com/300x200?text=Keyboard", Category = electronics, Seller = seller1 });

                AddIfMissing("Air Purifier Pro", new Models.Product { Name = "Air Purifier Pro", Description = "Компактный очиститель воздуха для дома.", Price = 129.00m, Quantity = 25, ImagePath = "https://via.placeholder.com/300x200?text=Air+Purifier", Category = appliances, Seller = seller2 });
                AddIfMissing("Blender 500W", new Models.Product { Name = "Blender 500W", Description = "Мощный блендер для смузи и супов.", Price = 59.99m, Quantity = 40, ImagePath = "https://via.placeholder.com/300x200?text=Blender", Category = appliances, Seller = seller2 });

                AddIfMissing("C# in Depth", new Models.Product { Name = "C# in Depth", Description = "Практическое руководство по C#.", Price = 39.99m, Quantity = 100, ImagePath = "https://via.placeholder.com/300x200?text=Book", Category = books, Seller = seller3 });
                AddIfMissing("Cooking Basics", new Models.Product { Name = "Cooking Basics", Description = "Книга рецептов для начинающих.", Price = 19.50m, Quantity = 60, ImagePath = "https://via.placeholder.com/300x200?text=Cookbook", Category = books, Seller = seller3 });

                AddIfMissing("Classic T-Shirt", new Models.Product { Name = "Classic T-Shirt", Description = "Удобная хлопковая футболка.", Price = 14.99m, Quantity = 200, ImagePath = "https://via.placeholder.com/300x200?text=T-Shirt", Category = clothing, Seller = seller1 });
                AddIfMissing("Denim Jeans", new Models.Product { Name = "Denim Jeans", Description = "Классические джинсы строгого кроя.", Price = 49.99m, Quantity = 80, ImagePath = "https://via.placeholder.com/300x200?text=Jeans", Category = clothing, Seller = seller1 });

                AddIfMissing("Building Blocks Set", new Models.Product { Name = "Building Blocks Set", Description = "Набор конструктора для детей.", Price = 29.99m, Quantity = 150, ImagePath = "https://via.placeholder.com/300x200?text=Blocks", Category = toys, Seller = seller2 });
                AddIfMissing("Remote Car Racer", new Models.Product { Name = "Remote Car Racer", Description = "Машинка на радиоуправлении для детей.", Price = 49.99m, Quantity = 70, ImagePath = "https://via.placeholder.com/300x200?text=Car", Category = toys, Seller = seller2 });

                ctx.SaveChanges();
            }
            catch (Exception ex)
            {
                var logger = provider.GetRequiredService<ILoggerFactory>().CreateLogger("SeedData");
                logger.LogError(ex, "Error seeding the database.");
            }
        }
        public static async Task InitializeAdminAsync(this IServiceProvider services)
        {
            var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

            string[] roles = { "Admin", "Seller", "User" };
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                    await roleManager.CreateAsync(new IdentityRole(role));
            }

            string adminEmail = "admin@cloneprom.com";
            string adminPassword = "Admin123!";

            if (await userManager.FindByEmailAsync(adminEmail) == null)
            {
                var admin = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail
                };
                var result = await userManager.CreateAsync(admin, adminPassword);
                if (result.Succeeded)
                    await userManager.AddToRoleAsync(admin, "Admin");
            }
        }
    }
}
