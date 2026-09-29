namespace ParkEasy.Web.Models.Enums
{
    using System.ComponentModel.DataAnnotations;
    public static class UserRoles
    {
        public const string Admin = "Admin";
        public const string Owner = "Owner";
        public const string User = "User";
    }

    public enum VehicleType
    {
        Car = 1,
        Motorcycle = 2,
        SUV = 3,
        Van = 4,
        ElectricVehicle = 5,
        Truck = 6
    }

    public enum SlotStatus
    {
        Available = 1,
        Occupied = 2,
        Reserved = 3,
        Maintenance = 4
    }

    public enum BookingStatus
    {
        Requested = 1,
        Confirmed = 2,
        Active = 3,
        Completed = 4,
        Cancelled = 5,
        Rejected = 6,
        Expired = 7
    }

    public enum PaymentMethod
    {
        [Display(Name = "Credit Card")]
        CreditCard = 1,
        [Display(Name = "Debit Card")]
        DebitCard = 2,
        [Display(Name = "Mobile Wallet")]
        MobileWallet = 3,
        [Display(Name = "Net Banking")]
        NetBanking = 4,
        [Display(Name = "Pay at Gate")]
        CashOnArrival = 5,
        [Display(Name = "bKash")]
        Bkash = 6,
        [Display(Name = "Nagad")]
        Nagad = 7,
        [Display(Name = "Rocket")]
        Rocket = 8,
        [Display(Name = "Bank Transfer")]
        BankTransfer = 10
    }

    public enum PaymentStatus
    {
        Pending = 1,
        Paid = 2,
        Failed = 3,
        Refunded = 4
    }

    public enum RefundStatus
    {
        Pending = 1,
        Approved = 2,
        Processed = 3,
        Rejected = 4
    }

    public enum NotificationType
    {
        BookingCreated = 1,
        BookingApproved = 2,
        BookingRejected = 3,
        PaymentSuccess = 4,
        PaymentFailed = 5,
        BookingCancelled = 6,
        RefundIssued = 7,
        Reminder = 8,
        ExpiryAlert = 9,
        SystemAlert = 10
    }
}
