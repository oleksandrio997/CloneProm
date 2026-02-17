using Microsoft.AspNetCore.Identity;

namespace CloneProm.Models
{
    // Наслідуємо від IdentityUser, без додаткових ключів
    public class ApplicationUser : IdentityUser
    {
        public string? FullName { get; set; }
        public Seller Seller { get; set; }
    }
}
