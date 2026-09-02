using EngineeCode.Models;
using Microsoft.EntityFrameworkCore;

namespace EngineeCode.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Product> Products { get; set; }
        public DbSet<ProductImage> ProductImages { get; set; }   // ✅ جديد
        public DbSet<Service> Services { get; set; }
        public DbSet<ContactMessage> ContactMessages { get; set; }
        public DbSet<Banner> Banners { get; set; }                // ✅ جديد — البنرات الإعلانية

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ===== Products =====
            modelBuilder.Entity<Product>(e =>
            {
                e.ToTable("Products");
                e.HasKey(p => p.Id);
                e.Property(p => p.Name).IsRequired().HasMaxLength(200);
                e.Property(p => p.SubName).HasMaxLength(300);
                e.Property(p => p.Category).IsRequired().HasMaxLength(50);
                e.Property(p => p.Badge).HasMaxLength(50);
                e.Property(p => p.ImagePath).HasMaxLength(500);
                e.Property(p => p.Price).HasColumnType("decimal(10,2)");
                e.Property(p => p.OldPrice).HasColumnType("decimal(10,2)");
                e.Ignore(p => p.DiscountPercent);
                e.HasIndex(p => p.Category);
                e.HasIndex(p => p.IsFeatured);
            });

            // ===== ProductImages ===== ✅ جديد
            modelBuilder.Entity<ProductImage>(e =>
            {
                e.ToTable("ProductImages");
                e.HasKey(pi => pi.Id);
                e.Property(pi => pi.ImagePath).IsRequired().HasMaxLength(500);
                e.HasOne(pi => pi.Product)
                 .WithMany(p => p.Images)
                 .HasForeignKey(pi => pi.ProductId)
                 .OnDelete(DeleteBehavior.Cascade);
                e.HasIndex(pi => pi.ProductId);
            });

            // ===== Services =====
            modelBuilder.Entity<Service>(e =>
            {
                e.ToTable("Services");
                e.HasKey(s => s.Id);
                e.Property(s => s.Title).IsRequired().HasMaxLength(200);
                e.Property(s => s.Icon).HasMaxLength(20);
                e.Property(s => s.Description).HasMaxLength(500);
                e.Property(s => s.PriceLabel).HasMaxLength(100);
            });

            // ===== ContactMessages =====
            modelBuilder.Entity<ContactMessage>(e =>
            {
                e.ToTable("ContactMessages");
                e.HasKey(m => m.Id);
                e.Property(m => m.Name).IsRequired().HasMaxLength(100);
                e.Property(m => m.Phone).IsRequired().HasMaxLength(20);
                e.Property(m => m.Subject).HasMaxLength(50);
                e.Property(m => m.Message).IsRequired().HasMaxLength(2000);
            });

            // ===== Banners ===== ✅ جديد
            modelBuilder.Entity<Banner>(e =>
            {
                e.ToTable("Banners");
                e.HasKey(b => b.Id);
                e.Property(b => b.Title).HasMaxLength(200);
                e.Property(b => b.Description).HasMaxLength(500);
                e.Property(b => b.BadgeText).HasMaxLength(50);
                e.Property(b => b.CtaText).HasMaxLength(50);
                e.Property(b => b.ImagePath).IsRequired().HasMaxLength(500);
                e.Property(b => b.TargetSlug).HasMaxLength(200);
                e.Property(b => b.ExternalUrl).HasMaxLength(500);
                e.HasIndex(b => b.IsActive);
                e.HasIndex(b => b.SortOrder);
            });

            // ===== Seed Data — البنر الحالي كأول بنر =====
            modelBuilder.Entity<Banner>().HasData(
                new Banner
                {
                    Id = 1,
                    Title = "خصم يصل إلى 30% على مستلزمات الكمبيوتر",
                    Description = "ماوس • كيبورد • سماعات • كاميرات وسيستم كاشير احترافي — لفترة محدودة",
                    BadgeText = "عرض محدود",
                    CtaText = "تسوق الآن ←",
                    ImagePath = "ad-banner.jpg",
                    LinkType = BannerLinkType.Page,
                    TargetSlug = "/Products",
                    SortOrder = 1,
                    IsActive = true
                }
            );

            // ===== Seed Data — المنتجات =====
            modelBuilder.Entity<Product>().HasData(
                new Product { Id = 1, Category = "mouse", Name = "ماوس ZERO ZR200", SubName = "ZERO ZR-200 WIRED MOUSE", Price = 85, OldPrice = null, ImagePath = "mouse-zr200.png", Badge = "Mouse", Rating = 4.5, ReviewsCount = 28, SalesCount = 143, IsFeatured = true },
                new Product { Id = 2, Category = "mouse", Name = "ماوس ZERO ZR250", SubName = "ZERO ZR-250 WIRED MOUSE", Price = 85, OldPrice = null, ImagePath = "mouse-zr250.png", Badge = "Mouse", Rating = 4.3, ReviewsCount = 19, SalesCount = 98, IsFeatured = false },
                new Product { Id = 3, Category = "mouse", Name = "ماوس ZERO ZR150", SubName = "ZERO ZR-150 WIRED MOUSE 1600DPI", Price = 60, OldPrice = null, ImagePath = "mouse-zr150.png", Badge = "Mouse", Rating = 4.1, ReviewsCount = 35, SalesCount = 210, IsFeatured = true },
                new Product { Id = 4, Category = "mouse", Name = "ماوس Zero ZR225", SubName = "ZERO ZR-225 WIRED MOUSE", Price = 80, OldPrice = null, ImagePath = "mouse-zr225.png", Badge = "Mouse", Rating = 4.4, ReviewsCount = 22, SalesCount = 87, IsFeatured = false },
                new Product { Id = 5, Category = "mouse", Name = "ماوس Zero ZR275", SubName = "ZERO ZR-275 WIRED MOUSE", Price = 80, OldPrice = null, ImagePath = "mouse-zr275.png", Badge = "Mouse", Rating = 4.2, ReviewsCount = 14, SalesCount = 64, IsFeatured = false },
                new Product { Id = 6, Category = "mouse", Name = "ماوس Zero ZR375", SubName = "ZERO ZR-375 WIRED MOUSE", Price = 80, OldPrice = null, ImagePath = "mouse-zr375.png", Badge = "Mouse", Rating = 4.0, ReviewsCount = 11, SalesCount = 52, IsFeatured = false },
                new Product { Id = 7, Category = "mouse", Name = "ماوس ZERO ZR1900 Gaming", SubName = "MOUSE USB GAMING ZERO ZR-1900", Price = 230, OldPrice = null, ImagePath = "mouse-zr1900.png", Badge = "Gaming", Rating = 4.7, ReviewsCount = 41, SalesCount = 176, IsFeatured = true },
                new Product { Id = 8, Category = "mouse", Name = "ماوس ZERO ZR1850 Gaming", SubName = "ZERO ZR-1850 WIRED GAMING MOUSE", Price = 230, OldPrice = null, ImagePath = "mouse-zr1850.png", Badge = "Gaming", Rating = 4.6, ReviewsCount = 33, SalesCount = 155, IsFeatured = false },
                new Product { Id = 9, Category = "mouse", Name = "ماوس Gigamax GM-16 Wireless", SubName = "GIGAMAX PLUS GM-16 RECHARGEABLE BLUETOOTH", Price = 250, OldPrice = null, ImagePath = "mouse-gm16.png", Badge = "Wireless", Rating = 4.8, ReviewsCount = 56, SalesCount = 289, IsFeatured = true },
                new Product { Id = 10, Category = "mouse", Name = "ماوس HP M100", SubName = "HP M100 WIRED MOUSE", Price = 100, OldPrice = null, ImagePath = "mouse-hpm100.png", Badge = "HP", Rating = 4.3, ReviewsCount = 27, SalesCount = 118, IsFeatured = false },
                new Product { Id = 11, Category = "mouse", Name = "ماوس HP WM186 Wireless", SubName = "HP WM186 RECHARGEABLE BLUETOOTH/WIRELESS", Price = 100, OldPrice = null, ImagePath = "mouse-hpwm186.png", Badge = "Wireless", Rating = 4.5, ReviewsCount = 38, SalesCount = 201, IsFeatured = false },
                new Product { Id = 12, Category = "keyboard", Name = "كيبورد ZERO ZR-200", SubName = "ZERO ZR-200 ENGLISH AND ARABIC WIRED KEYBOARD", Price = 130, OldPrice = null, ImagePath = "kb-zr200.png", Badge = "Keyboard", Rating = 4.4, ReviewsCount = 24, SalesCount = 132, IsFeatured = true },
                new Product { Id = 13, Category = "keyboard", Name = "كيبورد مالتي Zero ZR-2608", SubName = "ZERO ZR-2608 ENGLISH AND ARABIC WIRED KEYBOARD", Price = 150, OldPrice = null, ImagePath = "kb-zr2608.png", Badge = "Keyboard", Rating = 4.3, ReviewsCount = 18, SalesCount = 89, IsFeatured = false },
                new Product { Id = 14, Category = "keyboard", Name = "كيبورد جيمينج ZERO ZR-2080", SubName = "ZERO ZR-2080 ENGLISH AND ARABIC GAMING KEYBOARD", Price = 270, OldPrice = null, ImagePath = "kb-zr2080.png", Badge = "Gaming", Rating = 4.7, ReviewsCount = 47, SalesCount = 198, IsFeatured = true },
                new Product { Id = 15, Category = "keyboard", Name = "كيبورد ZERO LIGHT ZR2050", SubName = "KEYBOARD USB GAMING ZERO ZR-2050", Price = 220, OldPrice = null, ImagePath = "kb-zr2050.png", Badge = "Gaming", Rating = 4.5, ReviewsCount = 31, SalesCount = 143, IsFeatured = false },
                new Product { Id = 16, Category = "headphone", Name = "هيدفون LH-782 Gaming", SubName = "HEADPHONE LH 782 GAMING", Price = 230, OldPrice = 250, ImagePath = "hp-lh782.png", Badge = "Headphone", Rating = 4.4, ReviewsCount = 29, SalesCount = 112, IsFeatured = true },
                new Product { Id = 17, Category = "headphone", Name = "سماعة JEDEL GH-559", SubName = "JEDEL WIRED LIGHTING GAMING HEADSET GH-559", Price = 380, OldPrice = 400, ImagePath = "hp-jedel559.png", Badge = "Gaming", Rating = 4.3, ReviewsCount = 22, SalesCount = 87, IsFeatured = false },
                new Product { Id = 18, Category = "headphone", Name = "هيدفون SADES USB G50", SubName = "SADES G50 SURROUND SOUND STEREO USB GAMING", Price = 599, OldPrice = 650, ImagePath = "hp-sades-g50.png", Badge = "SADES", Rating = 4.9, ReviewsCount = 68, SalesCount = 334, IsFeatured = true }
            );

            // ===== Seed Data — الخدمات =====
            modelBuilder.Entity<Service>().HasData(
                new Service { Id = 1, Icon = "🖱️", Title = "مستلزمات الكمبيوتر", Description = "ماوس، كيبورد، سماعات، وجميع الإكسسوارات بأعلى جودة وأسعار تنافسية.", PriceLabel = "تبدأ من 60 جنيه", SortOrder = 1 },
                new Service { Id = 2, Icon = "🖥️", Title = "نظام نقطة البيع (POS)", Description = "سيستم كاشير متكامل بالـ .NET لإدارة مخزونك ومبيعاتك مع تقارير تفصيلية.", PriceLabel = "تواصل للسعر", SortOrder = 2 },
                new Service { Id = 3, Icon = "📷", Title = "كاميرات المراقبة", Description = "أنظمة مراقبة للمنازل والمحلات بأعلى دقة وأسهل طريقة للتركيب والمتابعة.", PriceLabel = "عروض خاصة", SortOrder = 3 },
                new Service { Id = 4, Icon = "🔧", Title = "صيانة ودعم فني", Description = "فريق متخصص لصيانة الأجهزة وحل المشكلات التقنية مع ضمان على الخدمات.", PriceLabel = "01118324397", SortOrder = 4 }
            );
        }
    }
}