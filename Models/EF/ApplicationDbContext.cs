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

        public DbSet<TimeSlots> TimeSlots { get; set; }
        public DbSet<PitchSlots> PitchSlots { get; set; }

        public DbSet<SystemSetting> SystemSetting { get; set; }
        public DbSet<PitchPriceSetting> PitchPriceSettings { get; set; }


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
            modelBuilder.Entity<PitchPriceSetting>()
    .HasOne(p => p.Pitch)
    .WithMany()
    .HasForeignKey(p => p.PitchId)
    .OnDelete(DeleteBehavior.Cascade);

            // Cấu hình Seed Data cho TimeSlots
            modelBuilder.Entity<TimeSlots>().HasData(
         
          new TimeSlots { SlotId = 1, SlotName = "Ca Sáng 1 (1.5h)", StartTime = new TimeSpan(6, 0, 0), EndTime = new TimeSpan(7, 30, 0), IsActive = true },
          new TimeSlots { SlotId = 2, SlotName = "Ca Sáng 2 (1.5h)", StartTime = new TimeSpan(7, 30, 0), EndTime = new TimeSpan(9, 0, 0), IsActive = true },
          new TimeSlots { SlotId = 3, SlotName = "Ca Sáng 3 (1.5h)", StartTime = new TimeSpan(9, 0, 0), EndTime = new TimeSpan(10, 30, 0), IsActive = true },

     
          new TimeSlots { SlotId = 4, SlotName = "Ca Trưa 1 (1h)", StartTime = new TimeSpan(10, 30, 0), EndTime = new TimeSpan(11, 30, 0), IsActive = true },
          new TimeSlots { SlotId = 5, SlotName = "Ca Trưa 2 (1h)", StartTime = new TimeSpan(11, 30, 0), EndTime = new TimeSpan(12, 30, 0), IsActive = true }, // ID 5 cũ là 19:00 -> Giờ sửa thành 11:30
          new TimeSlots { SlotId = 6, SlotName = "Ca Chiều 1 (1h)", StartTime = new TimeSpan(14, 0, 0), EndTime = new TimeSpan(15, 0, 0), IsActive = true },
          new TimeSlots { SlotId = 7, SlotName = "Ca Chiều 2 (1h)", StartTime = new TimeSpan(15, 0, 0), EndTime = new TimeSpan(16, 0, 0), IsActive = true },
          new TimeSlots { SlotId = 8, SlotName = "Ca Chiều 3 (1.5h)", StartTime = new TimeSpan(16, 0, 0), EndTime = new TimeSpan(17, 30, 0), IsActive = true },

      
          new TimeSlots { SlotId = 9, SlotName = "Giờ Vàng 1 (1.5h)", StartTime = new TimeSpan(17, 30, 0), EndTime = new TimeSpan(19, 0, 0), IsActive = true },
          new TimeSlots { SlotId = 10, SlotName = "Giờ Vàng 2 (1.5h)", StartTime = new TimeSpan(19, 0, 0), EndTime = new TimeSpan(20, 30, 0), IsActive = true },

         
          new TimeSlots { SlotId = 11, SlotName = "Ca Đêm 1 (1.5h)", StartTime = new TimeSpan(20, 30, 0), EndTime = new TimeSpan(22, 0, 0), IsActive = true },
          new TimeSlots { SlotId = 12, SlotName = "Ca Vét (1h)", StartTime = new TimeSpan(22, 0, 0), EndTime = new TimeSpan(23, 0, 0), IsActive = true }
      );
            //dữ liệu bảng SystemSetting
            modelBuilder.Entity<SystemSetting>().HasData(
    new SystemSetting { SettingKey = "RewardAmountStep", SettingValue = "10000" },
    new SystemSetting { SettingKey = "RewardPointBonus", SettingValue = "1" },
    new SystemSetting { SettingKey = "IsRewardActive", SettingValue = "true" }
);
        }
    }
}