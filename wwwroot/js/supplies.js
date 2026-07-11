document.addEventListener('DOMContentLoaded', function () {
    loadFilterData();
    applyFilters();
});

let selectedProduct = null;
let purchaseModal = null;
let qrModal = null;
let suppliesInterval;
let currentMode = 'buy'; // 'buy' or 'cart'

function loadFilterData() {
    fetch('/Supplies/GetFilterData')
        .then(response => {
            if (!response.ok) throw new Error('Network response was not ok: ' + response.statusText);
            return response.json();
        })
        .then(res => {
            if (res.success) {
                const catContainer = document.getElementById('categoryFilters');
                catContainer.innerHTML = res.categories.map(c => `
                    <div class="filter-option">
                        <input type="checkbox" class="cat-check" id="cat-${c.categoryId}" value="${c.categoryName}">
                        <label for="cat-${c.categoryId}">${c.categoryName}</label>
                    </div>
                `).join('');

                const brandContainer = document.getElementById('brandFilters');
                brandContainer.innerHTML = res.brands.map((b, i) => `
                    <div class="filter-option">
                        <input type="checkbox" class="brand-check" id="brand-${i}" value="${b}">
                        <label for="brand-${i}">${b}</label>
                    </div>
                `).join('');
            }
        })
        .catch(error => {
            console.error('Error fetching filter data:', error);
            document.getElementById('categoryFilters').innerHTML = '<p class="text-danger small">Lỗi tải dữ liệu lọc</p>';
        });
}

function applyFilters() {
    const search = document.getElementById('userSearchInput').value;
    const sortBy = document.getElementById('sortBy').value;
    const minPrice = document.getElementById('minPrice').value;
    const maxPrice = document.getElementById('maxPrice').value;

    const categories = Array.from(document.querySelectorAll('.cat-check:checked')).map(cb => cb.value).join(',');
    const brands = Array.from(document.querySelectorAll('.brand-check:checked')).map(cb => cb.value).join(',');

    const params = new URLSearchParams({
        search,
        categories,
        brands,
        minPrice,
        maxPrice,
        sortBy
    });

    const grid = document.getElementById('productsGrid');
    const countLabel = document.getElementById('productsCount');
    grid.innerHTML = '<div class="w-100 text-center py-5"><div class="spinner-border text-primary"></div></div>';

    fetch(`/Supplies/GetFilteredProducts?${params.toString()}`)
        .then(response => {
            if (!response.ok) throw new Error('Network response was not ok: ' + response.statusText);
            return response.json();
        })
        .then(res => {
            if (res.success) {
                renderProducts(res.data);
                countLabel.innerHTML = `Hiển thị <strong>${res.data.length}</strong> sản phẩm`;
            }
        })
        .catch(error => {
            console.error('Error fetching products:', error);
            grid.innerHTML = '<div class="w-100 text-center py-5"><p class="text-danger">Có lỗi xảy ra khi tải sản phẩm. Vui lòng thử lại sau.</p></div>';
        });
}

