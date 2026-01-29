
// CHUYỂN TIỀN NỘI BỘ

// Kiểm tra người nhận khi blur khỏi input SĐT
document.getElementById('trf_phone')?.addEventListener('blur', async function () {
    const phone = this.value.trim();
    const receiverNameDiv = document.getElementById('trf_receiver_name');

    if (!phone) {
        receiverNameDiv.style.display = 'none';
        return;
    }

    try {
        const response = await fetch(`/api/transaction/check-receiver?phone=${phone}`);
        const data = await response.json();

        if (data.success) {
            receiverNameDiv.innerHTML = `<i class="bi bi-person-check-fill"></i> Người nhận: <strong>${data.name}</strong>`;
            receiverNameDiv.style.display = 'block';
            receiverNameDiv.className = 'mt-2 small text-success';
        } else {
            receiverNameDiv.innerHTML = `<i class="bi bi-x-circle-fill"></i> ${data.message}`;
            receiverNameDiv.style.display = 'block';
            receiverNameDiv.className = 'mt-2 small text-danger';
        }
    } catch (error) {
        console.error('Lỗi kiểm tra người nhận:', error);
        receiverNameDiv.innerHTML = `<i class="bi bi-exclamation-triangle-fill"></i> Có lỗi xảy ra`;
        receiverNameDiv.style.display = 'block';
        receiverNameDiv.className = 'mt-2 small text-warning';
    }
});

// Xử lý khi bấm nút "Xác nhận chuyển"
document.getElementById('btn_confirm_transfer')?.addEventListener('click', async function () {
    const phone = document.getElementById('trf_phone').value.trim();
    const amount = parseFloat(document.getElementById('trf_amount').value);
    const message = document.getElementById('trf_message').value.trim();

    // Validate
    if (!phone) {
        Swal.fire('Lỗi', 'Vui lòng nhập số điện thoại người nhận', 'error');
        return;
    }

    if (!amount || amount < 1000) {
        Swal.fire('Lỗi', 'Số tiền tối thiểu là 1,000₫', 'error');
        return;
    }

    // Hiển thị loading
    Swal.fire({
        title: 'Đang xử lý...',
        text: 'Vui lòng đợi',
        allowOutsideClick: false,
        didOpen: () => {
            Swal.showLoading();
        }
    });

    try {
        const response = await fetch('/api/transaction/transfer', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({
                receiverPhone: phone,
                amount: amount,
                message: message
            })
        });

        const result = await response.json();

        if (result.success) {
            Swal.fire({
                icon: 'success',
                title: 'Thành công!',
                text: result.message,
                confirmButtonText: 'OK'
            }).then(() => {
                // Đóng modal
                const modal = bootstrap.Modal.getInstance(document.getElementById('transferMoneyModal'));
                modal.hide();

                // Reload trang để cập nhật số dư
                location.reload();
            });
        } else {
            Swal.fire('Thất bại', result.message, 'error');
        }
    } catch (error) {
        console.error('Lỗi chuyển tiền:', error);
        Swal.fire('Lỗi', 'Có lỗi xảy ra, vui lòng thử lại', 'error');
    }
});

// Quản lý sự kiện chọn tiền nhanh
document.addEventListener('DOMContentLoaded', function () {
    const amountButtons = document.querySelectorAll('.btn-amount');
    const amountInput = document.getElementById('topupAmount');

    amountButtons.forEach(btn => {
        btn.addEventListener('click', function () {
            amountInput.value = this.getAttribute('data-amount');
            amountButtons.forEach(b => b.classList.replace('btn-primary', 'btn-outline-primary'));
            this.classList.replace('btn-outline-primary', 'btn-primary');
        });
    });
});

async function processTopup() {
    const amountInput = document.getElementById('topupAmount');
    if (!amountInput) return;

    const amount = amountInput.value;

    if (!amount || amount < 10000) {
        alert("Vui lòng nhập số tiền nạp tối thiểu 10.000₫");
        return;
    }

    try {
        const response = await fetch('/Wallet/CreateVNPayUrl', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ amount: parseFloat(amount) })
        });

        const result = await response.json();
        if (result.success) {
            window.location.href = result.paymentUrl;
        } else {
            alert(result.message || "Lỗi tạo giao dịch");
        }
    } catch (error) {
        console.error("Lỗi:", error);
        alert("Không thể kết nối tới máy chủ thanh toán.");
    }
}

// PURCHASE LOGIC FOR PRODUCTS
let selectedProduct = null;
let purchaseModal = null;
let qrModal = null;
let homeInterval;
let purchaseType = 'food'; // 'food' or 'supply'

