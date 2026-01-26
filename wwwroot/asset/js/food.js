// Global variables
let selectedProduct = null;
let purchaseModal = null;
let qrModal = null;
let foodInterval;

$(document).ready(function () {
    purchaseModal = new bootstrap.Modal(document.getElementById('purchaseModal'));
    qrModal = new bootstrap.Modal(document.getElementById('qrModal'));

    $(".btn-apply-filter").click(function () {
        // 1. Thu thập ID danh mục
        var selectedCats = [];
        $(".category-checkbox:checked").each(function () {
            selectedCats.push(parseInt($(this).val()));
        });

        // 2. Thu thập Khoảng giá
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

// Hàm vẽ lại danh sách sản phẩm sau khi lọc
function renderProducts(products) {
    var html = '';
    products.forEach(p => {
        const safeName = p.productName.replace(/'/g, "\\'");

        html += `
            <div class="product-card">
                <div class="product-image"><img src="${p.imageUrl}" alt=""></div>
                <div class="product-info">
                    <div class="product-typeName">${p.categoryName}</div>
                    <div class="product-name">${p.productName}</div>
                    <div class="product-price">${p.formattedPrice}</div>
                    <div class="btn-contain d-flex justify-content-end align-items-center gap-2">
                    <button class="btn btn-primary py-2 fw-semibold flex-fill"  onclick="openPurchaseModal(${p.productId}, '${safeName}', ${p.price}, '${p.imageUrl}')">
                        <i class="fa-solid fa-bag-shopping me-2"></i>Mua ngay
                    </button>
                    <button class="btn btn-outline-primary py-2 fw-semibold flex-fill"onclick="addToCart(${p.productId}, '${safeName}', ${p.price})">
                        <i class="fa-solid fa-cart-plus me-2"></i>Thêm vào giỏ
                    </button>
                    </div>
                </div>
            </div>`;
    });
    $("#productsList").html(html);
}



window.openPurchaseModal = function (id, name, price, image) {
    selectedProduct = { id, name, price };
    document.getElementById('modalProductName').innerText = name;
    document.getElementById('modalProductPrice').innerText = price.toLocaleString('vi-VN') + '₫';
    document.getElementById('purchaseQuantity').value = 1;
    calculateModalTotal();

    purchaseModal.show();
};

window.updateQuantity = function (amount) {
    const input = document.getElementById('purchaseQuantity');
    let val = parseInt(input.value) + amount;
    if (val < 1) val = 1;
    input.value = val;
    calculateModalTotal();
};

function calculateModalTotal() {
    const quantity = parseInt(document.getElementById('purchaseQuantity').value);
    const total = selectedProduct.price * quantity;
    document.getElementById('modalTotalPrice').innerText = total.toLocaleString('vi-VN') + '₫';
}

window.confirmPayment = function () {
    const quantity = parseInt(document.getElementById('purchaseQuantity').value);

    const btn = document.querySelector('#purchaseModal .btn-primary');
    const originalText = btn.innerHTML;
    btn.innerHTML = '<i class="fas fa-spinner fa-spin"></i> Đang xử lý...';
    btn.disabled = true;

    $.ajax({
        url: '/Food/Purchase',
        type: 'POST',
        data: {
            productId: selectedProduct.id,
            quantity: quantity
        },
        success: function (res) {
            btn.innerHTML = originalText;
            btn.disabled = false;

            if (res.success) {
                purchaseModal.hide();
                document.getElementById('purchaseQrCode').src = 'data:image/png;base64,' + res.qrCode;
                const timerSpan = document.getElementById('foodTimer');
                const countdownDiv = document.getElementById('foodQrCountdown');
                if (timerSpan && countdownDiv) {
                    const remaining = res.remainingSeconds || 900;
                    startCountdown(remaining, timerSpan, countdownDiv);
                }

                qrModal.show();
            } else {
                alert(res.message);
            }
        },
        error: function (err) {
            btn.innerHTML = originalText;
            btn.disabled = false;
            alert('Có lỗi xảy ra khi kết nối máy chủ.');
        }
    });
};

function startCountdown(duration, display, container) {
    if (foodInterval) clearInterval(foodInterval);

    let timer = duration, minutes, seconds;
    const qrImg = document.getElementById('purchaseQrCode');
    if (qrImg) qrImg.style.opacity = '1';

    function updateDisplay() {
        minutes = parseInt(timer / 60, 10);
        seconds = parseInt(timer % 60, 10);

        minutes = minutes < 10 ? "0" + minutes : minutes;
        seconds = seconds < 10 ? "0" + seconds : seconds;

        display.textContent = minutes + ":" + seconds;

        if (--timer < 0) {
            clearInterval(foodInterval);
            container.innerHTML = "Mã QR đã hết hạn!";
            if (qrImg) qrImg.style.opacity = '0.2';
        }
    }

    updateDisplay();
    foodInterval = setInterval(updateDisplay, 1000);
}

window.addToCart = function (id, name, price) {
    if (typeof Swal !== 'undefined') {
        Swal.fire({
            icon: 'success',
            title: 'Đã thêm vào giỏ',
            text: `${name} đã được thêm vào giỏ hàng của bạn (Demo)`,
            timer: 2000,
            showConfirmButton: false
        });
    } else {
        alert(`Đã thêm ${name} vào giỏ hàng!`);
    }
};