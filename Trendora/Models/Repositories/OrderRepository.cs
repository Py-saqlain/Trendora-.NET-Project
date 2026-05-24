using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using Dapper;
using Trendora.Models.Interfaces;

namespace Trendora.Models.Repositories
{
    public class OrderRepository : IOrderRepository
    {
        private readonly string _connectionString;
        private readonly IProductRepository _productRepository;

        public OrderRepository(IConfiguration configuration, IProductRepository productRepository)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string not found.");
            _productRepository = productRepository;
        }

        private string GenerateOrderNumber()
        {
            using var connection = new SqlConnection(_connectionString);

            var datePart = DateTime.Now.ToString("yyyyMMdd");
            var countQuery = @"
                SELECT COUNT(*) 
                FROM Orders 
                WHERE CONVERT(varchar(10), OrderDate, 112) = @datePart";

            var count = connection.ExecuteScalar<int>(countQuery, new { datePart });

            return $"TRN-{datePart}-{count + 1:000000}";
        }

 
        public Order? GetOrderById(int orderId)
        {
            using var connection = new SqlConnection(_connectionString);

            var orderQuery = @"
                SELECT o.*
                FROM Orders o
                WHERE o.OrderId = @orderId";

            var order = connection.QueryFirstOrDefault<Order>(orderQuery, new { orderId });

            if (order == null) return null;

            if (!string.IsNullOrEmpty(order.CustomerId))
            {
                var customerQuery = "SELECT * FROM AspNetUsers WHERE Id = @CustomerId";
                order.Customer = connection.QueryFirstOrDefault<Customer>(customerQuery,
                    new { CustomerId = order.CustomerId });
            }

            order.OrderItems = GetOrderItems(orderId);
            return order;
        }

 
        public Order? GetOrderByNumber(string orderNumber)
        {
            using var connection = new SqlConnection(_connectionString);

            var query = @"
                SELECT o.*
                FROM Orders o
                WHERE o.OrderNumber = @orderNumber";

            var order = connection.QueryFirstOrDefault<Order>(query, new { orderNumber });

            if (order == null) return null;

            if (!string.IsNullOrEmpty(order.CustomerId))
            {
                var customerQuery = "SELECT * FROM AspNetUsers WHERE Id = @CustomerId";
                order.Customer = connection.QueryFirstOrDefault<Customer>(customerQuery,
                    new { CustomerId = order.CustomerId });
            }

            order.OrderItems = GetOrderItems(order.OrderId);
            return order;
        }

        public List<Order> GetCustomerOrders(string customerId)
        {
            using var connection = new SqlConnection(_connectionString);

            var orders = connection.Query<Order>(
                @"
                SELECT o.*
                FROM Orders o
                WHERE o.CustomerId = @customerId
                ORDER BY o.OrderDate DESC",
                new { customerId }
            ).AsList();

            foreach (var order in orders)
            {
                if (!string.IsNullOrEmpty(order.CustomerId))
                {
                    var customerQuery = "SELECT * FROM AspNetUsers WHERE Id = @CustomerId";
                    order.Customer = connection.QueryFirstOrDefault<Customer>(customerQuery,
                        new { CustomerId = order.CustomerId });
                }

                order.OrderItems = GetOrderItems(order.OrderId);
            }

            return orders;
        }

        public List<Order> GetAllOrders()
        {
            using var connection = new SqlConnection(_connectionString);

            var orders = connection.Query<Order>(
                @"
                SELECT o.*
                FROM Orders o
                ORDER BY o.OrderDate DESC"
            ).AsList();

            foreach (var order in orders)
            {
                if (!string.IsNullOrEmpty(order.CustomerId))
                {
                    var customerQuery = "SELECT * FROM AspNetUsers WHERE Id = @CustomerId";
                    order.Customer = connection.QueryFirstOrDefault<Customer>(customerQuery,
                        new { CustomerId = order.CustomerId });
                }

                order.OrderItems = GetOrderItems(order.OrderId);
            }

            return orders;
        }

