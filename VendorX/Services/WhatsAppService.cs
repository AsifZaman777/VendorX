namespace VendorX.Services
{
    public class WhatsAppService : IWhatsAppService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<WhatsAppService> _logger;

        public WhatsAppService(IConfiguration configuration, ILogger<WhatsAppService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<bool> SendWhatsAppMessageAsync(string phoneNumber, string message)
        {
            try
            {
                // Format phone number - remove spaces, dashes, and plus signs
                phoneNumber = phoneNumber?.Replace(" ", "")
                                         .Replace("-", "")
                                         .Replace("+", "")
                                         .Trim();

                // Add Bangladesh country code (88) if not already present
                if (!phoneNumber.StartsWith("88"))
                {
                    phoneNumber = "88" + phoneNumber;
                }

                // URL encode the message
                var encodedMessage = Uri.EscapeDataString(message);

                // Generate WhatsApp link
                var whatsAppUrl = $"https://wa.me/{phoneNumber}?text={encodedMessage}";

                // Log the WhatsApp link for reference
                _logger.LogInformation($"?? WhatsApp link generated for {phoneNumber}");
                _logger.LogInformation($"?? Link: {whatsAppUrl}");
                _logger.LogInformation($"?? Message: {message}");

                // Note: This returns true to indicate the link was generated successfully
                // The actual sending happens when user clicks the link or we trigger it programmatically
                return await Task.FromResult(true);
            }
            catch (Exception ex)
            {
                _logger.LogError($"? Error generating WhatsApp link: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Generates a WhatsApp web link that can be used to send messages
        /// </summary>
        public string GenerateWhatsAppLink(string phoneNumber, string message)
        {
            // Format phone number
            phoneNumber = phoneNumber?.Replace(" ", "")
                                     .Replace("-", "")
                                     .Replace("+", "")
                                     .Trim();

            // Add Bangladesh country code (88) if not already present
            if (!phoneNumber.StartsWith("88"))
            {
                phoneNumber = "88" + phoneNumber;
            }

            // URL encode the message
            var encodedMessage = Uri.EscapeDataString(message);

            // Return the WhatsApp link
            return $"https://wa.me/{phoneNumber}?text={encodedMessage}";
        }
    }
}
