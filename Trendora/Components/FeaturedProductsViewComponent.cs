using Microsoft.AspNetCore.Mvc;
using Trendora.Models;
using Trendora.Models.ViewModels;

namespace Trendora.Components
{
    public class FeaturedProductsViewComponent : ViewComponent
    {
      
       


        public IViewComponentResult Invoke(string title ,IEnumerable<Product> Products)
        {
            HomeViewModel model = new HomeViewModel
            {
                title = title,
                FeaturedProducts = Products,
                TrendingProducts = Products,

            };


            return View("default", model );
             
        }
    }

}
