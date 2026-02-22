namespace EngineeCode.Models
{
    public class ProductImage
    {
        public int Id { get; set; }
        public int ProductId { get; set; }

        /// <summary>
        /// مسار أو اسم ملف الصورة — مثال: product-1-side.png
        /// </summary>
        public string ImagePath { get; set; } = string.Empty;

        /// <summary>
        /// هل دي الصورة الرئيسية؟
        /// </summary>
        public bool IsMain { get; set; } = false;

        /// <summary>
        /// ترتيب العرض (0 = الأول)
        /// </summary>
        public int SortOrder { get; set; } = 0;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Navigation
        public Product Product { get; set; } = null!;
    }
}