using System.Threading.Tasks;

namespace ParkEasy.Web.Services.Interfaces
{
    public interface IEmailSimulationService
    {
        Task SendEmailAsync(string recipientEmail, string recipientName, string subject, string htmlBody);
    }
}
