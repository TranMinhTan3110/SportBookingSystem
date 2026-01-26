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
            var vnpHashSecret = _configuration["VnPay:HashSecret"]?.Trim();
            var vnpReturnUrl = _configuration["VnPay:ReturnUrl"];

            var vnpAmount = ((long)(amount * 100)).ToString();

            vnpay.AddRequestData("vnp_Version", "2.1.0");
            vnpay.AddRequestData("vnp_Command", "pay");
            vnpay.AddRequestData("vnp_TmnCode", vnpTmnCode);
            vnpay.AddRequestData("vnp_Amount", vnpAmount);

            var createDate = DateTime.Now.ToString("yyyyMMddHHmmss");
            var expireDate = DateTime.Now.AddMinutes(15).ToString("yyyyMMddHHmmss");

            vnpay.AddRequestData("vnp_CreateDate", createDate);
            vnpay.AddRequestData("vnp_ExpireDate", expireDate);
            vnpay.AddRequestData("vnp_CurrCode", "VND");

            var ipAddr = GetIpAddress(context);
            vnpay.AddRequestData("vnp_IpAddr", ipAddr);

            vnpay.AddRequestData("vnp_Locale", "vn");
            vnpay.AddRequestData("vnp_OrderInfo", $"Nap tien vi {txnRef}");
            vnpay.AddRequestData("vnp_OrderType", "topup");
            vnpay.AddRequestData("vnp_ReturnUrl", vnpReturnUrl);
            vnpay.AddRequestData("vnp_TxnRef", txnRef);

            var paymentUrl = vnpay.CreateRequestUrl(vnpUrl, vnpHashSecret);

            return paymentUrl;
        }

        public VnPayResponseModel ProcessVnPayReturn(IQueryCollection queryParams)
        {
            var vnpay = new VnPayLibrary();
            var vnpHashSecret = _configuration["VnPay:HashSecret"];

            foreach (var (key, value) in queryParams)
            {
                if (!string.IsNullOrEmpty(key) && key.StartsWith("vnp_") && key != "vnp_SecureHash")
                {
                    var encodedValue = System.Net.WebUtility.UrlEncode(value.ToString());
                    vnpay.AddResponseData(key, encodedValue);
                }
            }

            var vnpSecureHash = queryParams["vnp_SecureHash"].ToString();
            var isValidSignature = vnpay.ValidateSignature(vnpSecureHash, vnpHashSecret);

            var responseCode = vnpay.GetResponseData("vnp_ResponseCode");

            var result = new VnPayResponseModel
            {
                Success = isValidSignature && responseCode == "00",
                TransactionCode = vnpay.GetResponseData("vnp_TxnRef"),
                Amount = decimal.Parse(vnpay.GetResponseData("vnp_Amount")) / 100,
                BankCode = vnpay.GetResponseData("vnp_BankCode"),
                BankTransactionNo = vnpay.GetResponseData("vnp_BankTranNo"),
                CardType = vnpay.GetResponseData("vnp_CardType"),
                OrderInfo = vnpay.GetResponseData("vnp_OrderInfo"),
                PayDate = vnpay.GetResponseData("vnp_PayDate"),
                ResponseCode = responseCode,
                TransactionNo = vnpay.GetResponseData("vnp_TransactionNo"),
                IsValidSignature = isValidSignature
            };

            return result;
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