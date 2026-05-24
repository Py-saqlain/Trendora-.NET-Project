using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Trendora.Models;
using Trendora.Models.Interfaces;
using Trendora.Models.Repositories;

namespace Trendora.Controllers
{
    public class CartController : Controller
    {
        private readonly ICartRepository _cartRepository;
        private readonly IProductRepository _productRepository;

        public CartController(ICartRepository cartRepository, IProductRepository productRepository)
        {
            _cartRepository = cartRepository;
            _productRepository = productRepository;
        }

        private string GetCartId()
        {
            var cartRepo = _cartRepository as CartRepository;
            return cartRepo?.GetOrCreateCartId() ?? Guid.NewGuid().ToString();
        }

       
        [HttpGet]
        public IActionResult Index()
        {
            try
            {
                var cartId = GetCartId();
                var cart = _cartRepository.GetCart(cartId);
                return View(cart);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in Cart/Index: {ex.Message}");
                TempData["ErrorMessage"] = "An error occurred while loading your cart.";
                return View(new Cart());
            }
        }

        // POST: /Cart/AddToCart
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddToCart([FromBody] AddToCartRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new { success = false, message = "Invalid request data" });
                }

                var product = _productRepository.GetProductById(request.ProductId);
                if (product == null)
                {
                    return NotFound(new { success = false, message = "Product not found" });
                }

                // Check stock availability
                if (product.Quantity < request.Quantity)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = $"Only {product.Quantity} items available in stock.",
                        availableQuantity = product.Quantity
                    });
                }

                var cartId = GetCartId();
                var cart = _cartRepository.GetCart(cartId);

                cart.AddItem(product, request.Quantity, request.Size, request.Color);
                _cartRepository.SaveCart(cartId, cart);

                return Ok(new
                {
                    success = true,
                    message = $"{product.Name} added to cart!",
                    cartCount = cart.TotalItems,
                    cartTotal = cart.TotalPrice.ToString("0.00")
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in Cart/AddToCart: {ex.Message}");
                return StatusCode(500, new
                {
                    success = false,
                    message = "An error occurred while adding to cart"
                });
            }
        }

        // POST: /Cart/UpdateQuantity
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateQuantity([FromBody] UpdateQuantityRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new { success = false, message = "Invalid request data" });
                }

                var cartId = GetCartId();
                var cart = _cartRepository.GetCart(cartId);

                // Find the item
                var cartItem = cart.Items.FirstOrDefault(i =>
                    i.ProductId == request.ProductId &&
                    i.Size == request.Size &&
                    i.Color == request.Color);

                if (cartItem == null)
                {
                    return NotFound(new { success = false, message = "Item not found in cart" });
                }

                // Get product to check stock
                var product = _productRepository.GetProductById(request.ProductId);
                if (product == null)
                {
                    return NotFound(new { success = false, message = "Product not found" });
                }

                // Check stock availability
                if (product.Quantity < request.Quantity)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = $"Only {product.Quantity} items available in stock.",
                        availableQuantity = product.Quantity
                    });
                }

                // Update quantity
                cart.UpdateQuantity(request.ProductId, request.Quantity, request.Size, request.Color);
                _cartRepository.SaveCart(cartId, cart);

                // Get updated cart item
                var updatedItem = cart.Items.FirstOrDefault(i =>
                    i.ProductId == request.ProductId &&
                    i.Size == request.Size &&
                    i.Color == request.Color);

                // Calculate totals
                var shippingCost = 5.00m;
                var taxRate = 0.08m;
                var subtotal = cart.TotalPrice;
                var tax = subtotal * taxRate;
                var total = subtotal + shippingCost + tax;

                return Ok(new
                {
                    success = true,
                    message = "Quantity updated successfully",
                    cartTotal = cart.TotalPrice,
                    cartCount = cart.TotalItems,
                    itemTotal = updatedItem?.TotalPrice ?? 0,
                    subtotal = subtotal.ToString("0.00"),
                    tax = tax.ToString("0.00"),
                    shipping = shippingCost.ToString("0.00"),
                    grandTotal = total.ToString("0.00")
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in Cart/UpdateQuantity: {ex.Message}");
                return StatusCode(500, new
                {
                    success = false,
                    message = "An error occurred while updating quantity"
                });
            }
        }

        // POST: /Cart/RemoveItem
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult RemoveItem([FromBody] RemoveItemRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new { success = false, message = "Invalid request data" });
                }

                var cartId = GetCartId();
                var cart = _cartRepository.GetCart(cartId);

                cart.RemoveItem(request.ProductId, request.Size, request.Color);
                _cartRepository.SaveCart(cartId, cart);

                // Calculate totals
                var shippingCost = 5.00m;
                var taxRate = 0.08m;
                var subtotal = cart.TotalPrice;
                var tax = subtotal * taxRate;
                var total = subtotal + shippingCost + tax;

                return Ok(new
                {
                    success = true,
                    message = "Item removed from cart",
                    cartTotal = cart.TotalPrice,
                    cartCount = cart.TotalItems,
                    subtotal = subtotal.ToString("0.00"),
                    tax = tax.ToString("0.00"),
                    shipping = shippingCost.ToString("0.00"),
                    grandTotal = total.ToString("0.00")
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in Cart/RemoveItem: {ex.Message}");
                return StatusCode(500, new
                {
                    success = false,
                    message = "An error occurred while removing item"
                });
            }
        }

        // POST: /Cart/Clear
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Clear()
        {
            try
            {
                var cartId = GetCartId();
                _cartRepository.ClearCart(cartId);

                return Ok(new
                {
                    success = true,
                    message = "Cart cleared successfully",
                    cartCount = 0,
                    cartTotal = 0.00m,
                    subtotal = "0.00",
                    tax = "0.00",
                    shipping = "5.00",
                    grandTotal = "5.00"
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in Cart/Clear: {ex.Message}");
                return StatusCode(500, new
                {
                    success = false,
                    message = "An error occurred while clearing cart"
                });
            }
        }

        // GET: /Cart/GetCartCount
        [HttpGet]
        public IActionResult GetCartCount()
        {
            try
            {
                var cartId = GetCartId();
                var cart = _cartRepository.GetCart(cartId);

                return Json(new { success = true, count = cart.TotalItems });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in Cart/GetCartCount: {ex.Message}");
                return Json(new { success = false, count = 0 });
            }
        }

        // GET: /Cart/GetCartSummary
        [HttpGet]
        public IActionResult GetCartSummary()
        {
            try
            {
                var cartId = GetCartId();
                var cart = _cartRepository.GetCart(cartId);

                var shippingCost = 5.00m;
                var taxRate = 0.08m;
                var subtotal = cart.TotalPrice;
                var tax = subtotal * taxRate;
                var total = subtotal + shippingCost + tax;

                return Json(new
                {
                    success = true,
                    count = cart.TotalItems,
                    subtotal = subtotal.ToString("0.00"),
                    tax = tax.ToString("0.00"),
                    shipping = shippingCost.ToString("0.00"),
                    grandTotal = total.ToString("0.00")
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in Cart/GetCartSummary: {ex.Message}");
                return Json(new { success = false, count = 0 });
            }
        }
    }

    // Request models
    public class AddToCartRequest
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; } = 1;
        public string? Size { get; set; }
        public string? Color { get; set; }
    }

    public class UpdateQuantityRequest
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public string? Size { get; set; }
        public string? Color { get; set; }
    }

    public class RemoveItemRequest
    {
        public int ProductId { get; set; }
        public string? Size { get; set; }
        public string? Color { get; set; }
    }
}