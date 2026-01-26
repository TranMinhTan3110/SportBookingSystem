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
    const amount = document.getElementById('topupAmount').value;

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