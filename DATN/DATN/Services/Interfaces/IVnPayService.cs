using DATN.Dtos.Vnpay;

namespace DATN.Services.Interfaces
{
    public interface IVnPayService
    {
        string CreatePaymentUrl(PaymentInformationModel model, HttpContext context);
        //Task<PaymentResponseModel> PaymentExecute(IQueryCollection collections);
        Task<PaymentResponseModel> PaymentExecute(IQueryCollection collections);

    }
}
