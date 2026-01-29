namespace SportBookingSystem.Models.ViewModels
{
    /// ViewModel cho trang hiển thị sản phẩm đồ ăn
    public class FoodIndexViewModel
    {
        // Danh sách danh mục để hiển thị trong bộ lọc
        public List<CategoryFilterViewModel> Categories { get; set; } = new();

        // Danh sách sản phẩm ban đầu
        public List<ProductViewModelUser> Products { get; set; } = new();

        // Tổng số sản phẩm 
        public int TotalProducts { get; set; }
    }

    /// ViewModel cho danh mục
    public class CategoryFilterViewModel
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public int ProductCount { get; set; } // Số lượng sản phẩm trong danh mục
    }

    /// ViewModel cho sản phẩm
    public class ProductViewModelUser
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string FormattedPrice { get; set; } = string.Empty; // VD: "89.000₫"
        public string ImageUrl { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty; // Tên danh mục (product-typeName)
        public int CategoryId { get; set; }
        public int StockQuantity { get; set; }
        public string? Size { get; set; }
        public string? Brand { get; set; }
        public string? ProductType { get; set; }
        public bool IsAvailable => StockQuantity > 0;
    }

    /// Request model cho API lọc sản phẩm
    public class FilterProductRequest
    {
        // Danh sách ID danh mục được chọn 
        public List<int>? CategoryIds { get; set; }

        // Khoảng giá tối thiểu
        public decimal? MinPrice { get; set; }

        // Khoảng giá tối đa
        public decimal? MaxPrice { get; set; }

        // Sắp xếp: "price-asc", "price-desc", "name-asc", "default"
        public string SortBy { get; set; } = "default";
    }

    /// Response model cho API lọc sản phẩm
    public class FilterProductResponse
    {
        public List<ProductViewModelUser> Products { get; set; } = new();
        public int TotalCount { get; set; }
        public bool Success { get; set; } = true;
        public string? Message { get; set; }
    }
}