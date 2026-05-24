namespace Trendora.Models.Interfaces
{
    public interface IOrderRepository
    {
        Order? GetOrderById(int orderId);
        void UpdateOrder(Order order);
        Order? GetOrderByNumber(string orderNumber);
        List<Order> GetCustomerOrders(string customerId);
        List<Order> GetAllOrders();
        Order CreateOrder(Order order, List<CartItem> cartItems);
        bool UpdateOrderStatus(int orderId, string status);
        bool UpdatePaymentStatus(int orderId, string paymentStatus);
        bool DeleteOrder(int orderId);
        List<OrderItem> GetOrderItems(int orderId);
        int GetOrderCount();
        decimal GetTotalRevenue();
        bool OrderExists(int orderId);
    }
}
