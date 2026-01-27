// ============================================
// BIẾN TOÀN CỤC
// ============================================
let currentBookingPage = 1;
let currentTransactionPage = 1;
let currentTransferPage = 1;
const pageSize = 10; // Số bản ghi mỗi trang

// ============================================
// LOAD LỊCH SỬ ĐẶT SÂN
// ============================================
async function loadBookingHistory(page = 1) {
    try {
        const response = await fetch(`/api/transaction/bookings?page=${page}&pageSize=${pageSize}`);
        if (!response.ok) throw new Error('Không thể tải dữ liệu');

        const result = await response.json();
        const tbody = document.querySelector('#booking-tbody');

        if (result.data.length === 0) {
            tbody.innerHTML = `
                <tr>
                    <td colspan="6" style="text-align: center; padding: 40px;">
                        <i class="bi bi-inbox" style="font-size: 48px;"></i>
                        <p>Chưa có lịch sử đặt sân</p>
                    </td>
                </tr>
            `;
            document.getElementById('booking-pagination').innerHTML = '';
            return;
        }

        tbody.innerHTML = result.data.map(booking => `
            <tr>
                <td>${booking.bookingCode}</td>
                <td>
                    <div class="court-info">
                        <span class="court-name">${booking.pitchName}</span>
                        <span class="court-date">${formatDate(booking.bookingDate)}</span>
                    </div>
                </td>
                <td>-</td>
                <td class="price">-</td>
                <td><span class="badge success">Hoàn thành</span></td>
                <td><button class="btn-action">Chi tiết</button></td>
            </tr>
        `).join('');

        // Render phân trang
        renderPagination('booking-pagination', result.currentPage, result.totalPages, loadBookingHistory);
        currentBookingPage = page;

    } catch (error) {
        console.error('Lỗi:', error);
    }
}

// ============================================
// LOAD LỊCH SỬ GIAO DỊCH
// ============================================
async function loadTransactionHistory(page = 1) {
    try {
        const response = await fetch(`/api/transaction/history?page=${page}&pageSize=${pageSize}`);
        if (!response.ok) throw new Error('Không thể tải dữ liệu');

        const result = await response.json();
        const tbody = document.querySelector('#transaction-tbody');

        if (result.data.length === 0) {
            tbody.innerHTML = `
                <tr>
                    <td colspan="5" style="text-align: center; padding: 40px;">
                        <i class="bi bi-inbox" style="font-size: 48px;"></i>
                        <p>Chưa có giao dịch nào</p>
                    </td>
                </tr>
            `;
            document.getElementById('transaction-pagination').innerHTML = '';
            return;
        }

        tbody.innerHTML = result.data.map(trans => {
            const statusClass = trans.status === 'Thành công' ? 'success' : 'warning';

            // Xử lý hiển thị tiền:
            // Nếu là tích cực (+) -> Màu xanh, có dấu +
            // Nếu là tiêu cực (-) -> Màu đỏ, có dấu - (Amount trong DB đã lưu số âm rồi thì cứ hiển thị, hoặc format lại)
            const amountClass = trans.isPositive ? 'positive' : 'negative';
            const sign = trans.isPositive ? '+' : ''; // Thêm dấu + nếu là dương, âm thì nó tự có dấu - từ DB rồi

            // Format số tiền đẹp (ví dụ: -300,000đ)
            const formattedAmount = new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(trans.amount);

            return `
                <tr>
                    <td>${trans.transactionCode}</td>
                    <td>
                        <div class="trans-info">
                            <span class="trans-type">${trans.transactionType}</span>
                            <span class="trans-method text-muted small">${trans.transactionSource || 'System'}</span>
                        </div>
                    </td>
                    <td>${formatDateTime(trans.date)}</td>
                    <td class="amount ${amountClass}" style="font-weight: bold; color: ${trans.isPositive ? '#10b981' : '#ef4444'}">
                        ${trans.isPositive ? '+' : ''}${formattedAmount}
                    </td>
                    <td><span class="badge ${statusClass}">${trans.status}</span></td>
                </tr>
            `;
        }).join('');

        renderPagination('transaction-pagination', result.currentPage, result.totalPages, loadTransactionHistory);
        currentTransactionPage = page;

    } catch (error) {
        console.error('Lỗi:', error);
    }
}

