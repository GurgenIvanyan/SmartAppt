using System.Net.Http.Json;
using Business.SmartAppt.Models;
using Data.SmartAppt.SQL.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebApplication1.Pages.Bookings
{
    public class IndexModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;

        [BindProperty]
        public int BusinessId { get; set; }

        [BindProperty]
        public int? ServiceId { get; set; }

        [BindProperty]
        public string? Status { get; set; }

        [BindProperty]
        public DateTime? Date { get; set; }

        [BindProperty]
        public int Skip { get; set; } = 0;

        [BindProperty]
        public int Take { get; set; } = 50;

        public List<BookingEntity> Bookings { get; set; } = new();

        public string? Message { get; set; }

        public IndexModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostLoadAsync()
        {
            if (BusinessId <= 0)
            {
                Message = "BusinessId is required.";
                Bookings = new List<BookingEntity>();
                return Page();
            }

            if (Skip < 0) Skip = 0;
            if (Take <= 0 || Take > 1000) Take = 50;

            await LoadBookingsAsync();

            return Page();
        }

        public async Task<IActionResult> OnPostDecideAsync(int bookingId, bool confirm)
        {
            if (BusinessId <= 0 || bookingId <= 0)
            {
                Message = "Invalid BusinessId or BookingId.";
                return Page();
            }

            var client = _httpClientFactory.CreateClient("SmartApptApi");

            var response = await client.PostAsync(
                $"api/business/{BusinessId}/bookings/{bookingId}/decide?confirm={confirm.ToString().ToLower()}",
                null);

            if (!response.IsSuccessStatusCode)
            {
                Message = $"Decide failed. Status: {response.StatusCode}";
            }
            else
            {
                var res = await response.Content.ReadFromJsonAsync<BaseResponse>();
                Message = $"Decide completed. Status: {res?.Status}";
            }

            await LoadBookingsAsync();

            return Page();
        }

        private async Task LoadBookingsAsync()
        {
            var client = _httpClientFactory.CreateClient("SmartApptApi");

            string url = $"api/business/{BusinessId}/bookings?skip={Skip}&take={Take}";

            if (ServiceId.HasValue && ServiceId.Value > 0)
            {
                url += $"&serviceId={ServiceId.Value}";
            }

            if (!string.IsNullOrWhiteSpace(Status))
            {
                url += $"&status={Uri.EscapeDataString(Status)}";
            }

            if (Date.HasValue)
            {
                var dateOnly = DateOnly.FromDateTime(Date.Value);
                url += $"&date={dateOnly:yyyy-MM-dd}";
            }

            var response = await client.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                Message = $"Failed to load bookings. Status: {response.StatusCode}";
                Bookings = new List<BookingEntity>();
                return;
            }

            var listDto = await response.Content.ReadFromJsonAsync<BookingListDto>();

            if (listDto == null || listDto.Status != BaseResponseStatus.Success || listDto.Bookings == null)
            {
                Message = $"No bookings or error. Status: {listDto?.Status}";
                Bookings = new List<BookingEntity>();
                return;
            }

            Bookings = listDto.Bookings.ToList();
            Message = $"Loaded {Bookings.Count} bookings.";
        }
    }
}
