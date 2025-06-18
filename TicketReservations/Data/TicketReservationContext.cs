using Microsoft.EntityFrameworkCore;
using TicketReservations.Models;

namespace TicketReservations.Data
{
    public class TicketReservationContext : DbContext
    {
        public TicketReservationContext(DbContextOptions<TicketReservationContext> options)
            : base(options)
        {
        }

        public DbSet<Venue> Venues { get; set; }
        
        public DbSet<Event> Events { get; set; }
        
        public DbSet<TicketType> TicketTypes { get; set; }
        
        public DbSet<Reservation> Reservations { get; set; }
        
        public DbSet<Sale> Sales { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure Venue entity
            modelBuilder.Entity<Venue>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Address).IsRequired().HasMaxLength(200);
                entity.Property(e => e.City).HasMaxLength(50);
                entity.Property(e => e.State).HasMaxLength(50);
                entity.Property(e => e.ZipCode).HasMaxLength(20);
            });

            // Configure Event entity
            modelBuilder.Entity<Event>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Title).IsRequired().HasMaxLength(150);
                entity.Property(e => e.Description).IsRequired().HasMaxLength(500);
                entity.Property(e => e.Artist).HasMaxLength(100);
                entity.Property(e => e.Genre).HasMaxLength(50);
                
                // Configure relationship with Venue
                entity.HasOne(e => e.Venue)
                      .WithMany(v => v.Events)
                      .HasForeignKey(e => e.VenueId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Configure TicketType entity
            modelBuilder.Entity<TicketType>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Description).HasMaxLength(300);
                entity.Property(e => e.Price).HasPrecision(10, 2);
                
                // Configure relationship with Event
                entity.HasOne(e => e.Event)
                      .WithMany(ev => ev.TicketTypes)
                      .HasForeignKey(e => e.EventId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure Reservation entity
            modelBuilder.Entity<Reservation>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.UnitPrice).HasPrecision(10, 2);
                entity.Property(e => e.TotalAmount).HasPrecision(10, 2);
                entity.Property(e => e.CustomerName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.CustomerEmail).IsRequired().HasMaxLength(100);
                entity.Property(e => e.CustomerPhone).IsRequired().HasMaxLength(20);
                entity.Property(e => e.ReservationCode).HasMaxLength(50);
                
                // Configure relationships
                entity.HasOne(e => e.Event)
                      .WithMany(ev => ev.Reservations)
                      .HasForeignKey(e => e.EventId)
                      .OnDelete(DeleteBehavior.Restrict);
                
                entity.HasOne(e => e.TicketType)
                      .WithMany(tt => tt.Reservations)
                      .HasForeignKey(e => e.TicketTypeId)
                      .OnDelete(DeleteBehavior.Restrict);
                
                entity.HasOne(e => e.Sale)
                      .WithOne()
                      .HasForeignKey<Reservation>(e => e.SaleId)
                      .OnDelete(DeleteBehavior.SetNull);
            });

            // Configure Sale entity
            modelBuilder.Entity<Sale>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.UnitPrice).HasPrecision(10, 2);
                entity.Property(e => e.TotalAmount).HasPrecision(10, 2);
                entity.Property(e => e.ServiceFee).HasPrecision(10, 2);
                entity.Property(e => e.Tax).HasPrecision(10, 2);
                entity.Property(e => e.FinalAmount).HasPrecision(10, 2);
                entity.Property(e => e.RefundAmount).HasPrecision(10, 2);
                entity.Property(e => e.CustomerName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.CustomerEmail).IsRequired().HasMaxLength(100);
                entity.Property(e => e.CustomerPhone).IsRequired().HasMaxLength(20);
                entity.Property(e => e.PaymentTransactionId).HasMaxLength(100);
                entity.Property(e => e.PaymentMethod).HasMaxLength(50);
                entity.Property(e => e.ConfirmationCode).HasMaxLength(50);
                
                // Configure relationships
                entity.HasOne(e => e.Event)
                      .WithMany(ev => ev.Sales)
                      .HasForeignKey(e => e.EventId)
                      .OnDelete(DeleteBehavior.Restrict);
                
                entity.HasOne(e => e.TicketType)
                      .WithMany(tt => tt.Sales)
                      .HasForeignKey(e => e.TicketTypeId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Seed data
            SeedData(modelBuilder);
        }

        private static void SeedData(ModelBuilder modelBuilder)
        {
            // Seed Venues
            modelBuilder.Entity<Venue>().HasData(
                new Venue
                {
                    Id = 1,
                    Name = "Central Park Amphitheater",
                    Address = "Central Park West & 72nd St",
                    City = "New York",
                    State = "NY",
                    ZipCode = "10023",
                    TotalCapacity = 5000,
                    Description = "Outdoor amphitheater in the heart of Central Park",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new Venue
                {
                    Id = 2,
                    Name = "Convention Center Hall A",
                    Address = "123 Convention Blvd",
                    City = "San Francisco",
                    State = "CA",
                    ZipCode = "94107",
                    TotalCapacity = 1000,
                    Description = "Modern conference facility with state-of-the-art technology",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new Venue
                {
                    Id = 3,
                    Name = "Downtown Comedy Club",
                    Address = "456 Laugh Lane",
                    City = "Chicago",
                    State = "IL",
                    ZipCode = "60601",
                    TotalCapacity = 300,
                    Description = "Intimate comedy venue featuring top comedians",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                }
            );

            // Seed Events
            modelBuilder.Entity<Event>().HasData(
                new Event
                {
                    Id = 1,
                    Title = "Summer Music Festival",
                    Description = "A weekend of amazing music featuring top artists from around the world including rock, pop, and indie performers.",
                    EventDate = DateTime.UtcNow.AddDays(30),
                    DoorsOpen = DateTime.UtcNow.AddDays(30).AddHours(-1),
                    VenueId = 1,
                    Status = EventStatus.OnSale,
                    Artist = "Various Artists",
                    Genre = "Mixed",
                    MinimumAge = 0,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new Event
                {
                    Id = 2,
                    Title = "Tech Conference 2024",
                    Description = "Join industry leaders for the latest insights in technology and innovation, featuring keynotes and workshops.",
                    EventDate = DateTime.UtcNow.AddDays(45),
                    DoorsOpen = DateTime.UtcNow.AddDays(45).AddHours(-0.5),
                    VenueId = 2,
                    Status = EventStatus.OnSale,
                    Artist = "Tech Industry Leaders",
                    Genre = "Conference",
                    MinimumAge = 0,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new Event
                {
                    Id = 3,
                    Title = "Comedy Night Special",
                    Description = "An evening of laughter with renowned comedians from around the country performing their latest material.",
                    EventDate = DateTime.UtcNow.AddDays(15),
                    DoorsOpen = DateTime.UtcNow.AddDays(15).AddHours(-0.5),
                    VenueId = 3,
                    Status = EventStatus.OnSale,
                    Artist = "Comedy All-Stars",
                    Genre = "Comedy",
                    MinimumAge = 18,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                }
            );

            // Seed Ticket Types
            modelBuilder.Entity<TicketType>().HasData(
                // Summer Music Festival Tickets
                new TicketType
                {
                    Id = 1,
                    EventId = 1,
                    Name = "General Admission",
                    Description = "General admission to the festival grounds",
                    Price = 89.99m,
                    TotalQuantity = 3000,
                    AvailableQuantity = 3000,
                    ReservedQuantity = 0,
                    SoldQuantity = 0,
                    MaxPerCustomer = 8,
                    Category = TicketCategory.General,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new TicketType
                {
                    Id = 2,
                    EventId = 1,
                    Name = "VIP Experience",
                    Description = "VIP access with premium viewing area and exclusive amenities",
                    Price = 199.99m,
                    TotalQuantity = 500,
                    AvailableQuantity = 500,
                    ReservedQuantity = 0,
                    SoldQuantity = 0,
                    MaxPerCustomer = 4,
                    Category = TicketCategory.VIP,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                // Tech Conference Tickets
                new TicketType
                {
                    Id = 3,
                    EventId = 2,
                    Name = "Standard Pass",
                    Description = "Access to all sessions and networking events",
                    Price = 199.99m,
                    TotalQuantity = 800,
                    AvailableQuantity = 800,
                    ReservedQuantity = 0,
                    SoldQuantity = 0,
                    MaxPerCustomer = 5,
                    Category = TicketCategory.General,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new TicketType
                {
                    Id = 4,
                    EventId = 2,
                    Name = "Student Discount",
                    Description = "Discounted rate for students with valid ID",
                    Price = 99.99m,
                    TotalQuantity = 200,
                    AvailableQuantity = 200,
                    ReservedQuantity = 0,
                    SoldQuantity = 0,
                    MaxPerCustomer = 1,
                    Category = TicketCategory.Student,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                // Comedy Night Tickets
                new TicketType
                {
                    Id = 5,
                    EventId = 3,
                    Name = "Regular Seating",
                    Description = "Standard seating for the comedy show",
                    Price = 45.00m,
                    TotalQuantity = 250,
                    AvailableQuantity = 250,
                    ReservedQuantity = 0,
                    SoldQuantity = 0,
                    MaxPerCustomer = 6,
                    Category = TicketCategory.General,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new TicketType
                {
                    Id = 6,
                    EventId = 3,
                    Name = "Premium Seating",
                    Description = "Front row premium seating with complimentary drinks",
                    Price = 75.00m,
                    TotalQuantity = 50,
                    AvailableQuantity = 50,
                    ReservedQuantity = 0,
                    SoldQuantity = 0,
                    MaxPerCustomer = 4,
                    Category = TicketCategory.Premium,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                }
            );
        }
    }
} 