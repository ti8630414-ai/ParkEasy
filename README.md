# ParkEasy – Smart Parking Slot Booking System

**Course Name:** Software Development Project  
**Project Name:** ParkEasy – Smart Parking Slot Booking System  
**Programming Language:** C# (.NET 8)  
**Framework:** ASP.NET Core MVC (Model-View-Controller)  
**Database:** PostgreSQL (Npgsql + Entity Framework Core 8)  
**Authentication & Authorization:** ASP.NET Core Identity (Role-Based: Admin, Owner, User)  
**Real-Time Communications:** SignalR (`/parkingHub`)  
**QR Code Engine:** QRCoder (Base64 PNG generation)  
**Frontend:** Razor Views (.cshtml), HTML5, CSS3, JavaScript, Bootstrap 5.3, Bootstrap Icons, Leaflet / Google Maps API  

---

## 🌟 Executive Summary & Objectives

**ParkEasy** is a complete, modern web application designed for university software engineering demonstration and real-world deployment. It allows drivers to search for parking locations, inspect real-time slot availability, reserve designated parking slots with anti-overlapping concurrency locks, make simulated payments, generate digital QR parking passes, and track booking history.

Parking space owners and system administrators have access to dedicated management dashboards with live occupancy analytics, slot CRUD, booking approval workflows, dynamic pricing, earnings ledgers, cancellation management, and refund processing.

---

## 🏗️ System Architecture

```text
ParkEasy/
├── ParkEasy.sln                     # Visual Studio Solution File
├── README.md                        # Documentation and Setup Guide
└── ParkEasy.Web/                    # ASP.NET Core MVC Web Project
    ├── Controllers/
    │   ├── HomeController.cs        # Landing page, About, Contact, Privacy
    │   ├── AccountController.cs     # Auth, Registration, Profile, Password, RBAC
    │   ├── ParkingController.cs     # Public Search, Radius filtering, Details, Slot API
    │   ├── BookingController.cs     # Slot locking, Checkout, History, Details, QR Pass
    │   ├── PaymentController.cs     # Academic Payment Simulator & Receipts
    │   ├── OwnerController.cs       # Owner Dashboard, Spaces, Slots, Requests, Earnings, Occupancy
    │   ├── AdminController.cs       # Admin Dashboard, User management, System Oversight, Diagnostics
    │   └── NotificationController.cs# In-app alerts, badge updates, Read/Delete
    ├── Models/
    │   ├── Entities/                # Database entities mapped to PostgreSQL
    │   │   ├── ApplicationUser.cs   # Identity user with vehicle profiles
    │   │   ├── ParkingSpace.cs      # Facility locations, amenities, coordinates
    │   │   ├── ParkingSlot.cs       # Slots with status, zone, vehicle types, rates
    │   │   ├── Booking.cs           # Booking records with timestamps & references
    │   │   ├── Payment.cs           # Payment transaction ledger
    │   │   ├── Refund.cs            # Refund logs and status
    │   │   └── Notification.cs      # In-app notifications
    │   ├── Enums/
    │   │   └── Enums.cs             # VehicleType, SlotStatus, BookingStatus, Roles, etc.
    │   └── ViewModels/              # DTOs and strongly-typed viewmodels
    ├── Data/
    │   ├── ApplicationDbContext.cs  # EF Core DbContext with PostgreSQL fluent configs
    │   └── DbInitializer.cs         # Automated seed data (Admin, Owner, Driver, Spaces, Slots)
    ├── Services/
    │   ├── Interfaces/              # Service contracts
    │   └── Implementations/         # Business logic implementations
    │       ├── BookingService.cs    # Concurrency checks, slot locking, pass payloads
    │       ├── ParkingService.cs    # Haversine spatial search, slot availability
    │       ├── PaymentService.cs    # Payment simulator & refund processing
    │       ├── NotificationService.cs # Real-time SignalR notification dispatcher
    │       ├── QrCodeService.cs     # High-resolution QR code generator
    │       └── EmailSimulationService.cs # Formatted console email logger
    ├── Hubs/
    │   └── ParkingHub.cs            # SignalR WebSocket hub for live slot updates
    ├── Migrations/                  # EF Core database migrations
    ├── Views/                       # Clean Razor (.cshtml) responsive UI templates
    ├── wwwroot/                     # Static CSS, JavaScript, and asset libraries
    ├── appsettings.json             # Configuration & connection strings
    └── Program.cs                   # Dependency injection and HTTP pipeline setup
```

