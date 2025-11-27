using Data.SmartAppt.SQL.Models;


namespace Data.SmartAppt.SQL.Services.Interfaces
{
    public interface IHolidayRepository
    {
        Task<IEnumerable<HolidayEntity>> GetAllAsync(int skip = 0, int take = 10);
        Task<HolidayEntity?> GetByBusinessIdAsync(int businessId, DateTime date);
        Task<HolidayEntity?> GetByIdAsync(int holidayId);
        Task<int> CreateAsync(HolidayEntity entity);
        Task UpdateAsync(HolidayEntity entity);
        Task DeleteAsync(int holidayId);
        Task<List<HolidayEntity>> GetAllByMonthAsync(int businessId, int year, int month);
    }
}