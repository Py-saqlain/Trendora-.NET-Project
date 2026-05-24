using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Trendora.Hubs;
using Trendora.Models;
using Trendora.Models.Interfaces;
using Trendora.Services;

namespace Trendora.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly IProductRepository _productRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly ICustomerRepository _customerRepository;
        private readonly IOrderRepository _orderRepository;
        private readonly IHubContext<NotificationHub> _hubContext;

        public AdminController(
            IProductRepository productRepository,
            ICategoryRepository categoryRepository,
            ICustomerRepository customerRepository ,
            IOrderRepository orderRepository , IHubContext<NotificationHub> hubContext)
        {
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
            _customerRepository = customerRepository;
            _orderRepository = orderRepository;
            _hubContext = hubContext;
        }

        public IActionResult Index()
        {
            try
            {
                var products = _productRepository.GetAllProducts();
                var categories = _categoryRepository.GetAllCategories();
                var customers = _customerRepository.GetAllCustomers();

                ViewBag.Categories = categories;
                ViewBag.Customers = customers;
                ViewBag.ProductsCount = products.Count();
                ViewBag.CategoriesCount = categories.Count();
                ViewBag.CustomersCount = customers.Count();
                ViewBag.Orders = _orderRepository.GetAllOrders();
                ViewBag.TotalOrders = _orderRepository.GetOrderCount();
                ViewBag.TotalRevenue = _orderRepository.GetTotalRevenue();
                ViewBag.TotalCustomers = _customerRepository.GetCustomerCount();





                return View(products);
            }
            catch (Exception ex)
            {
                Logger.LogMessage($"Error in Admin/Index: {ex.Message}");
                TempData["ErrorMessage"] = "An error occurred while loading dashboard.";
                return View(new List<Product>());
            }
        }

        #region Product Actions

        public IActionResult AddProduct()
        {

            return PartialView("_AddProduct");

        }


        [HttpGet]
        public IActionResult CreateProduct()
        {
            try
            {
                ViewBag.Categories = _categoryRepository.GetActiveCategories();
                return View(new Product());
            }
            catch (Exception ex)
            {
                Logger.LogMessage($"Error in Admin/CreateProduct GET: {ex.Message}");
                TempData["ErrorMessage"] = "An error occurred while loading the form.";
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreateProduct(Product product, IFormFile ImageFile)
        {
            try
            {
                if (ModelState.IsValid)
                {
                   
                    if (ImageFile != null && ImageFile.Length > 0)
                    {
                        product.ImagePath = Helper.SaveImage(ImageFile, "Products");
                    }

                 

                    _productRepository.AddProduct(product);
                    TempData["SuccessMessage"] = "Product added successfully!";
                    return RedirectToAction("Index");
                }

                ViewBag.Categories = _categoryRepository.GetActiveCategories();
                return View(product);
            }
            catch (Exception ex)
            {
                Logger.LogMessage($"Error in Admin/CreateProduct POST: {ex.Message}");
                ModelState.AddModelError("", $"An error occurred: {ex.Message}");
                ViewBag.Categories = _categoryRepository.GetActiveCategories();
                return View(product);
            }
        }

 

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateProduct(Product product, IFormFile ImageFile)
        {
            try
            {
                Console.WriteLine($"=== UpdateProduct Called ===");
                Console.WriteLine($"ProductId: {product.ProductId}");
                Console.WriteLine($"Name: {product.Name}");
                Console.WriteLine($"CategoryId: {product.CategoryId}");
                Console.WriteLine($"Price: {product.Price}");

                if (!ModelState.IsValid)
                {
                    Console.WriteLine("ModelState is NOT valid. Errors:");
                    foreach (var key in ModelState.Keys)
                    {
                        var state = ModelState[key];
                        if (state.Errors.Count > 0)
                        {
                            Console.WriteLine($"  {key}: {string.Join(", ", state.Errors.Select(e => e.ErrorMessage))}");
                        }
                    }

                    TempData["ErrorMessage"] = "Please check the form for errors.";
                    return RedirectToAction("Index");
                }

                // Get existing product to preserve image if not changed
                var existingProduct = _productRepository.GetProductById(product.ProductId);
                if (existingProduct == null)
                {
                    Console.WriteLine($"ERROR: Product with ID {product.ProductId} not found");
                    TempData["ErrorMessage"] = "Product not found.";
                    return RedirectToAction("Index");
                }

                // Handle image upload
                if (ImageFile != null && ImageFile.Length > 0)
                {
                    Console.WriteLine("New image uploaded");
                    product.ImagePath = Helper.SaveImage(ImageFile, "Products");
                }
                else
                {
                    // Keep existing image
                    product.ImagePath = existingProduct.ImagePath;
                    Console.WriteLine("Keeping existing image");
                }

                Console.WriteLine("Calling UpdateProduct repository method");
                // Update product
                _productRepository.UpdateProduct(product);
                TempData["SuccessMessage"] = "Product updated successfully!";
                Console.WriteLine("Product updated successfully");

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR in UpdateProduct: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                Logger.LogMessage($"Error in Admin/UpdateProduct: {ex.Message}");
                TempData["ErrorMessage"] = $"An error occurred: {ex.Message}";
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteProduct(int id)
        {
            try
            {
                var product = _productRepository.GetProductById(id);
                if (product == null)
                {
                    TempData["ErrorMessage"] = "Product not found.";
                    return RedirectToAction("Index");
                }

                _productRepository.DeleteProduct(id);
                TempData["SuccessMessage"] = "Product deleted successfully!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                Logger.LogMessage($"Error in Admin/DeleteProduct: {ex.Message}");
                TempData["ErrorMessage"] = $"An error occurred: {ex.Message}";
                return RedirectToAction("Index");
            }
        }
        #endregion

        #region Category Actions
        [HttpGet]
        public IActionResult CreateCategory()
        {
            return View(new Category());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreateCategory(Category category, IFormFile ImageFile)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    if (ImageFile != null && ImageFile.Length > 0)
                    {
                        category.ImagePath = Helper.SaveImage(ImageFile, "Categories");
                    }

                    _categoryRepository.AddCategory(category);
                    TempData["SuccessMessage"] = "Category added successfully!";
                    return RedirectToAction("Index");
                }

                return View(category);
            }
            catch (Exception ex)
            {
                Logger.LogMessage($"Error in Admin/CreateCategory: {ex.Message}");
                ModelState.AddModelError("", $"An error occurred: {ex.Message}");
                return View(category);
            }
        }

        [HttpGet]
        public IActionResult EditCategory(int id)
        {
            try
            {
                var category = _categoryRepository.GetCategoryById(id);
                if (category == null)
                {
                    TempData["ErrorMessage"] = "Category not found.";
                    return RedirectToAction("Index");
                }
                return View(category);
            }
            catch (Exception ex)
            {
                Logger.LogMessage($"Error in Admin/EditCategory GET: {ex.Message}");
                TempData["ErrorMessage"] = "An error occurred while loading the category.";
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateCategory(Category category, IFormFile ImageFile)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    
                    var existingCategory = _categoryRepository.GetCategoryById(category.CategoryId);
                    if (existingCategory == null)
                    {
                        TempData["ErrorMessage"] = "Category not found.";
                        return RedirectToAction("Index");
                    }

                    // Handle image upload
                    if (ImageFile != null && ImageFile.Length > 0)
                    {
                        category.ImagePath = Helper.SaveImage(ImageFile, "Categories");
                    }
                    else
                    {
                        // Keep existing image
                        category.ImagePath = existingCategory.ImagePath;
                    }

                    _categoryRepository.UpdateCategory(category);
                    TempData["SuccessMessage"] = "Category updated successfully!";
                    return RedirectToAction("Index");
                }
                return View(category);
            }
            catch (Exception ex)
            {
                Logger.LogMessage($"Error in Admin/UpdateCategory: {ex.Message}");
                ModelState.AddModelError("", $"An error occurred: {ex.Message}");
                return View(category);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteCategory(int id, bool deleteProducts = false)
        {
            try
            {
                var category = _categoryRepository.GetCategoryById(id);
                if (category == null)
                {
                    TempData["ErrorMessage"] = "Category not found.";
                    return RedirectToAction("Index");
                }

                if (deleteProducts)
                {
                    // Delete all products in this category first
                    var products = _productRepository.GetProductsByCategory(id);
                    foreach (var product in products)
                    {
                        _productRepository.DeleteProduct(product.ProductId);
                    }

                    // Then delete the category
                    _categoryRepository.DeleteCategory(id);
                    TempData["SuccessMessage"] = $"Category and {products.Count()} products deleted successfully!";
                }
                else
                {
                    // Only delete category (products will have CategoryId set to null)
                    _categoryRepository.DeleteCategory(id);
                    TempData["SuccessMessage"] = "Category deleted successfully! Products in this category now have no category.";
                }

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                Logger.LogMessage($"Error in Admin/DeleteCategory: {ex.Message}");
                TempData["ErrorMessage"] = $"An error occurred: {ex.Message}";
                return RedirectToAction("Index");
            }
        }
        #endregion

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteCustomer(string id)
        {
            try
            {
                _customerRepository.DeleteCustomer(id);
                TempData["SuccessMessage"] = "Customer deleted successfully!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                Logger.LogMessage($"Error in Admin/DeleteCustomer: {ex.Message}");
                TempData["ErrorMessage"] = $"An error occurred: {ex.Message}";
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateOrderStatus(int orderId, string newStatus)
        {
            try
            {
                Console.WriteLine($"=== UpdateOrderStatus Called ===");
                Console.WriteLine($"OrderId: {orderId}, NewStatus: {newStatus}");

                if (orderId <= 0)
                {
                    return Json(new { success = false, message = "Invalid order ID." });
                }

               
                var validStatuses = new[] { "Pending", "Processing", "Shipped", "Delivered", "Cancelled" };
                if (!validStatuses.Contains(newStatus))
                {
                    return Json(new { success = false, message = "Invalid status value." });
                }

                // Get the order
                var order = _orderRepository.GetOrderById(orderId);
                if (order == null)
                {
                    return Json(new { success = false, message = "Order not found." });
                }

                // Update the status and timestamps
                order.Status = newStatus;
                order.UpdatedAt = DateTime.Now;

                // Update payment status based on order status
                if (newStatus == "Delivered")
                {
                    order.PaymentStatus = "Paid";
                }
                else if (newStatus == "Cancelled")
                {
                    order.PaymentStatus = "Cancelled";
                }

                // Save changes using UpdateOrder method
                _orderRepository.UpdateOrder(order);

                Console.WriteLine($"Order #{orderId} status updated to {newStatus}");
                return Json(new
                {
                    success = true,
                    message = "Status updated successfully.",
                    status = newStatus,
                    paymentStatus = order.PaymentStatus
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR in UpdateOrderStatus: {ex.Message}");
                Logger.LogMessage($"Error in Admin/UpdateOrderStatus: {ex.Message}");
                return Json(new { success = false, message = $"An error occurred: {ex.Message}" });
            }
        }


        [HttpPost]
        [ValidateAntiForgeryToken] // This validates the anti-forgery token
        public async Task<IActionResult> AnnouncementNotification(string message)
        {
            // Debug logging
            Console.WriteLine($"Received announcement request: {message}");
            Console.WriteLine($"ModelState isValid: {ModelState.IsValid}");

            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Invalid form submission. Please try again.";
                return RedirectToAction("Index");
            }

            if (string.IsNullOrWhiteSpace(message))
            {
                TempData["ErrorMessage"] = "Message cannot be empty!";
                return RedirectToAction("Index");
            }

            try
            {
                // Trim and sanitize message
                var cleanMessage = message.Trim();

                // Log before sending
                Console.WriteLine($"Sending announcement: {cleanMessage}");

                // Send announcement to all connected clients
                await _hubContext.Clients.All.SendAsync("ReceiveAnnouncement", cleanMessage);

                // Log success
                Console.WriteLine($"Announcement sent successfully: {cleanMessage}");

                TempData["SuccessMessage"] = $"Notification sent successfully: '{cleanMessage}'";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending announcement: {ex.Message}");
                TempData["ErrorMessage"] = $"Error sending notification: {ex.Message}";
            }

            return RedirectToAction("Index");
        }


        // Add these methods to your existing AdminController class

        [HttpGet]
        public IActionResult GetMonthlySalesData()
        {
            try
            {
                // Sample data - replace with your actual data
                var monthlyData = new[]
                {
            new { Month = "Jan", Sales = 45000, Orders = 45 },
            new { Month = "Feb", Sales = 52000, Orders = 52 },
            new { Month = "Mar", Sales = 48000, Orders = 48 },
            new { Month = "Apr", Sales = 61000, Orders = 61 },
            new { Month = "May", Sales = 55000, Orders = 55 },
            new { Month = "Jun", Sales = 72000, Orders = 72 }
        };

                return Json(monthlyData);
            }
            catch (Exception ex)
            {
                Logger.LogMessage($"Error in GetMonthlySalesData: {ex.Message}");
                return Json(new { error = "Failed to load sales data" });
            }
        }

        public async Task<IActionResult> GetProductDetailsAjax(int id)
        {
            var product = _productRepository.GetProductById(id);

            if (product == null)
            {
                return Json(new { success = false, message = "Product not found" });
            }

            return Json(new
            {
                success = true,
                product = new
                {
                    productId = product.ProductId,
                    name = product.Name,
                    description = product.Description,
                    price = product.Price,
                    originalPrice = product.OriginalPrice,
                    brand = product.Brand,
                    size = product.Size,
                    color = product.Color,
                    rating = product.Rating,
                    quantity = product.Quantity,
                    imagePath = product.ImagePath,
                    isNew = product.IsNew,
                    isSale = product.IsSale,
                    categoryName = product.Category?.Name
                }
            });
        }

        [HttpGet]
        public IActionResult GetCategoryWiseSales()
        {
            try
            {
                var categories = _categoryRepository.GetAllCategories();
                var categorySales = categories.Select(static c => new
                {
                    Category = c.Name,
                    Sales = (c.ProductQuantity) * 1200,
                    Products = c.ProductQuantity
                }).ToList();

                return Json(categorySales);
            }
            catch (Exception ex)
            {
                Logger.LogMessage($"Error in GetCategoryWiseSales: {ex.Message}");
                return Json(new { error = "Failed to load category data" });
            }
        }
    }
}