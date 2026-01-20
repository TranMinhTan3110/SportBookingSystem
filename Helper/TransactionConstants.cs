namespace SportBookingSystem.Constants
{
    public static class TransactionStatus
    {
        public const string Success = "Thành công";
        public const string Pending = "Chờ xử lý";
        public const string Canceled = "Đã hủy";
    }

    public static class TransactionTypes
    {
        public const string Recharge = "Nạp tiền";
        public const string Transfer = "Chuyển tiền";
        public const string Booking = "Thanh toán sân";
        public const string Order = "Thanh toán đồ";
    }

    public static class TransactionSources
    {
        public const string VNPay = "VNPay";
        public const string Cash = "Tiền mặt";
        public const string Bank = "Chuyển khoản";
        public const string Wallet = "Ví nội bộ";
    }
}