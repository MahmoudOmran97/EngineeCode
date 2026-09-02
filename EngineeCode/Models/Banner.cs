namespace EngineeCode.Models
{
    // نوع الوجهة اللي البنر بيوجه لها لما يتضغط
    public enum BannerLinkType
    {
        None = 0,       // بنر عرض بس من غير أي تفاعل
        Product = 1,    // يفتح صفحة منتج معين
        Category = 2,   // يفتح صفحة المنتجات مفلترة بكاتيجوري معينة
        Page = 3,       // يفتح صفحة داخلية معينة فى الموقع (زي /Services أو /Cashier)
        ExternalUrl = 4 // يفتح رابط خارجي فى تاب جديد
    }

    public class Banner
    {
        public int Id { get; set; }

        public string Title { get; set; } = string.Empty;       // العنوان الرئيسي فى البنر
        public string Description { get; set; } = string.Empty; // السطر التاني تحت العنوان
        public string BadgeText { get; set; } = string.Empty;   // نص الشارة الصغيرة (مثلا "عرض محدود")
        public string CtaText { get; set; } = "تسوق الآن ←";     // نص الزرار/الرابط

        public string ImagePath { get; set; } = string.Empty;   // اسم الصورة فقط (زي ImagePath بتاع المنتج)

        // ===== الوجهة اللي البنر بيوجه لها =====
        public BannerLinkType LinkType { get; set; } = BannerLinkType.None;
        public int? TargetId { get; set; }        // ProductId أو CategoryId حسب النوع
        public string? TargetSlug { get; set; }    // اسم الكاتيجوري ("mouse") أو اسم الصفحة ("/Services")
        public string? ExternalUrl { get; set; }   // الرابط الخارجي لو LinkType = ExternalUrl

        public int SortOrder { get; set; } = 0;    // ترتيب ظهور البنر فى الكاروسيل
        public bool IsActive { get; set; } = true;

        // جدولة اختيارية — لو فاضيين البنر يفضل شغال طول الوقت
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
