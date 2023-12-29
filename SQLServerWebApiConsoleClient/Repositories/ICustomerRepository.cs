using WebAPIModels.Models;

namespace SQLServerWebApiConsoleClient.Repositories
{
    public interface ICustomerRepository
    {
        Task<Customer?> GetAsync(string customerId);
        Task<Customer> UpdateAsync(string customerId,Customer customer);
        Task<bool?> DeleteAsync(string customerId);
        Task<IEnumerable<Customer>> GetAllAsync();
        Task<Customer> CreateAsync(Customer customer);
    }
}
