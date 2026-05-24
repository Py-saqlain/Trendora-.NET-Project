using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace Trendora.Models
{
    public class Customer:IdentityUser
    {
        public string FullName { get; set; } = string.Empty;
      
        public string? ProfileImgPath { get; set; }
        public string Address { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

  
        public List<Order>? OrderHistory { get; set; }
    }
}