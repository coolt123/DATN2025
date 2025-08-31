using DATN.Entities;
using Identity.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;


namespace DATN.DbContexts
{
    public class Data : IdentityDbContext<ApplicationUser>
    {
        public Data(DbContextOptions <Data> options) : base(options)
        { 
        }

        public DbSet<Product> Products { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderDetail> OrderDetails { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<Cart> Carts { get; set; }
        public DbSet<Coupon> Coupons { get; set; }
        public DbSet<Banner> Banners { get; set; }
        public DbSet<Post> Posts { get; set; }
        public DbSet<InventoryLog> InventoryLogs { get; set; }
        public DbSet<CustomerLoyalty> CustomerLoyalties { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<ProductImage> ProductImages { get; set; }
        public DbSet<Style> Styles { get; set; }
        public DbSet<Material> Materials { get; set; }
        public DbSet<ProductView> ProductViews { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Category>(e =>
            {
                e.ToTable("categories");
                e.HasKey(c => c.CategoryId);
                e.Property(c => c.NameCategory)
                    .IsRequired()
                    .HasMaxLength(255)
                    .IsUnicode(true);
            });
            modelBuilder.Entity<Style>(e =>
            {
                e.ToTable("styles");
                e.HasKey(s => s.Id);
                e.Property(s => s.Name)
                    .IsRequired()
                    .HasMaxLength(255)
                    .IsUnicode(true);

                e.HasIndex(s => s.Name).IsUnique();
            }
            );


            modelBuilder.Entity<Material>(e =>
            {
                e.ToTable("materials");
                e.HasKey(m => m.Id);
                e.Property(m => m.Name)
                    .IsRequired()
                    .HasMaxLength(255)
                    .IsUnicode(true);

                e.HasIndex(m => m.Name).IsUnique(); 
            });
            modelBuilder.Entity<ProductView>(e =>
            {
                e.ToTable("product_views");
                e.HasKey(pv => pv.Id);

                e.Property(pv => pv.UserId)
                    .HasMaxLength(450)
                    .IsUnicode(false);

                e.Property(pv => pv.SessionId)
                    .HasMaxLength(100)
                    .IsUnicode(false);

                e.Property(pv => pv.ViewedAt)
                    .HasDefaultValueSql("GETDATE()");

                e.HasOne(pv => pv.Product)
                    .WithMany()
                    .HasForeignKey(pv => pv.ProductId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Product>(e =>
            {
                e.ToTable("products");
                e.HasKey(e => e.ProductId);
                e.Property(e => e.ProductId)
                .ValueGeneratedOnAdd();
                e.Property(pr => pr.Price)
                .HasColumnType("decimal(18,4)");
                e.Property(e => e.NameProduct)
                .IsRequired()
                .HasMaxLength(255)
                .IsUnicode(true);
                e.HasOne(p => p.Category)
                   .WithMany(c => c.Products)
                   .HasForeignKey(p => p.CategoryId)
                   .OnDelete(DeleteBehavior.Restrict);
                e.HasOne(p => p.Material)
                    .WithMany(m => m.Products)
                    .HasForeignKey(p => p.MaterialId)
                    .OnDelete(DeleteBehavior.Restrict);
                e.HasOne(p => p.Style)
                    .WithMany(s => s.Products)
                    .HasForeignKey(p => p.StyleId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
            modelBuilder.Entity<ProductImage>(e =>
            {
                e.ToTable("productimages");
                e.HasKey(e => e.ProductImageId);
                e.Property(e => e.ProductImageId)
                    .ValueGeneratedOnAdd();
                e.Property(e => e.ImageUrl)
                    .IsRequired()
                    .HasMaxLength(500);  

                e.Property(e => e.DisplayOrder)
                    .HasDefaultValue(0);

                e.Property(e => e.IsMain)
                    .HasDefaultValue(false);

                e.HasOne(e => e.Product)
                    .WithMany(p => p.ProductImages)
                    .HasForeignKey(e => e.ProductId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
            modelBuilder.Entity<Order>(e =>
            {
                e.ToTable("orders");
                e.HasKey(o => o.OrderId);
                e.Property(o => o.TotalAmount)
                .HasColumnType("decimal(18,4)");
                e.HasOne(o => o.User)
                    .WithMany(u => u.Orders)
                    .HasForeignKey(o => o.IdUser)
                    .OnDelete(DeleteBehavior.Restrict);
            });
            modelBuilder.Entity<OrderDetail>(e =>
            {
                e.ToTable("order_details");
                e.HasKey(od => od.OrderDetailId);
                e.Property(od => od.UnitPrice)
                .HasColumnType("decimal(18,4)");
                e.HasOne(od => od.Order)
                    .WithMany(o => o.OrderDetails)
                    .HasForeignKey(od => od.OrderId)
                    .OnDelete(DeleteBehavior.Restrict);
                e.HasOne(od => od.Product)
                    .WithMany(p => p.OrderDetails)
                    .HasForeignKey(od => od.ProductId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
            modelBuilder.Entity<Review>(e =>
            {
                e.ToTable("reviews");
                e.HasKey(r => r.ReviewId);
                e.HasOne(r => r.Product)
                    .WithMany(p => p.Reviews)
                    .HasForeignKey(r => r.ProductId)
                    .OnDelete(DeleteBehavior.Restrict);
                e.HasOne(r => r.User)
                    .WithMany(u => u.Reviews)
                    .HasForeignKey(r => r.IdUser)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Cart>(e =>
            {
                e.ToTable("carts");
                e.HasKey(c => c.CartId);
                e.HasOne(c => c.User)
                    .WithMany(u => u.Carts)
                    .HasForeignKey(c => c.IdUser)
                    .OnDelete(DeleteBehavior.Restrict);
                e.HasOne(c => c.Product)
                    .WithMany(p => p.Carts)
                    .HasForeignKey(c => c.ProductId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Coupon>(e =>
            {
                e.ToTable("coupons");
                e.HasKey(c => c.CouponId);
            });

            
            modelBuilder.Entity<Banner>(e =>
            {
                e.ToTable("banners");
                e.HasKey(b => b.BannerId);
            });

          
            modelBuilder.Entity<Post>(e =>
            {
                e.ToTable("posts");
                e.HasKey(p => p.PostId);
            });
            modelBuilder.Entity<InventoryLog>(e =>
            {
                e.ToTable("inventory_logs");
                e.HasKey(i => i.LogId);
                e.HasOne(i => i.Product)
                    .WithMany(p => p.InventoryLogs)
                    .HasForeignKey(i => i.ProductId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
            modelBuilder.Entity<CustomerLoyalty>(e =>
            {
                e.ToTable("customer_loyalties");
                e.HasKey(cl => cl.LoyaltyId);
                e.HasOne(cl => cl.User)
                    .WithMany(u => u.CustomerLoyalties)
                    .HasForeignKey(cl => cl.IdUser)
                    .OnDelete(DeleteBehavior.Restrict);
            });
            modelBuilder.Entity<Payment>(e =>
            {
                e.ToTable("payments");
                e.HasKey(p => p.PaymentId);
                e.Property(p => p.Amount)
                .HasColumnType("decimal(18,4)");
                e.HasOne(p => p.Order)
                    .WithMany(o => o.Payments)
                    .HasForeignKey(p => p.OrderId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}
