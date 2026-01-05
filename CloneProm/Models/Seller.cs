using Microsoft.AspNetCore.Identity;

namespace CloneProm.Models
{
    public class Seller
    {
        public int Id { get; set; }

        public string UserId { get; set; }
        public IdentityUser User { get; set; }

        public string ShopName { get; set; }
        public string Description { get; set; }

        public double Rating { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public ICollection<Product> Products { get; set; }
    }
}
