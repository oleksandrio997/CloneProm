using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;

namespace CloneProm.Models
{
    public class Seller
    {
        public int Id { get; set; }

        public string UserId { get; set; }
        public ApplicationUser? User { get; set; }

        public string ShopName { get; set; }
        public string Description { get; set; }

        public double Rating { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // initialize collections to avoid possible NREs when iterating before EF loads them
        public ICollection<Product> Products { get; set; } = new List<Product>();
    }
}
