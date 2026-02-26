using CloneProm.Data;
using CloneProm.Models;
using CloneProm.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace CloneProm.Controllers
{
    [Authorize] // доступ лише для залогінених
    public class ProfileController : Controller
    {
        private readonly ClonePromDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private const string SessionFavKey = "Favorites";

        public ProfileController(ClonePromDbContext context,
                                 UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index(string tab = "dashboard")
        {
            // Користувач
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Challenge(); // або повернути Layout із Login/Register
            }

            ViewBag.IsSeller = await _userManager.IsInRoleAsync(user, "Seller");
            ViewBag.IsAdmin = await _userManager.IsInRoleAsync(user, "Admin");
            ViewBag.ActiveTab = tab;

            // Favorites
            if (tab == "favorites")
            {
                var favIds = HttpContext.Session.GetObject<List<int>>(SessionFavKey) ?? new List<int>();

                var products = await _context.Products
                    .Include(p => p.Category)
                    .Include(p => p.Seller)
                    .Where(p => favIds.Contains(p.Id))
                    .ToListAsync();

                ViewBag.Favorites = products;
            }

            // Dashboard — товари Seller/Admin
            if (tab == "dashboard")
            {
                if (ViewBag.IsAdmin)
                {
                    ViewBag.Products = await _context.Products
                        .Include(p => p.Category)
                        .Include(p => p.Seller)
                        .ToListAsync();
                }
                else if (ViewBag.IsSeller)
                {
                    var seller = await _context.Sellers
                        .FirstOrDefaultAsync(s => s.UserId == user.Id);

                    ViewBag.Products = seller != null
                        ? await _context.Products
                            .Include(p => p.Category)
                            .Where(p => p.SellerId == seller.Id)
                            .ToListAsync()
                        : new List<Product>();
                }
            }

            return View(user);
        }
    }
}