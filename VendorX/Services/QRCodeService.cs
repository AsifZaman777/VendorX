using QRCoder;

namespace VendorX.Services
{
    public class QRCodeService : IQRCodeService
    {
        public string GenerateQRCode(string content)
        {
            try
            {
                using var qrGenerator = new QRCodeGenerator();
                var qrCodeData = qrGenerator.CreateQrCode(content, QRCodeGenerator.ECCLevel.Q);
                using var qrCode = new PngByteQRCode(qrCodeData);
                var qrCodeBytes = qrCode.GetGraphic(20);
                
                // Convert to base64 string
                var base64String = Convert.ToBase64String(qrCodeBytes);
                return $"data:image/png;base64,{base64String}";
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
