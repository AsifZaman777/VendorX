namespace VendorX.Services
{
    public interface IEmailService
    {
        Task<bool> SendEmailAsync(string toEmail, string subject, string message);
    }

    public interface IWhatsAppService
    {
        Task<bool> SendWhatsAppMessageAsync(string phoneNumber, string message);
    }

    public interface IQRCodeService
    {
        string GenerateQRCode(string content);
    }
}
