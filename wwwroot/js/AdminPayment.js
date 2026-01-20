document.addEventListener('DOMContentLoaded', function () {
    const balanceSpan = document.getElementById('depositUserBalance');
    const phoneInput = document.getElementById('depositUser');

    // Cấu hình mặc định cho SweetAlert2 để đồng bộ giao diện
    const Toast = Swal.mixin({
        toast: true,
        position: 'top-end',
        showConfirmButton: false,
        timer: 3000,
        timerProgressBar: true
    });

    // 1. Chuyển Tab
    document.querySelectorAll('.tab-btn').forEach(btn => {
        btn.addEventListener('click', function () {
            document.querySelectorAll('.tab-btn, .tab-content').forEach(el => el.classList.remove('active'));
            this.classList.add('active');
            document.getElementById(`${this.dataset.tab}-tab`).classList.add('active');

            if (this.dataset.tab === 'purchase') {
                loadProducts();
            }
        });
    });

    // 2. Tìm khách hàng bằng SĐT (Tab Nạp tiền)
    phoneInput.addEventListener('change', async function () {
        const phone = this.value.trim();
        if (phone.length < 10) return;

        try {
            const res = await fetch(`/AdminPayment/GetUserByPhone?phone=${phone}`);
            if (res.ok) {
                const data = await res.json();
                balanceSpan.textContent = new Intl.NumberFormat('vi-VN').format(data.walletBalance) + 'đ';
                balanceSpan.dataset.userId = data.userId;
                Toast.fire({ icon: 'success', title: 'Đã tìm thấy khách hàng' });
            } else {
                Swal.fire({ icon: 'error', title: 'Lỗi', text: 'Số điện thoại này chưa đăng ký thành viên!' });
                balanceSpan.textContent = '0đ';
                balanceSpan.dataset.userId = "";
            }
        } catch (e) { console.error(e); }
    });

    // 3. Submit nạp tiền
    document.getElementById('depositForm').addEventListener('submit', async function (e) {
        e.preventDefault();
        const userId = balanceSpan.dataset.userId;
        if (!userId) {
            Swal.fire({ icon: 'warning', title: 'Chú ý', text: 'Vui lòng nhập SĐT khách hàng hợp lệ trước!' });
            return;
        }

        // Hiển thị xác nhận trước khi nạp
        const confirm = await Swal.fire({
            title: 'Xác nhận nạp tiền?',
            text: `Bạn có chắc chắn muốn nạp tiền cho khách hàng này không?`,
            icon: 'question',
            showCancelButton: true,
            confirmButtonColor: '#28a745',
            cancelButtonColor: '#d33',
            confirmButtonText: 'Đồng ý nạp',
            cancelButtonText: 'Hủy'
        });

        if (confirm.isConfirmed) {
            Swal.showLoading(); // Hiển thị loading khi đang gọi API

            const dto = {
                UserID: parseInt(userId),
                Amount: parseFloat(document.getElementById('depositAmount').value),
                PaymentMethod: document.getElementById('depositMethod').value,
                Message: document.getElementById('depositMessage').value
            };

            try {
                const res = await fetch('/AdminPayment/CreateDeposit', {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify(dto)
                });
                const result = await res.json();

                if (result.success) {
                    await Swal.fire({
                        icon: 'success',
                        title: 'Thành công',
                        text: 'Nạp tiền thành công!',
                        confirmButtonText: 'OK'
                    });
                    location.reload();
                } else {
                    Swal.fire({ icon: 'error', title: 'Thất bại', text: result.message });
                }
            } catch (e) {
                Swal.fire({ icon: 'error', title: 'Lỗi', text: 'Không thể kết nối đến máy chủ!' });
            }
        }
    });

    // 4. Nút Đặt lại
    document.querySelector('.btn-reset-form').addEventListener('click', function () {
        balanceSpan.textContent = '0đ';
        balanceSpan.dataset.userId = "";
    });

    // 5. Load danh sách sản phẩm
    async function loadProducts() {
        const productSelect = document.getElementById('purchaseProduct');
        try {
            const res = await fetch('/AdminPayment/GetProducts');
            if (res.ok) {
                const products = await res.json();
                productSelect.innerHTML = '<option value="">-- Chọn sản phẩm --</option>';
                products.forEach(p => {
                    const option = document.createElement('option');
                    option.value = p.productId;
                    option.textContent = `${p.productName} - ${p.price.toLocaleString('vi-VN')}đ`;
                    option.dataset.price = p.price;
                    option.dataset.stock = p.stock;
                    productSelect.appendChild(option);
                });
            }
        } catch (e) { console.error('Lỗi load sản phẩm:', e); }
    }

    // 6. Tìm khách hàng bằng SĐT (Tab Mua hàng)
    document.getElementById('purchaseUser').addEventListener('change', async function () {
        const phone = this.value.trim();
        if (phone.length < 10) return;

        try {
            const res = await fetch(`/AdminPayment/GetUserByPhone?phone=${phone}`);
            if (res.ok) {
                const data = await res.json();
                document.getElementById('purchaseUserBalance').textContent = new Intl.NumberFormat('vi-VN').format(data.walletBalance) + 'đ';
                document.getElementById('purchaseUserBalance').dataset.userId = data.userId;
                Toast.fire({ icon: 'success', title: 'Đã nhận diện khách hàng' });
            } else {
                Swal.fire({ icon: 'error', title: 'Lỗi', text: 'Không tìm thấy khách hàng!' });
                document.getElementById('purchaseUserBalance').textContent = '0đ';
                document.getElementById('purchaseUserBalance').dataset.userId = "";
            }
        } catch (e) { console.error(e); }
    });

    // 7. Cập nhật giá và tồn kho
    document.getElementById('purchaseProduct').addEventListener('change', function () {
        const selectedOption = this.options[this.selectedIndex];
        if (!selectedOption.value) return;

        const price = selectedOption.dataset.price || 0;
        const stock = selectedOption.dataset.stock || 0;

        document.getElementById('purchasePrice').value = parseFloat(price).toLocaleString('vi-VN') + 'đ';
        document.getElementById('productStock').textContent = stock;
        updatePurchaseTotal();
    });

    // 8. Tính tổng tiền
    document.getElementById('purchaseQuantity').addEventListener('input', updatePurchaseTotal);

    function updatePurchaseTotal() {
        const selectedOption = document.getElementById('purchaseProduct').selectedOptions[0];
        const price = parseFloat(selectedOption?.dataset.price || 0);
        const quantity = parseInt(document.getElementById('purchaseQuantity').value || 0);
        const total = price * quantity;
        document.getElementById('purchaseTotal').value = total.toLocaleString('vi-VN') + 'đ';
    }

    // 9. Submit mua hàng
    document.getElementById('purchaseForm').addEventListener('submit', function (e) {
        e.preventDefault();
        Swal.fire({
            icon: 'info',
            title: 'Thông báo',
            text: 'Chức năng mua hàng đang được xử lý ở phía Backend!',
            confirmButtonColor: '#3085d6'
        });
    });
});