using CloneProm.Data;
using CloneProm.Models;
using CloneProm.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CloneProm.Controllers
{
    public class FavoritesController : Controller
    {
        private const string SessionFavKey = "Favorites";
        private readonly ClonePromDbContext _context;

        public FavoritesController(ClonePromDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var favIds = HttpContext.Session.GetObject<List<int>>(SessionFavKey) ?? new List<int>();
            var products = await _context.Products.Where(p => favIds.Contains(p.Id)).ToListAsync();
            return View(products);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ToggleFavorite(int productId)
        {
            var favIds = HttpContext.Session.GetObject<List<int>>(SessionFavKey) ?? new List<int>();
            if (favIds.Contains(productId))
            {
                favIds.Remove(productId);
            }
            else
            {
                favIds.Add(productId);
            }

            HttpContext.Session.SetObject(SessionFavKey, favIds);
            var result = new { success = true, count = favIds.Count, added = favIds.Contains(productId) };
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                return Json(result);

            return RedirectToAction("Index");
        }
    }
}
