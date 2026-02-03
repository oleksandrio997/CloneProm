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
                // Ensure database created
                ctx.Database.EnsureCreated();

                if (ctx.Categories.Any()) return;

                // create categories
                var electronics = new Models.Category { Name = "Electronics" };
                var appliances = new Models.Category { Name = "Home Appliances" };
                var books = new Models.Category { Name = "Books" };
                var clothing = new Models.Category { Name = "Clothing" };
                var toys = new Models.Category { Name = "Toys" };

                ctx.Categories.AddRange(electronics, appliances, books, clothing, toys);

                // sellers
                var seller1 = new Models.Seller { UserId = "system", ShopName = "Default Shop", Description = "Seeded seller" };
                var seller2 = new Models.Seller { UserId = "shop2", ShopName = "HomeGoods", Description = "Household items" };
                var seller3 = new Models.Seller { UserId = "bookseller", ShopName = "Books & Co.", Description = "Bookseller" };
                ctx.Sellers.AddRange(seller1, seller2, seller3);

                // products across categories
                ctx.Products.AddRange(
                    // Electronics
                    new Models.Product { Name = "Notebook Alpha", Description = "Лёгкий ноутбук для повседневных задач.", Price = 149.99m, Quantity = 10, ImagePath = "https://via.placeholder.com/300x200?text=Notebook", Category = electronics, Seller = seller1 },
                    new Models.Product { Name = "Wireless Mouse Mini", Description = "Компактная эргономичная беспроводная мышь.", Price = 24.50m, Quantity = 50, ImagePath = "https://via.placeholder.com/300x200?text=Mouse", Category = electronics, Seller = seller1 },
                    new Models.Product { Name = "Mechanical Keyboard X", Description = "Механическая клавиатура с тактильными переключателями.", Price = 89.90m, Quantity = 20, ImagePath = "https://via.placeholder.com/300x200?text=Keyboard", Category = electronics, Seller = seller1 },

                    // Home Appliances
                    new Models.Product { Name = "Air Purifier Pro", Description = "Компактный очиститель воздуха для дома.", Price = 129.00m, Quantity = 25, ImagePath = "https://via.placeholder.com/300x200?text=Air+Purifier", Category = appliances, Seller = seller2 },
                    new Models.Product { Name = "Blender 500W", Description = "Мощный блендер для смузи и супов.", Price = 59.99m, Quantity = 40, ImagePath = "https://via.placeholder.com/300x200?text=Blender", Category = appliances, Seller = seller2 },

                    // Books
                    new Models.Product { Name = "C# in Depth", Description = "Практическое руководство по C#.", Price = 39.99m, Quantity = 100, ImagePath = "https://via.placeholder.com/300x200?text=Book", Category = books, Seller = seller3 },
                    new Models.Product { Name = "Cooking Basics", Description = "Книга рецептов для начинающих.", Price = 19.50m, Quantity = 60, ImagePath = "https://via.placeholder.com/300x200?text=Cookbook", Category = books, Seller = seller3 },

                    // Clothing
                    new Models.Product { Name = "Classic T-Shirt", Description = "Удобная хлопковая футболка.", Price = 14.99m, Quantity = 200, ImagePath = "https://via.placeholder.com/300x200?text=T-Shirt", Category = clothing, Seller = seller1 },
                    new Models.Product { Name = "Denim Jeans", Description = "Классические джинсы строгого кроя.", Price = 49.99m, Quantity = 80, ImagePath = "https://via.placeholder.com/300x200?text=Jeans", Category = clothing, Seller = seller1 },

                    // Toys
                    new Models.Product { Name = "Building Blocks Set", Description = "Набор конструктора для детей.", Price = 29.99m, Quantity = 150, ImagePath = "https://via.placeholder.com/300x200?text=Blocks", Category = toys, Seller = seller2 },
                    new Models.Product { Name = "Remote Car Racer", Description = "Машинка на радиоуправлении для детей.", Price = 49.99m, Quantity = 70, ImagePath = "https://via.placeholder.com/300x200?text=Car", Category = toys, Seller = seller2 }
                );

                ctx.SaveChanges();
            }
            catch (Exception ex)
            {
                var logger = provider.GetRequiredService<ILoggerFactory>().CreateLogger("SeedData");
                logger.LogError(ex, "Error seeding the database.");
            }
        }
    }
}
