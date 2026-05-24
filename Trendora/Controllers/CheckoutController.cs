using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Trendora.Hubs;
using Trendora.Models;
using Trendora.Models.Interfaces;
using Trendora.Models.ViewModels;

namespace Trendora.Controllers
{
    [Authorize]
    public class CheckoutController : Controller
    {
        private readonly ICartRepository _cartRepository;
        private readonly IOrderRepository _orderRepository;
        private readonly IProductRepository _productRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IHubContext<NotificationHub> _hubContext;

        public CheckoutController(
            ICartRepository cartRepository,
            IOrderRepository orderRepository,
            IProductRepository productRepository,
            IHttpContextAccessor httpContextAccessor,
            IHubContext<NotificationHub> hubContext)
        {
            _cartRepository = cartRepository;
            _orderRepository = orderRepository;
            _productRepository = productRepository;
            _httpContextAccessor = httpContextAccessor;
            _hubContext = hubContext;
        }

        private string GetCartId()
        {
            var cartRepo = _cartRepository as Trendora.Models.Repositories.CartRepository;
            return cartRepo?.GetOrCreateCartId() ?? Guid.NewGuid().ToString();
        }

        // GET: /Checkout
        [HttpGet]
        public IActionResult Index()
        {
            try
            {
                var cartId = GetCartId();
                var cart = _cartRepository.GetCart(cartId);

                if (!cart.Items.Any())
                {
                    TempData["ErrorMessage"] = "Your cart is empty. Please add items before checkout.";
                    return RedirectToAction("Index", "Cart");
                }

                // Check stock availability
                var outOfStockItems = new List<string>();
                foreach (var item in cart.Items)
                {
                    var product = _productRepository.GetProductById(item.ProductId);
                    if (product == null || product.Quantity < item.Quantity)
                    {
                        outOfStockItems.Add(item.ProductName);
                    }
                }

                if (outOfStockItems.Any())
                {
                    TempData["ErrorMessage"] = $"The following items are out of stock: {string.Join(", ", outOfStockItems)}";
                    return RedirectToAction("Index", "Cart");
                }

                // Get current user info if available
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var userEmail = User.FindFirstValue(ClaimTypes.Email);
                var userName = User.FindFirstValue(ClaimTypes.Name);

                ViewBag.Cart = cart;
                ViewBag.UserEmail = userEmail;
                ViewBag.UserName = userName;

                return View();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in Checkout/Index: {ex.Message}");
                TempData["ErrorMessage"] = "An error occurred while loading checkout page.";
                return RedirectToAction("Index", "Cart");
            }
        }

        // POST: /Checkout/PlaceOrder
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PlaceOrder([FromForm] CheckoutViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    TempData["ErrorMessage"] = "Please fill in all required fields correctly.";
                    return RedirectToAction("Index");
                }

                var cartId = GetCartId();
                var cart = _cartRepository.GetCart(cartId);

                if (!cart.Items.Any())
                {
                    TempData["ErrorMessage"] = "Your cart is empty.";
                    return RedirectToAction("Index", "Cart");
                }

                // Final stock validation
                var insufficientStockItems = new List<string>();
                foreach (var item in cart.Items)
                {
                    var product = _productRepository.GetProductById(item.ProductId);
                    if (product == null || product.Quantity < item.Quantity)
                    {
                        insufficientStockItems.Add(item.ProductName);
                    }
                }

                if (insufficientStockItems.Any())
                {
                    TempData["ErrorMessage"] = $"The following items are no longer available in requested quantity: {string.Join(", ", insufficientStockItems)}";
                    return RedirectToAction("Index", "Cart");
                }

                // Get current user
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var userName = User.FindFirstValue(ClaimTypes.Name) ?? "Guest Customer";

                // Generate order number
                var orderNumber = GenerateOrderNumber();

                // Create order
                var order = new Order
                {
                    CustomerId = userId ?? "guest",
                    OrderNumber = orderNumber,
                    OrderDate = DateTime.Now,
                    Status = "Pending",
                    ShippingAddress = model.ShippingAddress,
                    PaymentMethod = model.PaymentMethod,
                    PaymentStatus = "Pending",
                    Shipping = 5.00m ,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };

                // Create and save order using synchronous method
                var createdOrder = _orderRepository.CreateOrder(order, cart.Items.ToList());

                if (createdOrder == null)
                {
                    TempData["ErrorMessage"] = "Failed to create order. Please try again.";
                    return RedirectToAction("Index");
                }

                // Update order with items and calculate total
                createdOrder = _orderRepository.GetOrderById(createdOrder.OrderId);

