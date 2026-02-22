namespace EngineeCode.Models
{
    public class Product
    {
        public int Id { get; set; }

        /// <summary>
        /// فئة المنتج: mouse | keyboard | headphone
        /// </summary>
        public string Category { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// الاسم الإنجليزي / الموديل
        /// </summary>
        public string SubName { get; set; } = string.Empty;

        public decimal Price { get; set; }

        /// <summary>
        /// السعر القديم قبل الخصم (null لو مفيش خصم)
        /// </summary>
        public decimal? OldPrice { get; set; }

        /// <summary>
        /// اسم ملف الصورة الرئيسية — مثال: product-1.png
        /// </summary>
        public string ImagePath { get; set; } = string.Empty;

        /// <summary>
        /// اسم الـ Badge: Mouse / Gaming / Wireless / HP / Keyboard
        /// </summary>
        public string Badge { get; set; } = string.Empty;

        public double Rating { get; set; }

        public int ReviewsCount { get; set; }

        public int SalesCount { get; set; }

        public bool IsFeatured { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Computed: نسبة الخصم
        public int? DiscountPercent =>
            OldPrice.HasValue && OldPrice > 0
                ? (int)Math.Round((1 - (double)Price / (double)OldPrice.Value) * 100)
                : null;

        // Navigation — الصور المتعددة
        public ICollection<ProductImage> Images { get; set; } = new List<ProductImage>();
    }
}