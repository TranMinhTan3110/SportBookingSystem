using SportBookingSystem.Helpers;
using SportBookingSystem.Models.Entities;

namespace SportBookingSystem.Services
{
    public class CartService : ICartService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private const string CartSessionKey = "ShoppingCart";
        private const string CartCreatedAtKey = "CartCreatedAt";
        private const int DefaultCartTimeoutMinutes = 30;

        public CartService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        private ISession Session => _httpContextAccessor.HttpContext!.Session;

        public List<CartItem> GetCart()
        {
            return Session.GetObjectFromJson<List<CartItem>>(CartSessionKey) ?? new List<CartItem>();
        }

        public void AddToCart(CartItem item)
        {
            var cart = GetCart();

            // Nếu giỏ đang rỗng, ghi lại thời điểm tạo giỏ
            if (!cart.Any())
            {
                SetCartCreationTime(DateTime.Now);
            }

            var existing = cart.FirstOrDefault(c => c.ProductId == item.ProductId);

            if (existing != null)
            {
                existing.Quantity += item.Quantity;
            }
            else
            {
                cart.Add(item);
            }

            SaveCart(cart);
        }

        public void UpdateQuantity(int productId, int quantity)
        {
            var cart = GetCart();
            var item = cart.FirstOrDefault(c => c.ProductId == productId);
            if (item != null)
            {
                if (quantity <= 0)
                    cart.Remove(item);
                else
                    item.Quantity = quantity;
            }
            SaveCart(cart);
        }

        public void RemoveItem(int productId)
        {
            var cart = GetCart();
            cart.RemoveAll(c => c.ProductId == productId);
            SaveCart(cart);
        }

        public void ClearCart()
        {
            Session.Remove(CartSessionKey);
            Session.Remove(CartCreatedAtKey);
        }

        public int GetCartCount()
        {
            var cart = GetCart();
            return cart.Sum(i => i.Quantity);
        }

        /// <summary>Lấy thời điểm tạo giỏ hàng từ session</summary>
        public DateTime? GetCartCreationTime()
        {
            var str = Session.GetString(CartCreatedAtKey);
            if (string.IsNullOrEmpty(str)) return null;
            if (DateTime.TryParse(str, out var dt)) return dt;
            return null;
        }

        /// <summary>Ghi thời điểm tạo giỏ hàng vào session</summary>
        public void SetCartCreationTime(DateTime time)
        {
            Session.SetString(CartCreatedAtKey, time.ToString("O"));
        }

        /// <summary>Tính số giây còn lại của giỏ hàng</summary>
        public double GetRemainingSeconds()
        {
            var createdAt = GetCartCreationTime();
            if (createdAt == null) return 0;

            var elapsed = (DateTime.Now - createdAt.Value).TotalSeconds;
            var totalSeconds = DefaultCartTimeoutMinutes * 60;
            var remaining = totalSeconds - elapsed;

            return Math.Max(0, remaining);
        }

        /// <summary>Kiểm tra giỏ hàng đã hết hạn chưa</summary>
        public bool IsCartExpired()
        {
            return GetRemainingSeconds() <= 0 && GetCart().Any();
        }

        /// <summary>Lấy timeout (phút)</summary>
        public int GetCartTimeoutMinutes() => DefaultCartTimeoutMinutes;

        private void SaveCart(List<CartItem> cart)
        {
            Session.SetObjectAsJson(CartSessionKey, cart);
        }
    }
}
