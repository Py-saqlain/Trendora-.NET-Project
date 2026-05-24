namespace Trendora.Models.ViewModels
{
    public class HomeViewModel
    {
        public IEnumerable<Category> AllCategories { get; set; }
        public IEnumerable<Product> FeaturedProducts { get; set; }
        public IEnumerable<Product> TrendingProducts { get; set; }
        public string title { get; set; }
    }
}
