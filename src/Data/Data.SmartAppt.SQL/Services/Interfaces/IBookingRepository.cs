using Data.SmartAppt.SQL.Models;

namespace Data.SmartAppt.SQL.Services.Interfaces
{
    public interface IBookingRepository
    {
        Task<IEnumerable<BookingEntity>> GetAllAsync(int skip = 0, int take = 10);
        Task<IEnumerable<BookingEntity>> GetAllSpecAsync(BookingFilter filter);
        Task<BookingEntity?> GetByIdAsync(int bookingId);
        Task<int> CreateAsync(BookingEntity entity);
        Task UpdateAsync(BookingEntity entity);
        Task DeleteAsync(int bookingId);
        Task CancelAsync(int bookingId);
        Task<Dictionary<DateOnly, int>> GetBookingsCountByBusinessAsync(int businessId, int serviceId, DateOnly startDate, DateOnly endDate);
    }
}