using CloneProm.Data;
using CloneProm.Models;
using CloneProm.Services;
using CloneProm.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace CloneProm.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ClonePromDbContext _context;

        public HomeController(ILogger<HomeController> logger, ClonePromDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<IActionResult> Index(int page = 1)
        {
            int pageSize = 12;

            // Seed if database empty
            if (!await _context.Products.AnyAsync())
            {
                try
                {
                    await HttpContext.RequestServices.InitializeAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to run seed from Home.Index");
                }

                _context.ChangeTracker.Clear();
            }

            // Query parameters
            var search = HttpContext.Request.Query["q"].ToString();
            var catIdStr = HttpContext.Request.Query["category"].ToString();

            int? catId = null;
            if (int.TryParse(catIdStr, out var parsed))
                catId = parsed;

            var productsQuery = _context.Products.AsQueryable();

            // Search
            if (!string.IsNullOrWhiteSpace(search))
            {
                productsQuery = productsQuery
                    .Where(p => p.Name.Contains(search) || p.Description.Contains(search));
            }

            // Category filter
            if (catId.HasValue)
            {
                productsQuery = productsQuery
                    .Where(p => p.CategoryId == catId.Value);
            }

            // Pagination
            var totalProducts = await productsQuery.CountAsync();

            var products = await productsQuery
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var model = new HomeViewModel
            {
                Categories = await _context.Categories.ToListAsync(),
                Products = products,
                SelectedCategoryId = catId,
                SearchQuery = string.IsNullOrWhiteSpace(search) ? null : search,
                CurrentPage = page,
                TotalPages = (int)Math.Ceiling(totalProducts / (double)pageSize)
            };

            // Recommended products logic
            var favIds = HttpContext.Session.GetObject<List<int>>("Favorites") ?? new List<int>();

            if (favIds.Any())
            {
                var favProds = await _context.Products
                    .Where(p => favIds.Contains(p.Id))
                    .ToListAsync();

                var topCat = favProds
                    .GroupBy(p => p.CategoryId)
                    .OrderByDescending(g => g.Count())
                    .Select(g => g.Key)
                    .FirstOrDefault();

                if (topCat != 0)
                {
                    model.RecommendedProducts = await _context.Products
                        .Where(p => p.CategoryId == topCat && !favIds.Contains(p.Id))
                        .Take(6)
                        .ToListAsync();
                }
            }
            else
            {
                string[] seedNames = {
                    "Notebook Alpha",
                    "Wireless Mouse Mini",
                    "Mechanical Keyboard X",
                    "Air Purifier Pro",
                    "Blender 500W",
                    "C# in Depth",
                    "Cooking Basics",
                    "Classic T-Shirt",
                    "Denim Jeans",
                    "Building Blocks Set",
                    "Remote Car Racer"
                };

                var seeded = await _context.Products
                    .Where(p => seedNames.Contains(p.Name))
                    .ToListAsync();

                if (seeded.Any())
                {
                    model.RecommendedProducts = seeded
                        .OrderBy(p => Guid.NewGuid())
                        .Take(6)
                        .ToList();
                }
                else
                {
                    var all = await _context.Products.ToListAsync();

                    model.RecommendedProducts = all
                        .OrderBy(p => Guid.NewGuid())
                        .Take(6)
                        .ToList();
                }
            }

            return View(model);
        }

        // Diagnostic endpoint
        [HttpGet]
        [Route("home/dbstatus")]
        public async Task<IActionResult> DbStatus()
        {
            try
            {
                var cats = await _context.Categories.CountAsync();
                var prods = await _context.Products.CountAsync();
                return Json(new { categories = cats, products = prods });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "DbStatus error");
                return StatusCode(500, ex.Message);
            }
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
            });
        }

        public async Task<IActionResult> Order()
        {
            var sessionItems = HttpContext.Session.GetObject<List<SessionCartItem>>("CartItems")
                              ?? new List<SessionCartItem>();

            var productIds = sessionItems.Select(i => i.ProductId).ToList();

            var products = await _context.Products
                .Where(p => productIds.Contains(p.Id))
                .ToListAsync();

            var cart = sessionItems.Select(si => new CartItemViewModel
            {
                Product = products.First(p => p.Id == si.ProductId),
                Quantity = si.Quantity
            }).ToList();

            return View("~/Views/Order/Index.cshtml", cart);
        }
    }
}