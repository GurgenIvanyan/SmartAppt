using System.Net.Http.Json;
using Business.SmartAppt.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebApplication1.Pages.Business
{
    public class CreateModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;

        [BindProperty]
        public BusinessDto Business { get; set; } = new BusinessDto();

        public BaseResponseStatus? ResultStatus { get; set; }
        public int? CreatedBusinessId { get; set; }

        public CreateModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (string.IsNullOrWhiteSpace(Business.Name))
            {
                ModelState.AddModelError("Business.Name", "Name is required");
                return Page();
            }

            var client = _httpClientFactory.CreateClient("SmartApptApi");

            var response = await client.PostAsJsonAsync("api/business", Business);

            if (!response.IsSuccessStatusCode)
            {
                ResultStatus = BaseResponseStatus.UnknownError;
                return Page();
            }

            var created = await response.Content.ReadFromJsonAsync<BusinessDto>();

            if (created != null)
            {
                ResultStatus = created.Status;
                CreatedBusinessId = created.BusinessId;
            }

            return Page();
        }
    }
}
