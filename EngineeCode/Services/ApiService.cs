using System.Net.Http.Json;
using System.Text.Json;

namespace EngineeCode.Services
{
    public class ApiService
    {
        private readonly HttpClient _http;
        private const string BaseUrl = "https://enginecodeapi.runasp.net/api";

        private static readonly JsonSerializerOptions _jsonOpts = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public ApiService(HttpClient http) => _http = http;

        // ===== HELPER =====
        private async Task<ApiResult> ReadApiResult(HttpResponseMessage res)
        {
            var content = await res.Content.ReadAsStringAsync();

            if (res.IsSuccessStatusCode && string.IsNullOrWhiteSpace(content))
                return new ApiResult { Success = true, Message = "تمت العملية بنجاح" };

            if (string.IsNullOrWhiteSpace(content))
                return new ApiResult { Success = false, Message = "لا يوجد رد من السيرفر" };

            try
            {
                // جرب ApiResponse wrapper أول { success, data, message }
                var wrapper = JsonSerializer.Deserialize<ApiWrapper<object>>(content, _jsonOpts);
                if (wrapper != null)
                {
                    // لو فيه orderNumber في الـ data
                    string? orderNumber = null;
                    if (wrapper.Data != null)
                    {
                        try
                        {
                            var dataJson = JsonSerializer.Serialize(wrapper.Data);
                            var dataDict = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(dataJson, _jsonOpts);
                            if (dataDict != null && dataDict.TryGetValue("orderNumber", out var on))
                                orderNumber = on.GetString();
                        }
                        catch { }
                    }
                    return new ApiResult
                    {
                        Success = wrapper.Success,
                        Message = wrapper.Message,
                        OrderNumber = orderNumber
                    };
                }
            }
            catch { }

            try
            {
                return JsonSerializer.Deserialize<ApiResult>(content, _jsonOpts)
                       ?? new ApiResult { Success = false, Message = "خطأ في قراءة الرد" };
            }
            catch
            {
                if (res.IsSuccessStatusCode)
                    return new ApiResult { Success = true, Message = "تمت العملية بنجاح" };

                return new ApiResult { Success = false, Message = $"رد غير متوقع ({(int)res.StatusCode})" };
            }
        }

        // ======== AUTH ========
        public async Task<ApiResult> SendOtpAsync(string email)
        {
            try
            {
                var res = await _http.PostAsJsonAsync($"{BaseUrl}/Auth/send-otp", new { email });
                return await ReadApiResult(res);
            }
            catch (Exception ex) { return new ApiResult { Success = false, Message = $"فشل الاتصال: {ex.Message}" }; }
        }

        public async Task<ApiResult> SendLoginOtpAsync(string email)
        {
            try
            {
                var res = await _http.PostAsJsonAsync($"{BaseUrl}/Auth/login-otp", new { email });
                if (res.IsSuccessStatusCode)
                {
                    var result = await ReadApiResult(res);
                    if (result.Success) return result;
                }

                res = await _http.PostAsJsonAsync($"{BaseUrl}/Auth/send-login-otp", new { email });
                if (res.IsSuccessStatusCode)
                {
                    var result = await ReadApiResult(res);
                    if (result.Success) return result;
                }

                res = await _http.PostAsJsonAsync($"{BaseUrl}/Auth/send-otp", new { email });
                return await ReadApiResult(res);
            }
            catch (Exception ex) { return new ApiResult { Success = false, Message = $"فشل الاتصال: {ex.Message}" }; }
        }

        public async Task<ApiResult> VerifyOtpAsync(string email, string code)
        {
            try
            {
                var res = await _http.PostAsJsonAsync($"{BaseUrl}/Auth/verify-otp", new { email, code });
                return await ReadApiResult(res);
            }
            catch (Exception ex) { return new ApiResult { Success = false, Message = $"فشل الاتصال: {ex.Message}" }; }
        }

        // ======== CUSTOMERS ========
        public async Task<CustomerDto?> GetCustomerByEmailAsync(string email)
        {
            try
            {
                var res = await _http.GetAsync($"{BaseUrl}/Customers/by-email/{Uri.EscapeDataString(email)}");
                if (!res.IsSuccessStatusCode) return null;
                var content = await res.Content.ReadAsStringAsync();
                if (string.IsNullOrWhiteSpace(content)) return null;

                // جرب wrapper { success, data: { id, name, ... } }
                try
                {
                    var wrapper = JsonSerializer.Deserialize<ApiWrapper<CustomerDto>>(content, _jsonOpts);
                    if (wrapper?.Data != null) return wrapper.Data;
                }
                catch { }

                return JsonSerializer.Deserialize<CustomerDto>(content, _jsonOpts);
            }
            catch { return null; }
        }

        public async Task<ApiResult> RegisterCustomerAsync(RegisterCustomerRequest request)
        {
            try
            {
                var res = await _http.PostAsJsonAsync($"{BaseUrl}/Customers", request);
                return await ReadApiResult(res);
            }
            catch (Exception ex) { return new ApiResult { Success = false, Message = ex.Message }; }
        }

        // ======== PRODUCTS ========
        public async Task<List<ProductDto>> GetProductsAsync(string? category = null)
        {
            try
            {
                var url = string.IsNullOrEmpty(category)
                    ? $"{BaseUrl}/Products"
                    : $"{BaseUrl}/Products?category={category}";

                var content = await _http.GetStringAsync(url);

                // جرب wrapper أول
                try
                {
                    var wrapper = JsonSerializer.Deserialize<ApiWrapper<List<ProductDto>>>(content, _jsonOpts);
                    if (wrapper?.Data != null) return wrapper.Data;
                }
                catch { }

                return JsonSerializer.Deserialize<List<ProductDto>>(content, _jsonOpts) ?? new();
            }
            catch { return new(); }
        }

