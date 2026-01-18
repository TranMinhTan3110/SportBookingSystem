using Microsoft.AspNetCore.Mvc;

namespace SportBookingSystem.Controllers
{
    public class AdminPaymentController : Controller
    {
        public IActionResult Index()
        {
            // Mock Data: Transactions (Deposits, Bookings, Orders)
            var payments = new List<PaymentViewModel>
            {
                new PaymentViewModel { Code = "DEP-1001", User = "nguyenvanA", Amount = 5000000, Date = DateTime.Now.AddMinutes(-10), Type = "Nạp tiền", Status = "Completed", Source = "Banking" },
                new PaymentViewModel { Code = "DEP-1002", User = "lethic", Amount = 2000000, Date = DateTime.Now.AddHours(-1), Type = "Nạp tiền", Status = "Completed", Source = "Momo" },
                new PaymentViewModel { Code = "BKG-2023", User = "tranb", Amount = 450000, Date = DateTime.Now.AddHours(-2), Type = "Thanh toán Booking", Status = "Completed", Source = "Ví nội bộ" },
                new PaymentViewModel { Code = "ORD-5501", User = "phamvand", Amount = 120000, Date = DateTime.Now.AddDays(-1), Type = "Thanh toán Order", Status = "Completed", Source = "Tiền mặt" },
                new PaymentViewModel { Code = "DEP-1003", User = "tranb", Amount = 500000, Date = DateTime.Now.AddDays(-1), Type = "Nạp tiền", Status = "Pending", Source = "Banking" },
                new PaymentViewModel { Code = "BKG-2024", User = "nguyenvanA", Amount = 300000, Date = DateTime.Now.AddDays(-2), Type = "Thanh toán Booking", Status = "Completed", Source = "Ví nội bộ" },
                new PaymentViewModel { Code = "DEP-1004", User = "user_new", Amount = 100000, Date = DateTime.Now.AddDays(-2), Type = "Nạp tiền", Status = "Cancelled", Source = "Momo" },
                new PaymentViewModel { Code = "ORD-5502", User = "lethic", Amount = 85000, Date = DateTime.Now.AddDays(-3), Type = "Thanh toán Order", Status = "Completed", Source = "Ví nội bộ" },
                new PaymentViewModel { Code = "REF-9001", User = "phamvand", Amount = 120000, Date = DateTime.Now.AddDays(-3), Type = "Hoàn tiền", Status = "Completed", Source = "System" },
                new PaymentViewModel { Code = "DEP-1005", User = "vip_user", Amount = 10000000, Date = DateTime.Now.AddDays(-4), Type = "Nạp tiền", Status = "Completed", Source = "Banking" }
            };

            var model = new PaymentDashboardViewModel
            {
                Payments = payments,
                
                // 1. Total Deposits (Tổng nạp): Sum of "Nạp tiền" that are Completed
                TotalDeposits = payments.Where(p => p.Type == "Nạp tiền" && p.Status == "Completed").Sum(p => p.Amount),
                
                // 2. Revenue (Doanh thu): Sum of Bookings + Orders that are Completed
                Revenue = payments.Where(p => (p.Type.Contains("Booking") || p.Type.Contains("Order")) && p.Status == "Completed").Sum(p => p.Amount),
                
                // 3. Pending Deposits (Chờ duyệt): Count of "Nạp tiền" that are Pending
                PendingDeposits = payments.Count(p => p.Type == "Nạp tiền" && p.Status == "Pending"),

                // 4. Transaction Count
                TransactionCount = payments.Count,
                
                // 5. Active Users
                ActiveUserCount = payments.Select(p => p.User).Distinct().Count()
            };

            return View(model);
        }

        // API: Get Users for dropdown
        [HttpGet]
        public IActionResult GetUsers()
        {
            // Mock data - Replace with actual database query
            var users = new List<UserSelectViewModel>
            {
                new UserSelectViewModel { UserID = 1, Username = "nguyenvanA", FullName = "Nguyễn Văn A", WalletBalance = 500000 },
                new UserSelectViewModel { UserID = 2, Username = "lethic", FullName = "Lê Thị C", WalletBalance = 1200000 },
                new UserSelectViewModel { UserID = 3, Username = "tranb", FullName = "Trần B", WalletBalance = 300000 },
                new UserSelectViewModel { UserID = 4, Username = "phamvand", FullName = "Phạm Văn D", WalletBalance = 800000 }
            };
            
            return Json(users);
        }

