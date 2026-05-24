using System.Text.Json;
using Trendora.Models.Interfaces;

namespace Trendora.Models.Repositories
{
    public class CartRepository : ICartRepository
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CartRepository(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        private string GetCartKey(string cartId)
        {
            return $"Cart_{cartId}";
        }

        public Cart GetCart(string cartId)
        {
            var session = _httpContextAccessor.HttpContext?.Session;
            var cookieCart = _httpContextAccessor.HttpContext?.Request.Cookies[GetCartKey(cartId)];

            if (session != null)
            {
                var cartData = session.GetString(GetCartKey(cartId));
                if (!string.IsNullOrEmpty(cartData))
                {
                    return JsonSerializer.Deserialize<Cart>(cartData) ?? new Cart();
                }
            }
            if (!string.IsNullOrEmpty(cookieCart))
            {
                return JsonSerializer.Deserialize<Cart>(cookieCart) ?? new Cart();
            }

            return new Cart();
        }

        public void SaveCart(string cartId, Cart cart)
        {
            var session = _httpContextAccessor.HttpContext?.Session;
            var response = _httpContextAccessor.HttpContext?.Response;

            if (session != null)
            {
                session.SetString(GetCartKey(cartId), JsonSerializer.Serialize(cart));
            }

            if (response != null)
            {
            
                var options = new CookieOptions
                {
                    Expires = DateTime.Now.AddDays(30),
                    IsEssential = true,
                    HttpOnly = true,
                    SameSite = SameSiteMode.Strict
                };
                response.Cookies.Append(GetCartKey(cartId), JsonSerializer.Serialize(cart), options);
            }
        }

        public void ClearCart(string cartId)
        {
            var session = _httpContextAccessor.HttpContext?.Session;
            var response = _httpContextAccessor.HttpContext?.Response;

            if (session != null)
            {
                session.Remove(GetCartKey(cartId));
            }

            if (response != null)
            {
                response.Cookies.Delete(GetCartKey(cartId));
            }
        }

        public void MergeCarts(string sourceCartId, string targetCartId)
        {
            var sourceCart = GetCart(sourceCartId);
            var targetCart = GetCart(targetCartId);

            foreach (var item in sourceCart.Items)
            {
                targetCart.AddItem(new Product
                {
                    ProductId = item.ProductId,
                    Name = item.ProductName,
                    Price = item.Price,
                    ImagePath = item.ImagePath,
                    Brand = item.Brand
                }, item.Quantity, item.Size, item.Color);
            }

            SaveCart(targetCartId, targetCart);
            ClearCart(sourceCartId);
        }

        public string GetOrCreateCartId()
        {
            var httpContext = _httpContextAccessor.HttpContext;

            var sessionCartId = httpContext?.Session.GetString("CartId");
            if (!string.IsNullOrEmpty(sessionCartId))
            {
                return sessionCartId;
            }

            var cookieCartId = httpContext?.Request.Cookies["CartId"];
            if (!string.IsNullOrEmpty(cookieCartId))
            {
                httpContext?.Session.SetString("CartId", cookieCartId);
                return cookieCartId;
            }

            var newCartId = Guid.NewGuid().ToString();
            httpContext?.Session.SetString("CartId", newCartId);

            var options = new CookieOptions
            {
                Expires = DateTime.Now.AddDays(30),
                IsEssential = true,
                HttpOnly = true,
                SameSite = SameSiteMode.Strict
            };
            httpContext?.Response.Cookies.Append("CartId", newCartId, options);

            return newCartId;
        }
    }
}