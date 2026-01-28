using CloneProm.Data;
using CloneProm.Models;
using CloneProm.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CloneProm.Controllers
{
    public class CartController : Controller
    {
        private const string SessionCartKey = "CartItems";
        private readonly ClonePromDbContext _context;

        public CartController(ClonePromDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var sessionItems = HttpContext.Session.GetObject<List<SessionCartItem>>(SessionCartKey) ?? new List<SessionCartItem>();
            var productIds = sessionItems.Select(i => i.ProductId).ToList();
            var products = await _context.Products.Where(p => productIds.Contains(p.Id)).ToListAsync();

            var items = sessionItems.Select(si => new CartItemViewModel
            {
                Product = products.First(p => p.Id == si.ProductId),
                Quantity = si.Quantity
            }).ToList();

            return View(items);
        }

        [HttpPost]
        [IgnoreAntiforgeryToken]
        public IActionResult AddToCart(int productId, int quantity = 1)
        {
            var sessionItems = HttpContext.Session.GetObject<List<SessionCartItem>>(SessionCartKey) ?? new List<SessionCartItem>();
            var item = sessionItems.FirstOrDefault(i => i.ProductId == productId);
            if (item == null)
            {
                sessionItems.Add(new SessionCartItem { ProductId = productId, Quantity = quantity });
            }
            else
            {
                item.Quantity += quantity;
            }

            HttpContext.Session.SetObject(SessionCartKey, sessionItems);
            return Json(new { success = true, count = sessionItems.Sum(i => i.Quantity) });
        }

        [HttpPost]
        public IActionResult RemoveFromCart(int productId)
        {
            var sessionItems = HttpContext.Session.GetObject<List<SessionCartItem>>(SessionCartKey) ?? new List<SessionCartItem>();
            var item = sessionItems.FirstOrDefault(i => i.ProductId == productId);
            if (item != null)
            {
                sessionItems.Remove(item);
                HttpContext.Session.SetObject(SessionCartKey, sessionItems);
            }

            return RedirectToAction("Index");
        }
    }
}
