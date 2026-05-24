using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Trendora.Models
{
    public class Order
    {
        [Key]
        public int OrderId { get; set; }

        [Required]
        public string CustomerId { get; set; } = string.Empty;

        [ForeignKey("CustomerId")]
        public Customer? Customer { get; set; }

        [Required]
        public string OrderNumber { get; set; } = GenerateOrderNumber();

        public DateTime OrderDate { get; set; } = DateTime.Now;

        [Required]
        [StringLength(50)]
        public string Status { get; set; } = "Pending"; 

        [Column(TypeName = "decimal(18,2)")]
        public decimal SubTotal { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Shipping { get; set; } = 5.00m; 

        [Column(TypeName = "decimal(18,2)")]
        public decimal Tax { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Total { get; set; }

        [StringLength(500)]
        public string ShippingAddress { get; set; } = string.Empty;

        [StringLength(100)]
        public string PaymentMethod { get; set; } = "Cash on Delivery";

        [StringLength(100)]
        public string? PaymentStatus { get; set; } = "Pending";

        public List<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        private static string GenerateOrderNumber()
        {
            return $"TRN-{DateTime.Now:yyyyMMdd}-{new Random().Next(100000, 999999)}";
        }
    }
}