window.openPurchaseModal = function (id, name, price, image, type) {
    selectedProduct = { id, name, price };
    purchaseType = type;

    document.getElementById('modalProductImage').src = image;
    document.getElementById('modalProductName').innerText = name;
    document.getElementById('modalProductPrice').innerText = price.toLocaleString('vi-VN') + '₫';
    document.getElementById('purchaseQuantity').value = 1;

    const title = document.getElementById('purchaseModalTitle');
    if (title) {
        title.innerHTML = type === 'food'
            ? '<i class="fa-solid fa-utensils me-2"></i>Đặt món ăn'
            : '<i class="fa-solid fa-cart-shopping me-2"></i>Thanh toán sản phẩm';
    }

    calculateModalTotal();

    if (!purchaseModal) {
        purchaseModal = new bootstrap.Modal(document.getElementById('purchaseModal'));
    }
    purchaseModal.show();
};

window.updateQuantity = function (amount) {
    const input = document.getElementById('purchaseQuantity');
    if (!input) return;
    let val = parseInt(input.value) + amount;
    if (val < 1) val = 1;
    input.value = val;
    calculateModalTotal();
};

function calculateModalTotal() {
    if (!selectedProduct) return;
    const quantityInput = document.getElementById('purchaseQuantity');
    const totalDisplay = document.getElementById('modalTotalPrice');
    if (!quantityInput || !totalDisplay) return;

    const quantity = parseInt(quantityInput.value);
    const total = selectedProduct.price * quantity;
    totalDisplay.innerText = total.toLocaleString('vi-VN') + '₫';
}

window.confirmPayment = function () {
    if (!selectedProduct) return;

    const quantityInput = document.getElementById('purchaseQuantity');
    if (!quantityInput) return;
    const quantity = parseInt(quantityInput.value);
    const url = purchaseType === 'food' ? '/Food/Purchase' : '/Supplies/Purchase';

    Swal.fire({
        title: 'Đang xử lý...',
        allowOutsideClick: false,
        didOpen: () => { Swal.showLoading(); }
    });

    $.ajax({
        url: url,
        type: 'POST',
        data: {
            productId: selectedProduct.id,
            quantity: quantity
        },
        success: function (res) {
            Swal.close();
            if (res.success) {
                if (purchaseModal) purchaseModal.hide();

                document.getElementById('purchaseQrCode').src = 'data:image/png;base64,' + res.qrCode;

                const successTitle = document.getElementById('successTitle');
                const successMsg = document.getElementById('successMessage');

                if (purchaseType === 'food') {
                    if (successTitle) successTitle.innerText = "Đặt món thành công!";
                    if (successMsg) successMsg.innerText = "Vui lòng lưu lại mã QR dưới đây và đưa cho nhân viên canteen để nhận món.";
                } else {
                    if (successTitle) successTitle.innerText = "Thanh toán thành công!";
                    if (successMsg) successMsg.innerText = "Vui lòng lưu lại mã QR dưới đây và đưa cho nhân viên để nhận đồ.";
                }

                const timerSpan = document.getElementById('timer');
                const countdownDiv = document.getElementById('qrCountdown');
                if (timerSpan && countdownDiv) {
                    const remaining = res.remainingSeconds || 900;
                    startCountdown(remaining, timerSpan, countdownDiv);
                }

                if (!qrModal) {
                    qrModal = new bootstrap.Modal(document.getElementById('qrModal'));
                }
                qrModal.show();
            } else {
                Swal.fire({ icon: 'error', title: 'Thất bại', text: res.message });
            }
        },
        error: function (err) {
            Swal.close();
            Swal.fire({ icon: 'error', title: 'Lỗi', text: 'Có lỗi xảy ra khi kết nối máy chủ.' });
        }
    });
};

function startCountdown(duration, display, container) {
    if (homeInterval) clearInterval(homeInterval);

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
            clearInterval(homeInterval);
            container.innerHTML = "Mã QR đã hết hạn!";
            if (qrImg) qrImg.style.opacity = '0.2';
        }
    }

    updateDisplay();
    homeInterval = setInterval(updateDisplay, 1000);
}

window.addToCart = function (id, name, price) {
    Swal.fire({
        icon: 'success',
        title: 'Đã thêm vào giỏ',
        text: `${name} đã được thêm vào giỏ hàng của bạn (Demo)`,
        timer: 2000,
        showConfirmButton: false
    });
};
