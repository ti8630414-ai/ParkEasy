using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using ParkEasy.Web.Services.Interfaces;

namespace ParkEasy.Web.Services.Implementations
{
    public class EmailSimulationService : IEmailSimulationService
    {
        private readonly ILogger<EmailSimulationService> _logger;

        public EmailSimulationService(ILogger<EmailSimulationService> logger)
        {
            _logger = logger;
        }

        public Task SendEmailAsync(string recipientEmail, string recipientName, string subject, string htmlBody)
        {
            _logger.LogInformation(
                "\n==================== [PARKEASY EMAIL SIMULATOR] ====================\n" +
                "TO: {RecipientName} <{RecipientEmail}>\n" +
                "SUBJECT: {Subject}\n" +
                "TIMESTAMP: {Timestamp}\n" +
                "BODY:\n{Body}\n" +
                "====================================================================",
                recipientName, recipientEmail, subject, DateTime.UtcNow, htmlBody);

            return Task.CompletedTask;
        }
    }
}
