using CloneProm.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

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
                ctx.Database.Migrate();

                var logger = provider.GetRequiredService<ILoggerFactory>().CreateLogger("SeedData");

                // ===========================
                // 1. Категорії
                // ===========================
                string[] catNames = { "Electronics", "Home Appliances", "Books", "Clothing", "Toys" };
                foreach (var name in catNames)
                {
                    if (!ctx.Categories.Any(c => c.Name == name))
                        ctx.Categories.Add(new Category { Name = name });
                }
                ctx.SaveChanges();

                var electronics = ctx.Categories.First(c => c.Name == "Electronics");
                var appliances = ctx.Categories.First(c => c.Name == "Home Appliances");
                var books = ctx.Categories.First(c => c.Name == "Books");
                var clothing = ctx.Categories.First(c => c.Name == "Clothing");
                var toys = ctx.Categories.First(c => c.Name == "Toys");

                // ===========================
                // 2. Ролі
                // ===========================
                var roleManager = provider.GetRequiredService<RoleManager<IdentityRole>>();
                var userManager = provider.GetRequiredService<UserManager<ApplicationUser>>();

                string[] roles = { "Admin", "Seller", "User" };
                foreach (var role in roles)
                {
                    if (!await roleManager.RoleExistsAsync(role))
                        await roleManager.CreateAsync(new IdentityRole(role));
                }

                // ===========================
                // 3. Admin + Seller
                // ===========================
                string adminEmail = "admin@cloneprom.com";
                string adminPassword = "Admin123!";
                ApplicationUser adminUser = await userManager.FindByEmailAsync(adminEmail);
                if (adminUser == null)
                {
                    adminUser = new ApplicationUser { UserName = adminEmail, Email = adminEmail };
                    await userManager.CreateAsync(adminUser, adminPassword);
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                }

                var adminSeller = ctx.Sellers.FirstOrDefault(s => s.UserId == adminUser.Id);
                if (adminSeller == null)
                {
                    adminSeller = new Seller
                    {
                        UserId = adminUser.Id,
                        ShopName = "Admin Shop",
                        Description = "Seeded admin seller"
                    };
                    ctx.Sellers.Add(adminSeller);
                    ctx.SaveChanges();
                }

                // ===========================
                // 4. Test Seller
                // ===========================
                string testSellerEmail = "seller@cloneprom.com";
                ApplicationUser testSellerUser = await userManager.FindByEmailAsync(testSellerEmail);
                if (testSellerUser == null)
                {
                    testSellerUser = new ApplicationUser { UserName = testSellerEmail, Email = testSellerEmail };
                    await userManager.CreateAsync(testSellerUser, "Seller123!");
                    await userManager.AddToRoleAsync(testSellerUser, "Seller");
                }

                var testSeller = ctx.Sellers.FirstOrDefault(s => s.UserId == testSellerUser.Id);
                if (testSeller == null)
                {
                    testSeller = new Seller
                    {
                        UserId = testSellerUser.Id,
                        ShopName = "Test Seller Shop",
                        Description = "Seeded test seller"
                    };
                    ctx.Sellers.Add(testSeller);
                    ctx.SaveChanges();
                }

                // ===========================
                // 5. Функція додавання продукту
                // ===========================
                void AddIfMissing(Product product)
                {
                    if (!ctx.Products.Any(p => p.Name == product.Name && p.SellerId == product.SellerId))
                        ctx.Products.Add(product);
                }

                // ===========================
                // 6. Продукти Admin (6 шт)
                // ===========================
                AddIfMissing(new Product
                {
                    Name = "Notebook Alpha",
                    Description = "Легкий ноутбук для повсякденних завдань.",
                    Price = 149.99m,
                    Quantity = 10,
                    ImagePath = "/images/products/notebook_alpha.jpg",
                    CategoryId = electronics.Id,
                    SellerId = adminSeller.Id
                });

                AddIfMissing(new Product
                {
                    Name = "Wireless Mouse Mini",
                    Description = "Компактна ергономічна бездротова миша.",
                    Price = 24.50m,
                    Quantity = 50,
                    ImagePath = "/images/products/mouse_mini.jpg",
                    CategoryId = electronics.Id,
                    SellerId = adminSeller.Id
                });

                AddIfMissing(new Product
                {
                    Name = "Mechanical Keyboard X",
                    Description = "Механічна клавіатура із тактильними перемикачами.",
                    Price = 89.90m,
                    Quantity = 20,
                    ImagePath = "/images/products/keyboard_x.jpg",
                    CategoryId = electronics.Id,
                    SellerId = adminSeller.Id
                });

                AddIfMissing(new Product
                {
                    Name = "Classic T-Shirt",
                    Description = "Зручна бавовняна футболка.",
                    Price = 14.99m,
                    Quantity = 200,
                    ImagePath = "/images/products/classic_tshirt.jpg",
                    CategoryId = clothing.Id,
                    SellerId = adminSeller.Id
                });

                AddIfMissing(new Product
                {
                    Name = "Denim Jeans",
                    Description = "Класичні джинси строгого крою.",
                    Price = 49.99m,
                    Quantity = 80,
                    ImagePath = "/images/products/denim_jeans.jpg",
                    CategoryId = clothing.Id,
                    SellerId = adminSeller.Id
                });

                AddIfMissing(new Product
                {
                    Name = "C# in Depth",
                    Description = "Практичний посібник з C#.",
                    Price = 39.99m,
                    Quantity = 100,
                    ImagePath = "/images/products/csharp_in_depth.jpg",
                    CategoryId = books.Id,
                    SellerId = adminSeller.Id
                });

                // ===========================
                // 7. Продукти Test Seller (6 шт)
                // ===========================
                AddIfMissing(new Product
                {
                    Name = "Air Purifier Pro",
                    Description = "Компактний очисник для будинку.",
                    Price = 129.00m,
                    Quantity = 25,
                    ImagePath = "/images/products/air_purifier.jpg",
                    CategoryId = appliances.Id,
                    SellerId = testSeller.Id
                });

                AddIfMissing(new Product
                {
                    Name = "Blender 500W",
                    Description = "Потужний блендер для смузі та супів.",
                    Price = 59.99m,
                    Quantity = 40,
                    ImagePath = "/images/products/blender_500w.jpg",
                    CategoryId = appliances.Id,
                    SellerId = testSeller.Id
                });

                AddIfMissing(new Product
                {
                    Name = "Building Blocks Set",
                    Description = "Набір конструктор для дітей.",
                    Price = 29.99m,
                    Quantity = 150,
                    ImagePath = "/images/products/blocks_set.jpg",
                    CategoryId = toys.Id,
                    SellerId = testSeller.Id
                });

                AddIfMissing(new Product
                {
                    Name = "Remote Car Racer",
                    Description = "Машинка на радіокеруванні для дітей.",
                    Price = 49.99m,
                    Quantity = 70,
                    ImagePath = "/images/products/remote_car.jpg",
                    CategoryId = toys.Id,
                    SellerId = testSeller.Id
                });

                AddIfMissing(new Product
                {
                    Name = "Cooking Basics",
                    Description = "Книга рецептів для початківців.",
                    Price = 19.50m,
                    Quantity = 60,
                    ImagePath = "/images/products/cooking_basics.jpg",
                    CategoryId = books.Id,
                    SellerId = testSeller.Id
                });

                AddIfMissing(new Product
                {
                    Name = "Smartwatch Pro",
                    Description = "Смарт-годинник для фітнесу та сповіщень.",
                    Price = 199.99m,
                    Quantity = 30,
                    ImagePath = "/images/products/smartwatch_pro.jpg",
                    CategoryId = electronics.Id,
                    SellerId = testSeller.Id
                });

                ctx.SaveChanges();
                logger.LogInformation("SeedData completed: Admin + Test Seller with products created.");
            }
            catch (Exception ex)
            {
                var logger = provider.GetRequiredService<ILoggerFactory>().CreateLogger("SeedData");
                logger.LogError(ex, "Error seeding the database.");
            }
        }
    }
}