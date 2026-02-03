using CloneProm.Models;
using System.Collections.Generic;

namespace CloneProm.Models.ViewModels
{
    public class HomeViewModel
    {
        public IEnumerable<Category> Categories { get; set; } = new List<Category>();
        public IEnumerable<Product> Products { get; set; } = new List<Product>();
        public int? SelectedCategoryId { get; set; }
        public string? SearchQuery { get; set; }
    }
}
