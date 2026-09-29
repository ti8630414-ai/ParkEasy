# ParkEasy – কাজ বণ্টন পরিকল্পনা (২ জন টিম মেম্বার)

---

## 📋 সংক্ষিপ্ত বিবরণ

| | **মেম্বার ১ – ইউজার/ড্রাইভার মডিউল** | **মেম্বার ২ – অ্যাডমিন/ওনার মডিউল + ব্যাকএন্ড** |
|---|---|---|
| **ভূমিকা** | ব্যবহারকারী-মুখী ফিচার, সার্চ, বুকিং, ফ্রন্টএন্ড | ম্যানেজমেন্ট ড্যাশবোর্ড, পেমেন্ট, সার্ভিস, ডেটাবেস |
| **প্রধান ফোকাস** | ড্রাইভাররা যা দেখবে ও ব্যবহার করবে | অ্যাডমিন ও ওনাররা যা দেখবে + ব্যাকএন্ড লজিক |

---

## 👤 মেম্বার ১ – ইউজার/ড্রাইভার মডিউল (ফ্রন্টএন্ড + ইউজার ফ্লো)

### 1. HomeController + Views
- `Controllers/HomeController.cs`
- `Views/Home/Index.cshtml` (ল্যান্ডিং পেজ)
- `Views/Home/About.cshtml`
- `Views/Home/Contact.cshtml`
- `Views/Home/Privacy.cshtml`

### 2. AccountController + Views (অথেনটিকেশন)
- `Controllers/AccountController.cs`
- `Views/Account/Login.cshtml`
- `Views/Account/Register.cshtml`
- `Views/Account/Profile.cshtml`
- `Views/Account/ChangePassword.cshtml`
- Quick-fill বাটন (Admin/Owner/Driver)

### 3. ParkingController + Views (সার্চ ও স্লট লেআউট)
- `Controllers/ParkingController.cs`
- `Views/Parking/Index.cshtml` (সার্চ পেজ – সিটি, কীওয়ার্ড, প্রাইস স্লাইডার, অ্যামেনিটি ফিল্টার)
- `Views/Parking/Details.cshtml` (পার্কিং স্পেস ডিটেইল + ইন্টারঅ্যাক্টিভ স্লট গ্রিড)
- রিয়েল-টাইম স্লট স্ট্যাটাস (🟢🔵🔴🟡)
- ভেহিকল টাইপ ফিল্টার

### 4. BookingController + Views (বুকিং ফ্লো)
- `Controllers/BookingController.cs`
- `Views/Booking/Checkout.cshtml` (স্লল লকিং + চেকআউট)
- `Views/Booking/History.cshtml` (Upcoming / Active / Completed / Cancelled ট্যাব)
- `Views/Booking/Details.cshtml` (বুকিং ডিটেইল + QR পাস)
- QR পাস ডাউনলোড/প্রিন্ট

### 5. NotificationController + Views
- `Controllers/NotificationController.cs`
- `Views/Notification/Index.cshtml`
- ব্যাজ কাউন্ট আপডেট
- Read/Delete ফিচার

### 6. Shared Views + ফ্রন্টএন্ড অ্যাসেট
- `Views/Shared/_Layout.cshtml`
- `Views/Shared/_LoginPartial.cshtml`
- `Views/Shared/_NotificationBell.cshtml`
- `wwwroot/css/` (কাস্টম CSS)
- `wwwroot/js/` (স্লট গ্রিড, ফিল্টার, ইন্টারঅ্যাকশন)
- `wwwroot/lib/` (Bootstrap, Bootstrap Icons, Leaflet)

---

## 👤 মেম্বার ২ – অ্যাডমিন/ওনার মডিউল + ব্যাকএন্ড ইনফ্রাস্ট্রাকচার

### 1. AdminController + Views
- `Controllers/AdminController.cs`
- `Views/Admin/Dashboard.cshtml` (গ্লোবাল KPI, সিস্টেম ডায়াগনস্টিক্স)
- `Views/Admin/Users.cshtml` (ইউজার ম্যানেজমেন্ট – লকআউট, রোল এডিট)
- `Views/Admin/SystemHealth.cshtml` (PostgreSQL, SignalR, QRCoder স্ট্যাটাস)

