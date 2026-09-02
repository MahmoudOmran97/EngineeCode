using EngineeCode.Models;
using EngineeCode.Services;
using Microsoft.AspNetCore.Mvc;

namespace EngineeCode.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BannersController : ControllerBase
    {
        private readonly IBannerService _bannerService;
        private readonly IWebHostEnvironment _env;

        public BannersController(IBannerService bannerService, IWebHostEnvironment env)
        {
            _bannerService = bannerService;
            _env = env;
        }

        // ============================================================
        //  GET /api/banners  ← الموقع العام (الهوم بيج) بيستخدمه
        //  بيرجع البنرات الشغالة بس، مرتبة، ومراعية تواريخ الجدولة
        // ============================================================
        [HttpGet]
        public async Task<IActionResult> GetActive()
        {
            var banners = await _bannerService.GetActiveAsync();
            return Ok(banners.Select(ToResponse));
        }

        // ============================================================
        //  GET /api/banners/all  ← للإدارة، كل البنرات مهما كانت حالتها
        // ============================================================
        [HttpGet("all")]
        public async Task<IActionResult> GetAll()
        {
            var banners = await _bannerService.GetAllAsync();
            return Ok(banners.Select(ToResponse));
        }

        // GET /api/banners/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var banner = await _bannerService.GetByIdAsync(id);
            if (banner == null) return NotFound(new { success = false, message = "البنر غير موجود" });
            return Ok(ToResponse(banner));
        }

        // ============================================================
        //  POST /api/banners  ← إضافة بنر جديد (إدارة)
        // ============================================================
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] BannerRequest req)
        {
            if (string.IsNullOrWhiteSpace(req.ImagePath))
                return BadRequest(new { success = false, message = "لازم ترفع صورة للبنر" });

            var banner = FromRequest(req, new Banner());
            var created = await _bannerService.CreateAsync(banner);

            return CreatedAtAction(nameof(GetById), new { id = created.Id },
                new { success = true, message = "تم إضافة البنر بنجاح", banner = ToResponse(created) });
        }

        // PUT /api/banners/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] BannerRequest req)
        {
            var updated = await _bannerService.UpdateAsync(id, FromRequest(req, new Banner()));
            if (updated == null) return NotFound(new { success = false, message = "البنر غير موجود" });

            return Ok(new { success = true, message = "تم تعديل البنر", banner = ToResponse(updated) });
        }

        // DELETE /api/banners/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _bannerService.DeleteAsync(id);
            if (!result) return NotFound(new { success = false, message = "البنر غير موجود" });
            return Ok(new { success = true, message = "تم حذف البنر" });
        }

        // PATCH /api/banners/5/toggle-active
        [HttpPatch("{id}/toggle-active")]
        public async Task<IActionResult> ToggleActive(int id)
        {
            var result = await _bannerService.ToggleActiveAsync(id);
            if (!result) return NotFound(new { success = false, message = "البنر غير موجود" });
            return Ok(new { success = true, message = "تم تحديث حالة البنر" });
        }

        // PUT /api/banners/reorder
        [HttpPut("reorder")]
        public async Task<IActionResult> Reorder([FromBody] ReorderBannersRequest req)
        {
            await _bannerService.ReorderAsync(req.BannerIds);
            return Ok(new { success = true, message = "تم تحديث ترتيب البنرات" });
        }

        // ============================================================
        //  POST /api/banners/upload  ← رفع صورة البنر
        //  بيرجع الاسم بتاع الملف عشان يتحط فى ImagePath
        // ============================================================
        [HttpPost("upload")]
        [RequestSizeLimit(10 * 1024 * 1024)] // 10MB
        public async Task<IActionResult> UploadImage(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest(new { success = false, message = "لازم تختار صورة" });

            var allowedExt = new[] { ".jpg", ".jpeg", ".png", ".webp" };
            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!allowedExt.Contains(ext))
                return BadRequest(new { success = false, message = "الصورة لازم تكون jpg أو png أو webp" });

            var folder = Path.Combine(_env.WebRootPath, "images", "banners");
            Directory.CreateDirectory(folder);

            var fileName = $"banner-{Guid.NewGuid():N}{ext}";
            var fullPath = Path.Combine(folder, fileName);

            using (var stream = new FileStream(fullPath, FileMode.Create))
                await file.CopyToAsync(stream);

            // ده اللي بيتحط فى ImagePath — نفس كونفنشن صور المنتجات
            var imagePath = $"banners/{fileName}";
            return Ok(new { success = true, imagePath });
        }

        // ============================================================
        private static BannerResponse ToResponse(Banner b) => new()
        {
            Id = b.Id,
            Title = b.Title,
            Description = b.Description,
            BadgeText = b.BadgeText,
            CtaText = b.CtaText,
            ImagePath = b.ImagePath,
            LinkType = b.LinkType.ToString(),
            TargetId = b.TargetId,
            TargetSlug = b.TargetSlug,
            ExternalUrl = b.ExternalUrl,
            SortOrder = b.SortOrder,
            IsActive = b.IsActive,
            StartDate = b.StartDate,
            EndDate = b.EndDate
        };

        private static Banner FromRequest(BannerRequest req, Banner banner)
        {
            banner.Title = req.Title ?? string.Empty;
            banner.Description = req.Description ?? string.Empty;
            banner.BadgeText = req.BadgeText ?? string.Empty;
            banner.CtaText = string.IsNullOrWhiteSpace(req.CtaText) ? "تسوق الآن ←" : req.CtaText;
            banner.ImagePath = req.ImagePath ?? string.Empty;
            banner.LinkType = Enum.TryParse<BannerLinkType>(req.LinkType, true, out var lt) ? lt : BannerLinkType.None;
            banner.TargetId = req.TargetId;
            banner.TargetSlug = req.TargetSlug;
            banner.ExternalUrl = req.ExternalUrl;
            banner.SortOrder = req.SortOrder;
            banner.IsActive = req.IsActive;
            banner.StartDate = req.StartDate;
            banner.EndDate = req.EndDate;
            return banner;
        }
    }

    // ===== Request/Response Models =====
    public class BannerRequest
    {
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string? BadgeText { get; set; }
        public string? CtaText { get; set; }
        public string? ImagePath { get; set; }
        public string? LinkType { get; set; } // "None" | "Product" | "Category" | "Page" | "ExternalUrl"
        public int? TargetId { get; set; }
        public string? TargetSlug { get; set; }
        public string? ExternalUrl { get; set; }
        public int SortOrder { get; set; } = 0;
        public bool IsActive { get; set; } = true;
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }

    public class BannerResponse
    {
        public int Id { get; set; }
        public string Title { get; set; } = "";
        public string Description { get; set; } = "";
        public string BadgeText { get; set; } = "";
        public string CtaText { get; set; } = "";
        public string ImagePath { get; set; } = "";
        public string LinkType { get; set; } = "None";
        public int? TargetId { get; set; }
        public string? TargetSlug { get; set; }
        public string? ExternalUrl { get; set; }
        public int SortOrder { get; set; }
        public bool IsActive { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }

    public class ReorderBannersRequest
    {
        public List<int> BannerIds { get; set; } = new();
    }
}
