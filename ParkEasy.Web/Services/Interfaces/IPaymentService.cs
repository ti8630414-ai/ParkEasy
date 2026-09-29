using System.Collections.Generic;
using System.Threading.Tasks;
using ParkEasy.Web.Models.Entities;
using ParkEasy.Web.Models.ViewModels;

namespace ParkEasy.Web.Services.Interfaces
{
    public interface IPaymentService
    {
        Task<PaymentResultViewModel> ProcessPaymentSimulationAsync(PaymentSimulatorViewModel model, string currentUserId);
        Task<Payment?> GetPaymentByBookingIdAsync(int bookingId);
        Task<List<Payment>> GetAllPaymentsAsync();
        Task<List<Refund>> GetAllRefundsAsync();
        Task<bool> ProcessRefundAsync(RefundProcessViewModel model, string currentUserId, bool isAdmin = false);
    }
}
