using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using EngineeCode.Services;

namespace EngineeCode.Pages
{
    public class LoginModel : PageModel
    {
        public IActionResult OnGet()
        {
            // لو العميل مسجّل دخوله، روّحه للـ Profile
            if (HttpContext.Session.GetString("CustomerEmail") != null)
                return RedirectToPage("/Profile");
            return Page();
        }
    }
}