using Data.SmartAppt.SQL.Models;

namespace Data.SmartAppt.SQL.Services.Interfaces
{
    public interface IOpeningHoursRepository
    {
        Task<IEnumerable<OpeningHoursEntity>> GetAllAsync(int skip = 0, int take = 10);
        Task<OpeningHoursEntity?> GetByBusinessIdAndDowAsync(int businessId, byte dayOfWeek);
        Task<IEnumerable<OpeningHoursEntity>> GetByBusinessIdAsync(int businessId);
        Task<int> CreateAsync(OpeningHoursEntity entity);
        Task UpdateAsync(OpeningHoursEntity entity);
        Task DeleteAsync(int hoursId);
    }
}