### 2. OwnerController + Views
- `Controllers/OwnerController.cs`
- `Views/Owner/Dashboard.scss` (KPI – মোট স্পেস, স্লট, অকুপেন্সি %, আজকের আয়, মোট রেভেনিউ)
- `Views/Owner/Spaces.cshtml` (পার্কিং স্পেস CRUD + ম্যাপ পিকার)
- `Views/Owner/Slots.cshtml` (স্লট ক্রিয়েট/এডিট/মেইনটেন্যান্স টগল/ডিলিট)
- `Views/Owner/Requests.scss` (বুকিং রিকোয়েস্ট – Approve/Reject)
- `Views/Owner/Earnings.scss` (আর্থিক রিপোর্ট)

### 3. PaymentController + Views
- `Controllers/PaymentController.cs`
- `Views/Payment/Simulate.cshtml` (পেমেন্ট সিমুলেটর – কার্ড, ওয়ালেট, নেট ব্যাংকিং, ক্যাশ)
- `Views/Payment/Receipt.scss` (রসিদ)
- Success/Failure সিমুলেশন ট্রিগার

### 4. Services (সব বিজনেস লজিক)
- `Services/Interfaces/IBookingService.cs`
- `Services/Interfaces/IParkingService.cs`
- `Services/Interfaces/IPaymentService.cs`
- `Services/Interfaces/INotificationService.cs`
- `Services/Interfaces/IQrCodeService.cs`
- `Services/Implementations/BookingService.cs` (কনকারেন্সি চেক, স্লট লকিং)
- `Services/Implementations/ParkingService.cs` (Haversine স্পাটিয়াল সার্চ)
- `Services/Implementations/PaymentService.cs` (সিমুলেটর + রিফান্ড)
- `Services/Implementations/NotificationService.cs` (SignalR ডিসপ্যাচ)
- `Services/Implementations/QrCodeService.cs` (QR জেনারেটর)
- `Services/Implementations/EmailSimulationService.cs`

### 5. Hubs (SignalR রিয়েল-টাইম)
- `Hubs/ParkingHub.cs` (স্লট আপডেট ব্রডকাস্ট)

### 6. Data Layer + Models
- `Data/ApplicationDbContext.cs` (EF Core + PostgreSQL ফ্লুয়েন্ট কনফিগ)
- `Data/DbInitializer.cs` (সিড ডেটা – Admin, Owner, Driver, Spaces, Slots)
- `Models/Entities/ApplicationUser.cs`
- `Models/Entities/ParkingSpace.cs`
- `Models/Entities/ParkingSlot.cs`
- `Models/Entities/Booking.cs`
- `Models/Entities/Payment.cs`
- `Models/Entities/Refund.cs`
- `Models/Entities/Notification.cs`
- `Models/Enums/Enums.cs`
- `Models/ViewModels/` (সব DTO)
- `Migrations/` (EF Core মাইগ্রেশন)

---

## 🔗 যেসব জায়গায় ২ জনকে একসাথে কাজ করতে হবে (ইন্টিগ্রেশন পয়েন্ট)

| ইন্টিগ্রেশন | মেম্বার ১ | মেম্বার ২ |
|---|---|---|
| **SignalR স্লট আপডেট** | স্লট গ্রিডে রিয়েল-টাইম রেন্ডার | `ParkingHub.cs` ব্রডকাস্ট লজিক |
| **বুকিং → পেমেন্ট ফ্লো** | চেকআউট পেজ + QR পাস ভিউ | `PaymentService.cs` + `BookingService.cs` |
| **নোটিফিকেশন ব্যাজ** | ব্যাজ UI + পপওভার | `NotificationService.cs` + SignalR ডিসপ্যাচ |
| **অথ → রোল অ্যাক্সেস** | লগইন/রেজিস্টার ভিউ | RBAC কন্ট্রোলার অ্যাট্রিবিউট |
| **ডেটাবেস মাইগ্রেশন** | মডেল চেঞ্জ নোটিফাই | `Update-Database` চালানো |

