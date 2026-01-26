using SportBookingSystem.Helper;
using SportBookingSystem.Models.ViewModels;

namespace SportBookingSystem.Services
{
    public class VnPayService : IVnPayService
    {
        private readonly IConfiguration _configuration;

        public VnPayService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string CreatePaymentUrl(decimal amount, string txnRef, HttpContext context)
        {
            var vnpay = new VnPayLibrary();
            var vnpUrl = _configuration["VnPay:BaseUrl"];
            var vnpTmnCode = _configuration["VnPay:TmnCode"];
            var vnpHashSecret = _configuration["VnPay:HashSecret"];
            var vnpReturnUrl = _configuration["VnPay:ReturnUrl"];

            var vnpAmount = ((long)(amount * 100)).ToString();

            vnpay.AddRequestData("vnp_Version", "2.1.0");
            vnpay.AddRequestData("vnp_Command", "pay");
            vnpay.AddRequestData("vnp_TmnCode", vnpTmnCode);
            vnpay.AddRequestData("vnp_Amount", vnpAmount);
            vnpay.AddRequestData("vnp_CreateDate", DateTime.Now.ToString("yyyyMMddHHmmss"));
            vnpay.AddRequestData("vnp_ExpireDate", DateTime.Now.AddMinutes(15).ToString("yyyyMMddHHmmss"));
            vnpay.AddRequestData("vnp_CurrCode", "VND");
            vnpay.AddRequestData("vnp_IpAddr", GetIpAddress(context));
            vnpay.AddRequestData("vnp_Locale", "vn");

            vnpay.AddRequestData("vnp_OrderInfo", $"Naptienvi{txnRef}");

            vnpay.AddRequestData("vnp_OrderType", "topup");
            vnpay.AddRequestData("vnp_ReturnUrl", vnpReturnUrl);
            vnpay.AddRequestData("vnp_TxnRef", txnRef);

            return vnpay.CreateRequestUrl(vnpUrl, vnpHashSecret);
        }

        public VnPayResponseModel ProcessVnPayReturn(IQueryCollection queryParams)
        {
            var vnpay = new VnPayLibrary();
            var vnpHashSecret = _configuration["VnPay:HashSecret"];

            foreach (var (key, value) in queryParams)
            {
                if (!string.IsNullOrEmpty(key) && key.StartsWith("vnp_") && key != "vnp_SecureHash")
                {
                    vnpay.AddResponseData(key, value.ToString());
                }
            }

            var vnpSecureHash = queryParams["vnp_SecureHash"].ToString();
            var isValidSignature = vnpay.ValidateSignature(vnpSecureHash, vnpHashSecret);

            return new VnPayResponseModel
            {
                Success = isValidSignature && vnpay.GetResponseData("vnp_ResponseCode") == "00",
                TransactionCode = vnpay.GetResponseData("vnp_TxnRef"),
                Amount = decimal.Parse(vnpay.GetResponseData("vnp_Amount")) / 100,
                BankCode = vnpay.GetResponseData("vnp_BankCode"),
                BankTransactionNo = vnpay.GetResponseData("vnp_BankTranNo"),
                CardType = vnpay.GetResponseData("vnp_CardType"),
                OrderInfo = vnpay.GetResponseData("vnp_OrderInfo"),
                PayDate = vnpay.GetResponseData("vnp_PayDate"),
                ResponseCode = vnpay.GetResponseData("vnp_ResponseCode"),
                TransactionNo = vnpay.GetResponseData("vnp_TransactionNo"),
                IsValidSignature = isValidSignature
            };
        }

        private string GetIpAddress(HttpContext context)
        {
            var ipAddress = context.Connection.RemoteIpAddress?.ToString();

            if (string.IsNullOrEmpty(ipAddress) || ipAddress == "::1")
            {
                ipAddress = "127.0.0.1";
            }

            return ipAddress;
        }
    }
}