function openTab(evt, tabName) {
    // Ẩn tất cả nội dung tab
    var tabcontent = document.getElementsByClassName("tab-content");
    for (var i = 0; i < tabcontent.length; i++) {
        tabcontent[i].classList.remove("active");
    }

    // Xóa class active ở tất cả các nút tab
    var tablinks = document.getElementsByClassName("tab-btn");
    for (var i = 0; i < tablinks.length; i++) {
        tablinks[i].classList.remove("active");
    }

    // Hiện tab hiện tại và thêm class active vào nút
    document.getElementById(tabName).classList.add("active");
    evt.currentTarget.classList.add("active");

    // Load dữ liệu khi chuyển tab
    if (tabName === 'booking-history') {
        loadBookingHistory();
    } else if (tabName === 'transaction-history') {
        loadTransactionHistory();
    }
}

// Load lịch sử đặt sân
async function loadBookingHistory() {
    try {
        const response = await fetch('/api/transaction/bookings');
        if (!response.ok) {
            throw new Error('Không thể tải dữ liệu');
        }

        const bookings = await response.json();
        const tbody = document.querySelector('#booking-history tbody');

        if (bookings.length === 0) {
            tbody.innerHTML = `
                <tr>
                    <td colspan="6" style="text-align: center; padding: 40px; color: #999;">
                        <i class="bi bi-inbox" style="font-size: 48px;"></i>
                        <p style="margin-top: 10px;">Chưa có lịch sử đặt sân</p>
                    </td>
                </tr>
            `;
            return;
        }

        tbody.innerHTML = bookings.map(booking => {
            const statusClass = getBookingStatusClass(booking.status);
            const statusText = booking.status;
            const actionButton = getBookingActionButton(booking.status, booking.bookingCode);

            return `
                <tr>
                    <td>${booking.bookingCode}</td>
                    <td>
                        <div class="court-info">
                            <span class="court-name">${booking.pitchName}</span>
                            <span class="court-date">${formatDate(booking.bookingDate)}</span>
                        </div>
                    </td>
                    <td>${formatTime(booking.startTime)} - ${formatTime(booking.endTime)}</td>
                    <td class="price">${formatCurrency(booking.totalAmount)}</td>
                    <td><span class="badge ${statusClass}">${statusText}</span></td>
                    <td>${actionButton}</td>
                </tr>
            `;
        }).join('');
    } catch (error) {
        console.error('Lỗi khi tải lịch sử đặt sân:', error);
        alert('Không thể tải dữ liệu. Vui lòng thử lại!');
    }
}

// Load lịch sử giao dịch
async function loadTransactionHistory() {
    try {
        const response = await fetch('/api/transaction/history');
        if (!response.ok) {
            throw new Error('Không thể tải dữ liệu');
        }

        const transactions = await response.json();
        const tbody = document.querySelector('#transaction-history tbody');

        if (transactions.length === 0) {
            tbody.innerHTML = `
                <tr>
                    <td colspan="5" style="text-align: center; padding: 40px; color: #999;">
                        <i class="bi bi-inbox" style="font-size: 48px;"></i>
                        <p style="margin-top: 10px;">Chưa có giao dịch nào</p>
                    </td>
                </tr>
            `;
            return;
        }

        tbody.innerHTML = transactions.map(trans => {
            const statusClass = getTransactionStatusClass(trans.status);
            const typeClass = trans.isPositive ? 'deposit' : 'payment';
            const amountClass = trans.isPositive ? 'positive' : 'negative';

            return `
                <tr>
                    <td>${trans.transactionCode}</td>
                    <td>
                        <div class="trans-info">
                            <span class="trans-type ${typeClass}">${trans.transactionType}</span>
                            <span class="trans-method">${trans.transactionSource || 'N/A'}</span>
                        </div>
                    </td>
                    <td>${formatDateTime(trans.date)}</td>
                    <td class="amount ${amountClass}">${trans.amountDisplay}</td>
                    <td><span class="badge ${statusClass}">${trans.status}</span></td>
                </tr>
            `;
        }).join('');
    } catch (error) {
        console.error('Lỗi khi tải lịch sử giao dịch:', error);
        alert('Không thể tải dữ liệu. Vui lòng thử lại!');
    }
}

// Helper functions
function getBookingStatusClass(status) {
    switch (status) {
        case 'Đã hoàn thành': return 'success';
        case 'Sắp tới': return 'warning';
        case 'Đã hủy': return 'danger';
        default: return '';
    }
}

function getTransactionStatusClass(status) {
    switch (status) {
        case 'Thành công': return 'success';
        case 'Chờ xử lý': return 'warning';
        case 'Đã hủy': return 'danger';
        default: return '';
    }
}

function getBookingActionButton(status, bookingCode) {
    switch (status) {
        case 'Đã hoàn thành':
            return `<button class="btn-action" onclick="viewDetail('${bookingCode}')">Chi tiết</button>`;
        case 'Sắp tới':
            return `<button class="btn-action" onclick="cancelBooking('${bookingCode}')">Hủy sân</button>`;
        case 'Đã hủy':
            return `<button class="btn-action" onclick="rebookPitch('${bookingCode}')">Đặt lại</button>`;
        default:
            return '';
    }
}

function formatDate(dateString) {
    const date = new Date(dateString);
    return date.toLocaleDateString('vi-VN');
}

function formatDateTime(dateString) {
    const date = new Date(dateString);
    return date.toLocaleDateString('vi-VN') + ' ' + date.toLocaleTimeString('vi-VN', { hour: '2-digit', minute: '2-digit' });
}

function formatTime(timeString) {
    // TimeSpan format: "HH:mm:ss"
    const parts = timeString.split(':');
    return `${parts[0]}:${parts[1]}`;
}

function formatCurrency(amount) {
    return new Intl.NumberFormat('vi-VN').format(amount) + '₫';
}

// Action handlers
function viewDetail(bookingCode) {
    window.location.href = `/Booking/Detail/${bookingCode}`;
}

function cancelBooking(bookingCode) {
    if (confirm('Bạn có chắc chắn muốn hủy đặt sân này?')) {
        // TODO: Call API to cancel booking
        alert('Chức năng đang phát triển');
    }
}

function rebookPitch(bookingCode) {
    window.location.href = '/Booking/Create';
}

// Load dữ liệu khi trang load lần đầu
document.addEventListener('DOMContentLoaded', function () {
    loadBookingHistory(); // Load tab đầu tiên
});