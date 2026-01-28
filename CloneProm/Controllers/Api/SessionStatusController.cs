using CloneProm.Services;
using CloneProm.Models;
using Microsoft.AspNetCore.Mvc;
using System.Linq;


namespace CloneProm.Controllers.Api
{
    [Route("api/session")]
    [ApiController]
    public class SessionStatusController : ControllerBase
    {
        [HttpGet("status")]
        public IActionResult GetStatus()
        {
            var cart = HttpContext.Session.GetObject<List<SessionCartItem>>("CartItems") ?? new List<SessionCartItem>();
            var fav = HttpContext.Session.GetObject<List<int>>("Favorites") ?? new List<int>();
            var cartCount = cart.Sum(c => c.Quantity);
            return Ok(new { cartCount = cartCount, favCount = fav.Count });
        }
    }
}