        // API: Get Products for dropdown
        [HttpGet]
        public IActionResult GetProducts()
        {
            // Mock data - Replace with actual database query
            var products = new List<ProductSelectViewModel>
            {
                new ProductSelectViewModel { ProductID = 1, ProductName = "Giày đá bóng Nike", Price = 1500000, StockQuantity = 20 },
                new ProductSelectViewModel { ProductID = 2, ProductName = "Áo đấu Adidas", Price = 450000, StockQuantity = 50 },
                new ProductSelectViewModel { ProductID = 3, ProductName = "Bóng đá Molten", Price = 350000, StockQuantity = 30 },
                new ProductSelectViewModel { ProductID = 4, ProductName = "Nước uống Aquafina", Price = 10000, StockQuantity = 100 },
                new ProductSelectViewModel { ProductID = 5, ProductName = "Găng tay thủ môn", Price = 250000, StockQuantity = 15 }
            };
            
            return Json(products);
        }

        // POST: Create Deposit Transaction
        [HttpPost]
        public IActionResult CreateDeposit([FromBody] CreateDepositViewModel model)
        {
            try
            {
                if (model.Amount <= 0)
                {
                    return Json(new { success = false, message = "Số tiền phải lớn hơn 0" });
                }

                // TODO: Replace with actual database operations
                // 1. Create Transaction record
                // 2. Update User WalletBalance
                
                // Mock success response
                var transactionCode = $"DEP-{DateTime.Now:yyyyMMddHHmmss}";
                
                return Json(new { 
                    success = true, 
                    message = "Tạo giao dịch nạp tiền thành công!", 
                    transactionCode = transactionCode 
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Lỗi: {ex.Message}" });
            }
        }

        // POST: Create Purchase Order
        [HttpPost]
        public IActionResult CreatePurchase([FromBody] CreatePurchaseViewModel model)
        {
            try
            {
                if (model.Quantity <= 0)
                {
                    return Json(new { success = false, message = "Số lượng phải lớn hơn 0" });
                }

                // TODO: Replace with actual database operations
                // 1. Check product stock
                // 2. Create Order record
                // 3. Create OrderDetails record
                // 4. Update User WalletBalance (deduct)
                // 5. Update Product StockQuantity
                
                // Mock success response
                var orderCode = $"ORD-{DateTime.Now:yyyyMMddHHmmss}";
                
                return Json(new { 
                    success = true, 
                    message = "Tạo đơn hàng thành công!", 
                    orderCode = orderCode 
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Lỗi: {ex.Message}" });
            }
        }
    }

    public class PaymentDashboardViewModel
    {
        public List<PaymentViewModel> Payments { get; set; } = new List<PaymentViewModel>();
        
        public decimal TotalDeposits { get; set; } // Tổng tiền nạp vào (User Fund)
        public decimal Revenue { get; set; }       // Doanh thu thực (Sales)
        public int PendingDeposits { get; set; }   // Yêu cầu nạp chờ duyệt
        public int TransactionCount { get; set; }
        public int ActiveUserCount { get; set; }
    }

    public class PaymentViewModel
    {
        public string? Code { get; set; } 
        public string? User { get; set; }
        public decimal Amount { get; set; }
        public DateTime Date { get; set; }
        public string? Type { get; set; }    // "Nạp tiền", "Thanh toán Booking", "Thanh toán Order"
        public string? Status { get; set; }  // "Completed", "Pending", "Cancelled"
        public string? Source { get; set; }  // "Momo", "Banking", "Ví nội bộ"
    }

    // ViewModels for Transaction Management
    public class CreateDepositViewModel
    {
        public int UserID { get; set; }
        public decimal Amount { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
        public string? Message { get; set; }
    }

    public class CreatePurchaseViewModel
    {
        public int UserID { get; set; }
        public int ProductID { get; set; }
        public int Quantity { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
    }

    public class UserSelectViewModel
    {
        public int UserID { get; set; }
        public string? Username { get; set; }
        public string? FullName { get; set; }
        public decimal WalletBalance { get; set; }
    }

    public class ProductSelectViewModel
    {
        public int ProductID { get; set; }
        public string? ProductName { get; set; }
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }
    }
}
