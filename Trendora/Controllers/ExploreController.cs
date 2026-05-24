using Microsoft.AspNetCore.Mvc;
using Trendora.Models;
using Trendora.Models.Interfaces;

namespace Trendora.Controllers
{
    public class ExploreController : Controller
    {

        private readonly IProductRepository _productRepository;
        private readonly ICategoryRepository _categoryRepository;

        public ExploreController(IProductRepository productRepository,
                               ICategoryRepository categoryRepository)
        {
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
        }

        [HttpGet]
        public IActionResult Index(
            string search = null,
            string category = null,
            decimal maxPrice = 50000,
            string sizes = null,
            string brands = null,
            string colors = null,
            int rating = 0,
            string sortBy = "featured",
            int page = 1,
            int pageSize = 12) // Fixed: Always 8 products per page
        {
            try
            {
                // Parse filter parameters
                var sizeList = !string.IsNullOrEmpty(sizes) ?
                    sizes.Split(',', StringSplitOptions.RemoveEmptyEntries)
                         .Select(s => s.Trim())
                         .Where(s => !string.IsNullOrEmpty(s))
                         .ToList() : null;

                var brandList = !string.IsNullOrEmpty(brands) ?
                    brands.Split(',', StringSplitOptions.RemoveEmptyEntries)
                          .Select(b => b.Trim())
                          .Where(b => !string.IsNullOrEmpty(b))
                          .ToList() : null;

                var colorList = !string.IsNullOrEmpty(colors) ?
                    colors.Split(',', StringSplitOptions.RemoveEmptyEntries)
                          .Select(c => c.Trim())
                          .Where(c => !string.IsNullOrEmpty(c))
                          .ToList() : null;

                // Validate maxPrice
                if (maxPrice < 0) maxPrice = 50000;
                if (maxPrice > 50000) maxPrice = 50000;

                // Force pageSize to always be 8
                pageSize = 9;

                // Get filtered products
                Console.WriteLine($"Calling GetFilteredProducts...");
                var products = _productRepository.GetFilteredProducts(
                    search,
                    category,
                    maxPrice,
                    sizeList,
                    brandList,
                    colorList,
                    rating,
                    sortBy
                ).ToList();

                Console.WriteLine($"Total products found: {products.Count}");

                // Pagination logic
                var totalProducts = products.Count;
                var totalPages = (int)Math.Ceiling(totalProducts / (double)pageSize);

               
                if (page < 1) page = 1;
                if (page > totalPages && totalPages > 0) page = totalPages;

                // Take only 8 products for the current page
                var paginatedProducts = products
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                Console.WriteLine($"=== PAGINATION INFO ===");
                Console.WriteLine($"Total Products: {totalProducts}");
                Console.WriteLine($"Page Size: {pageSize}");
                Console.WriteLine($"Current Page: {page}");
                Console.WriteLine($"Total Pages: {totalPages}");
                Console.WriteLine($"Products on this page: {paginatedProducts.Count}");

                // Debug: Show which products are being displayed
                for (int i = 0; i < paginatedProducts.Count; i++)
                {
                    Console.WriteLine($"Product {i + 1}: {paginatedProducts[i].ProductId} - {paginatedProducts[i].Name}");
                }
                Console.WriteLine($"=== END DEBUG ===");

                // Get active categories for the filter
                var activeCategories = _categoryRepository.GetActiveCategories()?.ToList() ?? new List<Category>();

                // Get unique values for filters
                var allProductsForFilters = _productRepository.GetAllProducts().ToList();
                var uniqueSizes = allProductsForFilters
                    .Where(p => !string.IsNullOrEmpty(p.Size))
                    .SelectMany(p => p.Size?.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                  .Select(s => s.Trim()) ?? Enumerable.Empty<string>())
                    .Distinct()
                    .OrderBy(s => s)
                    .ToList();

                var uniqueBrands = allProductsForFilters
                    .Where(p => !string.IsNullOrEmpty(p.Brand))
                    .Select(p => p.Brand)
                    .Distinct()
                    .OrderBy(b => b)
                    .ToList();

                var uniqueColors = allProductsForFilters
                    .Where(p => !string.IsNullOrEmpty(p.Color))
                    .SelectMany(p => p.Color?.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                   .Select(c => c.Trim()) ?? Enumerable.Empty<string>())
                    .Distinct()
                    .OrderBy(c => c)
                    .ToList();

                // Pass data to view
                ViewBag.SearchTerm = search;
                ViewBag.SelectedCategory = category;
                ViewBag.MaxPrice = maxPrice;
                ViewBag.SelectedSizes = sizeList ?? new List<string>();
                ViewBag.SelectedBrands = brandList ?? new List<string>();
                ViewBag.SelectedColors = colorList ?? new List<string>();
                ViewBag.SelectedRating = rating;
                ViewBag.SortBy = sortBy;
                ViewBag.Page = page;
                ViewBag.PageSize = pageSize;
                ViewBag.TotalPages = totalPages;
                ViewBag.TotalProducts = totalProducts;
                ViewBag.ActiveCategories = activeCategories;
                ViewBag.UniqueSizes = uniqueSizes;
                ViewBag.UniqueBrands = uniqueBrands;
                ViewBag.UniqueColors = uniqueColors;

                // Calculate start and end product numbers for display
                ViewBag.StartProduct = ((page - 1) * pageSize) + 1;
                ViewBag.EndProduct = Math.Min(page * pageSize, totalProducts);

                return View(paginatedProducts);
            }
            catch (Exception ex)
            {
                // Log the error
                Console.WriteLine($"Error in Explore/Index: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");

                // Return empty results with error message
                ViewBag.ErrorMessage = "An error occurred while loading products. Please try again.";

                return View(new List<Product>());
            }
        }

