using WEBSAIGONGLISTEN.Models.Momo;
using WEBSAIGONGLISTEN.Models;

namespace WEBSAIGONGLISTEN.Services;

public interface IMomoService
{
    Task<MomoCreatePaymentResponseModel> CreatePaymentAsync(Transaction model);
    MomoExecuteResponseModel PaymentExecuteAsync(IQueryCollection collection);
}