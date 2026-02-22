using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using EngineeCode.Services;

namespace EngineeCode.Pages
{
    public class ContactModel : PageModel
    {
        private readonly IContactService _contactService;
        private readonly ILogger<ContactModel> _logger;

        public ContactModel(IContactService contactService, ILogger<ContactModel> logger)
        {
            _contactService = contactService;
            _logger = logger;
        }

        [BindProperty]
        public ContactInputModel Input { get; set; } = new();

        public bool MessageSent { get; set; } = false;

        public void OnGet() { }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            try
            {
                await _contactService.SaveMessageAsync(new Models.ContactMessage
                {
                    Name = Input.Name,
                    Phone = Input.Phone,
                    Subject = Input.Subject,
                    Message = Input.Message,
                    SentAt = DateTime.Now
                });

                MessageSent = true;
                _logger.LogInformation("????? ????? ??: {Name} - {Phone}", Input.Name, Input.Phone);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "??? ?? ??? ???????");
                ModelState.AddModelError("", "??? ???? ???? ??? ?????.");
            }

            return Page();
        }
    }

    public class ContactInputModel
    {
        [Required(ErrorMessage = "????? ?????")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "????? ??? 2 ? 100 ???")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "??? ???????? ?????")]
        [Phone(ErrorMessage = "??? ???????? ??? ????")]
        public string Phone { get; set; } = string.Empty;

        [Required(ErrorMessage = "???? ????? ???????")]
        public string Subject { get; set; } = "product";

        [Required(ErrorMessage = "???? ??????")]
        [StringLength(2000, MinimumLength = 10, ErrorMessage = "??????? ??? 10 ? 2000 ???")]
        public string Message { get; set; } = string.Empty;
    }
}