---

## 📅 প্রস্তাবিত কাজের ক্রম

### ধাপ ১ – সেটআপ (দুজনই)
- [ ] PostgreSQL ইনস্টল ও কানেকশন স্ট্রিং কনফিগার
- [ ] `dotnet restore` + `dotnet build`
- [ ] EF Core মাইগ্রেশন তৈরি (`Add-Migration InitialCreate`)
- [ ] সিড ডেটা ভেরিফাই

### ধাপ ২ – মেম্বার ১ (ইউজার সাইড)
- [ ] HomeController + ল্যান্ডিং পেজ
- [ ] AccountController + লগইন/রেজিস্টার
- [ ] ParkingController + সার্চ/ফিল্টার
- [ ] BookingController + বুকিং ফ্লো
- [ ] NotificationController + ব্যাজ UI

### ধাপ ২ – মেম্বার ২ (অ্যাডমিন/ওনার সাইড + ব্যাকএন্ড)
- [ ] ApplicationDbContext + মাইগ্রেশন
- [ ] সব Service ইন্টারফেস + ইমপ্লিমেন্টেশন
- [ ] ParkingHub (SignalR)
- [ ] AdminController + ড্যাশবোর্ড
- [ ] OwnerController + স্পেস/স্লট/রিকোয়েস্ট
- [ ] PaymentController + সিমুলেটর

### ধাপ ৩ – ইন্টিগ্রেশন (দুজনই একসাথে)
- [ ] SignalR স্লট আপডেট টেস্ট
- [ ] বুকিং → পেমেন্ট → QR পাস ফুল ফ্লো টেস্ট
- [ ] নোটিফিকেশন রিয়েল-টাইম টেস্ট
- [ ] RBAC অ্যাক্সেস কন্ট্রোল টেস্ট

### ধাপ ৪ – ফাইনাল টেস্টিং (দুজনই)
- [ ] ডাবল-বুকিং প্রভেনশন টেস্ট
- [ ] পেমেন্ট সাকসেস/ফেইলার টেস্ট
- [ ] QR কোড জেনারেশন টেস্ট
- [ ] সব ব্রাউজারে রেসপন্সিভ চেক

---

## 📁 ফাইল ম্যাপিং সারসংক্ষেপ

```
ParkEasy.Web/
├── Controllers/
│   ├── HomeController.cs          → মেম্বার ১
│   ├── AccountController.cs      → মেম্বার ১
│   ├── ParkingController.cs      → মেম্বার ১
│   ├── BookingController.cs      → মেম্বার ১
│   ├── NotificationController.cs  → মেম্বার ১
│   ├── AdminController.cs        → মেম্বার ২
│   ├── OwnerController.cs        → মেম্বার ২
│   └── PaymentController.cs      → মেম্বার ২
├── Views/
│   ├── Home/                      → মেম্বার ১
│   ├── Account/                   → মেম্বার ১
│   ├── Parking/                   → মেম্বার ১
│   ├── Booking/                   → মেম্বার ১
│   ├── Notification/              → মেম্বার ১
│   ├── Admin/                     → মেম্বার ২
│   ├── Owner/                     → মেম্বার ২
│   ├── Payment/                   → মেম্বার ২
│   └── Shared/                    → মেম্বার ১ (প্রাইমারি)
├── Services/                      → মেম্বার ২
├── Hubs/                          → মেম্বার ২
├── Data/                          → মেম্বার ২
├── Models/                        → মেম্বার ২ (Entities) + মেম্বার ১ (ViewModels)
├── Migrations/                    → মেম্বার ২
└── wwwroot/                       → মেম্বার ১
```

---

> 💡 **টিপ:** মেম্বার ১ ফ্রন্টএন্ড/UI-এ ফোকাস করবেন, মেম্বার ২ ব্যাকএন্ড/লজিক/ডেটাবেস-এ ফোকাস করবেন। ইন্টিগ্রেশন পয়েন্টগুলোতে (SignalR, বুকিং→পেমেন্ট ফ্লো) দুজনকে একসাথে কাজ করতে হবে।
