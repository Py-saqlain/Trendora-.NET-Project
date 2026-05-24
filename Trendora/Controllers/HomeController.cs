using Microsoft.AspNetCore.Mvc;
using Trendora.Models;
using Trendora.Models.Interfaces;
using Trendora.Models.ViewModels;
using Trendora.Services;


namespace Trendora_Frontend.Controllers
{

    public class HomeController : Controller
    {

        private readonly IProductRepository _productRepository;
        private readonly ICategoryRepository _categoryRepository;

        public HomeController(
            IProductRepository productRepository,
            ICategoryRepository categoryRepository)
        {
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
        }

        public IActionResult Index()
        {
            try
            {
               
                var model = new HomeViewModel
                {
                    
                    FeaturedProducts = _productRepository.GetFeaturedProducts()?.ToList() ?? new List<Product>(),
                    TrendingProducts = _productRepository.GetTrendingProducts()?.ToList() ?? new List<Product>(),
                    AllCategories = _categoryRepository.GetActiveCategories()?.ToList() ?? new List<Category>()
                };

                

                return View(model);
            }
            catch (Exception ex)
            {
               
                Logger.LogExpception(ex);

                
                return View(new HomeViewModel
                {
                    FeaturedProducts = new List<Product>(),
                    TrendingProducts = new List<Product>(),
                    AllCategories = new List<Category>()
                });
            }
        }

        
    }
}