using Data.SmartAppt.SQL.Models;
namespace Data.SmartAppt.SQL.Services.Interfaces
{
    public interface IServiceRepository
    {
        Task<int> CreateAsync(ServiceEntity service);
        Task<ServiceEntity?> GetByIdAsync(int serviceId);
        Task<IEnumerable<ServiceEntity>> GetAllAsync(int skip = 0, int take = 10);
        Task UpdateAsync(ServiceEntity service);
        Task DeleteAsync(int serviceid);

    }
}