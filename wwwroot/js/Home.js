
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
//xử lý khi bấm hủy 

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