using sib_api_v3_sdk.Api;
using sib_api_v3_sdk.Model;
using WEBSAIGONGLISTEN.Setting;

namespace WEBSAIGONGLISTEN.Services
{
    public class EmailService : IEmailService
    {
        private readonly string _senderEmail;
        private readonly string _senderName;
        private readonly string _key;

        public EmailService(IConfiguration configuration)
        {
            var config = configuration.GetSection("EmailBrevo");

            _senderEmail = config["Sender:Email"];
            _senderName = config["Sender:Name"]; // Đã sửa lại ở đây
            _key = config["Key"] ?? string.Empty;

            if (!sib_api_v3_sdk.Client.Configuration.Default.ApiKey.ContainsKey("api-key"))
            {
                sib_api_v3_sdk.Client.Configuration.Default.ApiKey.Add("api-key", _key);
            }
        }

        public async Task<bool> SendEmailAsync(EmailSetting emailSetting)
        {
            try
            {
                var transactionalEmailsApi = new TransactionalEmailsApi();

                var sender = new SendSmtpEmailSender(_senderName, _senderEmail);
                var recipients = new List<SendSmtpEmailTo>
                {
                    new SendSmtpEmailTo(emailSetting.To, emailSetting.Name)
                };

                string body = emailSetting.Content;

                // Xử lý CC
                List<SendSmtpEmailCc> ccList = emailSetting.CC.Select(cc => new SendSmtpEmailCc { Email = cc }).ToList();

                // Xử lý các tệp đính kèm
                List<SendSmtpEmailAttachment> attachments = emailSetting.AttachmentFiles.Select(path => new SendSmtpEmailAttachment { Url = path }).ToList();

                var email = new SendSmtpEmail(sender, recipients, null, ccList, body, null, emailSetting.Subject, null, attachments);

                // Gửi email
                CreateSmtpEmail result = await transactionalEmailsApi.SendTransacEmailAsync(email);

                return result != null;
            }
            catch (Exception ex)
            {
                // Ghi log hoặc xử lý lỗi ở đây nếu cần
                Console.WriteLine($"Error sending email: {ex.Message}");
                return false;
            }
        }
    }
}
