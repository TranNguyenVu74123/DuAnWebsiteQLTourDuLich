using WEBSAIGONGLISTEN.Setting;

namespace WEBSAIGONGLISTEN.Services
{
    public interface IEmailService
    {
        Task<bool> SendEmailAsync(EmailSetting emailSetting);
    }
}
