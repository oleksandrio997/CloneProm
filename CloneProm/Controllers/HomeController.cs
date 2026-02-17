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

        public async Task<IActionResult> Index()
        {
            // If DB has no products for any reason, ensure seed runs so UI can show seed data.
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

                // reload context queryable after seeding
                _context.ChangeTracker.Clear();
            }
            // support optional search and category filter via query string
            var search = HttpContext.Request.Query["q"].ToString();
            var catIdStr = HttpContext.Request.Query["category"].ToString();
            int? catId = null;
            if (int.TryParse(catIdStr, out var parsed)) catId = parsed;

            var productsQuery = _context.Products.AsQueryable();
            if (!string.IsNullOrWhiteSpace(search))
            {
                productsQuery = productsQuery.Where(p => p.Name.Contains(search) || p.Description.Contains(search));
            }
            if (catId.HasValue)
            {
                productsQuery = productsQuery.Where(p => p.CategoryId == catId.Value);
            }

            var model = new HomeViewModel
            {
                Categories = await _context.Categories.ToListAsync(),
                Products = await productsQuery.ToListAsync(),
                SelectedCategoryId = catId,
                SearchQuery = string.IsNullOrWhiteSpace(search) ? null : search
            };

            // compute recommended products based on session favorites
            var favIds = HttpContext.Session.GetObject<List<int>>("Favorites") ?? new List<int>();
            if (favIds.Any())
            {
                var favProds = await _context.Products.Where(p => favIds.Contains(p.Id)).ToListAsync();
                // choose the most frequent category among favorites
                var topCat = favProds.GroupBy(p => p.CategoryId).OrderByDescending(g => g.Count()).Select(g => g.Key).FirstOrDefault();
                if (topCat != 0)
                {
                    model.RecommendedProducts = await _context.Products.Where(p => p.CategoryId == topCat && !favIds.Contains(p.Id)).Take(6).ToListAsync();
                }
            }
            else
            {
                // initial recommendations: prefer products that were seeded (known seed names)
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

                var seeded = await _context.Products.Where(p => seedNames.Contains(p.Name)).ToListAsync();
                if (seeded.Any())
                {
                    model.RecommendedProducts = seeded.OrderBy(p => Guid.NewGuid()).Take(6).ToList();
                }
                else
                {
                    // fallback to random products
                    var all = await _context.Products.ToListAsync();
                    model.RecommendedProducts = all.OrderBy(p => Guid.NewGuid()).Take(6).ToList();
                }
            }

            return View(model);
        }

        // Diagnostic endpoint to check DB contents when debugging
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
    }
}
