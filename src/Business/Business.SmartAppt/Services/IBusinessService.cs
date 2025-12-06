using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Business.SmartAppt.Models;

namespace Business.SmartAppt.Services
{
    public interface IBusinessService
    {
        Task<BaseResponse> CreateBusinessAsync(BusinessDto business);
        Task<BaseResponse> UpdateBusinessByIdAsync(int id,BusinessDto business);
        Task<BaseResponse?> GetBusinessByIdAsync(int businessId);
        Task<BaseResponse> DeleteAsync(int businessId);


        Task<BaseResponse> AddServiceAsync(int businessId, ServiceDto service);
        Task<BaseResponse> DeleteService(int id);
        Task<BaseResponse> GetMyServicesAsync(int businessId, int skip = 0, int take = 10);
        Task<BaseResponse> DeactivateServiceAsync(int businessId, int serviceId);

        Task<BaseResponse> GetMonthlyCalendarAsync(int businessId, int serviceId, int month, int year);
        Task<BaseResponse> GetDailyBookingsAsync(int businessId, int serviceId, DateOnly date, int skip = 0, int take = 50);
     
        Task<BaseResponse> GetBookingsAsync(
            int businessId,
            int? serviceId = null,
            string? status = null,
            DateOnly? date = null,
            int skip = 0,
            int take = 50);

       
        Task<BaseResponse> DecideBookingAsync(int businessId, int bookingId, bool confirm);




    }
}
