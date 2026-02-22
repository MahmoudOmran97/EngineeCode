using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace EngineeCode.Pages
{
    public class RegisterModel : PageModel
    {
        public IActionResult OnGet()
        {
            // لو العميل مسجّل دخوله بالفعل، روّحه للـ Profile
            if (HttpContext.Session.GetString("CustomerEmail") != null)
                return RedirectToPage("/Profile");
            return Page();
        }
    }
}