---

## 🔑 Demo Login Accounts

The system automatically initializes these accounts upon first launch:

| Role | Email | Password | Pre-configured Profile |
| :--- | :--- | :--- | :--- |
| **System Admin** | `admin@parkeasy.com` | `Admin@123456` | Full system control & user management |
| **Space Owner** | `owner@parkeasy.com` | `Owner@123456` | Owner of Grand Central & Financial Bay Garages |
| **Space Owner 2**| `owner2@parkeasy.com`| `Owner@123456` | Owner of Silicon Valley Hub & Millennium Deck |
| **Driver / User** | `user@parkeasy.com` | `User@123456` | Driver with vehicle plate `NYC-7821` |
| **Driver 2** | `user2@parkeasy.com`| `User@123456` | Driver with EV vehicle plate `CA-9932` |

> 💡 **Quick Fill Feature**: On the Sign In page, click the **Admin**, **Owner**, or **Driver** quick-fill buttons to populate credentials with a single click.

---

## 🚀 Getting Started & Setup Guide

### 1. Prerequisites
- **.NET 8 SDK** (or later)
- **PostgreSQL 13+** installed and running on `localhost:5432` (or via Docker)
- **Visual Studio 2022** (v17.8+) or **Visual Studio Code** / JetBrains Rider

---

### 2. Configure PostgreSQL Database Connection

Open `ParkEasy.Web/appsettings.json` and adjust the PostgreSQL connection string if your PostgreSQL password differs from `postgres`:

```json
"ConnectionStrings": {
  "DefaultConnection": "Host=localhost;Port=5432;Database=parkeasy;Username=postgres;Password=YOUR_POSTGRES_PASSWORD"
}
```

---

### 3. Build & Run the Application

#### Option A: Using Visual Studio
1. Open `ParkEasy.sln` in Visual Studio 2022.
2. Select **ParkEasy.Web** as the startup project.
3. Open **Package Manager Console** (or terminal) and run:
   ```powershell
   Update-Database
   ```
4. Press **F5** (or Ctrl+F5) to launch the web application.

#### Option B: Using .NET CLI / Terminal
```bash
# 1. Clone or navigate to the repository folder
cd E:\

# 2. Restore NuGet dependencies
dotnet restore

# 3. Build the solution
dotnet build

# 4. Apply EF Core database migrations to PostgreSQL
dotnet ef database update --project ParkEasy.Web/ParkEasy.Web.csproj --startup-project ParkEasy.Web/ParkEasy.Web.csproj

# 5. Run the web application
dotnet run --project ParkEasy.Web/ParkEasy.Web.csproj
```

Open your browser at:  
👉 **`https://localhost:5001`** or **`http://localhost:5000`**

---

## 🛠️ Core Modules & Features

### 1. User & Driver Booking Module
- **Search & Radius Filtering**: Search parking facilities by city, keywords, max price slider, vehicle type, and amenities (CCTV, EV Charging, Covered, Wheelchair Access, Valet).
- **Interactive Visual Slot Layout**: Real-time grid displaying slots categorized by floor/zone with live color statuses:
  - 🟢 **Green**: Available for selected time window
  - 🔵 **Blue**: Selected by user
  - 🔴 **Red**: Occupied by another reservation
  - 🟡 **Orange**: Under Maintenance
