namespace SportBookingSystem.Constants
{
    public static class TransactionStatus
    {
        public const string Success = "Thành công";
        public const string Pending = "Chờ xử lý";
        public const string PendingConfirm = "Chờ xác nhận";
        public const string Canceled = "Đã hủy";
        public const string CancelBooking = "Đã hủy đặt sân";
    }

    public static class TransactionTypes
    {
        public const string Recharge = "Nạp tiền";
        public const string Transfer = "Chuyển tiền";
        public const string Booking = "Thanh toán Booking";
        public const string Order = "Thanh toán Order";
        public const string Refund = "Hoàn tiền";
        public const string RefundBooking = "Hoàn tiền đặt sân";
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
        public const string Recharge = "NAP";
        public const string Deposit = "DEP";
        public const string Booking = "BKG";
        public const string Order = "ORD";
        public const string Refund = "REF";
        public const string Transfer = "TRF";
        public const string RefundBooking = "RFB";
    }

    public static class BookingStatus
    {
        public const int PendingConfirm = 1;
        public const int CheckedIn = 2;
        public const int Completed = 3;
        public const int Cancelled = -1;
        public const int RefundBooking = -2;
    }
}