                // Send real-time notification to admin
                await SendOrderNotification(createdOrder, userName);

                // Clear cart after successful order
                _cartRepository.ClearCart(cartId);

                // Redirect to confirmation page
                return RedirectToAction("ConfirmOrder", new { orderId = createdOrder.OrderId });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error placing order: {ex.Message}");
                TempData["ErrorMessage"] = "An error occurred while placing your order. Please try again.";
                return RedirectToAction("Index");
            }
        }

        // GET: /Checkout/ConfirmOrder/{orderId}
        [HttpGet]
        public IActionResult ConfirmOrder(int orderId)
        {
            try
            {
                var order = _orderRepository.GetOrderById(orderId);

                if (order == null)
                {
                    TempData["ErrorMessage"] = "Order not found.";
                    return RedirectToAction("Index", "Home");
                }

                // Verify current user owns this order
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (order.CustomerId != userId && order.CustomerId != "guest")
                {
                    TempData["ErrorMessage"] = "You don't have permission to view this order.";
                    return RedirectToAction("Index", "Home");
                }

                return View(order);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in ConfirmOrder: {ex.Message}");
                TempData["ErrorMessage"] = "An error occurred while loading order confirmation.";
                return RedirectToAction("Index", "Home");
            }
        }

        // GET: /Checkout/MyOrders
        [HttpGet]
        public IActionResult MyOrders()
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    TempData["ErrorMessage"] = "Please login to view your orders.";
                    return RedirectToAction("Login", "Account", new { returnUrl = "/Checkout/MyOrders" });
                }

                var orders = _orderRepository.GetCustomerOrders(userId);

                return View(orders);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in MyOrders: {ex.Message}");
                TempData["ErrorMessage"] = "An error occurred while loading your orders.";
                return RedirectToAction("Index", "Home");
            }
        }

        // GET: /Checkout/OrderDetails/{orderId}
        [HttpGet]
        public IActionResult OrderDetails(int orderId)
        {
            try
            {
                var order = _orderRepository.GetOrderById(orderId);

                if (order == null)
                {
                    TempData["ErrorMessage"] = "Order not found.";
                    return RedirectToAction("MyOrders");
                }

                // Verify current user owns this order
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (order.CustomerId != userId)
                {
                    TempData["ErrorMessage"] = "You don't have permission to view this order.";
                    return RedirectToAction("MyOrders");
                }

                return View(order);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in OrderDetails: {ex.Message}");
                TempData["ErrorMessage"] = "An error occurred while loading order details.";
                return RedirectToAction("MyOrders");
            }
        }

        // POST: /Checkout/CancelOrder/{orderId}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CancelOrder(int orderId)
        {
            try
            {
                var order = _orderRepository.GetOrderById(orderId);

                if (order == null)
                {
                    TempData["ErrorMessage"] = "Order not found.";
                    return RedirectToAction("MyOrders");
                }

                // Verify current user owns this order
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (order.CustomerId != userId)
                {
                    TempData["ErrorMessage"] = "You don't have permission to cancel this order.";
                    return RedirectToAction("MyOrders");
                }

                // Check if order can be cancelled (only pending orders)
                if (order.Status != "Pending")
                {
                    TempData["ErrorMessage"] = $"Order cannot be cancelled. Current status: {order.Status}";
                    return RedirectToAction("OrderDetails", new { orderId });
                }

                // Update order status to cancelled
                var success = _orderRepository.UpdateOrderStatus(orderId, "Cancelled");

                if (success)
                {
                    TempData["SuccessMessage"] = "Order has been cancelled successfully.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to cancel order. Please try again.";
                }

                return RedirectToAction("OrderDetails", new { orderId });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in CancelOrder: {ex.Message}");
                TempData["ErrorMessage"] = "An error occurred while cancelling order.";
                return RedirectToAction("MyOrders");
            }
        }

        // GET: /Checkout/ContinueAsGuest
        [AllowAnonymous]
        [HttpGet]
        public IActionResult ContinueAsGuest()
        {
            try
            {
                var cartId = GetCartId();
                var cart = _cartRepository.GetCart(cartId);

                if (!cart.Items.Any())
                {
                    TempData["ErrorMessage"] = "Your cart is empty. Please add items before checkout.";
                    return RedirectToAction("Index", "Cart");
                }

                // Pre-fill with empty checkout view model
                var checkoutViewModel = new CheckoutViewModel();

                // Calculate order summary
                var shippingCost = 5.00m;
                var taxRate = 0.08m;
                var subtotal = cart.TotalPrice;
                var tax = subtotal * taxRate;
                var total = subtotal + shippingCost + tax;

                ViewBag.Cart = cart;
                ViewBag.Subtotal = subtotal;
                ViewBag.Shipping = shippingCost;
                ViewBag.Tax = tax;
                ViewBag.Total = total;
                ViewBag.CartItems = cart.Items;
                ViewBag.IsGuestCheckout = true;

                return View("Index", checkoutViewModel);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in ContinueAsGuest: {ex.Message}");
                TempData["ErrorMessage"] = "An error occurred while loading guest checkout.";
                return RedirectToAction("Index", "Cart");
            }
        }

        // POST: /Checkout/GuestPlaceOrder
        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GuestPlaceOrder(CheckoutViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    TempData["ErrorMessage"] = "Please fill in all required fields correctly.";
                    return RedirectToAction("ContinueAsGuest");
                }

                var cartId = GetCartId();
                var cart = _cartRepository.GetCart(cartId);

                if (!cart.Items.Any())
                {
                    TempData["ErrorMessage"] = "Your cart is empty.";
                    return RedirectToAction("Index", "Cart");
                }

                // Stock validation
                var insufficientStockItems = new List<string>();
                foreach (var item in cart.Items)
                {
                    var product = _productRepository.GetProductById(item.ProductId);
                    if (product == null || product.Quantity < item.Quantity)
                    {
                        insufficientStockItems.Add(item.ProductName);
                    }
                }

                if (insufficientStockItems.Any())
                {
                    TempData["ErrorMessage"] = $"The following items are no longer available: {string.Join(", ", insufficientStockItems)}";
                    return RedirectToAction("ContinueAsGuest");
                }

                // Generate order number
                var orderNumber = GenerateOrderNumber();

                // Create guest order
                var order = new Order
                {
                    CustomerId = "guest", // Mark as guest order
                    OrderNumber = orderNumber,
                    OrderDate = DateTime.Now,
                    Status = "Pending",
                    ShippingAddress = model.ShippingAddress,
                    PaymentMethod = model.PaymentMethod,
                    PaymentStatus = "Pending",
                    Shipping = 5.00m,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };

                var createdOrder = _orderRepository.CreateOrder(order, cart.Items.ToList());

                if (createdOrder == null)
                {
                    TempData["ErrorMessage"] = "Failed to create order. Please try again.";
                    return RedirectToAction("ContinueAsGuest");
                }

                // Update order with items and calculate total
                createdOrder = _orderRepository.GetOrderById(createdOrder.OrderId);

                // Send real-time notification to admin
                await SendOrderNotification(createdOrder, "Guest Customer");

                // Clear cart after successful order
                _cartRepository.ClearCart(cartId);

                // Store order ID in session for guest access
                HttpContext.Session.SetString($"GuestOrder_{createdOrder.OrderId}", createdOrder.OrderNumber);

                return RedirectToAction("ConfirmOrder", new { orderId = createdOrder.OrderId });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GuestPlaceOrder: {ex.Message}");
                TempData["ErrorMessage"] = "An error occurred while placing your order. Please try again.";
                return RedirectToAction("ContinueAsGuest");
            }
        }

        // Helper method to generate order number
        private string GenerateOrderNumber()
        {
            var datePart = DateTime.Now.ToString("yyyyMMdd");
            var randomPart = new Random().Next(100000, 999999).ToString();
            return $"TRN-{datePart}-{randomPart}";
        }

        // Helper method to send order notification to admin
        private async Task SendOrderNotification(Order order, string customerName)
        {
            try
            {
                // Calculate order total
                var orderTotal = order.OrderItems?.Sum(oi => oi.Quantity * oi.UnitPrice) ?? 0;
                var itemCount = order.OrderItems?.Count ?? 0;
                    
                // Create notification message
                var notificationMessage = $"🛒 New Order #{order.OrderNumber}! " +
                                         $"{customerName} placed an order worth ${orderTotal:F2}. " +
                                         $"Items: {itemCount}";

                // Create notification object
                var notificationData = new
                {
                    orderId = order.OrderId,
                    orderNumber = order.OrderNumber,
                    customerName = customerName,
                    orderDate = order.OrderDate.ToString("MMM dd, yyyy hh:mm tt"),
                    totalAmount = orderTotal,
                    itemCount = itemCount,
                    status = order.Status,
                    message = notificationMessage
                };

                // Send to all admins
                await _hubContext.Clients.Group("Admins").SendAsync("ReceiveOrderNotification", notificationData);

                Console.WriteLine($"Order notification sent for Order #{order.OrderNumber}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending order notification: {ex.Message}");
                // Don't throw - we don't want to fail the order just because notification failed
            }
        }
    }
}