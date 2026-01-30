/**
 * AdminQRHandler.js
 * Quản lý logic quét mã QR dùng chung cho hệ quản trị (Sân & Đồ)
 */

const AdminQR = (function () {
    const toast = Swal.mixin({
        toast: true,
        position: 'top-end',
        showConfirmButton: false,
        timer: 3000,
        timerProgressBar: true
    });

    let config = {
        onSuccess: null // Callback khi xử lý thành công
    };

    /**
     * Khởi tạo component
     * @param {Object} options 
     */
    function init(options = {}) {
        config = { ...config, ...options };
        _initEventListeners();
    }

    function _initEventListeners() {
        const modalEl = document.getElementById('qrFulfillmentModal');
        if (modalEl) {
            modalEl.addEventListener('hidden.bs.modal', function () {
                _resetFulfillmentModal();
            });
        }

        document.getElementById('btnFulfillSuccess')?.addEventListener('click', () => _processFulfillment('Thành công'));
        document.getElementById('btnFulfillCancel')?.addEventListener('click', () => _processFulfillment('Đã hủy'));
    }

    function _resetFulfillmentModal() {
        const loading = document.getElementById('fulfillmentLoading');
        const content = document.getElementById('fulfillmentContent');
        if (loading) loading.classList.remove('d-none');
        if (content) content.classList.add('d-none');
    }

    /**
     * Xử lý quét QR Đặt sân
     */
    async function handleBookingScan(bookingCode) {
        if (!bookingCode) return;

        try {
            const res = await fetch('/Booking/ScanBookingQr', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(bookingCode)
            });
            const response = await res.json();

            if (response.success) {
                const info = response.data;
                const result = await Swal.fire({
                    title: 'Thông tin đặt sân',
                    html: `
                        <div style="text-align:left; font-size:1.1em; line-height: 1.6;">
                            <p><b>Khách:</b> ${info.customerName}</p>
                            <p>🏟 <b>Sân:</b> <span class="text-primary fw-bold">${info.pitchName}</span></p>
                            <p><b>Ngày:</b> ${info.date}</p>
                            <p><b>Giờ đá:</b> <span class="text-danger fw-bold">${info.time}</span></p>
                            <hr>
                            <p class="text-success text-center mb-0"><i class="fas fa-check-circle"></i> Đủ điều kiện nhận sân</p>
                        </div>
                    `,
                    icon: 'info',
                    showCancelButton: true,
                    confirmButtonText: 'Xác nhận & Vào sân',
                    cancelButtonText: 'Hủy',
                    confirmButtonColor: '#198754',
                    cancelButtonColor: '#6c757d'
                });

                if (result.isConfirmed) {
                    await _confirmBookingCheckIn(bookingCode);
                }
            } else {
                Swal.fire({
                    icon: 'warning',
                    title: 'Không thể nhận sân',
                    text: response.message,
                    confirmButtonText: 'Đã hiểu'
                });
            }
        } catch (e) {
            console.error('QR Scan Error:', e);
            Swal.fire('Lỗi', 'Không kết nối được server', 'error');
        }
    }

    async function _confirmBookingCheckIn(code) {
        try {
            const res = await fetch('/Booking/ConfirmBookingCheckIn', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(code)
            });
            const result = await res.json();

            if (result.success) {
                Swal.fire('Thành công', 'Check-in hoàn tất!', 'success').then(() => {
                    if (typeof config.onSuccess === 'function') config.onSuccess(code, 'booking');
                    else location.reload();
                });
            } else {
                Swal.fire('Lỗi', result.message, 'error');
            }
        } catch (e) {
            Swal.fire('Lỗi', 'Lỗi hệ thống', 'error');
        }
    }

    /**
     * Hiển thị modal xử lý đơn hàng (Đồ uống/Phụ kiện)
     */
    async function showFulfillmentModal(orderId) {
        Swal.fire({
            title: 'Đang kiểm tra...',
            text: 'Vui lòng đợi',
            allowOutsideClick: false,
            didOpen: () => { Swal.showLoading(); }
        });

        try {
            const res = await fetch(`/AdminPayment/GetOrderForFulfillment?orderId=${orderId}`);
            const data = await res.json();
            Swal.close();

            if (data.error) {
                Swal.fire({
                    icon: 'warning',
                    title: 'Thông báo',
                    text: data.message,
                    confirmButtonText: 'Đã hiểu',
                    confirmButtonColor: '#dc3545'
                });
                return;
            }

            const modalEl = document.getElementById('qrFulfillmentModal');
            if (!modalEl) {
                Swal.fire('Lỗi', 'Giao diện xử lý chưa sẵn sàng.', 'error');
                return;
            }

            const modal = bootstrap.Modal.getInstance(modalEl) || new bootstrap.Modal(modalEl);

            // Fill data
            document.getElementById('fOrderId').value = data.orderId;
            document.getElementById('fOrderCode').textContent = data.orderCode;
            document.getElementById('fCustomerName').textContent = data.customerName;
            document.getElementById('fProductName').textContent = data.productName;
            document.getElementById('fQuantity').textContent = `x ${data.quantity}`;
            document.getElementById('fTotalAmount').textContent = new Intl.NumberFormat('vi-VN').format(data.totalAmount) + 'đ';

            document.getElementById('fulfillmentLoading').classList.add('d-none');
            document.getElementById('fulfillmentContent').classList.remove('d-none');

            modal.show();
        } catch (e) {
            console.error('Fulfillment Error:', e);
            Swal.close();
            Swal.fire({ icon: 'error', title: 'Lỗi', text: 'Không thể kết nối đến máy chủ' });
        }
    }

    async function _processFulfillment(status) {
        const orderId = document.getElementById('fOrderId').value;
        const confirm = await Swal.fire({
            title: status === 'Thành công' ? 'Xác nhận đơn hàng?' : 'Hủy đơn hàng?',
            text: status === 'Thành công' ? 'Xác nhận khách đã nhận đồ?' : 'Hủy đơn và hoàn tiền cho khách?',
            icon: status === 'Thành công' ? 'question' : 'warning',
            showCancelButton: true,
            confirmButtonText: 'Đồng ý',
            cancelButtonText: 'Không',
            confirmButtonColor: status === 'Thành công' ? '#198754' : '#dc3545'
        });

        if (confirm.isConfirmed) {
            try {
                const res = await fetch('/AdminPayment/UpdateOrderStatus', {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify({ orderId: parseInt(orderId), newStatus: status })
                });
                const result = await res.json();
                if (result.success) {
                    const modalEl = document.getElementById('qrFulfillmentModal');
                    bootstrap.Modal.getInstance(modalEl)?.hide();

                    const code = document.getElementById('fOrderCode').textContent.trim();
                    toast.fire({ icon: 'success', title: result.message });

                    if (typeof config.onSuccess === 'function') config.onSuccess(code, 'order');
                    else location.reload();
                } else {
                    Swal.fire('Lỗi', result.message, 'error');
                }
            } catch (e) {
                Swal.fire('Lỗi', 'Lỗi kết nối máy chủ', 'error');
            }
        }
    }

    return {
        init: init,
        handleBookingScan: handleBookingScan,
        showFulfillmentModal: showFulfillmentModal
    };
})();
