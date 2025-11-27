using Data.SmartAppt.SQL.Models;

namespace Data.SmartAppt.SQL.Services.Interfaces
{
    public interface ICustomerRepository
    {
        Task<IEnumerable<CustomerEntity>> GetAllAsync(int skip = 0, int take = 10);
        Task<IEnumerable<CustomerEntity>> GetByBusinessIdAsync(int businessId);
        Task<CustomerEntity?> GetByIdAsync(int customerId);
        Task<int> CreateAsync(CustomerEntity entity);
        Task UpdateAsync(CustomerEntity customerEntity);
        Task DeleteAsync(int customerId);
    }
}