namespace SportBookingSystem.Models.ViewModels
{
    public class ProductViewModel
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public decimal Price { get; set; }
        public string Unit { get; set; }
        public int StockQuantity { get; set; }
        public string ImageUrl { get; set; }
        public string ProductType { get; set; }
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
        public string StockStatus { get; set; }
        public bool Status { get; set; } 

        public string StockStatusText
        {
            get
            {
                return StockStatus switch
                {
                    "in" => $"{StockQuantity} {Unit}",
                    "low" => $"{StockQuantity} {Unit}",
                    "out" => "0 phần",
                    _ => $"{StockQuantity} {Unit}"
                };
            }
        }

        public string StockStatusClass
        {
            get
            {
                return StockStatus switch
                {
                    "in" => "stock-in",
                    "low" => "stock-low",
                    "out" => "stock-out",
                    _ => "stock-in"
                };
            }
        }

        public string BadgeClass
        {
            get
            {
                return CategoryName switch
                {
                    "Đồ ăn" => "badge-food",
                    "Nước giải khát" => "badge-drink",
                    "Trái cây" => "badge-dessert",
                    _ => "badge-food"
                };
            }
        }

        public string StatusText => Status ? "Đang bán" : "Đã ẩn";
        public string StatusClass => Status ? "status-active" : "status-hidden";
    }
}