using System;
using System.Threading.Tasks;
using Business.SmartAppt.Models;
using Business.SmartAppt.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace SmartAppt.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CustomerController : ControllerBase
    {
        private readonly ICustomerService _customerService;

        public CustomerController(ICustomerService customerService)
        {
            _customerService = customerService;
        }

        // POST: api/customer/{customerId}/bookings
        [HttpPost("{customerId:int}/bookings")]
      
        public async Task<BaseResponse> CreateBooking(
            int customerId,
            [FromBody] CreateBookingDto dto)
        {
            var response = await _customerService.CreateBookingAsync(customerId, dto);
            return response;
        }

        // PUT: api/customer/{customerId}/bookings/{bookingId}
        [HttpPut("{customerId:int}/bookings/{bookingId:int}")]
        [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status200OK)]
        public async Task<BaseResponse> UpdateBooking(
            int customerId,
            int bookingId,
            [FromBody] UpdateBookingDto dto)
        {
            var response = await _customerService.UpdateBookingAsync(customerId, bookingId, dto);
            return response;
        }

        // DELETE: api/customer/{customerId}/bookings/{bookingId}
        [HttpDelete("{customerId:int}/bookings/{bookingId:int}")]
        [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status200OK)]
        public async Task<BaseResponse> DeleteBooking(
            int customerId,
            int bookingId)
        {
            var response = await _customerService.DeleteBookingAsync(customerId, bookingId);
            return response;
        }

        // POST: api/customer/{customerId}/bookings/{bookingId}/cancel
        [HttpPost("{customerId:int}/bookings/{bookingId:int}/cancel")]
        [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status200OK)]
        public async Task<BaseResponse> CancelBooking(
            int customerId,
            int bookingId)
        {
            var response = await _customerService.CancelBookingAsync(customerId, bookingId);
            return response;
        }

        // GET: api/customer/{customerId}/bookings?skip=0&take=10
        [HttpGet("{customerId:int}/bookings")]
        [ProducesResponseType(typeof(BookingListDto), StatusCodes.Status200OK)]
        public async Task<BaseResponse> GetMyBookings(
            int customerId,
            [FromQuery] int skip = 0,
            [FromQuery] int take = 10)
        {
            var response = await _customerService.GetMyBookingsAsync(customerId, skip, take);
            return response;
        }

        // GET: api/customer/free-slots?businessId=1&serviceId=2&date=2025-12-05
        [HttpGet("free-slots")]
        [ProducesResponseType(typeof(DailySlotsDto), StatusCodes.Status200OK)]
        public async Task<BaseResponse> GetDailyFreeSlots(
            [FromQuery] int businessId,
            [FromQuery] int serviceId,
            [FromQuery] DateOnly date)
        {
            var response = await _customerService.GetDailyFreeSlots(businessId, serviceId, date);
            return response;
        }

        // GET: api/customer/monthly-calendar?businessId=1&serviceId=2&month=12&year=2025
        [HttpGet("monthly-calendar")]
        [ProducesResponseType(typeof(CalendarDto), StatusCodes.Status200OK)]
        public async Task<BaseResponse> GetMonthlyCalendar(
            [FromQuery] int businessId,
            [FromQuery] int serviceId,
            [FromQuery] int month,
            [FromQuery] int year)
        {
            var response = await _customerService.GetMonthlyCalendar(businessId, serviceId, month, year);
            return response;
        }
    }
}