function renderProducts(products) {
    const grid = document.getElementById('productsGrid');
    if (products.length === 0) {
        grid.innerHTML = '<div class="w-100 text-center py-5"><p class="text-muted">Không tìm thấy sản phẩm nào phù hợp.</p></div>';
        return;
    }

    grid.innerHTML = products.map(p => {
        const safeName = p.productName.replace(/'/g, "\\'");
        const safeCategory = (p.category ? p.category.categoryName : '').replace(/'/g, "\\'");
        const imageUrl = p.imageUrl || '/img/placeholder.jpg';
        return `
            <div class="product-card shadow-sm border-0" style="border-radius: 15px; overflow: hidden; transition: transform 0.3s;">
                <div class="product-image position-relative">
                    <img src="${imageUrl}" alt="${p.productName}" style="height: 200px; width: 100%; object-fit: cover;">
                    <span class="badge bg-primary position-absolute top-0 end-0 m-2 shadow-sm fs-6 px-2 py-1">${p.category ? p.category.categoryName : 'Khác'}</span>
                </div>
                <div class="product-info p-3">
                    <div class="product-name fw-bold mb-1" style="font-size: 1.1rem; height: 2.6rem; overflow: hidden; display: -webkit-box; -webkit-line-clamp: 2; -webkit-box-orient: vertical;">${p.productName}</div>
                    <div class="d-flex justify-content-between align-items-center mb-3">
                        <div class="product-price text-primary fw-bold fs-5">${p.price.toLocaleString('vi-VN')}₫</div>
                        <div class="text-muted small"><i class="fa-solid fa-box-open me-1"></i>Còn: ${p.stockQuantity}</div>
                    </div>
                    <div class="d-grid gap-2 d-md-flex justify-content-md-between">
                         <button class="btn btn-primary py-2 fw-semibold flex-fill" onclick="openPurchaseModal(${p.productId}, '${safeName}', ${p.price}, '${imageUrl}')" style="border-radius: 10px;">
                            <i class="fa-solid fa-bag-shopping me-2"></i>Mua ngay
                        </button>
                        <button class="btn btn-outline-primary py-2 fw-semibold flex-fill" onclick="openAddToCartModal(${p.productId}, '${safeName}', ${p.price}, '${imageUrl}', '${safeCategory}')" style="border-radius: 10px;">
                            <i class="fa-solid fa-cart-plus me-2"></i>Thêm vào giỏ
                        </button>
                    </div>
                </div>
            </div>
        `;
    }).join('');
}

// Mở modal Mua ngay
window.openPurchaseModal = function (id, name, price, image) {
    currentMode = 'buy';
    selectedProduct = { id, name, price };
    document.getElementById('modalProductImage').src = image;
    document.getElementById('modalProductName').innerText = name;
    document.getElementById('modalProductPrice').innerText = price.toLocaleString('vi-VN') + '₫';
    document.getElementById('purchaseQuantity').value = 1;
    calculateModalTotal();

    // Update modal UI for Buy mode
    document.getElementById('purchaseModalTitle').innerHTML = '<i class="fa-solid fa-cart-shopping me-2"></i>Thanh toán sản phẩm';
    document.getElementById('purchaseModalBtn').innerHTML = '<i class="fa-solid fa-check-circle me-2"></i>Xác nhận thanh toán';
    document.getElementById('purchaseModalInfo').innerHTML = '<i class="fa-solid fa-circle-info me-2"></i> Số dư sẽ được trừ trực tiếp vào ví sau khi nhấn thanh toán.';

    if (!purchaseModal) {
        purchaseModal = new bootstrap.Modal(document.getElementById('purchaseModal'));
    }
    purchaseModal.show();
};

// Mở modal Thêm vào giỏ
window.openAddToCartModal = function (id, name, price, image, category) {
    currentMode = 'cart';
    selectedProduct = { id, name, price, imageUrl: image, categoryName: category || '' };
    document.getElementById('modalProductImage').src = image;
    document.getElementById('modalProductName').innerText = name;
    document.getElementById('modalProductPrice').innerText = price.toLocaleString('vi-VN') + '₫';
    document.getElementById('purchaseQuantity').value = 1;
    calculateModalTotal();

    // Update modal UI for Cart mode
    document.getElementById('purchaseModalTitle').innerHTML = '<i class="fa-solid fa-cart-plus me-2"></i>Thêm vào giỏ hàng';
    document.getElementById('purchaseModalBtn').innerHTML = '<i class="fa-solid fa-cart-plus me-2"></i>Thêm vào giỏ';
    document.getElementById('purchaseModalInfo').innerHTML = '<i class="fa-solid fa-circle-info me-2"></i> Sản phẩm sẽ được thêm vào giỏ hàng. Bạn có thể thanh toán sau.';

    if (!purchaseModal) {
        purchaseModal = new bootstrap.Modal(document.getElementById('purchaseModal'));
    }
    purchaseModal.show();
};

function updateQuantity(amount) {
    const input = document.getElementById('purchaseQuantity');
    let val = parseInt(input.value) + amount;
    if (val < 1) val = 1;
    input.value = val;
    calculateModalTotal();
}

function calculateModalTotal() {
    const quantity = parseInt(document.getElementById('purchaseQuantity').value);
    const total = selectedProduct.price * quantity;
    document.getElementById('modalTotalPrice').innerText = total.toLocaleString('vi-VN') + '₫';
}

window.confirmPayment = function () {
    const quantity = parseInt(document.getElementById('purchaseQuantity').value);
    const btn = document.getElementById('purchaseModalBtn');
    const originalText = btn.innerHTML;
    btn.innerHTML = '<i class="fas fa-spinner fa-spin"></i> Đang xử lý...';
    btn.disabled = true;

    if (currentMode === 'cart') {
        // Add to cart mode
        var p = selectedProduct;
        window.addToCart(p.id, p.name, p.imageUrl || '', p.price, quantity, p.categoryName || '', '', '')
            .then(function (data) {
                btn.innerHTML = originalText;
                btn.disabled = false;
                if (data && data.success) {
                    purchaseModal.hide();
                }
            })
            .catch(function () {
                btn.innerHTML = originalText;
                btn.disabled = false;
            });
        return;
    }

    // Buy mode (existing)
    Swal.fire({
        title: 'Đang xử lý...',
        allowOutsideClick: false,
        didOpen: () => { Swal.showLoading(); }
    });

    fetch('/Supplies/Purchase', {
        method: 'POST',
        headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
        body: `productId=${selectedProduct.id}&quantity=${quantity}`
    })
        .then(response => response.json())
        .then(res => {
            Swal.close();
            btn.innerHTML = originalText;
            btn.disabled = false;

            if (res.success) {
                purchaseModal.hide();
                document.getElementById('purchaseQrCode').src = 'data:image/png;base64,' + res.qrCode;

                const timerSpan = document.getElementById('suppliesTimer');
                const countdownDiv = document.getElementById('suppliesQrCountdown');
                if (timerSpan && countdownDiv) {
                    const remaining = res.remainingSeconds || 900;
                    startSuppliesCountdown(remaining, timerSpan, countdownDiv);
                }

                if (!qrModal) {
                    qrModal = new bootstrap.Modal(document.getElementById('qrModal'));
                }
                qrModal.show();
            } else {
                Swal.fire({ icon: 'error', title: 'Thất bại', text: res.message });
            }
        })
        .catch(error => {
            Swal.close();
            btn.innerHTML = originalText;
            btn.disabled = false;
            console.error(error);
            Swal.fire({ icon: 'error', title: 'Lỗi', text: 'Có lỗi xảy ra khi kết nối máy chủ.' });
        });
};

function startSuppliesCountdown(duration, display, container) {
    if (suppliesInterval) clearInterval(suppliesInterval);

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
            clearInterval(suppliesInterval);
            container.innerHTML = "Mã QR đã hết hạn!";
            if (qrImg) qrImg.style.opacity = '0.2';
        }
    }

    updateDisplay();
    suppliesInterval = setInterval(updateDisplay, 1000);
}

// addToCart được định nghĩa trong cart.js (tải toàn cục qua Layout)

function resetFilters() {
    document.getElementById('userSearchInput').value = '';
    document.getElementById('sortBy').value = 'newest';
    document.getElementById('minPrice').value = '';
    document.getElementById('maxPrice').value = '';
    document.querySelectorAll('.cat-check, .brand-check').forEach(cb => cb.checked = false);
    applyFilters();
}
