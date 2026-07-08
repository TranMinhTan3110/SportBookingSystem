using System.Text.Json;

namespace SportBookingSystem.Helpers
{
    /// <summary>
    /// Extension methods cho ISession — lưu object dạng JSON
    /// </summary>
    public static class SessionExtensions
    {
        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };

        /// <summary>Lưu object vào session dạng JSON</summary>
        public static void SetObjectAsJson(this ISession session, string key, object value)
        {
            session.SetString(key, JsonSerializer.Serialize(value, _jsonOptions));
        }

        /// <summary>Đọc object từ session (JSON → T)</summary>
        public static T? GetObjectFromJson<T>(this ISession session, string key) where T : class
        {
            var json = session.GetString(key);
            if (string.IsNullOrEmpty(json)) return null;
            try { return JsonSerializer.Deserialize<T>(json, _jsonOptions); }
            catch { return null; }
        }
    }
}
