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

            var secureHash = HmacSHA512(vnpHashSecret, rawData);

            return $"{baseUrl}?{query}vnp_SecureHash={secureHash}";
        }




        public bool ValidateSignature(string inputHash, string secretKey)
        {
            var rspRaw = GetResponseData();

            var myChecksum = HmacSHA512(secretKey, rspRaw);

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

            var signableData = _responseData.Where(kv => kv.Key != "vnp_SecureHash" && kv.Key != "vnp_SecureHashType");

            foreach (var kv in signableData)
            {
                data.Append(kv.Key + "=" + kv.Value + "&");
            }

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