namespace Trendora.Models.Interfaces
{
    public interface IProductRepository
    {
        void AddProduct(Product product);
        IEnumerable<Product> GetAllProducts();
        Product GetProductById(int id);
        void UpdateProduct(Product product);
        void DeleteProduct(int id);
        IEnumerable<Product> GetFilteredProducts(string searchTerm = null, string category = null,
            decimal maxPrice = 500, List<string> sizes = null, List<string> brands = null,
            List<string> colors = null, int minRating = 0, string sortBy = "featured");
        IEnumerable<Product> GetSaleProducts(
     string searchTerm = null,
     string category = null,
     decimal maxPrice = 500,
     List<string> sizes = null,
     List<string> brands = null,
     List<string> colors = null,
     int minRating = 0,
     string sortBy = "featured");
        IEnumerable<Product> GetNewArrivals();
        IEnumerable<Product> GetFeaturedProducts();
        IEnumerable<Product> GetTrendingProducts();
        IEnumerable<Product> GetProductsByCategory(int categoryId);
        int GetTotalProductCount();



    }
}