        public Order CreateOrder(Order order, List<CartItem> cartItems)
        {
            using var connection = new SqlConnection(_connectionString);
            connection.Open();

            using var transaction = connection.BeginTransaction();

            try
            {
                order.OrderNumber = GenerateOrderNumber();

                order.SubTotal = cartItems.Sum(i => i.TotalPrice);
                order.Tax = order.SubTotal * 0.08m; 
                order.Total = order.SubTotal + order.Shipping + order.Tax;
                order.OrderDate = DateTime.Now;
                order.CreatedAt = DateTime.Now;
                order.UpdatedAt = DateTime.Now;
                order.Status = "Pending";
                order.PaymentStatus = "Pending";

                var orderId = connection.ExecuteScalar<int>(
                    @"
                    INSERT INTO Orders
                    (CustomerId, OrderNumber, OrderDate, Status, SubTotal, Shipping, Tax, Total,
                     ShippingAddress, PaymentMethod, PaymentStatus, CreatedAt, UpdatedAt)
                    VALUES
                    (@CustomerId, @OrderNumber, @OrderDate, @Status, @SubTotal, @Shipping, @Tax, @Total,
                     @ShippingAddress, @PaymentMethod, @PaymentStatus, @CreatedAt, @UpdatedAt);
                    SELECT CAST(SCOPE_IDENTITY() AS INT);",
                    order,
                    transaction
                );

                order.OrderId = orderId;

                foreach (var item in cartItems)
                {
                    connection.Execute(
                        @"
                        INSERT INTO OrderItems
                        (OrderId, ProductId, ProductName, Brand, Size, Color, Quantity,
                         UnitPrice, TotalPrice, ImagePath)
                        VALUES
                        (@OrderId, @ProductId, @ProductName, @Brand, @Size, @Color, @Quantity,
                         @UnitPrice, @TotalPrice, @ImagePath)",
                        new
                        {
                            OrderId = orderId,
                            item.ProductId,
                            item.ProductName,
                            item.Brand,
                            item.Size,
                            item.Color,
                            item.Quantity,
                            UnitPrice = item.Price,
                            TotalPrice = item.TotalPrice,
                            item.ImagePath
                        },
                        transaction
                    );

                    var product = _productRepository.GetProductById(item.ProductId);
                    if (product == null || product.Quantity < item.Quantity)
                        throw new InvalidOperationException($"Insufficient stock for product: {item.ProductName}");

                    product.Quantity -= item.Quantity;

                    connection.Execute(
                        "UPDATE Products SET Quantity = @Quantity, UpdatedAt = GETDATE() WHERE ProductId = @ProductId",
                        new { Quantity = product.Quantity, ProductId = product.ProductId },
                        transaction
                    );
                }

                transaction.Commit();
                return order;
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                Console.WriteLine($"Error creating order: {ex.Message}");
                throw;
            }
        }

        public bool UpdateOrderStatus(int orderId, string status)
        {
            using var connection = new SqlConnection(_connectionString);
            return connection.Execute(
                "UPDATE Orders SET Status = @status, UpdatedAt = GETDATE() WHERE OrderId = @orderId",
                new { orderId, status }) > 0;
        }

