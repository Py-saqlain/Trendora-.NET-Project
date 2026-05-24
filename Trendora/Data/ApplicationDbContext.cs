using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Trendora.Models;

namespace Trendora.Data
{
    public class ApplicationDbContext : IdentityDbContext<Customer>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Product> Products { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure the Customer entity
            modelBuilder.Entity<Customer>(entity =>
            {
                entity.Property(c => c.FullName)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(c => c.ProfileImgPath)
                    .HasMaxLength(500);

                entity.Property(c => c.Address)
                    .HasMaxLength(500)
                    .IsRequired(false); // Make it optional to match model

                entity.Property(c => c.CreatedAt)
                    .HasDefaultValueSql("GETDATE()");

                entity.Property(c => c.UpdatedAt)
                    .HasDefaultValueSql("GETDATE()");

                entity.HasMany(c => c.OrderHistory)
                    .WithOne(o => o.Customer)
                    .HasForeignKey(o => o.CustomerId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Category configuration
            modelBuilder.Entity<Category>(entity =>
            {
                entity.HasKey(c => c.CategoryId);

                entity.Property(c => c.Name)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(c => c.ImagePath)
                    .HasMaxLength(500);

                entity.Property(c => c.Description)
                    .HasMaxLength(1000);

                entity.Property(c => c.CreatedAt)
                    .HasDefaultValueSql("GETDATE()");

                entity.Property(c => c.UpdatedAt)
                    .HasDefaultValueSql("GETDATE()");

                // Relationship with Products
                entity.HasMany(c => c.Products)
                    .WithOne(p => p.Category)
                    .HasForeignKey(p => p.CategoryId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // Product configuration
            modelBuilder.Entity<Product>(entity =>
            {
                entity.HasKey(p => p.ProductId);

                entity.Property(p => p.Name)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.Property(p => p.Brand)
                    .HasMaxLength(100);

                entity.Property(p => p.Price)
                    .HasColumnType("decimal(18,2)");

                entity.Property(p => p.OriginalPrice)
                    .HasColumnType("decimal(18,2)");

                // FIX: Match model annotation (50 chars, not 100)
                entity.Property(p => p.Color)
                    .HasMaxLength(50);

                entity.Property(p => p.Gender)
                    .HasMaxLength(20);

                entity.Property(p => p.Rating)
                    .HasColumnType("decimal(3,2)")
                    .HasDefaultValue(0);

                entity.Property(p => p.Quantity)
                    .HasDefaultValue(0);

                entity.Property(p => p.Size)
                    .HasMaxLength(100);

                entity.Property(p => p.Description)
                    .HasMaxLength(1000);

                entity.Property(p => p.ImagePath)
                    .HasMaxLength(500);

                entity.Property(p => p.IsNew)
                    .HasDefaultValue(false);

                entity.Property(p => p.IsSale)
                    .HasDefaultValue(false);

                // Relationship with Category
                entity.HasOne(p => p.Category)
                    .WithMany(c => c.Products)
                    .HasForeignKey(p => p.CategoryId)
                    .OnDelete(DeleteBehavior.SetNull)
                    .IsRequired(false);

                entity.Property(p => p.CreatedAt)
                    .HasDefaultValueSql("GETDATE()");

                entity.Property(p => p.UpdatedAt)
                    .HasDefaultValueSql("GETDATE()");

                // Indexes for better performance
                entity.HasIndex(p => p.Name);
                entity.HasIndex(p => p.CategoryId);
                entity.HasIndex(p => p.Brand);
                entity.HasIndex(p => p.Price);
                entity.HasIndex(p => p.IsNew);
                entity.HasIndex(p => p.IsSale);
            });

            // Order configuration
            modelBuilder.Entity<Order>(entity =>
            {
                entity.HasKey(o => o.OrderId);

                entity.Property(o => o.OrderNumber)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(o => o.Status)
                    .IsRequired()
                    .HasMaxLength(50)
                    .HasDefaultValue("Pending"); // Match model default

                entity.Property(o => o.SubTotal)
                    .HasColumnType("decimal(18,2)")
                    .HasDefaultValue(0);

                entity.Property(o => o.Shipping)
                    .HasColumnType("decimal(18,2)")
                    .HasDefaultValue(5.00m);

                entity.Property(o => o.Tax)
                    .HasColumnType("decimal(18,2)")
                    .HasDefaultValue(0);

                entity.Property(o => o.Total)
                    .HasColumnType("decimal(18,2)")
                    .HasDefaultValue(0);

                entity.Property(o => o.ShippingAddress)
                    .IsRequired()
                    .HasMaxLength(500);

                entity.Property(o => o.PaymentMethod)
                    .IsRequired()
                    .HasMaxLength(100)
                    .HasDefaultValue("Cash on Delivery");

                entity.Property(o => o.PaymentStatus)
                    .HasMaxLength(100)
                    .HasDefaultValue("Pending");

                entity.Property(o => o.OrderDate)
                    .HasDefaultValueSql("GETDATE()");

                entity.Property(o => o.CreatedAt)
                    .HasDefaultValueSql("GETDATE()");

                entity.Property(o => o.UpdatedAt)
                    .HasDefaultValueSql("GETDATE()");

                entity.Property(o => o.CustomerId)
                    .IsRequired()
                    .HasMaxLength(450);

                // Relationship with Customer
                entity.HasOne(o => o.Customer)
                    .WithMany(c => c.OrderHistory)
                    .HasForeignKey(o => o.CustomerId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Relationship with OrderItems
                entity.HasMany(o => o.OrderItems)
                    .WithOne(oi => oi.Order)
                    .HasForeignKey(oi => oi.OrderId)
                    .OnDelete(DeleteBehavior.Cascade);

                // Indexes
                entity.HasIndex(o => o.CustomerId);
                entity.HasIndex(o => o.Status);
                entity.HasIndex(o => o.OrderDate);
                entity.HasIndex(o => o.OrderNumber).IsUnique();
            });

            // OrderItem configuration - FIXED Property Name
            modelBuilder.Entity<OrderItem>(entity =>
            {
                entity.HasKey(oi => oi.OrderItemId);

                entity.Property(oi => oi.ProductName)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.Property(oi => oi.Brand)
                    .HasMaxLength(100);

                entity.Property(oi => oi.Size)
                    .HasMaxLength(20);

                entity.Property(oi => oi.Color)
                    .HasMaxLength(20);

                entity.Property(oi => oi.UnitPrice)
                    .HasColumnType("decimal(18,2)");

                entity.Property(oi => oi.TotalPrice)
                    .HasColumnType("decimal(18,2)");

                entity.Property(oi => oi.ImagePath)
                    .HasMaxLength(500);

               
                entity.HasOne(oi => oi.Order)
                    .WithMany(o => o.OrderItems)
                    .HasForeignKey(oi => oi.OrderId)
                    .OnDelete(DeleteBehavior.Cascade);

                // FIX: Correct property name (Products → Product)
                entity.HasOne(oi => oi.Product)
                    .WithMany()
                    .HasForeignKey(oi => oi.ProductId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Indexes
                entity.HasIndex(oi => oi.OrderId);
                entity.HasIndex(oi => oi.ProductId);
            });
        }

        public override int SaveChanges()
        {
            UpdateTimestamps();
            return base.SaveChanges();
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            UpdateTimestamps();
            return base.SaveChangesAsync(cancellationToken);
        }

        private void UpdateTimestamps()
        {
            var entries = ChangeTracker.Entries()
                .Where(e => e.Entity is BaseEntity &&
                    (e.State == EntityState.Added || e.State == EntityState.Modified));

            foreach (var entry in entries)
            {
                var entity = (BaseEntity)entry.Entity;
                entity.UpdatedAt = DateTime.Now;

                if (entry.State == EntityState.Added)
                {
                    entity.CreatedAt = DateTime.Now;

                    // Generate order number for new orders
                    if (entry.Entity is Order order && string.IsNullOrEmpty(order.OrderNumber))
                    {
                        order.OrderNumber = GenerateOrderNumber();
                    }
                }
            }
        }

        private string GenerateOrderNumber()
        {
            var today = DateTime.Now.ToString("yyyyMMdd");
            var existingCount = Orders.Count(o => o.OrderDate.Date == DateTime.Today);
            return $"TRN-{today}-{existingCount + 1:000000}";
        }
    }

    // Base entity class for common properties
    public abstract class BaseEntity
    {
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}