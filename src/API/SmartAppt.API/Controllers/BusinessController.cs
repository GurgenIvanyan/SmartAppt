using System;
using System.Reflection;
using System.Threading.Tasks;
using Business.SmartAppt.Models;
using Business.SmartAppt.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace SmartAppt.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BusinessController : ControllerBase
    {
        private readonly IBusinessService _businessService;

        public BusinessController(IBusinessService businessService)
        {
            _businessService = businessService;
        }

        // POST: api/business
        [HttpPost]
        [ProducesResponseType(typeof(BusinessDto), StatusCodes.Status200OK)]
        public async Task<BaseResponse> CreateBusiness([FromBody] BusinessDto dto)
        {
            var response = await _businessService.CreateBusinessAsync(dto);
            return response;
        }

        // PUT: api/business/{id}
        [HttpPut("{id:int}")]
        [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status200OK)]
        public async Task<BaseResponse> UpdateBusiness(int id, [FromBody] BusinessDto dto)
        {
            var response = await _businessService.UpdateBusinessByIdAsync(id, dto);
            return response;
        }

        // GET: api/business/{id}
        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(BusinessDto), StatusCodes.Status200OK)]
        public async Task<BaseResponse> GetBusinessById(int id)
        {
            var response = await _businessService.GetBusinessByIdAsync(id);
            return response;
        }

        // DELETE: api/business/{businessId}
        [HttpDelete("{businessId:int}")]
        [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status200OK)]
        public async Task<BaseResponse> DeleteBusiness(int businessId)
        {
            var response = await _businessService.DeleteAsync(businessId);
            return response;
        }

        // POST: api/business/{businessId}/services
        [HttpPost("{businessId:int}/services")]
        [ProducesResponseType(typeof(ServiceDto), StatusCodes.Status200OK)]
        public async Task<BaseResponse> AddService(int businessId, [FromBody] ServiceDto dto)
        {
            var response = await _businessService.AddServiceAsync(businessId, dto);
            return response;
        }

        // DELETE: api/business/services/{id}
        [HttpDelete("services/{id:int}")]
        [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status200OK)]
        public async Task<BaseResponse> DeleteService(int id)
        {
            var response = await _businessService.DeleteService(id);
            return response;
        }

        // GET: api/business/{businessId}/services?skip=0&take=10
        [HttpGet("{businessId:int}/services")]
        [ProducesResponseType(typeof(ServiceListDto), StatusCodes.Status200OK)]
        public async Task<BaseResponse> GetMyServices(
            int businessId,
            [FromQuery] int skip = 0,
            [FromQuery] int take = 10)
        {
            var response = await _businessService.GetMyServicesAsync(businessId, skip, take);
            return response;
        }

        // POST: api/business/{businessId}/services/{serviceId}/deactivate
        [HttpPost("{businessId:int}/services/{serviceId:int}/deactivate")]
        [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status200OK)]
        public async Task<BaseResponse> DeactivateService(int businessId, int serviceId)
        {
            var response = await _businessService.DeactivateServiceAsync(businessId, serviceId);
            return response;
        }

        // GET: api/business/{businessId}/services/{serviceId}/calendar?month=12&year=2025
        [HttpGet("{businessId:int}/services/{serviceId:int}/calendar")]
        [ProducesResponseType(typeof(CalendarDto), StatusCodes.Status200OK)]
        public async Task<BaseResponse> GetMonthlyCalendar(
            int businessId,
            int serviceId,
            [FromQuery] int month,
            [FromQuery] int year)
        {
            var response = await _businessService.GetMonthlyCalendarAsync(businessId, serviceId, month, year);
            return response;
        }

        // GET: api/business/{businessId}/services/{serviceId}/bookings/daily?date=2025-12-05&skip=0&take=50
        [HttpGet("{businessId:int}/services/{serviceId:int}/bookings/daily")]
        [ProducesResponseType(typeof(DailyBookingsDto), StatusCodes.Status200OK)]
        public async Task<BaseResponse> GetDailyBookings(
            int businessId,
            int serviceId,
            [FromQuery] DateOnly date,
            [FromQuery] int skip = 0,
            [FromQuery] int take = 50)
        {
            var response = await _businessService.GetDailyBookingsAsync(businessId, serviceId, date, skip, take);
            return response;
        }

        // GET: api/business/{businessId}/bookings?serviceId=&status=&date=&skip=&take=
        [HttpGet("{businessId:int}/bookings")]
        [ProducesResponseType(typeof(BookingListDto), StatusCodes.Status200OK)]
        public async Task<BaseResponse> GetBookings(
            int businessId,
            [FromQuery] int? serviceId = null,
            [FromQuery] string? status = null,
            [FromQuery] DateOnly? date = null,
            [FromQuery] int skip = 0,
            [FromQuery] int take = 50)
        {
            var response = await _businessService.GetBookingsAsync(businessId, serviceId, status, date, skip, take);
            return response;
        }


        [HttpPost("{businessId:int}/bookings/{bookingId:int}/decide")]
        [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status200OK)]
        public async Task<BaseResponse> DecideBooking(
            int businessId,
            int bookingId,
            [FromQuery] bool confirm)
        {
            var response = await _businessService.DecideBookingAsync(businessId, bookingId, confirm);
            return response;
        }


    }
}