        public bool UpdatePaymentStatus(int orderId, string paymentStatus)
        {
            using var connection = new SqlConnection(_connectionString);
            return connection.Execute(
                "UPDATE Orders SET PaymentStatus = @paymentStatus, UpdatedAt = GETDATE() WHERE OrderId = @orderId",
                new { orderId, paymentStatus }) > 0;
        }

     
        public bool DeleteOrder(int orderId)
        {
            using var connection = new SqlConnection(_connectionString);
            connection.Open();

            using var transaction = connection.BeginTransaction();

            try
            {
               
                var items = connection.Query<dynamic>(
                    "SELECT ProductId, Quantity FROM OrderItems WHERE OrderId = @orderId",
                    new { orderId },
                    transaction
                );

                foreach (var item in items)
                {
                    connection.Execute(
                        "UPDATE Products SET Quantity = Quantity + @Quantity WHERE ProductId = @ProductId",
                        new { Quantity = item.Quantity, ProductId = item.ProductId },
                        transaction
                    );
                }

            
                connection.Execute("DELETE FROM OrderItems WHERE OrderId = @orderId",
                    new { orderId }, transaction);

                var rows = connection.Execute("DELETE FROM Orders WHERE OrderId = @orderId",
                    new { orderId }, transaction);

                transaction.Commit();
                return rows > 0;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        // ===================== HELPERS =====================
        public List<OrderItem> GetOrderItems(int orderId)
        {
            using var connection = new SqlConnection(_connectionString);

            var items = connection.Query<OrderItem>(
                @"
                SELECT oi.*
                FROM OrderItems oi
                WHERE oi.OrderId = @orderId",
                new { orderId }
            ).AsList();

            foreach (var item in items)
            {
                if (item.ProductId > 0)
                {
                    var product = _productRepository.GetProductById(item.ProductId);
                    item.Product = product;
                }
            }

            return items;
        }

        public int GetOrderCount()
        {
            using var connection = new SqlConnection(_connectionString);
            return connection.ExecuteScalar<int>("SELECT COUNT(*) FROM Orders");
        }

        public decimal GetTotalRevenue()
        {
            using var connection = new SqlConnection(_connectionString);
            return connection.ExecuteScalar<decimal?>(
                "SELECT SUM(Total) FROM Orders WHERE Status != 'Cancelled'") ?? 0;
        }

        public void UpdateOrder(Order order)
        {
            using var connection = new SqlConnection(_connectionString);
            connection.Open();

            using var transaction = connection.BeginTransaction();

            try
            {
               
                var rowsAffected = connection.Execute(
                    @"
            UPDATE Orders SET
                CustomerId = @CustomerId,
                Status = @Status,
                SubTotal = @SubTotal,
                Shipping = @Shipping,
                Tax = @Tax,
                Total = @Total,
                ShippingAddress = @ShippingAddress,
                PaymentMethod = @PaymentMethod,
                PaymentStatus = @PaymentStatus,
                UpdatedAt = GETDATE()
            WHERE OrderId = @OrderId",
                    new
                    {
                        order.OrderId,
                        order.CustomerId,
                        order.Status,
                        order.SubTotal,
                        order.Shipping,
                        order.Tax,
                        order.Total,
                        order.ShippingAddress,
                        order.PaymentMethod,
                        order.PaymentStatus
                    },
                    transaction
                );

                if (rowsAffected == 0)
                {
                    throw new InvalidOperationException($"Order with ID {order.OrderId} not found.");
                }

                transaction.Commit();
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                Console.WriteLine($"Error updating order: {ex.Message}");
                throw;
            }
        }

        public bool OrderExists(int orderId)
        {
            using var connection = new SqlConnection(_connectionString);
            return connection.ExecuteScalar<int>(
                "SELECT COUNT(1) FROM Orders WHERE OrderId = @orderId",
                new { orderId }) > 0;
        }

        public List<Order> GetRecentOrders(int count = 10)
        {
            using var connection = new SqlConnection(_connectionString);

            var orders = connection.Query<Order>(
                @"
                SELECT TOP (@count) o.*
                FROM Orders o
                ORDER BY o.OrderDate DESC",
                new { count }
            ).AsList();

            foreach (var order in orders)
            {
                if (!string.IsNullOrEmpty(order.CustomerId))
                {
                    order.Customer = connection.QueryFirstOrDefault<Customer>(
                        "SELECT * FROM AspNetUsers WHERE Id = @CustomerId",
                        new { CustomerId = order.CustomerId });
                }

                order.OrderItems = GetOrderItems(order.OrderId);
            }

            return orders;
        }

        public List<Order> GetOrdersByStatus(string status)
        {
            using var connection = new SqlConnection(_connectionString);

            var orders = connection.Query<Order>(
                @"
                SELECT o.*
                FROM Orders o
                WHERE o.Status = @status
                ORDER BY o.OrderDate DESC",
                new { status }
            ).AsList();

            foreach (var order in orders)
            {
                if (!string.IsNullOrEmpty(order.CustomerId))
                {
                    order.Customer = connection.QueryFirstOrDefault<Customer>(
                        "SELECT * FROM AspNetUsers WHERE Id = @CustomerId",
                        new { CustomerId = order.CustomerId });
                }

                order.OrderItems = GetOrderItems(order.OrderId);
            }

            return orders;
        }
    }
}