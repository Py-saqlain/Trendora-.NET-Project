using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Trendora.Models
{
    public class Product
    {
        public int ProductId { get; set; }

        [Required(ErrorMessage = "Product name is required")]
        [StringLength(200, ErrorMessage = "Product name cannot exceed 200 characters")]
        public string Name { get; set; } = string.Empty;

        public int? CategoryId { get; set; }

        [ForeignKey("CategoryId")]
        public Category? Category { get; set; }

        [NotMapped] 
        public string? CategoryName { get; set; }

        [StringLength(100, ErrorMessage = "Brand cannot exceed 100 characters")]
        public string? Brand { get; set; }

        [Required(ErrorMessage = "Price is required")]
        [Range(0.01, 10000.00, ErrorMessage = "Price must be between 0.01 and 10000.00")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal OriginalPrice { get; set; }

        [StringLength(50, ErrorMessage = "Color cannot exceed 50 characters")]
        public string? Color { get; set; }

        [StringLength(20, ErrorMessage = "Gender cannot exceed 20 characters")]
        public string? Gender { get; set; }

        [Range(0, 5, ErrorMessage = "Rating must be between 0 and 5")]
        [Column(TypeName = "decimal(3,2)")]
        public decimal Rating { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Quantity cannot be negative")]
        public int Quantity { get; set; }

        [StringLength(100, ErrorMessage = "Size cannot exceed 100 characters")]
        public string? Size { get; set; }

        public bool IsNew { get; set; }
        public bool IsSale { get; set; }

        [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters")]
        public string? Description { get; set; }

        [StringLength(500, ErrorMessage = "Image path cannot exceed 500 characters")]
        public string? ImagePath { get; set; }
        [NotMapped] 
        public int DiscountPercentage { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }
}