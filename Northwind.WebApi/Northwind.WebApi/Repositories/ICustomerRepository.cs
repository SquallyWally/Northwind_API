using Northwind.Common.EntityModels.SqlServer;

namespace Northwind.WebApi.Repositories
{
    public interface ICustomerRepository
    {
        Task<Customer?> CreateAsync(Customer customer);
        Task<IEnumerable<Customer>> RetrieveAllAsync();
        Task<Customer?> RetrieveAsync(string id);
        Task<Customer?> UpdateAsync(string id, Customer customer);
        Task<bool?> DeleteAsync(string id);
    }
}