- **Anti-Overlapping Concurrency Protection**: Strict mathematical interval check ensures slots cannot be double-booked:
  $$\text{Overlap} \iff (\text{Start}_{\text{req}} < \text{End}_{\text{exist}}) \land (\text{End}_{\text{req}} > \text{Start}_{\text{exist}})$$
- **Digital QR Parking Pass**: Generates high-resolution Base64 QR code tickets with encrypted vehicle details, booking reference, and time validity. Printable and downloadable as PDF.
- **Booking History**: Categorized tabs for Upcoming, Active, Completed, and Cancelled bookings.

---

### 2. Owner Management Module
- **Owner Command Center**: Real-time KPI statistics (Total Spaces, Total Slots, Occupancy %, Today's Earnings, Total Revenue).
- **Parking Spaces CRUD**: Add new spaces with interactive map coordinate picker (click/drag pin to set Latitude/Longitude), operating hours (24/7 or custom), and auto-generated slots.
- **Slot Management**: Create, edit, toggle maintenance, and delete individual slots.
- **Booking Request Approvals**: Inspect incoming requests with 1-click **Approve** or **Reject** (with custom reason modal).
- **Financial & Occupancy Reports**: Detailed earnings statements and occupancy rates per parking facility.

---

### 3. Administrator Module
- **Global Control Center**: Platform-wide metrics, system diagnostics, and revenue tracking.
- **User Management**: View all registered users, toggle active/lockout status, and elevate/reassign roles (Driver, Owner, Admin).
- **System Diagnostics (`/Admin/SystemHealth`)**: Live status indicators for PostgreSQL database connectivity, SignalR hub status, and QRCoder engine.

---

### 4. Academic Payment Simulator
- Accessible during checkout at `/Payment/Simulate/{bookingId}`.
- Supports multiple simulated payment methods:
  - 💳 **Credit / Debit Card**
  - 📱 **Mobile Wallet (PayPal / Demo Pay)**
  - 🏦 **Net Banking**
  - 💵 **Pay on Arrival / Cash at Gate**
- Includes one-click demonstration triggers:
  - **Simulate Success (Approved)**: Generates unique transaction ID (e.g. `TXN-PE-20260921-XXXXXX`), marks booking as Confirmed, and unlocks the QR Pass.
  - **Simulate Failure (Declined)**: Generates decline response code `402` to showcase error-handling workflows.

---

### 5. SignalR Real-Time System
- Connects to `/parkingHub`.
- Broadcasts real-time slot occupancy updates and in-app notifications to connected browser clients without requiring page refreshes.

---

## 🧪 Testing & Verification Checklist

- [x] **Authentication**: Register new Driver & Owner accounts, Login, Logout, Remember Me.
- [x] **Role Authorization**: Access restrictions properly enforced on Owner and Admin routes.
- [x] **Search & Filters**: Text query, city filter, price slider, and amenities filters tested.
- [x] **Interactive Slot Layout**: Dynamic slot grid renders accurately with live color status.
- [x] **Double-Booking Prevention**: Concurrency locks prevent overlapping reservations.
- [x] **Payment Simulator**: Success & failure simulations generate accurate transaction records.
- [x] **Digital QR Pass**: QRCoder generates valid Base64 PNG passes with print styling.
- [x] **Cancellations & Refunds**: Cancellation updates slot availability and logs refund records.
- [x] **Notifications**: In-app notifications update badge count in real-time.
- [x] **Owner Dashboard**: Live metrics, slot CRUD, booking approval workflows verified.
- [x] **Admin Dashboard**: User status toggling, role editing, and diagnostics verified.
- [x] **PostgreSQL Integration**: EF Core migrations and relational schemas verified.

---

## 📄 License & Academic Attribution
Developed as part of the **University Software Development Project** curriculum.  
Built with **C# .NET 8**, **ASP.NET Core MVC**, **PostgreSQL (Npgsql)**, and **Bootstrap 5**.
