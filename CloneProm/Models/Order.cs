using Microsoft.AspNetCore.Identity;

namespace CloneProm.Models
{
    public class Order
    {
        public int Id { get; set; }

        public string UserId { get; set; }
        public IdentityUser User { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public string Status { get; set; }

        public decimal TotalPrice { get; set; }

        public ICollection<OrderItem> Items { get; set; }
    }
}
