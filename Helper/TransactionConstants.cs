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
        public const string Booking = "Thanh toán Booking";
        public const string Order = "Thanh toán Order";
        public const string Refund = "Hoàn tiền";
    }

    public static class TransactionSources
    {
        public const string VNPay = "VNPay";
        public const string Cash = "Tiền mặt";
        public const string Momo = "Momo";
        public const string Banking = "Banking";
        public const string Wallet = "Ví nội bộ";
        public const string System = "System";
    }

   
    public static class TransactionPrefixes
    {
        public const string Recharge = "NAP";      // Nạp tiền
        public const string Deposit = "DEP";       // Nạp tiền (nếu dùng riêng)
        public const string Booking = "BKG";       // Thanh toán booking
        public const string Order = "ORD";         // Thanh toán order
        public const string Refund = "REF";        // Hoàn tiền
        public const string Transfer = "TRF";      // Chuyển tiền
    }
}