        public async Task<ProductDto?> GetProductAsync(int id)
        {
            try
            {
                var content = await _http.GetStringAsync($"{BaseUrl}/Products/{id}");

                try
                {
                    var wrapper = JsonSerializer.Deserialize<ApiWrapper<ProductDto>>(content, _jsonOpts);
                    if (wrapper?.Data != null) return wrapper.Data;
                }
                catch { }

                return JsonSerializer.Deserialize<ProductDto>(content, _jsonOpts);
            }
            catch { return null; }
        }

        // ======== ORDERS ========
        // ✅ الـ endpoint الصح هو /api/Customers/{id}/orders مش /api/Orders/customer/{id}
        public async Task<List<OrderDto>> GetCustomerOrdersAsync(int customerId)
        {
            try
            {
                var content = await _http.GetStringAsync($"{BaseUrl}/Customers/{customerId}/orders");

                // جرب wrapper { success, data: [...] }
                try
                {
                    var wrapper = JsonSerializer.Deserialize<ApiWrapper<List<OrderDto>>>(content, _jsonOpts);
                    if (wrapper?.Data != null) return wrapper.Data;
                }
                catch { }

                // جرب paged wrapper { success, data: { items: [...] } }
                try
                {
                    var paged = JsonSerializer.Deserialize<ApiWrapper<PagedData<OrderDto>>>(content, _jsonOpts);
                    if (paged?.Data?.Items != null) return paged.Data.Items;
                }
                catch { }

                return JsonSerializer.Deserialize<List<OrderDto>>(content, _jsonOpts) ?? new();
            }
            catch { return new(); }
        }

        public async Task<ApiResult> CreateOrderAsync(CreateOrderRequest request)
        {
            try
            {
                var res = await _http.PostAsJsonAsync($"{BaseUrl}/Orders", request);
                return await ReadApiResult(res);
            }
            catch (Exception ex) { return new ApiResult { Success = false, Message = ex.Message }; }
        }
    }

    // ===== DTOs =====
    public class ApiWrapper<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; } = "";
        public T? Data { get; set; }
    }

    public class PagedData<T>
    {
        public List<T> Items { get; set; } = new();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
    }

    public class ApiResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = "";
        public object? Data { get; set; }
        public string? Token { get; set; }
        public string? OrderNumber { get; set; }
    }

    public class CustomerDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string Phone { get; set; } = "";
        public string Email { get; set; } = "";
        public string Address { get; set; } = "";
        public string City { get; set; } = "";
        public DateTime CreatedAt { get; set; }
    }

    public class RegisterCustomerRequest
    {
        public string Name { get; set; } = "";
        public string Phone { get; set; } = "";
        public string Email { get; set; } = "";
        public string City { get; set; } = "";
        public string Address { get; set; } = "";
        public string OtpCode { get; set; } = "";
    }

    public class ProductDto
    {
        public int Id { get; set; }
        public string Category { get; set; } = "";
        public string Name { get; set; } = "";
        public string SubName { get; set; } = "";
        public decimal Price { get; set; }
        public decimal? OldPrice { get; set; }
        public string ImagePath { get; set; } = "";
        public string Badge { get; set; } = "";
        public double Rating { get; set; }
        public int ReviewsCount { get; set; }
        public int Stock { get; set; }
        public bool IsFeatured { get; set; }
    }

    public class OrderDto
    {
        public int Id { get; set; }
        public string OrderNumber { get; set; } = "";

        // الـ API بيرجع Status كـ string
        public string Status { get; set; } = "";

        // ✅ حقول التوصيل والدفع
        public string? DeliveryCity { get; set; }
        public string? DeliveryAddress { get; set; }
        public string? Notes { get; set; }
        public string? PaymentMethod { get; set; }

        public string StatusText => Status switch
        {
            "Pending" => "قيد المراجعة",
            "Confirmed" => "تم التأكيد",
            "Shipped" => "تم الشحن",
            "Delivered" => "تم التسليم",
            "Cancelled" => "ملغي",
            _ => Status
        };

        public string StatusColor => Status switch
        {
            "Pending" => "warning",
            "Confirmed" => "info",
            "Shipped" => "primary",
            "Delivered" => "success",
            "Cancelled" => "danger",
            _ => "secondary"
        };

        public int? StatusInt { get; set; }

        public string FinalStatusText => string.IsNullOrEmpty(Status) && StatusInt.HasValue
            ? StatusInt switch { 0 => "قيد المراجعة", 1 => "تم التأكيد", 2 => "تم الشحن", 3 => "تم التسليم", 4 => "ملغي", _ => "غير معروف" }
            : StatusText;

        public string FinalStatusColor => string.IsNullOrEmpty(Status) && StatusInt.HasValue
            ? StatusInt switch { 0 => "warning", 1 => "info", 2 => "primary", 3 => "success", 4 => "danger", _ => "secondary" }
            : StatusColor;

        public decimal Total { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<OrderItemDto> Items { get; set; } = new();
    }

    public class OrderItemDto
    {
        public string ProductName { get; set; } = "";
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
    }

    public class CreateOrderRequest
    {
        public int CustomerId { get; set; }
        public int PaymentMethod { get; set; }
        public string DeliveryAddress { get; set; } = "";
        public string DeliveryCity { get; set; } = "";
        public string Notes { get; set; } = "";
        public List<OrderItemRequest> Items { get; set; } = new();
    }

    public class OrderItemRequest
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
    }
}