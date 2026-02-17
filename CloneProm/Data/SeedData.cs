using CloneProm.Models;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;
using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CloneProm.Data
{
    public static class SeedData
    {
        public static async Task InitializeAsync(this IServiceProvider services)
        {
            using var scope = services.CreateScope();
            var provider = scope.ServiceProvider;
            try
            {
                var ctx = provider.GetRequiredService<ClonePromDbContext>();
                // Prefer migrations in production-like DBs. Apply pending migrations before seeding.
                ctx.Database.Migrate();

                // do not short-circuit seeding when some products exist; keep idempotent AddIfMissing
                // (previous behavior could skip adding missing seed items if Products.Any() was true)

                // ensure categories exist (idempotent)
                string[] catNames = { "Electronics", "Home Appliances", "Books", "Clothing", "Toys" };
                foreach (var name in catNames)
                {
                    if (!ctx.Categories.Any(c => c.Name == name))
                        ctx.Categories.Add(new Models.Category { Name = name });
                }
                ctx.SaveChanges();
                var loggerInfo = provider.GetRequiredService<ILoggerFactory>().CreateLogger("SeedData");
                loggerInfo.LogInformation("SeedData: categories={CountCats}, sellers={CountSellers}, products={CountProds}", ctx.Categories.Count(), ctx.Sellers.Count(), ctx.Products.Count());

                // Ensure admin and roles/users exist before creating sellers that reference users.
                // Try to create minimal users if they don't exist to satisfy FK constraints.
                var userManager = provider.GetService<UserManager<ApplicationUser>>();
                if (userManager != null)
                {
                    // desired seed users and their emails
                    string[] seedEmails = { "system@local", "shop2@local", "bookseller@local" };

                    foreach (var email in seedEmails)
                    {
                        // Find by email first; if doesn't exist, create with fixed password
                        var existing = await userManager.FindByEmailAsync(email).ConfigureAwait(false);
                        if (existing == null)
                        {
                            var u = new ApplicationUser { UserName = email, Email = email };
                            var createRes = await userManager.CreateAsync(u, "ChangeMe123!").ConfigureAwait(false);
                            // we don't need to store the Id here; sellers will be created without UserId
                        }
                    }
                }

                // ensure sellers and associate them with the created users (UserId is non-nullable FK)
                (string email, string shop, string desc)[] sellers = {
                    ("system@local","Default Shop","Seeded seller"),
                    ("shop2@local","HomeGoods","Household items"),
                    ("bookseller@local","Books & Co.","Bookseller")
                };

                foreach (var s in sellers)
                {
                    // find corresponding application user
                    var user = await userManager.FindByEmailAsync(s.email).ConfigureAwait(false);
                    var userId = user?.Id;

                    if (userId == null)
                    {
                        // fallback: try to find any user with the same shop name in Sellers (unlikely) or skip
                        // Log warning and skip creating seller to avoid FK/NOT NULL violations
                        var lg = provider.GetRequiredService<ILoggerFactory>().CreateLogger("SeedData");
                        lg.LogWarning("No ApplicationUser found for seller email {Email}; creating seller without user will fail. Skipping.", s.email);
                        continue;
                    }

                    if (!ctx.Sellers.Any(x => x.ShopName == s.shop))
                        ctx.Sellers.Add(new Models.Seller { UserId = userId, ShopName = s.shop, Description = s.desc });
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

                AddIfMissing("Notebook Alpha", new Models.Product { Name = "Notebook Alpha", Description = "Лёгкий ноутбук для повседневных задач.", Price = 149.99m, Quantity = 10, ImagePath = "https://via.placeholder.com/300x200?text=Notebook", CategoryId = electronics.Id, SellerId = seller1.Id });
                AddIfMissing("Wireless Mouse Mini", new Models.Product { Name = "Wireless Mouse Mini", Description = "Компактная эргономичная беспроводная мышь.", Price = 24.50m, Quantity = 50, ImagePath = "https://via.placeholder.com/300x200?text=Mouse", CategoryId = electronics.Id, SellerId = seller1.Id });
                AddIfMissing("Mechanical Keyboard X", new Models.Product { Name = "Mechanical Keyboard X", Description = "Механическая клавиатура с тактильными переключателями.", Price = 89.90m, Quantity = 20, ImagePath = "https://via.placeholder.com/300x200?text=Keyboard", CategoryId = electronics.Id, SellerId = seller1.Id });

                AddIfMissing("Air Purifier Pro", new Models.Product { Name = "Air Purifier Pro", Description = "Компактный очиститель воздуха для дома.", Price = 129.00m, Quantity = 25, ImagePath = "https://via.placeholder.com/300x200?text=Air+Purifier", CategoryId = appliances.Id, SellerId = seller2.Id });
                AddIfMissing("Blender 500W", new Models.Product { Name = "Blender 500W", Description = "Мощный блендер для смузи и супов.", Price = 59.99m, Quantity = 40, ImagePath = "https://via.placeholder.com/300x200?text=Blender", CategoryId = appliances.Id, SellerId = seller2.Id });

                AddIfMissing("C# in Depth", new Models.Product { Name = "C# in Depth", Description = "Практическое руководство по C#.", Price = 39.99m, Quantity = 100, ImagePath = "https://via.placeholder.com/300x200?text=Book", CategoryId = books.Id, SellerId = seller3.Id });
                AddIfMissing("Cooking Basics", new Models.Product { Name = "Cooking Basics", Description = "Книга рецептов для начинающих.", Price = 19.50m, Quantity = 60, ImagePath = "https://via.placeholder.com/300x200?text=Cookbook", CategoryId = books.Id, SellerId = seller3.Id });

                AddIfMissing("Classic T-Shirt", new Models.Product { Name = "Classic T-Shirt", Description = "Удобная хлопковая футболка.", Price = 14.99m, Quantity = 200, ImagePath = "https://via.placeholder.com/300x200?text=T-Shirt", CategoryId = clothing.Id, SellerId = seller1.Id });
                AddIfMissing("Denim Jeans", new Models.Product { Name = "Denim Jeans", Description = "Классические джинсы строгого кроя.", Price = 49.99m, Quantity = 80, ImagePath = "https://via.placeholder.com/300x200?text=Jeans", CategoryId = clothing.Id, SellerId = seller1.Id });

                AddIfMissing("Building Blocks Set", new Models.Product { Name = "Building Blocks Set", Description = "Набор конструктора для детей.", Price = 29.99m, Quantity = 150, ImagePath = "https://via.placeholder.com/300x200?text=Blocks", CategoryId = toys.Id, SellerId = seller2.Id });
                AddIfMissing("Remote Car Racer", new Models.Product { Name = "Remote Car Racer", Description = "Машинка на радиоуправлении для детей.", Price = 49.99m, Quantity = 70, ImagePath = "https://via.placeholder.com/300x200?text=Car", CategoryId = toys.Id, SellerId = seller2.Id });

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

            var context = services.GetRequiredService<ClonePromDbContext>();

            if (!context.Categories.Any())
            {
                context.Categories.AddRange(
                    new Category { Name = "Electronics" },
                    new Category { Name = "Clothing" },
                    new Category { Name = "Books" },
                    new Category { Name = "Sports" },
                    new Category { Name = "Home & Garden" }
                );
                await context.SaveChangesAsync();
            }
        }
    }
}
