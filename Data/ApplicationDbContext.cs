using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using ParkEasy.Web.Models.Entities;

namespace ParkEasy.Web.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<ParkingSpace> ParkingSpaces => Set<ParkingSpace>();
        public DbSet<ParkingSlot> ParkingSlots => Set<ParkingSlot>();
        public DbSet<Booking> Bookings => Set<Booking>();
        public DbSet<Payment> Payments => Set<Payment>();
        public DbSet<Refund> Refunds => Set<Refund>();
        public DbSet<Notification> Notifications => Set<Notification>();

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Safety net: force all DateTime values to UTC so Npgsql never sees Kind=Local
            // for 'timestamp with time zone' columns. Local -> ToUniversalTime(),
            // Unspecified -> SpecifyKind(Utc), Utc -> as-is. On read, ensure Kind=Utc.
            var dateTimeConverter = new ValueConverter<DateTime, DateTime>(
                v => v.Kind == DateTimeKind.Utc ? v
                    : v.Kind == DateTimeKind.Local ? v.ToUniversalTime()
                    : DateTime.SpecifyKind(v, DateTimeKind.Utc),
                v => DateTime.SpecifyKind(v, DateTimeKind.Utc));

            var nullableDateTimeConverter = new ValueConverter<DateTime?, DateTime?>(
                v => v == null ? v
                    : v.Value.Kind == DateTimeKind.Utc ? v
                    : v.Value.Kind == DateTimeKind.Local ? v.Value.ToUniversalTime()
                    : DateTime.SpecifyKind(v.Value, DateTimeKind.Utc),
                v => v == null ? v : DateTime.SpecifyKind(v.Value, DateTimeKind.Utc));

            foreach (var entityType in builder.Model.GetEntityTypes())
            {
                foreach (var property in entityType.GetProperties())
                {
                    if (property.ClrType == typeof(DateTime))
                        property.SetValueConverter(dateTimeConverter);
                    else if (property.ClrType == typeof(DateTime?))
                        property.SetValueConverter(nullableDateTimeConverter);
                }
            }

            // Configure table names
            builder.Entity<ApplicationUser>().ToTable("Users");
            builder.Entity<ParkingSpace>().ToTable("ParkingSpaces");
            builder.Entity<ParkingSlot>().ToTable("ParkingSlots");
            builder.Entity<Booking>().ToTable("Bookings");
            builder.Entity<Payment>().ToTable("Payments");
            builder.Entity<Refund>().ToTable("Refunds");
            builder.Entity<Notification>().ToTable("Notifications");

            // ParkingSpace relations
            builder.Entity<ParkingSpace>()
                .HasOne(p => p.Owner)
                .WithMany(u => u.OwnedParkingSpaces)
                .HasForeignKey(p => p.OwnerId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<ParkingSpace>()
                .HasIndex(p => p.City);

            builder.Entity<ParkingSpace>()
                .HasIndex(p => new { p.Latitude, p.Longitude });

            // ParkingSlot relations
            builder.Entity<ParkingSlot>()
                .HasOne(s => s.ParkingSpace)
                .WithMany(p => p.Slots)
                .HasForeignKey(s => s.ParkingSpaceId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<ParkingSlot>()
                .HasIndex(s => new { s.ParkingSpaceId, s.SlotNumber })
                .IsUnique();

            // Booking relations
            builder.Entity<Booking>()
                .HasOne(b => b.User)
                .WithMany(u => u.Bookings)
                .HasForeignKey(b => b.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Booking>()
                .HasOne(b => b.ParkingSpace)
                .WithMany(p => p.Bookings)
                .HasForeignKey(b => b.ParkingSpaceId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Booking>()
                .HasOne(b => b.ParkingSlot)
                .WithMany(s => s.Bookings)
                .HasForeignKey(b => b.ParkingSlotId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Booking>()
                .HasIndex(b => b.BookingReference)
                .IsUnique();

            builder.Entity<Booking>()
                .HasIndex(b => new { b.ParkingSlotId, b.StartTime, b.EndTime });

            // Payment relations
            builder.Entity<Payment>()
                .HasOne(p => p.Booking)
                .WithOne(b => b.Payment)
                .HasForeignKey<Payment>(p => p.BookingId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Payment>()
                .HasIndex(p => p.TransactionId)
                .IsUnique();

            // Refund relations
            builder.Entity<Refund>()
                .HasOne(r => r.Booking)
                .WithOne(b => b.Refund)
                .HasForeignKey<Refund>(r => r.BookingId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Refund>()
                .HasOne(r => r.Payment)
                .WithOne(p => p.Refund)
                .HasForeignKey<Refund>(r => r.PaymentId)
                .OnDelete(DeleteBehavior.Restrict);

            // Decimal Precision
            builder.Entity<ParkingSpace>()
                .Property(p => p.Latitude)
                .HasPrecision(9, 6);

            builder.Entity<ParkingSpace>()
                .Property(p => p.Longitude)
                .HasPrecision(9, 6);

            builder.Entity<ParkingSpace>()
                .Property(p => p.BasePricePerHour)
                .HasPrecision(18, 2);

            builder.Entity<ParkingSlot>()
                .Property(s => s.PricePerHour)
                .HasPrecision(18, 2);

            builder.Entity<Booking>()
                .Property(b => b.PricePerHour)
                .HasPrecision(18, 2);

            builder.Entity<Booking>()
                .Property(b => b.TotalAmount)
                .HasPrecision(18, 2);

            builder.Entity<Payment>()
                .Property(p => p.Amount)
                .HasPrecision(18, 2);

            builder.Entity<Refund>()
                .Property(r => r.Amount)
                .HasPrecision(18, 2);
        }
    }
}
