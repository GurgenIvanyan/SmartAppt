using System;
using System.Linq;
using System.Net.Http.Json;
using Business.SmartAppt.Models;
using Common.Logging;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebApplication1.Pages
{
    public class IndexModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;

   
        private static readonly Common.Logging.ILogger Log =
            AppLoggerFactory.CreateLogger<IndexModel>();

        public IndexModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

      
        [BindProperty(SupportsGet = true)]
        public int BusinessId { get; set; } = 1;

      
        public int TodayBookings { get; set; }
        public int PendingBookings { get; set; }
        public int CanceledToday { get; set; }

        public string? Message { get; set; }

        public async Task OnGetAsync()
        {
            Log.Info("Dashboard OnGetAsync started", new { BusinessId });

            var client = _httpClientFactory.CreateClient("SmartApptApi");

            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            var dateStr = today.ToString("yyyy-MM-dd");

            try
            {
                var bookingsResponse = await client.GetAsync(
                    $"api/business/{BusinessId}/bookings?date={dateStr}&skip=0&take=500");

                if (!bookingsResponse.IsSuccessStatusCode)
                {
                    Message = $"Failed to load today's bookings: {bookingsResponse.StatusCode}";
                    Log.Warn("Bookings API returned non-success", new
                    {
                        BusinessId,
                        bookingsResponse.StatusCode
                    });
                    return;
                }

                var listDto = await bookingsResponse.Content
                    .ReadFromJsonAsync<BookingListDto>();

                if (listDto == null ||
                    listDto.Status != BaseResponseStatus.Success ||
                    listDto.Bookings == null)
                {
                    Message = $"Error loading bookings. Status: {listDto?.Status}";
                    Log.Warn("Bookings payload invalid or empty", new
                    {
                        BusinessId,
                        listDto?.Status
                    });
                    return;
                }

                var list = listDto.Bookings.ToList();

                TodayBookings = list.Count;
                PendingBookings = list.Count(b =>
                    string.Equals(b.Status, "Pending", StringComparison.OrdinalIgnoreCase));
                CanceledToday = list.Count(b =>
                    string.Equals(b.Status, "Cancelled", StringComparison.OrdinalIgnoreCase));

                Log.Info("Dashboard stats calculated", new
                {
                    BusinessId,
                    TodayBookings,
                    PendingBookings,
                    CanceledToday
                });
            }
            catch (Exception ex)
            {
                Message = "Error while loading dashboard data. Please check API / connection.";
                Log.Error("Unexpected error while loading dashboard", ex, new
                {
                    BusinessId,
                    dateStr
                });
            }
        }
    }
}
