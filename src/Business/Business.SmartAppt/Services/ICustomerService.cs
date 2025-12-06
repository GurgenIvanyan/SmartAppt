using Business.SmartAppt.Models;

namespace Business.SmartAppt.Services
{
    public interface ICustomerService
    {
        Task<BaseResponse> GetMyBookingsAsync(int customerId, int skip = 0, int take = 10);
        Task<BaseResponse> CreateBookingAsync(int customerId, CreateBookingDto booking);
        Task<BaseResponse> UpdateBookingAsync(int customerId, int bookingId, UpdateBookingDto booking);
        Task<BaseResponse> CancelBookingAsync(int customerId, int bookingId);
        Task<BaseResponse> DeleteBookingAsync(int customerId, int bookingId);
        Task<BaseResponse> GetMonthlyCalendar(int businessId, int serviceId, int month, int year);
        Task<BaseResponse> GetDailyFreeSlots(int businessId, int serviceId, DateOnly date);

    }
}