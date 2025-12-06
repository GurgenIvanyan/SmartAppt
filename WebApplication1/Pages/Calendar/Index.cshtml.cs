using System.Net.Http.Json;
using Business.SmartAppt.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebApplication1.Pages.Calendar
{
    public class IndexModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;

        [BindProperty]
        public int BusinessId { get; set; }

        [BindProperty]
        public int ServiceId { get; set; }

        [BindProperty]
        public int Month { get; set; } = DateTime.UtcNow.Month;

        [BindProperty]
        public int Year { get; set; } = DateTime.UtcNow.Year;

        [BindProperty]
        public int Skip { get; set; } = 0;

        [BindProperty]
        public int Take { get; set; } = 50;

        public CalendarDto? MonthlyCalendar { get; set; }

        public DailyBookingsDto? DailyBookings { get; set; }

        [BindProperty]
        public int? SelectedDay { get; set; }

        public string? Message { get; set; }

        public IndexModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostLoadCalendarAsync()
        {
            if (BusinessId <= 0 || ServiceId <= 0)
            {
                Message = "BusinessId and ServiceId are required.";
                return Page();
            }

            if (Month < 1 || Month > 12 || Year < 1)
            {
                Message = "Invalid month/year.";
                return Page();
            }

            await LoadCalendarAsync();
            DailyBookings = null;
            SelectedDay = null;

            return Page();
        }

        public async Task<IActionResult> OnPostLoadBookingsAsync(int day)
        {
            if (BusinessId <= 0 || ServiceId <= 0)
            {
                Message = "BusinessId and ServiceId are required.";
                return Page();
            }

            if (day < 1 || day > DateTime.DaysInMonth(Year, Month))
            {
                Message = "Invalid day.";
                return Page();
            }

            SelectedDay = day;

            await LoadCalendarAsync();
            await LoadDailyBookingsAsync(day);

            return Page();
        }

        public async Task<IActionResult> OnPostCancelBookingAsync(int bookingId, int day)
        {
            if (BusinessId <= 0 || bookingId <= 0)
            {
                Message = "Invalid BusinessId or BookingId.";
                return Page();
            }

            var client = _httpClientFactory.CreateClient("SmartApptApi");

            var response = await client.PostAsync(
                $"api/business/{BusinessId}/bookings/{bookingId}/decide?confirm=false",
                null);

            if (!response.IsSuccessStatusCode)
            {
                Message = $"Cancel failed. Status: {response.StatusCode}";
            }
            else
            {
                var res = await response.Content.ReadFromJsonAsync<BaseResponse>();
                Message = $"Cancel completed. Status: {res?.Status}";
            }

            SelectedDay = day;

            await LoadCalendarAsync();
            await LoadDailyBookingsAsync(day);

            return Page();
        }

        private async Task LoadCalendarAsync()
        {
            var client = _httpClientFactory.CreateClient("SmartApptApi");

            var response = await client.GetAsync(
                $"api/business/{BusinessId}/services/{ServiceId}/calendar?month={Month}&year={Year}");

            if (!response.IsSuccessStatusCode)
            {
                Message = $"Failed to load calendar. Status: {response.StatusCode}";
                MonthlyCalendar = null;
                return;
            }

            var dto = await response.Content.ReadFromJsonAsync<CalendarDto>();

            if (dto == null || dto.Status != BaseResponseStatus.Success)
            {
                Message = $"Calendar error. Status: {dto?.Status}";
                MonthlyCalendar = null;
                return;
            }

            MonthlyCalendar = dto;
        }

        private async Task LoadDailyBookingsAsync(int day)
        {
            var client = _httpClientFactory.CreateClient("SmartApptApi");

            var date = new DateOnly(Year, Month, day);
            var dateStr = date.ToString("yyyy-MM-dd");

            var response = await client.GetAsync(
                $"api/business/{BusinessId}/services/{ServiceId}/bookings/daily?date={dateStr}&skip={Skip}&take={Take}");

            if (!response.IsSuccessStatusCode)
            {
                Message = $"Failed to load bookings. Status: {response.StatusCode}";
                DailyBookings = null;
                return;
            }

            var dto = await response.Content.ReadFromJsonAsync<DailyBookingsDto>();

            if (dto == null || dto.Status != BaseResponseStatus.Success)
            {
                Message = $"Bookings error. Status: {dto?.Status}";
                DailyBookings = null;
                return;
            }

            DailyBookings = dto;
        }
    }
}
