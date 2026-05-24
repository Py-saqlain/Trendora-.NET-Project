using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Trendora.Data;
using Trendora.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Trendora.Services
{
    public static class DbSeeder
    {
        public static async Task SeedDataAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<Customer>>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            // Ensure database is created or migrated
            await context.Database.MigrateAsync();

            // Ensure "User" role exists
            if (!await roleManager.RoleExistsAsync("User"))
            {
                await roleManager.CreateAsync(new IdentityRole("User"));
            }

            // Ensure "User" role exists
            if (!await roleManager.RoleExistsAsync("User"))
            {
                await roleManager.CreateAsync(new IdentityRole("User"));
            }

            // Ensure "User" role exists
            if (!await roleManager.RoleExistsAsync("User"))
            {
                await roleManager.CreateAsync(new IdentityRole("User"));
            }

            // Ensure "User" role exists
            if (!await roleManager.RoleExistsAsync("User"))
            {
                await roleManager.CreateAsync(new IdentityRole("User"));
            }

            // Ensure "User" role exists
            if (!await roleManager.RoleExistsAsync("User"))
            {
                await roleManager.CreateAsync(new IdentityRole("User"));
            }

            // Ensure "User" role exists
            if (!await roleManager.RoleExistsAsync("User"))
            {
                await roleManager.CreateAsync(new IdentityRole("User"));
            }

            // Ensure "User" role exists
            if (!await roleManager.RoleExistsAsync("User"))
            {
                await roleManager.CreateAsync(new IdentityRole("User"));
            }

            // Seed Categories
            if (!context.Categories.Any())
            {
                var categories = new List<Category>
                {
                    new Category { Name = "Men's Clothing", Description = "Stylish clothing for men", ImagePath = "/images/categories/mens-clothing.jpg", IsActive = true, CreatedAt = DateTime.Now },
                    new Category { Name = "Women's Clothing", Description = "Elegant clothing for women", ImagePath = "/images/categories/womens-clothing.jpg", IsActive = true, CreatedAt = DateTime.Now },
                    new Category { Name = "Kids' Fashion", Description = "Cute and comfortable kids wear", ImagePath = "/images/categories/kids-fashion.jpg", IsActive = true, CreatedAt = DateTime.Now },
                    new Category { Name = "Footwear", Description = "Premium shoes and footwear", ImagePath = "/images/categories/footwear.jpg", IsActive = true, CreatedAt = DateTime.Now },
                    new Category { Name = "Accessories", Description = "Fashion accessories", ImagePath = "/images/categories/accessories.jpg", IsActive = true, CreatedAt = DateTime.Now },
                    new Category { Name = "Sports Wear", Description = "Active and sports clothing", ImagePath = "/images/categories/sports-wear.jpg", IsActive = true, CreatedAt = DateTime.Now },
                    new Category { Name = "Formal Wear", Description = "Professional formal attire", ImagePath = "/images/categories/formal-wear.jpg", IsActive = true, CreatedAt = DateTime.Now },
                    new Category { Name = "Casual Wear", Description = "Comfortable casual clothes", ImagePath = "/images/categories/casual-wear.jpg", IsActive = true, CreatedAt = DateTime.Now },
                    new Category { Name = "Winter Collection", Description = "Warm winter clothing", ImagePath = "/images/categories/winter-collection.jpg", IsActive = true, CreatedAt = DateTime.Now },
                    new Category { Name = "Summer Collection", Description = "Cool summer outfits", ImagePath = "/images/categories/summer-collection.jpg", IsActive = true, CreatedAt = DateTime.Now }
                };

                await context.Categories.AddRangeAsync(categories);
                await context.SaveChangesAsync();
            }

            // Seed Products
            if (!context.Products.Any())
            {
                var categories = await context.Categories.ToListAsync();
                if (!categories.Any())
                {
                    throw new InvalidOperationException("No categories found in the database.");
                }

                if (!categories.Any())
                {
                    throw new InvalidOperationException("No categories found in the database.");
                }

                if (!categories.Any())
                {
                    throw new InvalidOperationException("No categories found in the database.");
                }

                if (!categories.Any())
                {
                    throw new InvalidOperationException("No categories found in the database.");
                }

                if (!categories.Any())
                {
                    throw new InvalidOperationException("No categories found in the database.");
                }

                if (!categories.Any())
                {
                    throw new InvalidOperationException("No categories found in the database.");
                }

                if (!categories.Any())
                {
                    throw new InvalidOperationException("No categories found in the database.");
                }

                                    var random = new Random();
                var sizes = new[] { "S", "M", "L", "XL" };
                var colors = new[] { "Black", "White", "Blue", "Red", "Green", "Navy", "Gray", "Brown" };
                var brands = new[] { "Nike", "Adidas", "Zara", "H&M", "Puma", "Levi's", "Gucci", "Prada" };

                if (!sizes.Any() || !colors.Any() || !brands.Any())
                {
                    throw new InvalidOperationException("Sizes, colors, or brands arrays are empty.");
                }

                var products = new List<Product>
                {
                    new Product
                    {
                        Name = "Classic Denim Jacket",
                        Description = "Classic blue denim jacket, perfect for casual occasions",
                        Price = 89.99m,
                        OriginalPrice = 129.99m,
                        Quantity = 50,
                        CategoryId = categories[0].CategoryId,
                        Brand = brands[random.Next(brands.Length)],
                        Size = sizes[random.Next(sizes.Length)],
                        Color = colors[random.Next(colors.Length)],
                        Rating = 4,
                        ImagePath = "/images/products/denim-jacket.jpg",
                        IsNew = true,
                        IsSale = true,
                        CreatedAt = DateTime.Now
                    },

                    new Product
                    {
                        Name = "Men's Cotton T-Shirt",
                        Description = "Soft cotton t-shirt, breathable and comfortable",
                        Price = 24.99m,
                        OriginalPrice = 39.99m,
                        Quantity = 100,
                        CategoryId = categories[0].CategoryId,
                        Brand = brands[3],
                        Size = sizes[random.Next(sizes.Length)],
                        Color = colors[0],
                        Rating = 4,
                        ImagePath = "/images/products/mens-tshirt.jpg",
                        IsNew = false,
                        IsSale = true,
                        CreatedAt = DateTime.Now
                    },

                    new Product
                    {
                        Name = "Men's Slim Fit Jeans",
                        Description = "Stylish slim fit jeans with stretch comfort",
                        Price = 59.99m,
                        OriginalPrice = 89.99m,
                        Quantity = 75,
                        CategoryId = categories[0].CategoryId,
                        Brand = brands[5],
                        Size = sizes[random.Next(sizes.Length)],
                        Color = colors[2],
                        Rating = 4,
                        ImagePath = "/images/products/mens-jeans.jpg",
                        IsNew = true,
                        IsSale = false,
                        CreatedAt = DateTime.Now
                    },

                    // Women's Clothing Products (Category 1)
                    new Product
                    {
                        Name = "Floral Summer Dress",
                        Description = "Beautiful floral print dress for summer",
                        Price = 49.99m,
                        OriginalPrice = 79.99m,
                        Quantity = 60,
                        CategoryId = categories[1].CategoryId,
                        Brand = brands[3],
                        Size = sizes[random.Next(sizes.Length)],
                        Color = colors[3],
                        Rating = 3,
                        ImagePath = "/images/products/floral-dress.jpg",
                        IsNew = true,
                        IsSale = true,
                        CreatedAt = DateTime.Now
                    },

                    new Product
                    {
                        Name = "Women's Blazer",
                        Description = "Elegant blazer for professional look",
                        Price = 79.99m,
                        OriginalPrice = 119.99m,
                        Quantity = 40,
                        CategoryId = categories[1].CategoryId,
                        Brand = brands[6],
                        Size = sizes[random.Next(sizes.Length)],
                        Color = colors[0],
                        Rating = 3,
                        ImagePath = "/images/products/womens-blazer.jpg",
                        IsNew = false,
                        IsSale = false,
                        CreatedAt = DateTime.Now
                    },

                    // Kids' Fashion Products (Category 2)
                    new Product
                    {
                        Name = "Kids Cartoon T-Shirt",
                        Description = "Colorful cartoon print t-shirt for kids",
                        Price = 19.99m,
                        OriginalPrice = 29.99m,
                        Quantity = 120,
                        CategoryId = categories[2].CategoryId,
                        Brand = brands[3],
                        Size = sizes[random.Next(sizes.Length)],
                        Color = colors[3],
                        Rating = 4,
                        ImagePath = "/images/products/kids-tshirt.jpg",
                        IsNew = true,
                        IsSale = true,
                        CreatedAt = DateTime.Now
                    },

                    // Footwear Products (Category 3)
                    new Product
                    {
                        Name = "Running Shoes",
                        Description = "Lightweight running shoes with cushioning",
                        Price = 99.99m,
                        OriginalPrice = 149.99m,
                        Quantity = 45,
                        CategoryId = categories[3].CategoryId,
                        Brand = brands[0],
                        Size = sizes[random.Next(sizes.Length)],
                        Color = colors[4],
                        Rating = 4,
                        ImagePath = "/images/products/running-shoes.jpg",
                        IsNew = true,
                        IsSale = true,
                        CreatedAt = DateTime.Now
                    },

                    new Product
                    {
                        Name = "Casual Sneakers",
                        Description = "Comfortable casual sneakers for daily wear",
                        Price = 69.99m,
                        OriginalPrice = 99.99m,
                        Quantity = 80,
                        CategoryId = categories[3].CategoryId,
                        Brand = brands[1],
                        Size = sizes[random.Next(sizes.Length)],
                        Color = colors[5],
                        Rating = 4,
                        ImagePath = "/images/products/casual-sneakers.jpg",
                        IsNew = false,
                        IsSale = false,
                        CreatedAt = DateTime.Now
                    },

                    // Accessories Products (Category 4)
                    new Product
                    {
                        Name = "Leather Wallet",
                        Description = "Genuine leather wallet with multiple compartments",
                        Price = 29.99m,
                        OriginalPrice = 49.99m,
                        Quantity = 150,
                        CategoryId = categories[4].CategoryId,
                        Brand = brands[6],
                        Size = "One Size",
                        Color = colors[7],
                        Rating = 4,
                        ImagePath = "/images/products/leather-wallet.jpg",
                        IsNew = false,
                        IsSale = true,
                        CreatedAt = DateTime.Now
                    },

                    new Product
                    {
                        Name = "Sports Watch",
                        Description = "Digital sports watch with fitness tracking",
                        Price = 149.99m,
                        OriginalPrice = 199.99m,
                        Quantity = 35,
                        CategoryId = categories[4].CategoryId,
                        Brand = brands[0],
                        Size = "One Size",
                        Color = colors[0],
                        Rating = 4,
                        ImagePath = "/images/products/sports-watch.jpg",
                        IsNew = true,
                        IsSale = false,
                        CreatedAt = DateTime.Now
                    },

                    // Sports Wear Products (Category 5)
                    new Product
                    {
                        Name = "Gym Training T-Shirt",
                        Description = "Moisture-wicking gym t-shirt",
                        Price = 34.99m,
                        OriginalPrice = 54.99m,
                        Quantity = 90,
                        CategoryId = categories[5].CategoryId,
                        Brand = brands[0],
                        Size = sizes[random.Next(sizes.Length)],
                        Color = colors[1],
                        Rating = 4,
                        ImagePath = "/images/products/gym-tshirt.jpg",
                        IsNew = true,
                        IsSale = true,
                        CreatedAt = DateTime.Now
                    },

                    new Product
                    {
                        Name = "Yoga Pants",
                        Description = "Stretchy yoga pants for flexibility",
                        Price = 44.99m,
                        OriginalPrice = 69.99m,
                        Quantity = 70,
                        CategoryId = categories[5].CategoryId,
                        Brand = brands[0],
                        Size = sizes[random.Next(sizes.Length)],
                        Color = colors[0],
                        Rating = 4,
                        ImagePath = "/images/products/yoga-pants.jpg",
                        IsNew = false,
                        IsSale = false,
                        CreatedAt = DateTime.Now
                    },

                    // Formal Wear Products (Category 6)
                    new Product
                    {
                        Name = "Men's Dress Shirt",
                        Description = "Crisp white dress shirt for formal occasions",
                        Price = 54.99m,
                        OriginalPrice = 84.99m,
                        Quantity = 65,
                        CategoryId = categories[6].CategoryId,
                        Brand = brands[5],
                        Size = sizes[random.Next(sizes.Length)],
                        Color = colors[1],
                        Rating = 4,
                        ImagePath = "/images/products/dress-shirt.jpg",
                        IsNew = false,
                        IsSale = true,
                        CreatedAt = DateTime.Now
                    },

                    new Product
                    {
                        Name = "Women's Pencil Skirt",
                        Description = "Professional pencil skirt for office wear",
                        Price = 49.99m,
                        OriginalPrice = 74.99m,
                        Quantity = 55,
                        CategoryId = categories[6].CategoryId,
                        Brand = brands[6],
                        Size = sizes[random.Next(sizes.Length)],
                        Color = colors[0],
                        Rating = 3,
                        ImagePath = "/images/products/pencil-skirt.jpg",
                        IsNew = true,
                        IsSale = false,
                        CreatedAt = DateTime.Now
                    },

                    // Casual Wear Products (Category 7)
                    new Product
                    {
                        Name = "Hooded Sweatshirt",
                        Description = "Comfortable hoodie for casual days",
                        Price = 59.99m,
                        OriginalPrice = 89.99m,
                        Quantity = 85,
                        CategoryId = categories[7].CategoryId,
                        Brand = brands[1],
                        Size = sizes[random.Next(sizes.Length)],
                        Color = colors[5],
                        Rating = 3,
                        ImagePath = "/images/products/hoodie.jpg",
                        IsNew = true,
                        IsSale = true,
                        CreatedAt = DateTime.Now
                    },

                    new Product
                    {
                        Name = "Cargo Shorts",
                        Description = "Practical cargo shorts with pockets",
                        Price = 39.99m,
                        OriginalPrice = 59.99m,
                        Quantity = 95,
                        CategoryId = categories[7].CategoryId,
                        Brand = brands[4],
                        Size = sizes[random.Next(sizes.Length)],
                        Color = colors[7],
                        Rating = 5,
                        ImagePath = "/images/products/cargo-shorts.jpg",
                        IsNew = false,
                        IsSale = false,
                        CreatedAt = DateTime.Now
                    },

                    // Winter Collection Products (Category 8)
                    new Product
                    {
                        Name = "Woolen Sweater",
                        Description = "Warm wool sweater for cold weather",
                        Price = 79.99m,
                        OriginalPrice = 119.99m,
                        Quantity = 40,
                        CategoryId = categories[8].CategoryId,
                        Brand = brands[6],
                        Size = sizes[random.Next(sizes.Length)],
                        Color = colors[5],
                        Rating = 4,
                        ImagePath = "/images/products/woolen-sweater.jpg",
                        IsNew = true,
                        IsSale = true,
                        CreatedAt = DateTime.Now
                    },

                    new Product
                    {
                        Name = "Winter Jacket",
                        Description = "Insulated winter jacket with hood",
                        Price = 129.99m,
                        OriginalPrice = 189.99m,
                        Quantity = 30,
                        CategoryId = categories[8].CategoryId,
                        Brand = brands[0],
                        Size = sizes[random.Next(sizes.Length)],
                        Color = colors[0],
                        Rating = 4,
                        ImagePath = "/images/products/winter-jacket.jpg",
                        IsNew = true,
                        IsSale = false,
                        CreatedAt = DateTime.Now
                    },

                    // Summer Collection Products (Category 9)
                    new Product
                    {
                        Name = "Beach Shorts",
                        Description = "Quick-dry beach shorts",
                        Price = 29.99m,
                        OriginalPrice = 44.99m,
                        Quantity = 110,
                        CategoryId = categories[9].CategoryId,
                        Brand = brands[4],
                        Size = sizes[random.Next(sizes.Length)],
                        Color = colors[2],
                        Rating = 4,
                        ImagePath = "/images/products/beach-shorts.jpg",
                        IsNew = false,
                        IsSale = true,
                        CreatedAt = DateTime.Now
                    },

                    new Product
                    {
                        Name = "Summer Hat",
                        Description = "Sun protection summer hat",
                        Price = 19.99m,
                        OriginalPrice = 29.99m,
                        Quantity = 130,
                        CategoryId = categories[9].CategoryId,
                        Brand = brands[3],
                        Size = "One Size",
                        Color = colors[1],
                        Rating = 4,
                        ImagePath = "/images/products/summer-hat.jpg",
                        IsNew = true,
                        IsSale = false,
                        CreatedAt = DateTime.Now
                    }
                };

                await context.Products.AddRangeAsync(products);
                await context.SaveChangesAsync();
            }

            // Seed Customers (Users)
            if (!context.Users.Any(u => u.Email == "john.doe@example.com"))
            {
                var customers = new List<Customer>
                {
                    new Customer { UserName = "john.doe@example.com", Email = "john.doe@example.com", EmailConfirmed = true, FullName = "John Doe", Address = "123 Main St, New York, NY 10001", PhoneNumber = "555-0101" },
                    new Customer { UserName = "jane.smith@example.com", Email = "jane.smith@example.com", EmailConfirmed = true, FullName = "Jane Smith", Address = "456 Oak Ave, Los Angeles, CA 90001", PhoneNumber = "555-0102" },
                    new Customer { UserName = "mike.johnson@example.com", Email = "mike.johnson@example.com", EmailConfirmed = true, FullName = "Mike Johnson", Address = "789 Pine Rd, Chicago, IL 60601", PhoneNumber = "555-0103" },
                    new Customer { UserName = "sarah.williams@example.com", Email = "sarah.williams@example.com", EmailConfirmed = true, FullName = "Sarah Williams", Address = "321 Elm St, Houston, TX 77001", PhoneNumber = "555-0104" },
                    new Customer { UserName = "david.brown@example.com", Email = "david.brown@example.com", EmailConfirmed = true, FullName = "David Brown", Address = "654 Maple Dr, Phoenix, AZ 85001", PhoneNumber = "555-0105" },
                    new Customer { UserName = "emily.davis@example.com", Email = "emily.davis@example.com", EmailConfirmed = true, FullName = "Emily Davis", Address = "987 Cedar Ln, Philadelphia, PA 19101", PhoneNumber = "555-0106" },
                    new Customer { UserName = "chris.miller@example.com", Email = "chris.miller@example.com", EmailConfirmed = true, FullName = "Chris Miller", Address = "147 Birch Way, San Antonio, TX 78201", PhoneNumber = "555-0107" },
                    new Customer { UserName = "lisa.wilson@example.com", Email = "lisa.wilson@example.com", EmailConfirmed = true, FullName = "Lisa Wilson", Address = "258 Spruce Ct, San Diego, CA 92101", PhoneNumber = "555-0108" },
                    new Customer { UserName = "robert.moore@example.com", Email = "robert.moore@example.com", EmailConfirmed = true, FullName = "Robert Moore", Address = "369 Willow Dr, Dallas, TX 75201", PhoneNumber = "555-0109" },
                    new Customer { UserName = "amanda.taylor@example.com", Email = "amanda.taylor@example.com", EmailConfirmed = true, FullName = "Amanda Taylor", Address = "741 Ash Ave, San Jose, CA 95101", PhoneNumber = "555-0110" }
                };

                string defaultPassword = "User123!";

                foreach (var customer in customers)
                {
                    var result = await userManager.CreateAsync(customer, defaultPassword);
                    if (result.Succeeded)
                    {
                        await userManager.AddToRoleAsync(customer, "User");
                    }
                }
            }

            // Seed Orders
            if (!context.Orders.Any())
            {
                var users = await userManager.GetUsersInRoleAsync("User");
                var products = await context.Products.ToListAsync();
                var random = new Random();
                var orderStatuses = new[] { "Pending", "Processing", "Shipped", "Delivered", "Cancelled" };
                var paymentMethods = new[] { "Credit Card", "PayPal", "Bank Transfer", "Cash on Delivery" };
                var paymentStatuses = new[] { "Pending", "Paid", "Failed", "Refunded" };

                var orders = new List<Order>();

                for (int i = 0; i < 10; i++)
                {
                    var user = users[random.Next(users.Count)];
                    var status = orderStatuses[random.Next(orderStatuses.Length)];
                    var paymentMethod = paymentMethods[random.Next(paymentMethods.Length)];
                    var paymentStatus = status == "Delivered" ? "Paid" : (status == "Cancelled" ? "Refunded" : paymentStatuses[random.Next(3)]);

                    var order = new Order
                    {
                        CustomerId = user.Id,
                        OrderDate = DateTime.Now.AddDays(-random.Next(1, 30)),
                        Total = 0m,
                        Status = "Pending",
                        PaymentMethod = "Credit Card",
                        PaymentStatus = "Paid",
                        ShippingAddress = user.Address,
                        CreatedAt = DateTime.Now.AddDays(-random.Next(1, 30)),
                        UpdatedAt = DateTime.Now,
                        OrderItems = new List<OrderItem>()
                    };

                    // Add 1-4 items per order
                    int itemCount = random.Next(1, 5);
                    decimal totalAmount = 0;

                    for (int j = 0; j < itemCount; j++)
                    {
                        var product = products[random.Next(products.Count)];
                        var quantity = random.Next(1, 4);
                        var unitPrice = product.Price;
                        var itemTotal = unitPrice * quantity;
                        totalAmount += itemTotal;

                        order.OrderItems.Add(new OrderItem
                        {
                            ProductId = product.ProductId,
                            Quantity = quantity,
                            UnitPrice = unitPrice,
                            TotalPrice = itemTotal
                        });
                    }

                    if (!order.OrderItems.Any())
                    {
                        throw new InvalidOperationException("Order must have at least one item.");
                    }

                    order.Total = totalAmount;
                    orders.Add(order);
                }

                await context.Orders.AddRangeAsync(orders);
                await context.SaveChangesAsync();
            }
        }
    }
}