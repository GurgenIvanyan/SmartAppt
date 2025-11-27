using Business.SmartAppt.Models;
using Business.SmartAppt.Services;
using Microsoft.AspNetCore.Mvc;

namespace SmartAppt.API.Controllers
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

        private IActionResult GetStatusCodeResult<T>(T result, int code)
        {
            return code switch
            {
                0 => Ok(result),
                1 => NotFound(result),
                2 => BadRequest(result),
                _ => StatusCode(500, result)
            };
        }

        [HttpPost("{customerId}/bookings")]
        public async Task<IActionResult> CreateBooking(int customerId, [FromBody] CreateBookingDto dto)
        {
            var result = await _customerService.CreateBookingAsync(customerId, dto);
            return GetStatusCodeResult(result, result.StatusCode);
        }

        [HttpPut("{customerId}/bookings/{bookingId}")]
        public async Task<IActionResult> UpdateBooking(int customerId, int bookingId, [FromBody] UpdateBookingDto dto)
        {
            var result = await _customerService.UpdateBookingAsync(customerId, bookingId, dto);
            return GetStatusCodeResult(result, result.StatusCode);
        }

        [HttpDelete("{customerId}/bookings/{bookingId}")]
        public async Task<IActionResult> DeleteBooking(int customerId, int bookingId)
        {
            var result = await _customerService.DeleteBookingAsync(customerId, bookingId);
            return GetStatusCodeResult(result, result.StatusCode);
        }

        [HttpPost("{customerId}/bookings/{bookingId}/cancel")]
        public async Task<IActionResult> CancelBooking(int customerId, int bookingId)
        {
            var result = await _customerService.CancelBookingAsync(customerId, bookingId);
            return GetStatusCodeResult(result, result.StatusCode);
        }

        [HttpGet("{customerId}/bookings")]
        public async Task<IActionResult> GetMyBookings(int customerId, [FromQuery] int skip = 0, [FromQuery] int take = 10)
        {
            var result = await _customerService.GetMyBookingsAsync(customerId, skip, take);
            return GetStatusCodeResult(result, result.StatusCode);
        }


        [HttpGet("/api/businesses/{businessId}/services/{serviceId}/calendar/{year}/{month}/{day}/slots")]
        public async Task<IActionResult> GetDailyFreeSlots(int businessId, int serviceId, int year, int month, int day)
        {
            var date = new DateOnly(year, month, day);
            var result = await _customerService.GetDailyFreeSlots(businessId, serviceId, date);
            return GetStatusCodeResult(result, result.StatusCode);
        }

        [HttpGet("/api/businesses/{businessId}/services/{serviceId}/calendar/{year}/{month}")]
        public async Task<IActionResult> GetMonthlyCalendar(int businessId, int serviceId, int year, int month)
        {
            var result = await _customerService.GetMonthlyCalendar(businessId, serviceId, month, year);
            return GetStatusCodeResult(result, result.StatusCode);
        }
    }
}