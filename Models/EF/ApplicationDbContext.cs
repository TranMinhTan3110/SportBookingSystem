using Microsoft.EntityFrameworkCore;
using SportBookingSystem.Models;
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


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            
            modelBuilder.Entity<Users>()
                .HasIndex(u => u.Username)
                .IsUnique();

         
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
                .HasOne(t => t.Sender)
                .WithMany(u => u.SentTransactions)
                .HasForeignKey(t => t.UserId)
                .OnDelete(DeleteBehavior.Restrict);

           
            modelBuilder.Entity<Transactions>()
                .HasOne(t => t.Receiver)
                .WithMany(u => u.ReceivedTransactions)
                .HasForeignKey(t => t.ReceiverId)
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

            // Cấu hình Seed Data cho TimeSlots
            modelBuilder.Entity<TimeSlots>().HasData(
                new TimeSlots { SlotId = 1, SlotName = "Ca 1", StartTime = new TimeSpan(6, 0, 0), EndTime = new TimeSpan(7, 30, 0) },
                new TimeSlots { SlotId = 2, SlotName = "Ca 2", StartTime = new TimeSpan(7, 30, 0), EndTime = new TimeSpan(9, 0, 0) },
                new TimeSlots { SlotId = 3, SlotName = "Ca 3", StartTime = new TimeSpan(16, 0, 0), EndTime = new TimeSpan(17, 30, 0) },
                new TimeSlots { SlotId = 4, SlotName = "Ca 4 (Vàng)", StartTime = new TimeSpan(17, 30, 0), EndTime = new TimeSpan(19, 0, 0) },
                new TimeSlots { SlotId = 5, SlotName = "Ca 5 (Vàng)", StartTime = new TimeSpan(19, 0, 0), EndTime = new TimeSpan(20, 30, 0) }
            );
        }
    }
    }