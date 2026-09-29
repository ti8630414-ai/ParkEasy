using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ParkEasy.Web.Models.Entities;
using ParkEasy.Web.Models.Enums;

namespace ParkEasy.Web.Data
{
    public static class DbInitializer
    {
        public static async Task SeedAsync(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            // Apply pending migrations automatically on startup
            if ((await context.Database.GetPendingMigrationsAsync()).Any())
            {
                await context.Database.MigrateAsync();
            }

            // 1. Seed Roles
            string[] roles = { UserRoles.Admin, UserRoles.Owner, UserRoles.User };
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            // 2. Seed Users
            // Admin
            var adminEmail = "admin@parkeasy.com";
            var adminUser = await userManager.FindByEmailAsync(adminEmail);
            if (adminUser == null)
            {
                adminUser = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    FullName = "System Administrator",
                    EmailConfirmed = true,
                    PhoneNumber = "+1 (555) 010-0001",
                    CreatedAt = DateTime.UtcNow
                };
                var result = await userManager.CreateAsync(adminUser, "Admin@123456");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, UserRoles.Admin);
                }
            }

            // Owner 1
            var owner1Email = "owner@parkeasy.com";
            var owner1 = await userManager.FindByEmailAsync(owner1Email);
            if (owner1 == null)
            {
                owner1 = new ApplicationUser
                {
                    UserName = owner1Email,
                    Email = owner1Email,
                    FullName = "Marcus Sterling",
                    EmailConfirmed = true,
                    PhoneNumber = "+1 (555) 019-2834",
                    CreatedAt = DateTime.UtcNow
                };
                var result = await userManager.CreateAsync(owner1, "Owner@123456");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(owner1, UserRoles.Owner);
                }
            }

            // Owner 2
            var owner2Email = "owner2@parkeasy.com";
            var owner2 = await userManager.FindByEmailAsync(owner2Email);
            if (owner2 == null)
            {
                owner2 = new ApplicationUser
                {
                    UserName = owner2Email,
                    Email = owner2Email,
                    FullName = "Elena Vance",
                    EmailConfirmed = true,
                    PhoneNumber = "+1 (555) 018-9921",
                    CreatedAt = DateTime.UtcNow
                };
                var result = await userManager.CreateAsync(owner2, "Owner@123456");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(owner2, UserRoles.Owner);
                }
            }

            // Demo User (Driver)
            var user1Email = "user@parkeasy.com";
            var user1 = await userManager.FindByEmailAsync(user1Email);
            if (user1 == null)
            {
                user1 = new ApplicationUser
                {
                    UserName = user1Email,
                    Email = user1Email,
                    FullName = "David Miller",
                    EmailConfirmed = true,
                    PhoneNumber = "+1 (555) 014-7733",
                    VehiclePlateNumber = "NYC-7821",
                    PreferredVehicleType = VehicleType.Car,
                    CreatedAt = DateTime.UtcNow
                };
                var result = await userManager.CreateAsync(user1, "User@123456");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(user1, UserRoles.User);
                }
            }

            // Demo User 2 (Driver)
            var user2Email = "user2@parkeasy.com";
            var user2 = await userManager.FindByEmailAsync(user2Email);
            if (user2 == null)
            {
                user2 = new ApplicationUser
                {
                    UserName = user2Email,
                    Email = user2Email,
                    FullName = "Sophia Martinez",
                    EmailConfirmed = true,
                    PhoneNumber = "+1 (555) 016-4411",
                    VehiclePlateNumber = "CA-9932",
                    PreferredVehicleType = VehicleType.ElectricVehicle,
                    CreatedAt = DateTime.UtcNow
                };
                var result = await userManager.CreateAsync(user2, "User@123456");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(user2, UserRoles.User);
                }
            }

            // 3. Seed Parking Spaces & Slots
            if (!await context.ParkingSpaces.AnyAsync())
            {
                var space1 = new ParkingSpace
                {
                    OwnerId = owner1.Id,
                    Name = "Grand Central Plaza Parking",
                    Description = "Premium covered underground parking with 24/7 security, high-speed EV chargers, and valet service right next to Grand Central Terminal.",
                    Address = "120 E 42nd St",
                    City = "New York",
                    State = "NY",
                    PostalCode = "10017",
                    Latitude = 40.7527m,
                    Longitude = -73.9772m,
                    BasePricePerHour = 1250m,
                    Is24Hours = true,
                    ImageUrl = "https://images.unsplash.com/photo-1506521781263-d8422e82f27a?auto=format&fit=crop&w=800&q=80",
                    HasCCTV = true,
                    HasEVCharging = true,
                    HasCoveredParking = true,
                    HasDisabledAccess = true,
                    HasValet = true,
                    HasSecurityGuard = true,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-30)
                };

                var space2 = new ParkingSpace
                {
                    OwnerId = owner1.Id,
                    Name = "Downtown Financial Bay Garage",
                    Description = "Modern multi-storey automated parking facility located in the heart of the Financial District with smart license-plate entry.",
                    Address = "450 Mission St",
                    City = "San Francisco",
                    State = "CA",
                    PostalCode = "94105",
                    Latitude = 37.7937m,
                    Longitude = -122.3965m,
                    BasePricePerHour = 900m,
                    Is24Hours = true,
                    ImageUrl = "https://images.unsplash.com/photo-1590674899484-d5640e854abe?auto=format&fit=crop&w=800&q=80",
                    HasCCTV = true,
                    HasEVCharging = true,
                    HasCoveredParking = true,
                    HasDisabledAccess = true,
                    HasValet = false,
                    HasSecurityGuard = true,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-20)
                };

                var space3 = new ParkingSpace
                {
                    OwnerId = owner2.Id,
                    Name = "Silicon Valley Tech Hub Parking",
                    Description = "Spacious outdoor and solar-covered parking lot with rapid Level 3 DC fast chargers for electric vehicles.",
                    Address = "200 S 1st St",
                    City = "San Jose",
                    State = "CA",
                    PostalCode = "95113",
                    Latitude = 37.3382m,
                    Longitude = -121.8863m,
                    BasePricePerHour = 650m,
                    Is24Hours = false,
                    OpeningTime = new TimeSpan(6, 0, 0),
                    ClosingTime = new TimeSpan(23, 0, 0),
                    ImageUrl = "https://images.unsplash.com/photo-1573348722427-f1d6819fdf98?auto=format&fit=crop&w=800&q=80",
                    HasCCTV = true,
                    HasEVCharging = true,
                    HasCoveredParking = true,
                    HasDisabledAccess = true,
                    HasValet = false,
                    HasSecurityGuard = true,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-15)
                };

                var space4 = new ParkingSpace
                {
                    OwnerId = owner2.Id,
                    Name = "Millennium Park North Deck",
                    Description = "Secure central parking facility just minutes from major museums, restaurants, and downtown attractions.",
                    Address = "221 N Columbus Dr",
                    City = "Chicago",
                    State = "IL",
                    PostalCode = "60601",
                    Latitude = 41.8826m,
                    Longitude = -87.6226m,
                    BasePricePerHour = 800m,
                    Is24Hours = true,
                    ImageUrl = "https://images.unsplash.com/photo-1526628953301-3e589a6a8b74?auto=format&fit=crop&w=800&q=80",
                    HasCCTV = true,
                    HasEVCharging = false,
                    HasCoveredParking = true,
                    HasDisabledAccess = true,
                    HasValet = true,
                    HasSecurityGuard = true,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-10)
                };

                context.ParkingSpaces.AddRange(space1, space2, space3, space4);
                await context.SaveChangesAsync();

                // Generate slots for each space (3 per location)
                var spaces = new[] { space1, space2, space3, space4 };
                foreach (var s in spaces)
                {
                    var slots = new List<ParkingSlot>();
                    for (int i = 1; i <= 3; i++)
                    {
                        var vehicleType = i switch { 2 => VehicleType.SUV, 3 => VehicleType.ElectricVehicle, _ => VehicleType.Car };

                        var rate = vehicleType switch
                        {
                            VehicleType.Motorcycle => s.BasePricePerHour * 0.6m,
                            VehicleType.SUV => s.BasePricePerHour * 1.2m,
                            VehicleType.ElectricVehicle => s.BasePricePerHour * 1.1m,
                            _ => s.BasePricePerHour
                        };

                        slots.Add(new ParkingSlot
                        {
                            ParkingSpaceId = s.Id,
                            SlotNumber = $"A-{i:D2}",
                            FloorOrZone = "Ground Floor",
                            SupportedVehicleType = vehicleType,
                            PricePerHour = Math.Round(rate, 2),
                            HasEVCharger = vehicleType == VehicleType.ElectricVehicle,
                            IsActive = true,
                            CurrentStatus = SlotStatus.Available,
                            CreatedAt = DateTime.UtcNow.AddDays(-10)
                        });
                    }
                    context.ParkingSlots.AddRange(slots);
                }
                await context.SaveChangesAsync();

                // 4. Seed Sample Bookings
                var slot1 = await context.ParkingSlots.FirstAsync(s => s.ParkingSpaceId == space1.Id && s.SlotNumber == "A-01");
                var slot2 = await context.ParkingSlots.FirstAsync(s => s.ParkingSpaceId == space1.Id && s.SlotNumber == "A-02");
                var slot3 = await context.ParkingSlots.FirstAsync(s => s.ParkingSpaceId == space2.Id && s.SlotNumber == "A-01");

                // Booking 1: Upcoming Confirmed Booking with Paid Payment
                var booking1 = new Booking
                {
                    BookingReference = "PE-20260921-1001",
                    UserId = user1.Id,
                    ParkingSpaceId = space1.Id,
                    ParkingSlotId = slot1.Id,
                    VehiclePlateNumber = "NYC-7821",
                    VehicleType = VehicleType.Car,
                    StartTime = DateTime.UtcNow.AddHours(2),
                    EndTime = DateTime.UtcNow.AddHours(5),
                    DurationHours = 3,
                    PricePerHour = slot1.PricePerHour,
                    TotalAmount = slot1.PricePerHour * 3,
                    Status = BookingStatus.Confirmed,
                    CreatedAt = DateTime.UtcNow.AddHours(-1)
                };

                // Booking 2: Active Booking
                var booking2 = new Booking
                {
                    BookingReference = "PE-20260921-1002",
                    UserId = user2.Id,
                    ParkingSpaceId = space1.Id,
                    ParkingSlotId = slot2.Id,
                    VehiclePlateNumber = "CA-9932",
                    VehicleType = VehicleType.ElectricVehicle,
                    StartTime = DateTime.UtcNow.AddHours(-1),
                    EndTime = DateTime.UtcNow.AddHours(3),
                    DurationHours = 4,
                    PricePerHour = slot2.PricePerHour,
                    TotalAmount = slot2.PricePerHour * 4,
                    Status = BookingStatus.Active,
                    CheckedInAt = DateTime.UtcNow.AddHours(-1),
                    CreatedAt = DateTime.UtcNow.AddHours(-2)
                };

                // Booking 3: Completed Booking
                var booking3 = new Booking
                {
                    BookingReference = "PE-20260920-1003",
                    UserId = user1.Id,
                    ParkingSpaceId = space2.Id,
                    ParkingSlotId = slot3.Id,
                    VehiclePlateNumber = "NYC-7821",
                    VehicleType = VehicleType.Car,
                    StartTime = DateTime.UtcNow.AddDays(-1),
                    EndTime = DateTime.UtcNow.AddDays(-1).AddHours(3),
                    DurationHours = 3,
                    PricePerHour = slot3.PricePerHour,
                    TotalAmount = slot3.PricePerHour * 3,
                    Status = BookingStatus.Completed,
                    CheckedInAt = DateTime.UtcNow.AddDays(-1),
                    CheckedOutAt = DateTime.UtcNow.AddDays(-1).AddHours(3),
                    CreatedAt = DateTime.UtcNow.AddDays(-1).AddHours(-1)
                };

                context.Bookings.AddRange(booking1, booking2, booking3);
                await context.SaveChangesAsync();

                // 5. Seed Payments
                var payment1 = new Payment
                {
                    BookingId = booking1.Id,
                    TransactionId = "TXN-PE-20260921-998811",
                    Amount = booking1.TotalAmount,
                    PaymentMethod = PaymentMethod.CreditCard,
                    Status = PaymentStatus.Paid,
                    CardLast4 = "4242",
                    GatewayResponse = "SIMULATED_APPROVED_200_OK",
                    PaidAt = DateTime.UtcNow.AddHours(-1),
                    CreatedAt = DateTime.UtcNow.AddHours(-1)
                };

                var payment2 = new Payment
                {
                    BookingId = booking2.Id,
                    TransactionId = "TXN-PE-20260921-887722",
                    Amount = booking2.TotalAmount,
                    PaymentMethod = PaymentMethod.MobileWallet,
                    MobileWalletProvider = "PayPal / Demo Pay",
                    Status = PaymentStatus.Paid,
                    GatewayResponse = "SIMULATED_APPROVED_200_OK",
                    PaidAt = DateTime.UtcNow.AddHours(-2),
                    CreatedAt = DateTime.UtcNow.AddHours(-2)
                };

                var payment3 = new Payment
                {
                    BookingId = booking3.Id,
                    TransactionId = "TXN-PE-20260920-776633",
                    Amount = booking3.TotalAmount,
                    PaymentMethod = PaymentMethod.CreditCard,
                    Status = PaymentStatus.Paid,
                    CardLast4 = "4242",
                    GatewayResponse = "SIMULATED_APPROVED_200_OK",
                    PaidAt = DateTime.UtcNow.AddDays(-1).AddHours(-1),
                    CreatedAt = DateTime.UtcNow.AddDays(-1).AddHours(-1)
                };

                context.Payments.AddRange(payment1, payment2, payment3);
                await context.SaveChangesAsync();

                // 6. Seed In-app Notifications
                var notif1 = new Notification
                {
                    UserId = user1.Id,
                    Title = "Welcome to ParkEasy!",
                    Message = "Your account has been set up successfully. Explore nearby parking spaces and reserve guaranteed spots in seconds.",
                    Type = NotificationType.SystemAlert,
                    IsRead = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-2)
                };

                var notif2 = new Notification
                {
                    UserId = user1.Id,
                    Title = "Booking Confirmed: PE-20260921-1001",
                    Message = "Your parking slot A-01 at Grand Central Plaza Parking is reserved. Click to view your QR digital pass.",
                    Type = NotificationType.BookingCreated,
                    IsRead = false,
                    ActionUrl = $"/Booking/Details/{booking1.Id}",
                    CreatedAt = DateTime.UtcNow.AddHours(-1)
                };

                var notif3 = new Notification
                {
                    UserId = owner1.Id,
                    Title = "New Booking Received",
                    Message = "Slot A-01 at Grand Central Plaza Parking has been booked for today.",
                    Type = NotificationType.BookingCreated,
                    IsRead = false,
                    ActionUrl = "/Owner/Requests",
                    CreatedAt = DateTime.UtcNow.AddHours(-1)
                };

                context.Notifications.AddRange(notif1, notif2, notif3);
                await context.SaveChangesAsync();
            }

            // 7. Extra locations with fully-empty slot grids (idempotent: runs on every startup)
            var extraOwner1 = await userManager.FindByEmailAsync("owner@parkeasy.com");
            var extraOwner2 = await userManager.FindByEmailAsync("owner2@parkeasy.com");
            if (extraOwner1 != null && extraOwner2 != null)
            {
                var extraSpaces = new[]
                {
                    new { OwnerId = extraOwner1.Id, Name = "Sunset Boulevard Open Lot", Description = "Open-air budget lot off Sunset Blvd with wide SUV-friendly bays and easy freeway access.", Address = "7500 Sunset Blvd", City = "Los Angeles", State = "CA", PostalCode = "90046", Lat = 34.0980m, Lng = -118.3350m, Price = 750m, Is24 = true, Open = new TimeSpan(6, 0, 0), Close = new TimeSpan(23, 0, 0), Img = "https://images.unsplash.com/photo-1477959858617-67f85cf4f1df?auto=format&fit=crop&w=800&q=80", EV = true, Covered = false, Valet = false },
                    new { OwnerId = extraOwner2.Id, Name = "Space Needle View Garage", Description = "Covered multi-level garage two blocks from the Space Needle with EV chargers on every floor.", Address = "325 Broad St", City = "Seattle", State = "WA", PostalCode = "98109", Lat = 47.6205m, Lng = -122.3493m, Price = 850m, Is24 = true, Open = new TimeSpan(6, 0, 0), Close = new TimeSpan(23, 0, 0), Img = "https://images.unsplash.com/photo-1502175353174-a7a70e73b362?auto=format&fit=crop&w=800&q=80", EV = true, Covered = true, Valet = false },
                    new { OwnerId = extraOwner1.Id, Name = "Beacon Hill Secure Deck", Description = "Historic-district secure deck with valet, CCTV, and heated indoor bays for winter months.", Address = "1 Beacon St", City = "Boston", State = "MA", PostalCode = "02108", Lat = 42.3588m, Lng = -71.0570m, Price = 950m, Is24 = false, Open = new TimeSpan(6, 0, 0), Close = new TimeSpan(23, 0, 0), Img = "https://images.unsplash.com/photo-1558036117-15d82a90b9b1?auto=format&fit=crop&w=800&q=80", EV = false, Covered = true, Valet = true },
                    new { OwnerId = extraOwner2.Id, Name = "Congress Avenue River Lot", Description = "Gravel riverside lot near downtown Austin. Cheapest hourly rate in the network, 24/7 access.", Address = "100 S Congress Ave", City = "Austin", State = "TX", PostalCode = "78704", Lat = 30.2500m, Lng = -97.7490m, Price = 550m, Is24 = true, Open = new TimeSpan(0, 0, 0), Close = new TimeSpan(23, 59, 0), Img = "https://images.unsplash.com/photo-1531218150217-54595bc2b934?auto=format&fit=crop&w=800&q=80", EV = false, Covered = false, Valet = false },
                    new { OwnerId = extraOwner1.Id, Name = "Ocean Drive Beach Parking", Description = "Covered beachside parking on Ocean Drive with outdoor rinse area and 24/7 security patrol.", Address = "1000 Ocean Dr", City = "Miami", State = "FL", PostalCode = "33139", Lat = 25.7780m, Lng = -80.1300m, Price = 700m, Is24 = true, Open = new TimeSpan(0, 0, 0), Close = new TimeSpan(23, 59, 0), Img = "https://images.unsplash.com/photo-1506966953602-c20cc11f75e3?auto=format&fit=crop&w=800&q=80", EV = true, Covered = true, Valet = false },
                    new { OwnerId = extraOwner2.Id, Name = "Union Station Transit Hub Garage", Description = "Transit-adjacent garage at Union Station. Ideal park-and-ride with motorcycle bays and CCTV.", Address = "1701 Wynkoop St", City = "Denver", State = "CO", PostalCode = "80202", Lat = 39.7525m, Lng = -104.9995m, Price = 600m, Is24 = true, Open = new TimeSpan(0, 0, 0), Close = new TimeSpan(23, 59, 0), Img = "https://images.unsplash.com/photo-1494515843206-f3117d3f51b7?auto=format&fit=crop&w=800&q=80", EV = false, Covered = true, Valet = false },
                    new { OwnerId = extraOwner1.Id, Name = "Desert Sky Airport Lot", Description = "Shaded airport lot with misted walkways, five minutes from the terminals via free shuttle.", Address = "3400 E Sky Harbor Blvd", City = "Phoenix", State = "AZ", PostalCode = "85034", Lat = 33.4342m, Lng = -112.0080m, Price = 500m, Is24 = true, Open = new TimeSpan(0, 0, 0), Close = new TimeSpan(23, 59, 0), Img = "https://images.unsplash.com/photo-1558618666-fcd25c85cd64?auto=format&fit=crop&w=800&q=80", EV = false, Covered = true, Valet = false },
                    new { OwnerId = extraOwner2.Id, Name = "Rose Quarter Riverside Garage", Description = "Riverside event garage by the arena with EV chargers and covered bike racks.", Address = "1 N Center Ct St", City = "Portland", State = "OR", PostalCode = "97227", Lat = 45.5316m, Lng = -122.6668m, Price = 650m, Is24 = true, Open = new TimeSpan(0, 0, 0), Close = new TimeSpan(23, 59, 0), Img = "https://images.unsplash.com/photo-1541450805268-842c0c845a97?auto=format&fit=crop&w=800&q=80", EV = true, Covered = true, Valet = false },
                    new { OwnerId = extraOwner1.Id, Name = "Broadway Honky Tonk Deck", Description = "Downtown nightlife deck on Broadway with valet till late and live-camera security.", Address = "308 Broadway", City = "Nashville", State = "TN", PostalCode = "37201", Lat = 36.1627m, Lng = -86.7816m, Price = 750m, Is24 = false, Open = new TimeSpan(6, 0, 0), Close = new TimeSpan(23, 0, 0), Img = "https://images.unsplash.com/photo-1545419913-775e21e8cb97?auto=format&fit=crop&w=800&q=80", EV = false, Covered = true, Valet = true },
                    new { OwnerId = extraOwner2.Id, Name = "Gulshan Avenue Secure Garage", Description = "Covered secure garage in Gulshan with CCTV, EV chargers, and valet service.", Address = "House 12, Road 5, Gulshan", City = "Dhaka", State = "Dhaka", PostalCode = "1212", Lat = 23.7808m, Lng = 90.4167m, Price = 450m, Is24 = true, Open = new TimeSpan(0, 0, 0), Close = new TimeSpan(23, 59, 0), Img = "https://images.unsplash.com/photo-1506521781263-d8422e82f27a?auto=format&fit=crop&w=800&q=80", EV = true, Covered = true, Valet = true },
                    new { OwnerId = extraOwner1.Id, Name = "GEC Circle Central Parking", Description = "Multi-level parking near GEC Circle with wide bays for SUVs and 24/7 guard.", Address = "CDA Avenue, Muradpur", City = "Chattogram", State = "Chattogram", PostalCode = "4000", Lat = 22.3569m, Lng = 91.7832m, Price = 350m, Is24 = true, Open = new TimeSpan(0, 0, 0), Close = new TimeSpan(23, 59, 0), Img = "https://images.unsplash.com/photo-1590674899484-d5640e854abe?auto=format&fit=crop&w=800&q=80", EV = false, Covered = true, Valet = false },
                    new { OwnerId = extraOwner2.Id, Name = "Zindabazar Riverside Deck", Description = "Riverside deck in Zindabazar commercial hub with covered bays and CCTV.", Address = "Zindabazar", City = "Sylhet", State = "Sylhet", PostalCode = "3100", Lat = 24.8949m, Lng = 91.8690m, Price = 300m, Is24 = false, Open = new TimeSpan(6, 0, 0), Close = new TimeSpan(23, 0, 0), Img = "https://images.unsplash.com/photo-1573348722427-f1d6819fdf98?auto=format&fit=crop&w=800&q=80", EV = false, Covered = true, Valet = false },
                    new { OwnerId = extraOwner1.Id, Name = "KDA Avenue Transit Lot", Description = "Budget transit lot on KDA Avenue. Park-and-ride with 24/7 access.", Address = "KDA Avenue", City = "Khulna", State = "Khulna", PostalCode = "9100", Lat = 22.8456m, Lng = 89.5403m, Price = 200m, Is24 = true, Open = new TimeSpan(0, 0, 0), Close = new TimeSpan(23, 59, 0), Img = "https://images.unsplash.com/photo-1526628953301-3e589a6a8b74?auto=format&fit=crop&w=800&q=80", EV = false, Covered = false, Valet = false },
                    new { OwnerId = extraOwner2.Id, Name = "Shaheb Bazar City Garage", Description = "Central city garage at Shaheb Bazar with security guard and disabled access.", Address = "Shaheb Bazar", City = "Rajshahi", State = "Rajshahi", PostalCode = "6000", Lat = 24.3745m, Lng = 88.6042m, Price = 250m, Is24 = true, Open = new TimeSpan(0, 0, 0), Close = new TimeSpan(23, 59, 0), Img = "https://images.unsplash.com/photo-1477959858617-67f85cf4f1df?auto=format&fit=crop&w=800&q=80", EV = false, Covered = true, Valet = false },
                    new { OwnerId = extraOwner1.Id, Name = "Sadar Road River View Lot", Description = "Open river-view lot on Sadar Road, minutes from the launch terminal.", Address = "Sadar Road", City = "Barishal", State = "Barishal", PostalCode = "8200", Lat = 22.7010m, Lng = 90.3535m, Price = 200m, Is24 = true, Open = new TimeSpan(0, 0, 0), Close = new TimeSpan(23, 59, 0), Img = "https://images.unsplash.com/photo-1502175353174-a7a70e73b362?auto=format&fit=crop&w=800&q=80", EV = false, Covered = false, Valet = false },
                    new { OwnerId = extraOwner2.Id, Name = "Station Road Central Deck", Description = "Covered deck near Rangpur station with EV charging and CCTV.", Address = "Station Road", City = "Rangpur", State = "Rangpur", PostalCode = "5400", Lat = 25.7439m, Lng = 89.2752m, Price = 250m, Is24 = false, Open = new TimeSpan(6, 0, 0), Close = new TimeSpan(23, 0, 0), Img = "https://images.unsplash.com/photo-1558036117-15d82a90b9b1?auto=format&fit=crop&w=800&q=80", EV = true, Covered = true, Valet = false },
                };

                foreach (var e in extraSpaces)
                {
                    if (await context.ParkingSpaces.AnyAsync(p => p.Name == e.Name))
                        continue;

                    var space = new ParkingSpace
                    {
                        OwnerId = e.OwnerId,
                        Name = e.Name,
                        Description = e.Description,
                        Address = e.Address,
                        City = e.City,
                        State = e.State,
                        PostalCode = e.PostalCode,
                        Latitude = e.Lat,
                        Longitude = e.Lng,
                        BasePricePerHour = e.Price,
                        Is24Hours = e.Is24,
                        OpeningTime = e.Open,
                        ClosingTime = e.Close,
                        ImageUrl = e.Img,
                        HasCCTV = true,
                        HasEVCharging = e.EV,
                        HasCoveredParking = e.Covered,
                        HasDisabledAccess = true,
                        HasValet = e.Valet,
                        HasSecurityGuard = true,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    };
                    context.ParkingSpaces.Add(space);
                    await context.SaveChangesAsync();

                    // Full empty grid: 3 slots, no bookings reference them
                    var slots = new List<ParkingSlot>();
                    for (int i = 1; i <= 3; i++)
                    {
                        var vehicleType = i switch { 2 => VehicleType.SUV, 3 => VehicleType.ElectricVehicle, _ => VehicleType.Car };

                        var rate = vehicleType switch
                        {
                            VehicleType.Motorcycle => space.BasePricePerHour * 0.6m,
                            VehicleType.SUV => space.BasePricePerHour * 1.2m,
                            VehicleType.ElectricVehicle => space.BasePricePerHour * 1.1m,
                            _ => space.BasePricePerHour
                        };

                        slots.Add(new ParkingSlot
                        {
                            ParkingSpaceId = space.Id,
                            SlotNumber = $"A-{i:D2}",
                            FloorOrZone = "Ground Floor",
                            SupportedVehicleType = vehicleType,
                            PricePerHour = Math.Round(rate, 2),
                            HasEVCharger = vehicleType == VehicleType.ElectricVehicle,
                            IsActive = true,
                            CurrentStatus = SlotStatus.Available,
                            CreatedAt = DateTime.UtcNow
                        });
                    }
                    context.ParkingSlots.AddRange(slots);
                    await context.SaveChangesAsync();
                }
            }

            // 8. Keep USA locations off the public map (idempotent: runs on every startup).
            // Bookings/payments/history are preserved — spaces are only hidden from search/map.
            var usaStates = new[] { "NY", "CA", "IL", "WA", "MA", "TX", "FL", "CO", "AZ", "OR", "TN" };
            var usaSpaces = await context.ParkingSpaces
                .Where(p => p.State != null && usaStates.Contains(p.State) && p.IsActive)
                .ToListAsync();
            foreach (var s in usaSpaces)
                s.IsActive = false;
            if (usaSpaces.Count > 0)
                await context.SaveChangesAsync();
        }
    }
}

