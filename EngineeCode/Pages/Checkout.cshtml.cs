using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace EngineeCode.Pages
{
    public class CheckoutModel : PageModel
    {
        // ✅ أسماء المحافظات فقط — قائمة المدن التابعة بتتحدد ديناميكيًا في الـ JS
        // من ملف egypt-locations.js (نفس المصدر المستخدم في صفحة التسجيل)
        public List<string> Governorates { get; } = new()
        {
            "القاهرة", "الجيزة", "الإسكندرية", "الدقهلية", "الشرقية", "القليوبية",
            "كفر الشيخ", "الغربية", "المنوفية", "البحيرة", "الإسماعيلية",
            "بورسعيد", "السويس", "الفيوم", "بني سويف", "المنيا", "أسيوط",
            "سوهاج", "قنا", "الأقصر", "أسوان", "مطروح", "شمال سيناء",
            "جنوب سيناء", "البحر الأحمر", "الوادي الجديد", "دمياط"
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