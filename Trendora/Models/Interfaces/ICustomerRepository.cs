namespace Trendora.Models.Interfaces
{
    public interface ICustomerRepository
    {
        void SaveCustomer(Customer customer);
        Customer GetCustomer(string? email);

        IEnumerable<Customer> GetAllCustomers();
        void UpdatePassword(string userId, string passwordHash);
        void UpdateCustomer(Customer customer);
        Customer GetCustomerById(string id);

        int GetCustomerCount();

        void DeleteCustomer(string id);
       


    }
        
}