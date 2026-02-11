using CloneProm.Data;
using CloneProm.Models;
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
