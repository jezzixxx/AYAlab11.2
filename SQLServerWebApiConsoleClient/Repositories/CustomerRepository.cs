using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Collections.Concurrent;
using WebAPIModels.Models;

namespace SQLServerWebApiConsoleClient.Repositories
{
    public class CustomerRepository : ICustomerRepository
    {

        public static ConcurrentDictionary<string, Customer> customersCache;

        public DbSet<Customer> customers;
        private NorthwindContext db;

        public CustomerRepository(NorthwindContext db)
        {
            this.db = db;

            if(customersCache == null)
            {
                //ключи - CustomerId
                customersCache = new ConcurrentDictionary<string, Customer>(db.Customers.ToDictionary(c=>c.CustomerId));
            }
        }
        private Customer UpdateCache(string id, Customer customer)
        {
            //TryGetValue - приставка "Try", concurrentDictionary многопоточная коллекция
            Customer old;
            if (customersCache.TryGetValue(id, out old))
            {
                if (customersCache.TryUpdate(id, customer, old))
                {
                    return customer;
                }
            }
            return null;
        }

        public async Task<Customer> CreateAsync(Customer customer)
        {
            //верхний регистр - как в БД
            customer.CustomerId = customer.CustomerId.ToUpper();

            EntityEntry<Customer> added = await db.Customers.AddAsync(customer);
            int affectedRows = await db.SaveChangesAsync();
            if(affectedRows > 0)
            {
                return customersCache.AddOrUpdate(customer.CustomerId, customer, UpdateCache);
            }
            else
            {
                return null;
            }
        }

        public async Task<bool?> DeleteAsync(string customerId)
        {
            customerId = customerId.ToUpper();
            // удаление из базы данных
            Customer? c = db.Customers.Find(customerId);
            db.Customers.Remove(c);
            int affected = await db.SaveChangesAsync();
            if (affected == 1)
            {
                // удаление из кэша
                return customersCache.TryRemove(customerId, out c);
            }
            else
            {
                return null;
            }
        }

        public Task<IEnumerable<Customer>> GetAllAsync()
        {
            // извлечение из кэша для производительности
            return Task.Run<IEnumerable<Customer>>(() => customersCache.Values);
        }

        public Task<Customer?> GetAsync(string customerId)
        {
            return Task.Run(() =>
            {
                // извлечение из кэша для производительности
                customerId = customerId.ToUpper();
                customersCache.TryGetValue(customerId, out Customer? c);
                return c;
            });
        }

        public async Task<Customer> UpdateAsync(string customerId, Customer customer)
        {
            // нормализация ID клиента
            customerId = customerId.ToUpper();
            customer.CustomerId = customer.CustomerId.ToUpper();
            // обновление в базе данных
            db.Customers.Update(customer);
            int affected = await db.SaveChangesAsync();
            if (affected == 1)
            {
                // обновление в кэше
                return UpdateCache(customerId, customer);
            }
            return null;
        }
    }
}
