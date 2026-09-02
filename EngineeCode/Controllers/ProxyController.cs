using EngineeCode.Services;
using Microsoft.AspNetCore.Mvc;

namespace EngineeCode.Controllers
{
    [Route("api/proxy")]
    [ApiController]
    public class ProxyController : ControllerBase
    {
        private readonly ApiService _api;
        public ProxyController(ApiService api) => _api = api;

        // ============================================================
        //  POST api/proxy/send-otp-register  ← REGISTER
        //  يرفض الإيميل المسجل مسبقاً
        // ============================================================
        [HttpPost("send-otp-register")]
        public async Task<IActionResult> SendOtpRegister([FromBody] EmailRequest req)
        {
            if (string.IsNullOrWhiteSpace(req.Email))
                return Ok(new ApiResult { Success = false, Message = "الإيميل مطلوب" });

            var result = await _api.SendOtpAsync(req.Email);
            return Ok(result);
        }

        // ============================================================
        //  POST api/proxy/send-login-otp  ← LOGIN
        //  يقبل فقط الإيميلات المسجلة
        // ============================================================
        [HttpPost("send-login-otp")]
        public async Task<IActionResult> SendLoginOtp([FromBody] EmailRequest req)
        {
            if (string.IsNullOrWhiteSpace(req.Email))
                return Ok(new ApiResult { Success = false, Message = "الإيميل مطلوب" });

            var result = await _api.SendLoginOtpAsync(req.Email);
            return Ok(result);
        }

        // ============================================================
        //  POST api/proxy/verify-otp  ← LOGIN
        //  بيسيت CustomerEmail في الـ Session
        //  ✅ فيكس: بيستخدم verify-login-otp، وبيرفض الدخول لو مفيش
        //     Customer فعلي مسجل بنفس الإيميل — عشان محدش يدخل بحساب
        //     غير مكتمل التسجيل حتى لو الكود صح.
        // ============================================================
        [HttpPost("verify-otp")]
        public async Task<IActionResult> VerifyOtp([FromBody] VerifyRequest req)
        {
            if (string.IsNullOrWhiteSpace(req.Email) || string.IsNullOrWhiteSpace(req.Code))
                return Ok(new ApiResult { Success = false, Message = "الإيميل والكود مطلوبين" });

            var result = await _api.VerifyLoginOtpAsync(req.Email, req.Code);
            if (!result.Success)
                return Ok(result);

            // ✅ لازم يكون فيه Customer حقيقي قبل ما نفتح Session
            var customer = await _api.GetCustomerByEmailAsync(req.Email);
            if (customer == null)
            {
                return Ok(new ApiResult
                {
                    Success = false,
                    Message = "الحساب غير مكتمل التسجيل. من فضلك أنشئ حساب جديد أولاً."
                });
            }

            HttpContext.Session.SetString("CustomerEmail", req.Email);
            HttpContext.Session.SetString("CustomerName", customer.Name);
            HttpContext.Session.SetString("CustomerId", customer.Id.ToString());

            return Ok(result);
        }

        // ============================================================
        //  POST api/proxy/register  ← REGISTER
        //  بيبعت OtpCode مع بيانات التسجيل في request واحد للـ API
        // ============================================================
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest req)
        {
            if (string.IsNullOrWhiteSpace(req.OtpCode))
                return Ok(new ApiResult { Success = false, Message = "كود التحقق مطلوب" });

            try
            {
                var result = await _api.RegisterCustomerAsync(new RegisterCustomerRequest
                {
                    Name = req.Name,
                    Phone = req.Phone,
                    Email = req.Email,
                    City = req.City ?? "",
                    Address = req.Address ?? "",
                    OtpCode = req.OtpCode
                });
                if (result.Success)
                {
                    HttpContext.Session.SetString("CustomerEmail", req.Email);
                    HttpContext.Session.SetString("CustomerName", req.Name);

                    // ✅ جيب الـ Customer مباشرة بعد التسجيل واحفظ الـ ID
                    var customer = await _api.GetCustomerByEmailAsync(req.Email);
                    if (customer != null)
                        HttpContext.Session.SetString("CustomerId", customer.Id.ToString());
                }
                return Ok(result);
            }
            catch (Exception ex)
            {
                return Ok(new ApiResult { Success = false, Message = ex.Message });
            }
        }

        // ============================================================
        //  POST api/proxy/logout
        // ============================================================
        [HttpPost("logout")]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return Ok(new { success = true });
        }

        // ============================================================
        //  POST api/proxy/create-order
        [HttpPost("create-order")]
        public async Task<IActionResult> CreateOrder([FromBody] CheckoutRequest req)
        {
            var email = HttpContext.Session.GetString("CustomerEmail");
            if (string.IsNullOrEmpty(email))
                return Ok(new ApiResult { Success = false, Message = "يجب تسجيل الدخول أولاً" });

            // ✅ جرب الـ ID من الـ Session أولاً — أسرع وأضمن
            var customerIdStr = HttpContext.Session.GetString("CustomerId");
            int customerId = 0;

            if (!string.IsNullOrEmpty(customerIdStr) && int.TryParse(customerIdStr, out customerId))
            {
                // عندنا الـ ID جاهز من الـ Session ✅
            }
            else
            {
                // Fallback — اجيبه من الـ API
                var customer = await _api.GetCustomerByEmailAsync(email);
                if (customer == null)
                    return Ok(new ApiResult { Success = false, Message = "لم يتم العثور على العميل، سجّل الدخول مرة أخرى" });

                customerId = customer.Id;
                HttpContext.Session.SetString("CustomerId", customerId.ToString());
            }

            var result = await _api.CreateOrderAsync(new CreateOrderRequest
            {
                CustomerId = customerId,
                PaymentMethod = req.PaymentMethod,
                DeliveryCity = req.DeliveryCity,
                DeliveryAddress = req.DeliveryAddress,
                Notes = req.Notes,
                Items = req.Items.Select(i => new OrderItemRequest
                {
                    ProductId = i.ProductId,
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice
                }).ToList()
            });
            return Ok(result);
        }
    }

    public class EmailRequest { public string Email { get; set; } = ""; }
    public class VerifyRequest { public string Email { get; set; } = ""; public string Code { get; set; } = ""; }

    public class RegisterRequest
    {
        public string Name { get; set; } = "";
        public string Phone { get; set; } = "";
        public string Email { get; set; } = "";
        public string? City { get; set; }
        public string? Address { get; set; }
        public string OtpCode { get; set; } = "";   // ✅ مطلوب من الـ External API
    }
    public class CustomerResponse
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string Phone { get; set; } = "";
        public string Email { get; set; } = "";
        public string Address { get; set; } = "";
        public string City { get; set; } = "";
        public int TotalOrders { get; set; }
        public decimal TotalSpent { get; set; }
    }
    public class CheckoutRequest
    {
        public int PaymentMethod { get; set; }
        public string DeliveryCity { get; set; } = "";
        public string DeliveryAddress { get; set; } = "";
        public string Notes { get; set; } = "";
        public List<CheckoutItem> Items { get; set; } = new();
    }

    public class CheckoutItem
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
    }
}