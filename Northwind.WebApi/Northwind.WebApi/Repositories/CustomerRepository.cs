using System.Collections.Concurrent;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Northwind.Common.EntityModels.SqlServer;

namespace Northwind.WebApi.Repositories
{
    public class CustomerRepository : ICustomerRepository
    {
        // use a static thread-safe dictionary field to cache the customers
        private static ConcurrentDictionary<string, Customer>? customersCache;

        // instance of db ctx because it should not be cahced due to their internal caching
        private readonly NorthwindContext _db;

        public CustomerRepository(NorthwindContext injectedContext)
        {
            _db = injectedContext;

            if (customersCache is null)
            {
                // laad al het customers vanuit database al op
                // naar een thread-safe CocnurrentDict
                customersCache = new ConcurrentDictionary<string, Customer>(_db.Customers.ToDictionary(c => c.CustomerId));
            }
        }

        public async Task<Customer?> CreateAsync(Customer customer)
        {
            // normaliseer CustomerIdc naar upper
            customer.CustomerId = customer.CustomerId.ToUpper();

            // Database naar EF core
            EntityEntry<Customer> added = await _db.Customers.AddAsync(customer); // waar wordt dit gebruikt?

            int affected = await _db.SaveChangesAsync();
            if (affected == 1)
            {
                if (customersCache is null)
                {
                    return customer;
                }

                // new? naar cahche, anders roep je UpdateCache
                return customersCache.AddOrUpdate(customer.CustomerId, customer, UpdateCache);
            }

            return null;
        }

        private Customer UpdateCache(string id, Customer customer)
        {
            if (customersCache is null) return null!;
            if (!customersCache.TryGetValue(id, out var old)) return null!;
            return customersCache.TryUpdate(id, newValue: customer, comparisonValue: old) ? customer : null!;
        }

        public Task<IEnumerable<Customer>> RetrieveAllAsync()
        {
            // Als customerCache geen waardes heeft, dan roep je op een lege enum
            return Task.FromResult(customersCache?.Values ?? Enumerable.Empty<Customer>());
        }

        public Task<Customer?> RetrieveAsync(string id)
        {
            id = id.ToUpper();
            if (customersCache is null) return null!;
            customersCache.TryGetValue(id, out Customer? customer);
            return Task.FromResult(customer);
        }

        public async Task<Customer?> UpdateAsync(string id, Customer customer)
        {
            // normalize customer id
            id = id.ToUpper();
            customer.CustomerId = customer.CustomerId.ToUpper();

            //update to database
            _db.Customers.Update(customer);
            int affected = await _db.SaveChangesAsync();
            //update in cache, or else it returns null
            return affected == 1 ? UpdateCache(id, customer) : null;
        }

        public async Task<bool?> DeleteAsync(string id)
        {
            id = id.ToUpper();

            Customer? customer = _db.Customers.Find(id);
            if (customer is null) return null;
            _db.Customers.Remove(customer);

            int affected = await _db.SaveChangesAsync();

            return affected == 1 ? customersCache?.TryRemove(id, out customer) : null;
        }
    }
}