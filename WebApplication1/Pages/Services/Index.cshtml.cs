using System.Net.Http.Json;
using Business.SmartAppt.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebApplication1.Pages.Services
{
    public class IndexModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;

        [BindProperty]
        public int BusinessId { get; set; }

        [BindProperty]
        public int Skip { get; set; } = 0;

        [BindProperty]
        public int Take { get; set; } = 20;

        [BindProperty]
        public NewServiceInput NewService { get; set; } = new();

        public List<ServiceItem> Services { get; set; } = new();

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
                Message = "Please enter a valid Business Id.";
                Services = new List<ServiceItem>();
                return Page();
            }

            await LoadServicesAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostAddAsync()
        {
            if (BusinessId <= 0)
            {
                Message = "BusinessId is required.";
                return Page();
            }

            if (!ModelState.IsValid)
            {
                await LoadServicesAsync();
                return Page();
            }

            var client = _httpClientFactory.CreateClient("SmartApptApi");

            var dto = new ServiceDto
            {
                BusinessId = BusinessId,
                Name = NewService.Name,
                DurationMin = NewService.DurationMin,
                Price = NewService.Price,
                IsActive = true
            };

            var response = await client.PostAsJsonAsync(
                $"api/business/{BusinessId}/services",
                dto);

            if (!response.IsSuccessStatusCode)
            {
                Message = $"Add failed. Status: {response.StatusCode}";
                await LoadServicesAsync();
                return Page();
            }

            var resDto = await response.Content.ReadFromJsonAsync<ServiceDto>();
            Message = $"Add completed. Status: {resDto?.Status}";

            NewService = new NewServiceInput();

            await LoadServicesAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostDisableAsync(int serviceId)
        {
            if (BusinessId <= 0 || serviceId <= 0)
            {
                Message = "Invalid BusinessId or ServiceId.";
                return Page();
            }

            var client = _httpClientFactory.CreateClient("SmartApptApi");

            var response = await client.PostAsync(
                $"api/business/{BusinessId}/services/{serviceId}/deactivate",
                null);

            if (!response.IsSuccessStatusCode)
            {
                Message = $"Disable failed. Status: {response.StatusCode}";
                await LoadServicesAsync();
                return Page();
            }

            var res = await response.Content.ReadFromJsonAsync<BaseResponse>();
            Message = $"Disable completed. Status: {res?.Status}";

            await LoadServicesAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostDeleteAsync(int serviceId)
        {
            if (serviceId <= 0)
            {
                Message = "Invalid ServiceId.";
                return Page();
            }

            var client = _httpClientFactory.CreateClient("SmartApptApi");

            var response = await client.DeleteAsync(
                $"api/business/services/{serviceId}");

            if (!response.IsSuccessStatusCode)
            {
                Message = $"Delete failed. Status: {response.StatusCode}";
                await LoadServicesAsync();
                return Page();
            }

            var res = await response.Content.ReadFromJsonAsync<BaseResponse>();
            Message = $"Delete completed. Status: {res?.Status}";

            await LoadServicesAsync();
            return Page();
        }

        private async Task LoadServicesAsync()
        {
            var client = _httpClientFactory.CreateClient("SmartApptApi");

            var response = await client.GetAsync(
                $"api/business/{BusinessId}/services?skip={Skip}&take={Take}");

            if (!response.IsSuccessStatusCode)
            {
                Message = $"Failed to load services. Status: {response.StatusCode}";
                Services = new List<ServiceItem>();
                return;
            }

            var listDto = await response.Content.ReadFromJsonAsync<ServiceListDto>();

            if (listDto == null || listDto.Status != BaseResponseStatus.Success || listDto.Services == null)
            {
                Message = $"No services or error. Status: {listDto?.Status}";
                Services = new List<ServiceItem>();
                return;
            }

            Services = listDto.Services
                .Select(s => new ServiceItem
                {
                    ServiceId = s.ServiceId,
                    BusinessId = s.BusinessId,
                    Name = s.Name,
                    DurationMin = s.DurationMin,
                    Price = s.Price,
                    IsActive = s.IsActive
                })
                .ToList();

            Message = $"Loaded {Services.Count} services.";
        }

        public class NewServiceInput
        {
            public string Name { get; set; } = string.Empty;
            public int DurationMin { get; set; }
            public decimal Price { get; set; }
        }

        public class ServiceItem
        {
            public int ServiceId { get; set; }
            public int BusinessId { get; set; }
            public string Name { get; set; } = string.Empty;
            public int DurationMin { get; set; }
            public decimal Price { get; set; }
            public bool IsActive { get; set; }
        }
    }
}
