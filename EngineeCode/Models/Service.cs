namespace EngineeCode.Models
{
    public class Service
    {
        public int Id { get; set; }

        public string Icon { get; set; } = string.Empty;

        public string Title { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// النص اللي بيظهر في بطاقة السعر — مثال: "تبدأ من 60 جنيه"
        /// </summary>
        public string PriceLabel { get; set; } = string.Empty;

        public int SortOrder { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
