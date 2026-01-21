using Microsoft.EntityFrameworkCore;
using SportBookingSystem.Models.Entities;

namespace SportBookingSystem.Models.EF
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<Role> Roles { get; set; }
        public DbSet<Users> Users { get; set; }
        public DbSet<Categories> Categories { get; set; }
        public DbSet<Pitches> Pitches { get; set; }
        public DbSet<Products> Products { get; set; }
        public DbSet<Bookings> Bookings { get; set; }
        public DbSet<BookingServices> BookingServices { get; set; }
        public DbSet<Orders> Orders { get; set; }
        public DbSet<OrderDetails> OrderDetails { get; set; }
        public DbSet<Transactions> Transactions { get; set; }
        public DbSet<SystemSetting> SystemSetting { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Users>()
                .HasIndex(u => u.Username)
                .IsUnique();

            modelBuilder.Entity<Users>()
                .Property(u => u.CreatedAt)
                .HasDefaultValueSql("GETDATE()");

            modelBuilder.Entity<Users>()
                .HasIndex(u => u.Phone)
                .IsUnique();

            modelBuilder.Entity<Users>()
                .HasIndex(u => u.Email)
                .IsUnique();

            modelBuilder.Entity<Bookings>()
                .HasIndex(b => b.CheckInCode)
                .IsUnique();

            modelBuilder.Entity<Orders>()
                .HasIndex(o => o.OrderCode)
                .IsUnique();

            modelBuilder.Entity<Transactions>()
                .HasIndex(t => t.TransactionCode)
                .IsUnique();

            modelBuilder.Entity<Transactions>()
                .HasOne(t => t.Sender)
                .WithMany(u => u.SentTransactions)
                .HasForeignKey(t => t.UserId) // Đã sửa từ SenderId về UserId
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Transactions>()
                .HasOne(t => t.Receiver)
                .WithMany(u => u.ReceivedTransactions)
                .HasForeignKey(t => t.ReceiverId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Transactions>()
                .HasOne(t => t.Booking)
                .WithMany()
                .HasForeignKey(t => t.BookingId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Transactions>()
                .HasOne(t => t.Order)
                .WithMany()
                .HasForeignKey(t => t.OrderId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<BookingServices>()
                .HasOne(bs => bs.Product)
                .WithMany()
                .HasForeignKey(bs => bs.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<BookingServices>()
                .HasOne(bs => bs.Booking)
                .WithMany(b => b.BookingServices)
                .HasForeignKey(bs => bs.BookingId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<OrderDetails>()
                .HasOne(od => od.Product)
                .WithMany()
                .HasForeignKey(od => od.ProductId)
                .OnDelete(DeleteBehavior.Restrict);
            //dữ liệu bảng SystemSetting
            modelBuilder.Entity<SystemSetting>().HasData(
    new SystemSetting { SettingKey = "RewardAmountStep", SettingValue = "10000" },
    new SystemSetting { SettingKey = "RewardPointBonus", SettingValue = "1" },
    new SystemSetting { SettingKey = "IsRewardActive", SettingValue = "true" }
);
        }
    }
}