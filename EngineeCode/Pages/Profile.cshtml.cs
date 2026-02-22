using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using EngineeCode.Services;

namespace EngineeCode.Pages
{
    public class ProfileModel : PageModel
    {
        private readonly ApiService _api;

        public CustomerDto? Customer { get; set; }
        public List<OrderDto> Orders { get; set; } = new();
        public string? ErrorMessage { get; set; }

        public ProfileModel(ApiService api) => _api = api;

        public async Task<IActionResult> OnGetAsync()
        {
            var email = HttpContext.Session.GetString("CustomerEmail");

            // لو مفيش Session → روح Login
            if (string.IsNullOrEmpty(email))
                return RedirectToPage("/Login");

            try
            {
                Customer = await _api.GetCustomerByEmailAsync(email);
            }
            catch
            {
                // لو الـ API فشل، اعمل Customer من الـ Session
                Customer = new CustomerDto
                {
                    Name = HttpContext.Session.GetString("CustomerName") ?? email,
                    Email = email,
                    Phone = "",
                    City = "",
                    Address = ""
                };
            }

            // لو Customer null خلينا نعمله من الـ session بدل ما نرجع Login
            if (Customer == null)
            {
                Customer = new CustomerDto
                {
                    Name = HttpContext.Session.GetString("CustomerName") ?? email,
                    Email = email
                };
            }

            // جيب الطلبات لو الـ Id موجود
            if (Customer.Id > 0)
            {
                try
                {
                    Orders = await _api.GetCustomerOrdersAsync(Customer.Id);
                }
                catch
                {
                    Orders = new List<OrderDto>();
                }
            }

            return Page();
        }
    }
}