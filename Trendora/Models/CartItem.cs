using System.ComponentModel.DataAnnotations.Schema;

namespace Trendora.Models
{
    public class CartItem
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;

        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        public int Quantity { get; set; }
        public string? Size { get; set; }
        public string? Color { get; set; }
        public string? ImagePath { get; set; }
        public string? Brand { get; set; }

      
        [ForeignKey("ProductId")]
        public Product? Product { get; set; }

        [NotMapped]
        public decimal TotalPrice => Price * Quantity;
    }
}