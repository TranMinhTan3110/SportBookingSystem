using System.Globalization;
using System.Net;
using System.Security.Cryptography;
using System.Text;

namespace SportBookingSystem.Helper
{
    public class VnPayLibrary
    {
        private readonly SortedList<string, string> _requestData = new SortedList<string, string>(new VnPayCompare());
        private readonly SortedList<string, string> _responseData = new SortedList<string, string>(new VnPayCompare());

        public void AddRequestData(string key, string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                _requestData.Add(key, value);
            }
        }

        public void AddResponseData(string key, string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                _responseData.Add(key, value);
            }
        }

        public string GetResponseData(string key)
        {
            return _responseData.TryGetValue(key, out var retValue) ? retValue : string.Empty;
        }

        public string CreateRequestUrl(string baseUrl, string vnpHashSecret)
        {
            var query = new StringBuilder();
            var signData = new StringBuilder();

            foreach (var kv in _requestData)
            {
                if (!string.IsNullOrEmpty(kv.Value))
                {
                    query.Append($"{WebUtility.UrlEncode(kv.Key)}={WebUtility.UrlEncode(kv.Value)}&");
                    signData.Append($"{kv.Key}={WebUtility.UrlEncode(kv.Value)}&");
                }
            }

            var rawData = signData.ToString().TrimEnd('&');

            Console.WriteLine("===== VNPAY RAW DATA =====");
            Console.WriteLine(rawData);
            Console.WriteLine("=========================");

            var secureHash = HmacSHA512(vnpHashSecret, rawData);

            Console.WriteLine("===== VNPAY HASH =====");
            Console.WriteLine(secureHash);
            Console.WriteLine("======================");

            return $"{baseUrl}?{query}vnp_SecureHash={secureHash}";
        }




        public bool ValidateSignature(string inputHash, string secretKey)
        {
            var rspRaw = GetResponseData();

            Console.WriteLine("==================== RESPONSE RAW DATA ====================");
            Console.WriteLine(rspRaw);
            Console.WriteLine("===========================================================");

            var myChecksum = HmacSHA512(secretKey, rspRaw);

            Console.WriteLine($"Input Hash (VNPay):  {inputHash}");
            Console.WriteLine($"My Checksum (Calc):  {myChecksum}");
            Console.WriteLine($"Match: {myChecksum.Equals(inputHash, StringComparison.InvariantCultureIgnoreCase)}");
            Console.WriteLine("===========================================================");

            return myChecksum.Equals(inputHash, StringComparison.InvariantCultureIgnoreCase);
        }

        
        private string HmacSHA512(string key, string inputData)
        {
            var hash = new StringBuilder();
            var keyBytes = Encoding.UTF8.GetBytes(key);
            var inputBytes = Encoding.UTF8.GetBytes(inputData);

            using (var hmac = new HMACSHA512(keyBytes))
            {
                var hashValue = hmac.ComputeHash(inputBytes);

                foreach (var b in hashValue)
                {
                    hash.Append(b.ToString("x2")); 
                }
            }

            return hash.ToString();
        }

        private string GetResponseData()
        {
            var data = new StringBuilder();

            foreach (var kv in _responseData)
            {
                // ✅ Bỏ qua các key không cần thiết
                if (kv.Key == "vnp_SecureHashType" || kv.Key == "vnp_SecureHash")
                    continue;

                if (!string.IsNullOrEmpty(kv.Value))
                {
                    data.Append($"{kv.Key}={kv.Value}&");
                }
            }

            // Remove dấu & cuối cùng
            if (data.Length > 0)
            {
                data.Remove(data.Length - 1, 1);
            }

            return data.ToString();
        }
    }

    public class VnPayCompare : IComparer<string>
    {
        public int Compare(string x, string y)
        {
            return string.CompareOrdinal(x, y);
        }
    }
}