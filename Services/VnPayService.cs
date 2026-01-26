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
            // ===== ENHANCED DEBUG LOGGING =====
            Console.WriteLine("\n╔══════════════════════════════════════════════════════════╗");
            Console.WriteLine("║           VNPAY PAYMENT URL CREATION - DEBUG             ║");
            Console.WriteLine("╚══════════════════════════════════════════════════════════╝");

            var vnpay = new VnPayLibrary();

            // Read config
            var vnpUrl = _configuration["VnPay:BaseUrl"];
            var vnpTmnCode = _configuration["VnPay:TmnCode"];
            var vnpHashSecret = _configuration["VnPay:HashSecret"]?.Trim();
            var vnpReturnUrl = _configuration["VnPay:ReturnUrl"];

            // Debug config
            Console.WriteLine("\n📋 CONFIGURATION:");
            Console.WriteLine($"   BaseUrl: {vnpUrl}");
            Console.WriteLine($"   TmnCode: {vnpTmnCode}");
            Console.WriteLine($"   TmnCode Length: {vnpTmnCode?.Length ?? 0}");
            Console.WriteLine($"   HashSecret: {(string.IsNullOrEmpty(vnpHashSecret) ? "❌ EMPTY!" : $"✅ Set ({vnpHashSecret.Length} chars)")}");

            if (!string.IsNullOrEmpty(vnpHashSecret))
            {
                Console.WriteLine($"   HashSecret Preview: {vnpHashSecret.Substring(0, Math.Min(5, vnpHashSecret.Length))}...{vnpHashSecret.Substring(Math.Max(0, vnpHashSecret.Length - 5))}");

                // Check for whitespace
                if (vnpHashSecret != vnpHashSecret.Trim())
                {
                    Console.WriteLine("   ⚠️  WARNING: HashSecret has leading/trailing whitespace!");
                    Console.WriteLine($"   Before trim: [{vnpHashSecret}]");
                    Console.WriteLine($"   After trim: [{vnpHashSecret.Trim()}]");
                }
            }

            Console.WriteLine($"   ReturnUrl: {vnpReturnUrl}");

            // Prepare amount
            var vnpAmount = ((long)(amount * 100)).ToString();

            Console.WriteLine("\n💰 PAYMENT INFO:");
            Console.WriteLine($"   Original Amount: {amount}");
            Console.WriteLine($"   VNPay Amount (x100): {vnpAmount}");
            Console.WriteLine($"   TxnRef: {txnRef}");

            // Add request data
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

            Console.WriteLine($"   CreateDate: {createDate}");
            Console.WriteLine($"   ExpireDate: {expireDate}");
            Console.WriteLine($"   IpAddr: {ipAddr}");

            Console.WriteLine("\n🔗 Creating payment URL...");

            // Create URL - VnPayLibrary will print raw data and hash
            var paymentUrl = vnpay.CreateRequestUrl(vnpUrl, vnpHashSecret);

            Console.WriteLine($"\n✅ FINAL URL LENGTH: {paymentUrl.Length} characters");
            Console.WriteLine($"   URL Preview: {paymentUrl.Substring(0, Math.Min(100, paymentUrl.Length))}...");
            Console.WriteLine("╚══════════════════════════════════════════════════════════╝\n");

            return paymentUrl;
        }

        public VnPayResponseModel ProcessVnPayReturn(IQueryCollection queryParams)
        {
            Console.WriteLine("\n========== BẮT ĐẦU XỬ LÝ RETURN TỪ VNPAY ==========");

            var vnpay = new VnPayLibrary();
            var vnpHashSecret = _configuration["VnPay:HashSecret"]?.Trim();

            // In ra HashSecret để check
            Console.WriteLine($"HashSecret Length: {vnpHashSecret?.Length}");
            Console.WriteLine($"HashSecret First 10: {vnpHashSecret?.Substring(0, Math.Min(10, vnpHashSecret.Length))}...");

            // In tất cả params nhận được
            Console.WriteLine("\n===== TẤT CẢ PARAMS TỪ VNPAY =====");
            foreach (var (key, value) in queryParams)
            {
                Console.WriteLine($"{key} = {value}");

                if (!string.IsNullOrEmpty(key) && key.StartsWith("vnp_") && key != "vnp_SecureHash")
                {
                    vnpay.AddResponseData(key, value.ToString());
                }
            }

            var vnpSecureHash = queryParams["vnp_SecureHash"].ToString();
            Console.WriteLine($"\nHash từ VNPay: {vnpSecureHash}");

            var isValidSignature = vnpay.ValidateSignature(vnpSecureHash, vnpHashSecret);

            var responseCode = vnpay.GetResponseData("vnp_ResponseCode");
            Console.WriteLine($"Response Code: {responseCode}");
            Console.WriteLine($"Signature Valid: {isValidSignature}");

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

            Console.WriteLine($"Final Success: {result.Success}");
            Console.WriteLine("========== KẾT THÚC XỬ LÝ ==========\n");

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