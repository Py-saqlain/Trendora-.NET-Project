using System.ComponentModel.DataAnnotations.Schema;

namespace Trendora.Models
{
    public class Cart
    {
        public List<CartItem> Items { get; set; } = new List<CartItem>();

        [NotMapped]
        public int TotalItems => Items?.Sum(i => i.Quantity) ?? 0;

        [NotMapped]
        public decimal TotalPrice => Items?.Sum(i => i.TotalPrice) ?? 0;

        public void AddItem(Product product, int quantity, string size = null, string color = null)
        {
            if (product == null)
                throw new ArgumentNullException(nameof(product));

            if (quantity <= 0)
                throw new ArgumentException("Quantity must be greater than zero", nameof(quantity));

            var existingItem = Items.FirstOrDefault(i =>
                i.ProductId == product.ProductId &&
                i.Size == size &&
                i.Color == color);

            if (existingItem != null)
            {
                existingItem.Quantity += quantity;
            }
            else
            {
                Items.Add(new CartItem
                {
                    ProductId = product.ProductId,
                    ProductName = product.Name ?? "Unknown Product",
                    Price = product.Price,
                    Quantity = quantity,
                    Size = size,
                    Color = color,
                    ImagePath = product.ImagePath,
                    Brand = product.Brand,
                    Product = product
                });
            }
        }

        public void RemoveItem(int productId, string size = null, string color = null)
        {
            var item = Items.FirstOrDefault(i =>
                i.ProductId == productId &&
                i.Size == size &&
                i.Color == color);

            if (item != null)
            {
                Items.Remove(item);
            }
        }

        public void UpdateQuantity(int productId, int quantity, string size = null, string color = null)
        {
            if (quantity <= 0)
            {
                RemoveItem(productId, size, color);
                return;
            }

            var item = Items.FirstOrDefault(i =>
                i.ProductId == productId &&
                i.Size == size &&
                i.Color == color);

            if (item != null)
            {
                item.Quantity = quantity;
            }
        }

        public void Clear()
        {
            Items.Clear();
        }

        public bool ContainsProduct(int productId, string size = null, string color = null)
        {
            return Items.Any(i =>
                i.ProductId == productId &&
                i.Size == size &&
                i.Color == color);
        }
    }
}