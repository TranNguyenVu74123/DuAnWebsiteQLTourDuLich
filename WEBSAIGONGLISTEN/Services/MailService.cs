//using Mailjet.Client;
//using Mailjet.Client.Resources;
//using Newtonsoft.Json.Linq;
//using System.Threading.Tasks;
//using Microsoft.Extensions.Logging;

//namespace WEBSAIGONGLISTEN.Services
//{
//    public class MailService : IMailService // Đảm bảo bạn thực hiện interface IMailService
//    {
//        private readonly MailjetClient _client;
//        private readonly ILogger<MailService> _logger; // Để ghi log

//        public MailService(string apiKey, string apiSecret, ILogger<MailService> logger)
//        {
//            if (string.IsNullOrEmpty(apiKey)) throw new ArgumentNullException(nameof(apiKey), "API Key không thể null.");
//            if (string.IsNullOrEmpty(apiSecret)) throw new ArgumentNullException(nameof(apiSecret), "API Secret không thể null.");

//            _client = new MailjetClient(apiKey, apiSecret);
//            _logger = logger;
//        }

//        public async Task<bool> SendPaymentConfirmationEmail(string toEmail, string orderId, double totalPrice)
//        {
//            var request = new MailjetRequest
//            {
//                Resource = Send.Resource
//            }
//            .Property(Send.Messages, new JArray
//            {
//                new JObject
//                {
//                    { "From", new JObject { { "Email", "trannguyenvu774411@gmail.com" }, { "Name", "TranNguyenVu" } } },
//                    { "To", new JArray { new JObject { { "Email", toEmail } } } },
//                    { "Subject", "Thanh toán thành công" },
//                    { "TextPart", $"Cảm ơn bạn đã thanh toán đơn hàng {orderId} với tổng số tiền {totalPrice}." },
//                    { "HTMLPart", $"<h3>Cảm ơn bạn đã thanh toán đơn hàng {orderId}</h3><p>Tổng số tiền: <strong>{totalPrice}</strong></p>" }
//                }
//            });

//            var response = await _client.PostAsync(request);

//            // Kiểm tra phản hồi
//            if (response.IsSuccessStatusCode)
//            {
//                _logger.LogInformation("Email đã được gửi thành công đến {ToEmail}", toEmail);
//                return true;
//            }
//            else
//            {
//                // Xử lý lỗi
//                _logger.LogError("Gửi email thất bại: {StatusCode}, {ResponseContent}", response.StatusCode, response.Content);
//                return false;
//            }
//        }
//    }
//}
