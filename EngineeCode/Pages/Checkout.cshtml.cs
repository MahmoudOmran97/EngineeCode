using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace EngineeCode.Pages
{
    public class CheckoutModel : PageModel
    {
        public List<string> Cities { get; } = new()
        {
            "القاهرة", "الجيزة", "الإسكندرية", "الدقهلية", "الشرقية", "القليوبية",
            "كفر الشيخ", "الغربية", "المنوفية", "البحيرة", "الإسماعيلية",
            "بورسعيد", "السويس", "الفيوم", "بني سويف", "المنيا", "أسيوط",
            "سوهاج", "قنا", "الأقصر", "أسوان", "مطروح", "شمال سيناء",
            "جنوب سيناء", "البحر الأحمر", "الوادي الجديد"
        };

        public IActionResult OnGet()
        {
            var email = HttpContext.Session.GetString("CustomerEmail");
            if (string.IsNullOrEmpty(email))
                return RedirectToPage("/Login");
            return Page();
        }
    }
}