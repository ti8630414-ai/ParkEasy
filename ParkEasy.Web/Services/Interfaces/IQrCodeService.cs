namespace ParkEasy.Web.Services.Interfaces
{
    public interface IQrCodeService
    {
        string GenerateQrCodeBase64(string payload);
    }
}
