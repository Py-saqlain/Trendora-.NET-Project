namespace Trendora.Models.Interfaces
{
    public interface ICartRepository
    {
        Cart GetCart(string cartId);
        void SaveCart(string cartId, Cart cart);
        void ClearCart(string cartId);
        void MergeCarts(string sourceCartId, string targetCartId);
    }
}
