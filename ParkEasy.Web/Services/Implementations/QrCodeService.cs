using System;
using ParkEasy.Web.Services.Interfaces;
using QRCoder;

namespace ParkEasy.Web.Services.Implementations
{
    public class QrCodeService : IQrCodeService
    {
        public string GenerateQrCodeBase64(string payload)
        {
            if (string.IsNullOrWhiteSpace(payload))
                return string.Empty;

            try
            {
                using var qrGenerator = new QRCodeGenerator();
                using var qrCodeData = qrGenerator.CreateQrCode(payload, QRCodeGenerator.ECCLevel.Q);
                using var qrCode = new PngByteQRCode(qrCodeData);
                byte[] qrCodeBytes = qrCode.GetGraphic(10);
                return $"data:image/png;base64,{Convert.ToBase64String(qrCodeBytes)}";
            }
            catch
            {
                return string.Empty;
            }
        }
    }
}
