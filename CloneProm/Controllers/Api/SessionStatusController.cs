using CloneProm.Services;
using Microsoft.AspNetCore.Mvc;

namespace CloneProm.Controllers.Api
{
    [Route("api/session")]
    [ApiController]
    public class SessionStatusController : ControllerBase
    {
        [HttpGet("status")]
        public IActionResult GetStatus()
        {
            var cart = HttpContext.Session.GetObject<List<object>>("CartItems") ?? new List<object>();
            var fav = HttpContext.Session.GetObject<List<int>>("Favorites") ?? new List<int>();
            var cartCount = cart.Count > 0 ? cart.Sum(c => (int)((dynamic)c).Quantity) : 0;
            return Ok(new { cartCount = cartCount, favCount = fav.Count });
        }
    }
}
