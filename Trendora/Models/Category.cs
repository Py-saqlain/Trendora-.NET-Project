using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Trendora.Models
{
    public class Category
    {
        [Key]
        public int CategoryId { get; set; }

        [Required(ErrorMessage = "Category name is required")]
        [StringLength(100, ErrorMessage = "Category name cannot exceed 100 characters")]
        public string Name { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;

        [NotMapped] 
        public int ProductQuantity { get; set; } = 0;

        [StringLength(500, ErrorMessage = "Image path cannot exceed 500 characters")]
        public string? ImagePath { get; set; }

        public bool IsNew { get; set; }
        public bool IsSale { get; set; }

        [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters")]
        public string? Description { get; set; }

        public ICollection<Product> Products { get; set; } = new List<Product>();

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }
}