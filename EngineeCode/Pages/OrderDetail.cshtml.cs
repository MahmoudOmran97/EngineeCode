using EngineeCode.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace EngineeCode.Pages
{
    public class OrderDetailModel : PageModel
    {
        private readonly ApiService _api;
        public OrderDetailModel(ApiService api) => _api = api;

        [BindProperty(SupportsGet = true)]
        public int Id { get; set; }

        public OrderDetailDto? Order { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var email = HttpContext.Session.GetString("CustomerEmail");
            if (string.IsNullOrEmpty(email))
                return RedirectToPage("/Login");

            var customerIdStr = HttpContext.Session.GetString("CustomerId");
            if (!int.TryParse(customerIdStr, out int customerId))
            {
                var customer = await _api.GetCustomerByEmailAsync(email);
                if (customer == null) return RedirectToPage("/Login");
                customerId = customer.Id;
                HttpContext.Session.SetString("CustomerId", customerId.ToString());
            }

            // ??? ?? ??????? ????? ?? ??? id ???????
            var orders = await _api.GetCustomerOrdersAsync(customerId);
            var order = orders.FirstOrDefault(o => o.Id == Id);

            if (order == null) return Page(); // ????? "??? ?????"

            Order = new OrderDetailDto
            {
                Id = order.Id,
                OrderNumber = order.OrderNumber,
                Status = order.Status,
                StatusInt = order.StatusInt,
                Total = order.Total,
                CreatedAt = order.CreatedAt,
                
                DeliveryCity = order.DeliveryCity,
                DeliveryAddress = order.DeliveryAddress,
                Notes = order.Notes,
                PaymentMethod = order.PaymentMethod,
                Items = order.Items.Select(i => new OrderItemDto
                {
                    ProductName = i.ProductName,
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice,
                    ProductId = i.ProductId,
                    ImagePath = i.ImagePath  // ده هييجي كـ mouse-gm16.png
                }).ToList()
            };

            return Page();
        }
    }

    public class OrderDetailDto
    {
        public int Id { get; set; }
        public string OrderNumber { get; set; } = "";
        public string Status { get; set; } = "";
        public int? StatusInt { get; set; }
        public decimal Total { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<EngineeCode.Services.OrderItemDto> Items { get; set; } = new();
        public string? DeliveryCity { get; set; }
        public string? DeliveryAddress { get; set; }
        public string? Notes { get; set; }
        public string? PaymentMethod { get; set; }


        public string PaymentMethodText => PaymentMethod switch
        {
            "1" or "CashOnDelivery" => "????? ??? ????????",
            "2" or "VodafoneCash" => "??????? ???",
            "3" or "InstaPay" => "????????",
            "4" or "Fawry" => "????",
            _ => PaymentMethod ?? "—"
        };
    }
}