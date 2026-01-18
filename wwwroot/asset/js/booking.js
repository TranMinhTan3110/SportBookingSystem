// Set today's date as default
const dateInput = document.getElementById('bookingDate');
const today = new Date().toISOString().split('T')[0];
dateInput.value = today;

// Time slot selection
document.querySelectorAll('.time-slot').forEach(slot => {
    slot.addEventListener('click', function () {
        if (!this.classList.contains('booked')) {
            document.querySelectorAll('.time-slot').forEach(s => s.classList.remove('selected'));
            this.classList.add('selected');
        }
    });
});

// Filter clear
document.querySelector('.filter-clear').addEventListener('click', function () {
    document.querySelectorAll('.filter-option input').forEach(checkbox => {
        checkbox.checked = false;
    });
    document.querySelectorAll('.time-slot').forEach(slot => {
        slot.classList.remove('selected');
    });
});

// Deposit button
document.querySelectorAll('.btn-deposit').forEach(btn => {
    btn.addEventListener('click', function (e) {
        e.stopPropagation();
        const fieldName = this.closest('.field-card').querySelector('.field-name').textContent;
        const depositPrice = this.closest('.field-card').querySelector('.pricing-row:first-child .pricing-value').textContent;

        if (confirm(`Xác nhận đặt cọc ${depositPrice} cho ${fieldName}?`)) {
            alert(`Đang chuyển đến trang thanh toán đặt cọc...\nSân: ${fieldName}\nSố tiền: ${depositPrice}`);
        }
    });
});

// Book button
document.querySelectorAll('.btn-book').forEach(btn => {
    btn.addEventListener('click', function (e) {
        e.stopPropagation();
        const fieldName = this.closest('.field-card').querySelector('.field-name').textContent;
        const fullPrice = this.closest('.field-card').querySelector('.pricing-row:last-child .pricing-value').textContent;

        if (confirm(`Xác nhận đặt sân và thanh toán full ${fullPrice} cho ${fieldName}?`)) {
            alert(`Đang chuyển đến trang thanh toán...\nSân: ${fieldName}\nSố tiền: ${fullPrice}`);
        }
    });
});

// Field card click
document.querySelectorAll('.field-card').forEach(card => {
    card.addEventListener('click', function (e) {
        if (!e.target.closest('button')) {
            const fieldName = this.querySelector('.field-name').textContent;
            alert(`Xem chi tiết: ${fieldName}`);
        }
    });
});
// Cập nhật Filter clear cho nút mới
document.querySelector('.filter-clear-btn').addEventListener('click', function () {
    document.querySelectorAll('.filter-option input').forEach(checkbox => {
        checkbox.checked = false;
    });
    document.querySelectorAll('.time-slot').forEach(slot => {
        slot.classList.remove('selected');
    });
    // Reset thêm các input khác nếu cần
    document.getElementById('bookingDate').value = today;
});

// Sự kiện cho nút Lọc
document.querySelector('.filter-submit-btn').addEventListener('click', function () {
    alert('Đang thực hiện lọc dữ liệu...');
});