using Microsoft.AspNetCore.Mvc;
using Trendora.Models;
using Trendora.Models.ViewModels;

namespace Trendora.Components
{
   
    public class HeroSectionViewComponent: ViewComponent
    {
        public IViewComponentResult Invoke(string title, string discription)

        {

            HeroSection section = new HeroSection
            {
                title = title,
                description = discription
            };




            return View("default",section);

        }

       
    }
}
