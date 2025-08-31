using DATN.DbContexts;
using DATN.Dtos.Vnpay;
using DATN.Libraries;
using DATN.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DATN.Services.Implements
{
    public class VnPayService : IVnPayService
    {
        private readonly IConfiguration _configuration;
        private readonly Data _context;
        public VnPayService(IConfiguration configuration, Data data)
        {
            _configuration = configuration;
            _context = data;
        }

        public string CreatePaymentUrl(PaymentInformationModel model, HttpContext context)
        {
            var timeZoneById = TimeZoneInfo.FindSystemTimeZoneById(_configuration["TimeZoneId"]);
            var timeNow = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, timeZoneById);
            var pay = new VnPayLibrary();
            var urlCallBack = _configuration["Vnpay:PaymentBackReturnUrl"];

            pay.AddRequestData("vnp_Version", _configuration["Vnpay:Version"]);
            pay.AddRequestData("vnp_Command", _configuration["Vnpay:Command"]);
            pay.AddRequestData("vnp_TmnCode", _configuration["Vnpay:TmnCode"]);
            pay.AddRequestData("vnp_Amount", ((int)model.Amount * 100).ToString());
            pay.AddRequestData("vnp_CreateDate", timeNow.ToString("yyyyMMddHHmmss"));
            pay.AddRequestData("vnp_ExpireDate", DateTime.UtcNow.AddHours(7).AddMinutes(15).ToString("yyyyMMddHHmmss"));
            pay.AddRequestData("vnp_CurrCode", _configuration["Vnpay:CurrCode"]);
            pay.AddRequestData("vnp_IpAddr", pay.GetIpAddress(context));
            pay.AddRequestData("vnp_Locale", _configuration["Vnpay:Locale"]);
            pay.AddRequestData("vnp_OrderInfo", $"{model.Name} {model.OrderDescription} {model.Amount}");
            pay.AddRequestData("vnp_OrderType", model.OrderType);
            pay.AddRequestData("vnp_ReturnUrl", urlCallBack);
            pay.AddRequestData("vnp_TxnRef", model.OrderId.ToString());

            var paymentUrl =
                pay.CreateRequestUrl(_configuration["Vnpay:BaseUrl"], _configuration["Vnpay:HashSecret"]);

            return paymentUrl;

        }
        public  async Task<PaymentResponseModel> PaymentExecute(IQueryCollection collections)
        {
            var pay = new VnPayLibrary();
            var response = pay.GetFullResponseData(collections, _configuration["Vnpay:HashSecret"]);
            var orderid = int.Parse(response.OrderId);
            var order =await _context.Orders.Include(o => o.Payments).FirstOrDefaultAsync(o => o.OrderId == orderid);
            if (order == null)
                return null;
            var payment = order.Payments.FirstOrDefault();
            bool isSuccess = response.VnPayResponseCode == "00";
            if (isSuccess)
            {
                if (payment != null)
                {
                    payment.PaymentStatus = "paid";
                    payment.PaymentDate = DateTime.Now;
                    payment.PaymentMethod = "VNPAY";
                }
                order.Status = "Processing";
            }
            else
            {
                if (payment != null)
                {
                    payment.PaymentStatus = "Fail";
                    payment.PaymentDate = DateTime.Now;
                    payment.PaymentMethod = "VNPAY";
                }
                order.Status = "Cancelled";
            }
            await _context.SaveChangesAsync();
            response.Success=isSuccess;
            return response;
        }

    }
}
