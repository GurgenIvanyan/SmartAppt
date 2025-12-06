using System.Net.Http.Json;
using Business.SmartAppt.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebApplication1.Pages.Business
{
    public class ManageModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;

        [BindProperty]
        public int SearchId { get; set; }

        [BindProperty]
        public BusinessDto Business { get; set; } = new BusinessDto();

        public string? Message { get; set; }

        public ManageModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task OnGetAsync(int? id)
        {
            if (id.HasValue)
            {
                SearchId = id.Value;
                await LoadBusinessAsync(id.Value);
            }
        }

        public async Task<IActionResult> OnPostLoadAsync()
        {
            if (SearchId <= 0)
            {
                Message = "Please enter a valid Business Id.";
                return Page();
            }

            await LoadBusinessAsync(SearchId);
            return Page();
        }

        public async Task<IActionResult> OnPostSaveAsync()
        {
            if (Business == null || Business.BusinessId <= 0)
            {
                Message = "No business loaded.";
                return Page();
            }

            if (string.IsNullOrWhiteSpace(Business.Name))
            {
                ModelState.AddModelError("Business.Name", "Name is required.");
                return Page();
            }

            var client = _httpClientFactory.CreateClient("SmartApptApi");

            var response = await client.PutAsJsonAsync(
                $"api/business/{Business.BusinessId}",
                Business);

            if (!response.IsSuccessStatusCode)
            {
                Message = $"Update failed. Status code: {response.StatusCode}";
                return Page();
            }

            var res = await response.Content.ReadFromJsonAsync<BaseResponse>();
            Message = $"Update completed. Status: {res?.Status}";
            return Page();
        }

        public async Task<IActionResult> OnPostDeleteAsync()
        {
            if (Business == null || Business.BusinessId <= 0)
            {
                Message = "No business loaded to delete.";
                return Page();
            }

            var client = _httpClientFactory.CreateClient("SmartApptApi");

            var response = await client.DeleteAsync(
                $"api/business/{Business.BusinessId}");

            if (!response.IsSuccessStatusCode)
            {
                Message = $"Delete failed. Status code: {response.StatusCode}";
                return Page();
            }

            var res = await response.Content.ReadFromJsonAsync<BaseResponse>();
            Message = $"Delete completed. Status: {res?.Status}";

            Business = new BusinessDto();
            SearchId = 0;

            return Page();
        }

        private async Task LoadBusinessAsync(int id)
        {
            var client = _httpClientFactory.CreateClient("SmartApptApi");

            var response = await client.GetAsync($"api/business/{id}");

            if (!response.IsSuccessStatusCode)
            {
                Message = $"Failed to load business. Status code: {response.StatusCode}";
                Business = new BusinessDto();
                return;
            }

            var dto = await response.Content.ReadFromJsonAsync<BusinessDto>();

            if (dto == null || dto.Status != BaseResponseStatus.Success)
            {
                Message = $"Business not found or invalid. Status: {dto?.Status}";
                Business = new BusinessDto();
                return;
            }

            Business = dto;
            SearchId = dto.BusinessId;
            Message = "Business loaded successfully.";
        }
    }
}