// ============================================
// LOAD LỊCH SỬ CHUYỂN TIỀN
// ============================================
async function loadTransferHistory(page = 1) {
    try {
        const response = await fetch(`/api/transaction/transfers?page=${page}&pageSize=${pageSize}`);
        if (!response.ok) throw new Error('Không thể tải dữ liệu');

        const result = await response.json();
        const tbody = document.getElementById('transfer-tbody');

        if (result.data.length === 0) {
            tbody.innerHTML = `
                <tr>
                    <td colspan="7" style="text-align:center; padding:40px;">
                        Chưa có giao dịch chuyển tiền nào
                    </td>
                </tr>
            `;
            document.getElementById('transfer-pagination').innerHTML = '';
            return;
        }

        tbody.innerHTML = result.data.map(t => `
            <tr>
                <td>${t.transactionCode}</td>
                <td>${t.senderName}</td>
                <td>${t.receiverName}</td>
                <td>${formatDateTime(t.date)}</td>
                <td class="amount ${t.amountClass}">${t.amountDisplay}</td>
                <td>${t.message || '-'}</td>
                <td><span class="badge ${t.status === 'Thành công' ? 'success' : 'warning'}">${t.status}</span></td>
            </tr>
        `).join('');

        renderPagination('transfer-pagination', result.currentPage, result.totalPages, loadTransferHistory);
        currentTransferPage = page;

    } catch (error) {
        console.error('Lỗi:', error);
    }
}

// ============================================
// RENDER PHÂN TRANG
// ============================================
function renderPagination(containerId, currentPage, totalPages, loadFunction) {
    const container = document.getElementById(containerId);

    // Nếu không có dữ liệu (0 trang) thì ẩn phân trang
    if (totalPages < 1) {
        container.innerHTML = '';
        return;
    }

    let html = '<div class="pagination">';

    // Nút Previous (chỉ hiện khi trang > 1)
    if (currentPage > 1) {
        html += `<button class="page-btn" onclick="${loadFunction.name}(${currentPage - 1})">‹</button>`;
    }

    // Tính toán dải trang hiển thị (tối đa 5 trang xung quanh trang hiện tại)
    const startPage = Math.max(1, currentPage - 2);
    const endPage = Math.min(totalPages, currentPage + 2);

    // Trang đầu tiên và dấu ...
    if (startPage > 1) {
        html += `<button class="page-btn" onclick="${loadFunction.name}(1)">1</button>`;
        if (startPage > 2) html += '<span class="page-dots">...</span>';
    }

    // Vòng lặp các trang ở giữa
    for (let i = startPage; i <= endPage; i++) {
        html += `<button class="page-btn ${i === currentPage ? 'active' : ''}" onclick="${loadFunction.name}(${i})">${i}</button>`;
    }

    // Trang cuối cùng và dấu ...
    if (endPage < totalPages) {
        if (endPage < totalPages - 1) html += '<span class="page-dots">...</span>';
        html += `<button class="page-btn" onclick="${loadFunction.name}(${totalPages})">${totalPages}</button>`;
    }

    // Nút Next (chỉ hiện khi chưa đến trang cuối)
    if (currentPage < totalPages) {
        html += `<button class="page-btn" onclick="${loadFunction.name}(${currentPage + 1})">›</button>`;
    }

    html += '</div>';
    container.innerHTML = html;
}

// ============================================
// CHUYỂN TAB
// ============================================
function openTab(evt, tabName) {
    document.querySelectorAll('.tab-content').forEach(tab => tab.classList.remove('active'));
    document.querySelectorAll('.tab-btn').forEach(btn => btn.classList.remove('active'));

    document.getElementById(tabName).classList.add('active');
    evt.currentTarget.classList.add('active');

    if (tabName === 'booking-history') loadBookingHistory(currentBookingPage);
    else if (tabName === 'transaction-history') loadTransactionHistory(currentTransactionPage);
    else if (tabName === 'transfer-history') loadTransferHistory(currentTransferPage);
}

// ============================================
// HELPER FUNCTIONS
// ============================================
function formatDate(dateString) {
    return new Date(dateString).toLocaleDateString('vi-VN');
}

function formatDateTime(dateString) {
    const date = new Date(dateString);
    return date.toLocaleDateString('vi-VN') + ' ' + date.toLocaleTimeString('vi-VN', { hour: '2-digit', minute: '2-digit' });
}

// ============================================
// KHỞI TẠO
// ============================================
document.addEventListener('DOMContentLoaded', function () {
    // Mặc định load tab Lịch sử Giao dịch
    // 1. Ẩn các tab khác, hiện tab transaction
    document.querySelectorAll('.tab-content').forEach(t => t.classList.remove('active'));
    document.getElementById('transaction-history').classList.add('active');

    // 2. Set active cho nút tab tương ứng
    document.querySelectorAll('.tab-btn').forEach(b => b.classList.remove('active'));
    // Giả sử nút thứ 2 là nút Lịch sử Giao dịch (index 1)
    document.querySelectorAll('.tab-btn')[1].classList.add('active');

    // 3. Load dữ liệu
    loadTransactionHistory(1);
});