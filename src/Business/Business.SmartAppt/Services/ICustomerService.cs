using Business.SmartAppt.Models;

namespace Business.SmartAppt.Services
{
    public interface ICustomerService
    {
        Task<ResponseBase> GetMyBookingsAsync(int customerId, int skip = 0, int take = 10);
        Task<ResponseBase> CreateBookingAsync(int customerId, CreateBookingDto booking);
        Task<ResponseBase> UpdateBookingAsync(int customerId, int bookingId, UpdateBookingDto booking);
        Task<ResponseBase> CancelBookingAsync(int customerId, int bookingId);
        Task<ResponseBase> DeleteBookingAsync(int customerId, int bookingId);
        Task<ResponseBase> GetMonthlyCalendar(int businessId, int serviceId, int month, int year);
        Task<ResponseBase> GetDailyFreeSlots(int businessId, int serviceId, DateOnly date);

    }
}