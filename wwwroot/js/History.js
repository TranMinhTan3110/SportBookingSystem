// ============================================
// BIẾN TOÀN CỤC
// ============================================
let currentBookingPage = 1;
let currentTransactionPage = 1;
let currentTransferPage = 1;
const pageSize = 10;

// ============================================
// LOAD LỊCH SỬ ĐẶT SÂN (TAB 1) 
// ============================================
async function loadBookingHistory(page = 1) {
    try {
        const response = await fetch(`/api/transaction/bookings?page=${page}&pageSize=${pageSize}`);
        if (!response.ok) throw new Error('Không thể tải dữ liệu');

        const result = await response.json();
        console.log('📦 Booking API Response:', result);

        const tbody = document.querySelector('#booking-tbody');

        if (!result.data || result.data.length === 0) {
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

        tbody.innerHTML = result.data.map(booking => {
           
            let statusClass, statusText;

            // Tìm đoạn này trong History.js và thay thế
            switch (booking.status) {
                case 1: // PendingConfirm (Chờ xác nhận)
                    statusClass = 'warning';
                    statusText = 'Chờ xác nhận';
                    break;
                case 2: // CheckedIn (Đã check-in / Đã nhận sân)
                    statusClass = 'success'; // Đổi từ secondary sang success để có màu xanh
                    statusText = 'Đã nhận sân';
                    break;
                case 3: // Completed (Hoàn thành)
                    statusClass = 'success'; // Màu xanh lá
                    statusText = 'Hoàn thành';
                    break;
                case -1: // Cancelled (Đã hủy)
                    statusClass = 'danger'; // Màu đỏ
                    statusText = 'Đã hủy';
                    break;
                default:
                    statusClass = 'secondary';
                    statusText = 'Không xác định';
            }

            const priceFormatted = booking.totalPrice
                ? new Intl.NumberFormat('vi-VN').format(booking.totalPrice) + '₫'
                : '0₫';

            let timeDisplay = '-';
            if (booking.timeRange) {
                timeDisplay = booking.timeRange;
            } else if (booking.startTime && booking.endTime) {
                const start = new Date(booking.startTime);
                const end = new Date(booking.endTime);
                timeDisplay = `${start.getHours().toString().padStart(2, '0')}:${start.getMinutes().toString().padStart(2, '0')} - ${end.getHours().toString().padStart(2, '0')}:${end.getMinutes().toString().padStart(2, '0')}`;
            }

            return `
            <tr>
                <td><strong>${booking.bookingCode}</strong></td>
                <td>
                    <div class="court-info">
                        <span class="court-name">${booking.pitchName}</span>
                        <span class="court-date">${formatDate(booking.bookingDate)}</span>
                    </div>
                </td>
                <td><strong>${timeDisplay}</strong></td>
                <td class="price" style="font-weight: bold; color: #ef4444;">-${priceFormatted}</td>
                <td><span class="badge ${statusClass}">${statusText}</span></td>
                <td>
                    ${booking.checkInCode ?
                    `<button class="btn-action btn-qr" data-code="${booking.checkInCode}" style="background: linear-gradient(135deg, #10b981, #059669); color: white; border: none; padding: 8px 16px; border-radius: 6px; cursor: pointer;">
                            <i class="fas fa-qrcode"></i> Xem QR
                        </button>`
                    : '<span class="text-muted" style="font-size: 13px;">Không có mã</span>'
                }
                </td>
            </tr>
        `;
        }).join('');

        document.querySelectorAll('.btn-qr').forEach(btn => {
            btn.addEventListener('click', function () {
                const code = this.dataset.code;
                showBookingQR(code);
            });
        });

        renderPagination('booking-pagination', result.currentPage, result.totalPages, loadBookingHistory);
        currentBookingPage = page;

    } catch (error) {
        console.error('❌ Lỗi load booking:', error);
    }
}

function showBookingQR(checkInCode) {
    fetch(`/UserProfile/GenerateBookingQr?checkInCode=${checkInCode}`)
        .then(res => res.json())
        .then(data => {
            if (data.success) {
                Swal.fire({
                    title: 'Mã Check-in Đặt Sân',
                    html: `
                        <div style="text-align: center;">
                            <p style="color: #64748b; margin-bottom: 10px;">Mã: <strong>${checkInCode}</strong></p>
                            <div style="background: #fff; padding: 10px; display: inline-block; border: 1px dashed #ccc; border-radius: 8px;">
                                <img src="data:image/png;base64,${data.qrCode}" alt="QR Code" style="width: 250px; height: 250px;">
                            </div>
                            <div class="alert alert-info mt-3" style="font-size: 13px; text-align: left;">
                                <i class="fas fa-info-circle"></i> Vui lòng đưa mã này cho nhân viên tại sân.
                            </div>
                        </div>
                    `,
                    icon: 'success',
                    confirmButtonText: 'Đóng',
                    confirmButtonColor: '#10b981'
                });
            }
        })
        .catch(err => console.error('❌ Lỗi tạo QR:', err));
}

async function loadTransactionHistory(page = 1) {
    try {
        const response = await fetch(`/api/transaction/history?page=${page}&pageSize=${pageSize}`);
        if (!response.ok) throw new Error('Không thể tải dữ liệu');

        const result = await response.json();
        const tbody = document.querySelector('#transaction-tbody');

        if (!result.data || result.data.length === 0) {
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
            const amountClass = trans.isPositive ? 'positive' : 'negative';
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
        console.error('❌ Lỗi load transaction:', error);
    }
}

async function loadTransferHistory(page = 1) {
    try {
        const response = await fetch(`/api/transaction/transfers?page=${page}&pageSize=${pageSize}`);
        if (!response.ok) throw new Error('Không thể tải dữ liệu');

        const result = await response.json();
        const tbody = document.getElementById('transfer-tbody');

        if (!result.data || result.data.length === 0) {
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

        tbody.innerHTML = result.data.map(t => {
            const amountClass = t.isSender ? 'negative' : 'positive';
            const formattedAmount = new Intl.NumberFormat('vi-VN').format(t.amount) + '₫';
            const amountDisplay = t.isSender ? `-${formattedAmount}` : `+${formattedAmount}`;

            return `
            <tr>
                <td>${t.transactionCode}</td>
                <td>${t.senderName}</td>
                <td>${t.receiverName}</td>
                <td>${formatDateTime(t.date)}</td>
                <td class="amount ${amountClass}" style="font-weight: bold; color: ${t.isSender ? '#ef4444' : '#10b981'}">
                    ${amountDisplay}
                </td>
                <td>${t.message || '-'}</td>
                <td><span class="badge ${t.status === 'Thành công' ? 'success' : 'warning'}">${t.status}</span></td>
            </tr>
        `;
        }).join('');

        renderPagination('transfer-pagination', result.currentPage, result.totalPages, loadTransferHistory);
        currentTransferPage = page;

    } catch (error) {
        console.error('❌ Lỗi load transfer:', error);
    }
}

function renderPagination(containerId, currentPage, totalPages, loadFunction) {
    const container = document.getElementById(containerId);

    if (totalPages < 1) {
        container.innerHTML = '';
        return;
    }

    let html = '<div class="pagination">';

    if (currentPage > 1) {
        html += `<button class="page-btn" onclick="${loadFunction.name}(${currentPage - 1})">‹</button>`;
    }

    const startPage = Math.max(1, currentPage - 2);
    const endPage = Math.min(totalPages, currentPage + 2);

    if (startPage > 1) {
        html += `<button class="page-btn" onclick="${loadFunction.name}(1)">1</button>`;
        if (startPage > 2) html += '<span class="page-dots">...</span>';
    }

    for (let i = startPage; i <= endPage; i++) {
        html += `<button class="page-btn ${i === currentPage ? 'active' : ''}" onclick="${loadFunction.name}(${i})">${i}</button>`;
    }

    if (endPage < totalPages) {
        if (endPage < totalPages - 1) html += '<span class="page-dots">...</span>';
        html += `<button class="page-btn" onclick="${loadFunction.name}(${totalPages})">${totalPages}</button>`;
    }

    if (currentPage < totalPages) {
        html += `<button class="page-btn" onclick="${loadFunction.name}(${currentPage + 1})">›</button>`;
    }

    html += '</div>';
    container.innerHTML = html;
}

function openTab(evt, tabName) {
    document.querySelectorAll('.tab-content').forEach(tab => tab.classList.remove('active'));
    document.querySelectorAll('.tab-btn').forEach(btn => btn.classList.remove('active'));

    document.getElementById(tabName).classList.add('active');
    evt.currentTarget.classList.add('active');

    if (tabName === 'booking-history') loadBookingHistory(currentBookingPage);
    else if (tabName === 'transaction-history') loadTransactionHistory(currentTransactionPage);
    else if (tabName === 'transfer-history') loadTransferHistory(currentTransferPage);
}

function formatDate(dateString) {
    return new Date(dateString).toLocaleDateString('vi-VN');
}

function formatDateTime(dateString) {
    const date = new Date(dateString);
    return date.toLocaleDateString('vi-VN') + ' ' + date.toLocaleTimeString('vi-VN', { hour: '2-digit', minute: '2-digit' });
}

document.addEventListener('DOMContentLoaded', function () {
    console.log('✅ History.js loaded');

    document.querySelectorAll('.tab-content').forEach(t => t.classList.remove('active'));
    document.getElementById('booking-history').classList.add('active');

    document.querySelectorAll('.tab-btn').forEach(b => b.classList.remove('active'));
    document.querySelectorAll('.tab-btn')[0].classList.add('active');

    loadBookingHistory(1);

    // ✅ THÊM EVENT LISTENER ĐỂ RELOAD KHI BOOKING MỚI ĐƯỢC TẠO
    window.addEventListener('bookingCreated', function () {
        console.log('🔔 Nhận được event bookingCreated từ booking.js');

        // Kiểm tra xem có đang ở tab booking history không
        const bookingTab = document.getElementById('booking-history');
        if (bookingTab && bookingTab.classList.contains('active')) {
            console.log('🔄 Đang ở tab Booking History, reloading...');
            loadBookingHistory(1);
        } else {
            console.log('ℹ️ Không ở tab Booking History, sẽ reload khi chuyển sang tab');
        }
    });
});