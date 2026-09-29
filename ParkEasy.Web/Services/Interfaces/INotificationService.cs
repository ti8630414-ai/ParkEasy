using System.Collections.Generic;
using System.Threading.Tasks;
using ParkEasy.Web.Models.Entities;
using ParkEasy.Web.Models.Enums;

namespace ParkEasy.Web.Services.Interfaces
{
    public interface INotificationService
    {
        Task<Notification> CreateNotificationAsync(string userId, string title, string message, NotificationType type, string? actionUrl = null);
        Task<List<Notification>> GetUserNotificationsAsync(string userId, int count = 20);
        Task<int> GetUnreadCountAsync(string userId);
        Task<bool> MarkAsReadAsync(int notificationId, string userId);
        Task<bool> MarkAllAsReadAsync(string userId);
        Task<bool> DeleteNotificationAsync(int notificationId, string userId);
    }
}
