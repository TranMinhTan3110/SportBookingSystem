$(document).ready(function () {
    $(document).ready(function () {
        $(".btn-apply-filter").click(function () {
            // 1. Thu thập ID danh mục
            var selectedCats = [];
            $(".category-checkbox:checked").each(function () {
                selectedCats.push(parseInt($(this).val()));
            });

            // 2. Thu thập Khoảng giá - Quan trọng: Phải lấy đúng Selector
            var minVal = $(".price-range input[placeholder='Từ']").val();
            var maxVal = $(".price-range input[placeholder='Đến']").val();

            // 3. Thu thập kiểu sắp xếp
            var sortVal = $(".sort-dropdown select").val();

            // Gửi dữ liệu qua AJAX
            $.ajax({
                url: '/Food/GetFilteredProducts',
                type: 'POST',
                contentType: 'application/json',
                data: JSON.stringify({
                    categoryIds: selectedCats,
                    minPrice: minVal ? parseFloat(minVal) : null, // Gửi giá tối thiểu
                    maxPrice: maxVal ? parseFloat(maxVal) : null, // Gửi giá tối đa
                    sortBy: sortVal
                }),
                success: function (response) {
                    if (response.success) {
                        renderProducts(response.products);
                        // Cập nhật số lượng hiển thị trên tiêu đề
                        $(".products-count strong").text(response.products.length);
                    }
                }
            });
        });
    });

    // Hàm vẽ lại danh sách sản phẩm sau khi lọc
    function renderProducts(products) {
        var html = '';
        products.forEach(p => {
            html += `
                <div class="product-card">
                    <div class="product-image"><img src="${p.imageUrl}" alt=""></div>
                    <div class="product-info">
                        <div class="product-typeName">${p.categoryName}</div>
                        <div class="product-name">${p.productName}</div>
                        <div class="product-price">${p.formattedPrice}</div>
                        <div class="btn-contain d-flex justify-content-end align-items-center">
                            <button class="btn border-0 bg-transparent"><i class="fa-solid fa-wallet fa-xl" style="color: #63E6BE;"></i></button>
                            <button class="btn border-0 bg-transparent"><i class="fa-solid fa-cart-arrow-down fa-xl" style="color: #0e86e1;"></i></button>
                        </div>
                    </div>
                </div>`;
        });
        $("#productsList").html(html);
    }
    // xoa bo loc 
    $(".btn-reset-filter").click(function () {
        // Bỏ tích tất cả checkbox danh mục
        $(".category-checkbox").prop("checked", false);

        // Xóa trắng các ô nhập giá
        $(".price-range input").val("");

        // Đưa dropdown sắp xếp về mặc định
        $(".sort-dropdown select").val("default");

        // Tự động gọi lại hàm Áp dụng để tải lại toàn bộ sản phẩm
        $(".btn-apply-filter").click();
    });
});