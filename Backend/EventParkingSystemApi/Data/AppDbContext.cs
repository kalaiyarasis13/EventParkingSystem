using EventParkingSystemApi.Models;
using Microsoft.EntityFrameworkCore;

namespace EventParkingSystemApi.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Customer> Customers => Set<Customer>();
        public DbSet<Venue> Venues => Set<Venue>();
        public DbSet<EventCategory> EventCategories => Set<EventCategory>();
        public DbSet<Event> Events => Set<Event>();
        public DbSet<Seat> Seats => Set<Seat>();
        public DbSet<ParkingSlot> ParkingSlots => Set<ParkingSlot>();
        public DbSet<Booking> Bookings => Set<Booking>();
        public DbSet<BookingSeat> BookingSeats => Set<BookingSeat>();
        public DbSet<ParkingReservation> ParkingReservations => Set<ParkingReservation>();
        public DbSet<Payment> Payments => Set<Payment>();
        public DbSet<Notification> Notifications => Set<Notification>();

        public DbSet<Feedback> Feedbacks => Set<Feedback>();
        public DbSet<SeatHold> SeatHolds => Set<SeatHold>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // ---- Customers ----
            modelBuilder.Entity<Customer>(e =>
            {
                e.HasIndex(c => c.Email).IsUnique();
                e.Property(c => c.Role).HasMaxLength(20);
            });

            // ---- EventCategories ----
            modelBuilder.Entity<EventCategory>(e =>
            {
                e.HasKey(x => x.CategoryId);
                e.HasIndex(c => c.Name).IsUnique();
            });

            // ---- Events ----
            modelBuilder.Entity<Event>(e =>
            {
                e.Property(x => x.TicketPrice).HasColumnType("decimal(10,2)");
                e.Property(x => x.ParkingFee).HasColumnType("decimal(10,2)");
                e.HasOne(x => x.Venue).WithMany(v => v.Events).HasForeignKey(x => x.VenueId);
                e.HasOne(x => x.Category).WithMany(c => c.Events).HasForeignKey(x => x.CategoryId);
            });

            // ---- Seats ----
            modelBuilder.Entity<Seat>(e =>
            {
                e.HasIndex(s => new { s.EventId, s.SeatRow, s.SeatNumber }).IsUnique();
                e.HasOne(s => s.Event).WithMany(ev => ev.Seats).HasForeignKey(s => s.EventId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // ---- ParkingSlots ----
            modelBuilder.Entity<ParkingSlot>(e =>
            {
                e.HasIndex(p => new { p.EventId, p.SlotLabel }).IsUnique();
                e.HasOne(p => p.Event).WithMany(ev => ev.ParkingSlots).HasForeignKey(p => p.EventId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // ---- Bookings ----
            modelBuilder.Entity<Booking>(e =>
            {
                e.HasIndex(b => b.BookingNumber).IsUnique();
                e.Property(b => b.TotalAmount).HasColumnType("decimal(10,2)");
                e.HasOne(b => b.Customer).WithMany(c => c.Bookings).HasForeignKey(b => b.CustomerId);
                e.HasOne(b => b.Event).WithMany(ev => ev.Bookings).HasForeignKey(b => b.EventId);
            });

            // ---- BookingSeats ----
            modelBuilder.Entity<BookingSeat>(e =>
            {
                e.Property(bs => bs.PriceAtBooking).HasColumnType("decimal(10,2)");
                e.HasIndex(bs => bs.SeatId).IsUnique(); // a seat can only be in one active booking
                e.HasOne(bs => bs.Booking).WithMany(b => b.BookingSeats).HasForeignKey(bs => bs.BookingId)
                    .OnDelete(DeleteBehavior.Cascade);
                e.HasOne(bs => bs.Seat).WithOne(s => s.BookingSeat).HasForeignKey<BookingSeat>(bs => bs.SeatId)
        .OnDelete(DeleteBehavior.Restrict);
            });

            // ---- ParkingReservations ----
            modelBuilder.Entity<ParkingReservation>(e =>
            {
                e.Property(pr => pr.FeeAtBooking).HasColumnType("decimal(10,2)");
                e.HasIndex(pr => pr.BookingId).IsUnique();     // max 1 slot per booking
                e.HasIndex(pr => pr.ParkingSlotId).IsUnique(); // 1 active reservation per slot
                e.HasOne(pr => pr.Booking).WithOne(b => b.ParkingReservation)
                    .HasForeignKey<ParkingReservation>(pr => pr.BookingId).OnDelete(DeleteBehavior.Cascade);
                e.HasOne(pr => pr.ParkingSlot).WithOne(s => s.ParkingReservation)
        .HasForeignKey<ParkingReservation>(pr => pr.ParkingSlotId)
        .OnDelete(DeleteBehavior.Restrict);
            });

            // ---- Payments ----
            modelBuilder.Entity<Payment>(e =>
            {
                e.Property(p => p.AmountPaid).HasColumnType("decimal(10,2)");
                e.HasIndex(p => p.BookingId).IsUnique(); // no double payment
                e.HasOne(p => p.Booking).WithOne(b => b.Payment)
                    .HasForeignKey<Payment>(p => p.BookingId).OnDelete(DeleteBehavior.Cascade);
            });

            // ---- Feedbacks ----
            modelBuilder.Entity<Feedback>(e =>
            {
                e.HasKey(f => f.FeedbackId);

                e.Property(f => f.Comment)
                    .HasMaxLength(1000);

                e.HasIndex(f => f.BookingId)
                    .IsUnique(); // one feedback per booking

                e.HasOne(f => f.Booking)
                    .WithOne(b => b.Feedback)
                    .HasForeignKey<Feedback>(f => f.BookingId)
                    .OnDelete(DeleteBehavior.Cascade);

                e.HasOne(f => f.Customer)
     .WithMany(c => c.Feedbacks)
     .HasForeignKey(f => f.CustomerId)
     .OnDelete(DeleteBehavior.Restrict);
            });

            // ---- Seat Holds ----
            modelBuilder.Entity<SeatHold>(e =>
            {
                e.HasKey(h => h.HoldId);

                e.HasOne(h => h.Seat)
    .WithMany(s => s.SeatHolds)
    .HasForeignKey(h => h.SeatId)
    .OnDelete(DeleteBehavior.Restrict);

                e.HasOne(h => h.Event)
                    .WithMany()
                    .HasForeignKey(h => h.EventId)
                    .OnDelete(DeleteBehavior.Restrict);

                e.HasOne(h => h.Customer)
                    .WithMany()
                    .HasForeignKey(h => h.CustomerId)
                    .OnDelete(DeleteBehavior.Restrict);

                e.HasIndex(h => new
                {
                    h.SeatId,
                    h.Status
                });
            });

            // ---- Notifications ----
            modelBuilder.Entity<Notification>(e =>
            {
                e.HasOne(n => n.Customer).WithMany(c => c.Notifications).HasForeignKey(n => n.CustomerId)
                    .OnDelete(DeleteBehavior.Cascade);
                e.HasIndex(n => new { n.CustomerId, n.CreatedAt });
            });

            SeedData(modelBuilder);
        }

        // Seed data applied by the initial migration (dotnet ef database update).
        // NOTE: the seeded password hashes below are placeholders and are NOT valid
        // login credentials — register real accounts via POST /api/auth/register.
        // To test the Admin panel, register a customer normally, then run:
        //   UPDATE Customers SET Role = 'Admin' WHERE Email = 'you@example.com';
        private static void SeedData(ModelBuilder modelBuilder)
        {
            var seedDate = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            const string placeholderHash = "PLACEHOLDER_NOT_A_VALID_HASH_REGISTER_VIA_API";

            modelBuilder.Entity<Customer>().HasData(
                new Customer
                {
                    CustomerId = 1,
                    FullName = "System Admin",
                    Email = "admin@eventparking.com",
                    Phone = "0000000000",
                    PasswordHash = placeholderHash,
                    Role = "Admin",
                    IsActive = true,
                    CreatedAt = seedDate,
                    UpdatedAt = seedDate
                },
                new Customer
                {
                    CustomerId = 2,
                    FullName = "Nila Kumar",
                    Email = "nila@example.com",
                    Phone = "9876500001",
                    PasswordHash = placeholderHash,
                    Role = "Customer",
                    IsActive = true,
                    CreatedAt = seedDate,
                    UpdatedAt = seedDate
                }
            );

            modelBuilder.Entity<Venue>().HasData(
                new Venue { VenueId = 1, Name = "Marina Grand Arena", Address = "12 Beach Road, Chennai", TotalCapacity = 500, CreatedAt = seedDate, UpdatedAt = seedDate }
            );

            modelBuilder.Entity<EventCategory>().HasData(
                new EventCategory { CategoryId = 1, Name = "Concert", CreatedAt = seedDate },
                new EventCategory { CategoryId = 2, Name = "Sports", CreatedAt = seedDate },
                new EventCategory { CategoryId = 3, Name = "Conference", CreatedAt = seedDate },
                new EventCategory { CategoryId = 4, Name = "Workshop", CreatedAt = seedDate }
            );

            modelBuilder.Entity<Event>().HasData(
                new Event
                {
                    EventId = 1,
                    Name = "Sunburn Live 2026",
                    VenueId = 1,
                    CategoryId = 1,
                    EventDate = new DateOnly(2026, 12, 20),
                    EventTime = new TimeOnly(19, 0),
                    TicketPrice = 1500.00m,
                    ParkingFee = 100.00m,
                    Description = "A night of live music under the stars.",
                    IsLocked = false,
                    CreatedAt = seedDate,
                    UpdatedAt = seedDate
                }
            );

            // 3 rows (A-C) x 10 seats = 30 seats for the sample event.
            var seats = new List<Seat>();
            var seatId = 1;
            foreach (var rowLetter in new[] { "A", "B", "C" })
            {
                for (var n = 1; n <= 10; n++)
                {
                    seats.Add(new Seat { SeatId = seatId++, EventId = 1, SeatRow = rowLetter, SeatNumber = n, Status = SeatStatus.Available });
                }
            }
            modelBuilder.Entity<Seat>().HasData(seats);

            // 10 parking slots (P1-P10) for the sample event.
            var slots = Enumerable.Range(1, 10)
                .Select(n => new ParkingSlot { ParkingSlotId = n, EventId = 1, SlotLabel = $"P{n}", Status = ParkingSlotStatus.Available });
            modelBuilder.Entity<ParkingSlot>().HasData(slots);
        }
    }
}