        // GET: /Explore/GetFilteredProductsAjax - AJAX endpoint for filtering
        [HttpGet]
        public IActionResult GetFilteredProductsAjax(
            string search = null,
            string category = null,
            decimal maxPrice = 500,
            string sizes = null,
            string brands = null,
            string colors = null,
            int rating = 0,
            string sortBy = "featured",
            int page = 1,
            int pageSize = 12) // Fixed: Always 8 for AJAX too
        {
            try
            {
                // Parse filter parameters
                var sizeList = !string.IsNullOrEmpty(sizes) ?
                    sizes.Split(',', StringSplitOptions.RemoveEmptyEntries)
                         .Select(s => s.Trim())
                         .Where(s => !string.IsNullOrEmpty(s))
                         .ToList() : null;

                var brandList = !string.IsNullOrEmpty(brands) ?
                    brands.Split(',', StringSplitOptions.RemoveEmptyEntries)
                          .Select(b => b.Trim())
                          .Where(b => !string.IsNullOrEmpty(b))
                          .ToList() : null;

                var colorList = !string.IsNullOrEmpty(colors) ?
                    colors.Split(',', StringSplitOptions.RemoveEmptyEntries)
                          .Select(c => c.Trim())
                          .Where(c => !string.IsNullOrEmpty(c))
                          .ToList() : null;

                // Validate maxPrice
                if (maxPrice < 0) maxPrice = 500;
                if (maxPrice > 5000) maxPrice = 5000;

                // Force pageSize to always be 8
                pageSize = 8;

                // Get filtered products
                var products = _productRepository.GetFilteredProducts(
                    search,
                    category,
                    maxPrice,
                    sizeList,
                    brandList,
                    colorList,
                    rating,
                    sortBy
                ).ToList();

                // Pagination
                var totalProducts = products.Count;
                var totalPages = (int)Math.Ceiling(totalProducts / (double)pageSize);

                // Ensure page is within valid range
                if (page < 1) page = 1;
                if (page > totalPages && totalPages > 0) page = totalPages;

                // Take only 8 products
                var paginatedProducts = products
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                Console.WriteLine($"AJAX: Showing {paginatedProducts.Count} products on page {page} of {totalPages}");

                // Return JSON response
                return Json(new
                {
                    success = true,
                    products = paginatedProducts,
                    totalProducts = totalProducts,
                    totalPages = totalPages,
                    currentPage = page,
                    pageSize = pageSize,
                    startProduct = ((page - 1) * pageSize) + 1,
                    endProduct = Math.Min(page * pageSize, totalProducts),
                    filters = new
                    {
                        searchTerm = search,
                        category = category,
                        maxPrice = maxPrice,
                        selectedSizes = sizeList,
                        selectedBrands = brandList,
                        selectedColors = colorList,
                        selectedRating = rating,
                        sortBy = sortBy
                    }
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetFilteredProductsAjax: {ex.Message}");
                return Json(new
                {
                    success = false,
                    message = "An error occurred while loading products."
                });
            }
        }

        // GET: /Explore/Product/{id}
        [HttpGet]
        public IActionResult Product(int id)
        {
            try
            {
                var product = _productRepository.GetProductById(id);
                if (product == null)
                {
                    TempData["ErrorMessage"] = "Product not found.";
                    return RedirectToAction("Index");
                }

                // Get related products (same category)
                var relatedProducts = new List<Product>();
                if (product.CategoryId.HasValue)
                {
                    relatedProducts = _productRepository.GetProductsByCategory(product.CategoryId.Value)
                        .Where(p => p.ProductId != id)
                        .Take(4)
                        .ToList();
                }

                ViewBag.RelatedProducts = relatedProducts;
                return View(product);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in Explore/Product: {ex.Message}");
                TempData["ErrorMessage"] = "An error occurred while loading the product.";
                return RedirectToAction("Index");
            }
        }

        // GET: /Explore/Category/{categoryName}
        [HttpGet]
        public IActionResult Category(string categoryName)
        {
            try
            {
                if (string.IsNullOrEmpty(categoryName))
                {
                    return RedirectToAction("Index");
                }

                // Get filtered products for this category
                var products = _productRepository.GetFilteredProducts(
                    category: categoryName,
                    sortBy: "newest"
                ).ToList();

                // Apply pagination - only show 8 products
                int page = 1;
                int pageSize = 8;
                int totalProducts = products.Count;
                int totalPages = (int)Math.Ceiling(totalProducts / (double)pageSize);

                var paginatedProducts = products
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                ViewBag.SelectedCategory = categoryName;
                ViewBag.CategoryName = categoryName;
                ViewBag.Page = page;
                ViewBag.PageSize = pageSize;
                ViewBag.TotalPages = totalPages;
                ViewBag.TotalProducts = totalProducts;
                ViewBag.StartProduct = 1;
                ViewBag.EndProduct = Math.Min(pageSize, totalProducts);

                return View("Index", paginatedProducts);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in Explore/Category: {ex.Message}");
                TempData["ErrorMessage"] = "An error occurred while loading category products.";
                return RedirectToAction("Index");
            }
        }

        // GET: /Explore/Page/{pageNumber} - Direct page navigation
        [HttpGet]
        public IActionResult Page(int pageNumber, string search = null, string category = null)
        {
            if (pageNumber < 1) pageNumber = 1;

            return RedirectToAction("Index", new
            {
                page = pageNumber,
                search = search,
                category = category,
                pageSize = 8 // Explicitly set page size
            });
        }
    }
}