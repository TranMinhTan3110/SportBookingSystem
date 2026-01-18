// Set today's date as default
const dateInput = document.getElementById('bookingDate');
const today = new Date().toISOString().split('T')[0];
dateInput.value = today;

// Time slot selection
let selectedSlotId = null;
document.querySelectorAll('.time-slot').forEach(slot => {
    slot.addEventListener('click', function () {
        document.querySelectorAll('.time-slot').forEach(s => s.classList.remove('selected'));
        this.classList.add('selected');
        selectedSlotId = this.getAttribute('data-slot-id');
        loadPitches(); // Tự động load khi chọn slot
    });
});

// Hàm lấy dữ liệu filter
function getFilterData() {
    const selectedDate = document.getElementById('bookingDate').value;

    // Lấy các category được chọn
    const categoryCheckboxes = document.querySelectorAll('.filter-section:nth-child(3) .filter-option input[type="checkbox"]:checked');
    const categoryIds = Array.from(categoryCheckboxes).map(cb => {
        const label = cb.closest('.filter-option');
        return parseInt(label.getAttribute('data-category-id'));
    }).filter(id => !isNaN(id));

    // Lấy status filter
    const statusCheckboxes = document.querySelectorAll('.filter-section:nth-child(4) .filter-option input[type="checkbox"]:checked');
    const statusFilter = Array.from(statusCheckboxes).map(cb => {
        const label = cb.closest('.filter-option');
        return label.getAttribute('data-status');
    }).filter(status => status);

    // Lấy giá min/max
    const minPriceText = document.querySelector('.price-input:first-child input').value;
    const maxPriceText = document.querySelector('.price-input:last-child input').value;

    const minPrice = parsePrice(minPriceText);
    const maxPrice = parsePrice(maxPriceText);

    return {
        date: selectedDate,
        slotId: selectedSlotId ? parseInt(selectedSlotId) : null,
        categoryIds: categoryIds.length > 0 ? categoryIds : null,
        statusFilter: statusFilter.length > 0 ? statusFilter : null,
        minPrice: minPrice,
        maxPrice: maxPrice
    };
}

// Hàm chuyển đổi giá từ text sang number
function parsePrice(priceText) {
    if (!priceText) return null;
    const cleaned = priceText.replace(/[^\d]/g, '');
    return cleaned ? parseInt(cleaned) : null;
}

// Hàm format giá tiền
function formatPrice(price) {
    return new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(price);
}

// Hàm render danh sách sân
function renderPitches(pitchSlots) {
    const grid = document.querySelector('.fields-grid');
    if (!grid) return;

    if (!pitchSlots || pitchSlots.length === 0) {
        grid.innerHTML = '<div class="no-results" style="text-align: center; padding: 40px; color: #94a3b8;">Không tìm thấy sân phù hợp với bộ lọc của bạn</div>';
        return;
    }

    grid.innerHTML = pitchSlots.map(pitch => {
        const gradients = [
            'linear-gradient(135deg, #10B981, #059669)',
            'linear-gradient(135deg, #3B82F6, #2563EB)',
            'linear-gradient(135deg, #8B5CF6, #7C3AED)',
            'linear-gradient(135deg, #06B6D4, #0891B2)',
            'linear-gradient(135deg, #14B8A6, #0D9488)',
            'linear-gradient(135deg, #F59E0B, #D97706)',
            'linear-gradient(135deg, #EC4899, #DB2777)',
            'linear-gradient(135deg, #EF4444, #DC2626)'
        ];
        const gradient = gradients[pitch.pitchId % gradients.length];

        const isBooked = pitch.status === 'booked';
        const depositBtnStyle = isBooked ? 'opacity: 0.5; cursor: not-allowed;' : '';
        const bookBtnStyle = isBooked ? 'background: #94a3b8; cursor: not-allowed;' : '';
        const bookBtnText = isBooked ? 'Hết chỗ' : 'Đặt sân';
        const noteStyle = isBooked ? 'color: #ef4444; font-weight: bold;' : '';
        const noteText = isBooked ? '* Khung giờ này đã được đặt kín.' : '* Đặt cọc: Thanh toán 30% trước, còn lại trả khi đến sân';

        // Xử lý hình ảnh
        const imageUrl = pitch.imageUrl && pitch.imageUrl !== '/images/default-pitch.jpg'
            ? pitch.imageUrl
            : null;

        return `
            <div class="field-card">
                <div class="field-image" style="background: ${imageUrl ? `url(${imageUrl}) center/cover, ${gradient}` : gradient};">
                    <span class="field-status-badge ${pitch.status}">${pitch.statusText}</span>
                </div>
                <div class="field-content">
                    <div class="field-header">
                        <h3 class="field-name">${pitch.pitchName}</h3>
                        <span class="field-type">⚽ Sân ${pitch.capacity} người</span>
                    </div>

                    <div class="field-pricing">
                        <div class="pricing-header">
                            <span class="pricing-label">Bảng giá - ${pitch.slotName}</span>
                            <span class="pricing-time">${pitch.timeRange}</span>
                        </div>
                        <div class="pricing-row">
                            <span class="pricing-type">Giá đặt cọc (30%)</span>
                            <span class="pricing-value">${formatPrice(pitch.depositPrice)}</span>
                        </div>
                        <div class="pricing-row">
                            <span class="pricing-type">Giá đặt sân full</span>
                            <span class="pricing-value">${formatPrice(pitch.fullPrice)}</span>
                        </div>
                        <div class="deposit-note" style="${noteStyle}">
                            ${noteText}
                        </div>
                    </div>

                    <div class="field-actions">
                        <button class="btn-deposit" ${isBooked ? 'disabled' : ''} style="${depositBtnStyle}" 
                            data-pitch-id="${pitch.pitchId}" 
                            data-slot-id="${pitch.slotId}"
                            data-price="${pitch.depositPrice}" 
                            data-name="${pitch.pitchName}"
                            data-time="${pitch.timeRange}">
                            <span class="btn-icon">💳</span>
                            <span>Đặt cọc</span>
                        </button>
                        <button class="btn-book" ${isBooked ? 'disabled' : ''} style="${bookBtnStyle}" 
                            data-pitch-id="${pitch.pitchId}" 
                            data-slot-id="${pitch.slotId}"
                            data-price="${pitch.fullPrice}" 
                            data-name="${pitch.pitchName}"
                            data-time="${pitch.timeRange}">
                            <span class="btn-icon">${isBooked ? '✕' : '✓'}</span>
                            <span>${bookBtnText}</span>
                        </button>
                    </div>
                </div>
            </div>
        `;
    }).join('');

    // Gắn lại sự kiện cho các nút
    attachButtonEvents();
}

// Hàm gắn sự kiện cho các nút
function attachButtonEvents() {
    // Deposit button
    document.querySelectorAll('.btn-deposit').forEach(btn => {
        btn.addEventListener('click', function (e) {
            e.stopPropagation();
            if (this.disabled) return;

            const fieldName = this.getAttribute('data-name');
            const timeRange = this.getAttribute('data-time');
            const depositPrice = formatPrice(parseFloat(this.getAttribute('data-price')));
            const pitchId = this.getAttribute('data-pitch-id');
            const slotId = this.getAttribute('data-slot-id');

            if (confirm(`Xác nhận đặt cọc cho:\n\nSân: ${fieldName}\nKhung giờ: ${timeRange}\nSố tiền: ${depositPrice}`)) {
                alert(`Đang chuyển đến trang thanh toán...\n\nThông tin:\n- Sân: ${fieldName}\n- Khung giờ: ${timeRange}\n- Tiền cọc: ${depositPrice}\n- Pitch ID: ${pitchId}\n- Slot ID: ${slotId}`);
                // TODO: Chuyển đến trang thanh toán
                // window.location.href = `/Payment/Deposit?pitchId=${pitchId}&slotId=${slotId}`;
            }
        });
    });

    // Book button
    document.querySelectorAll('.btn-book').forEach(btn => {
        btn.addEventListener('click', function (e) {
            e.stopPropagation();
            if (this.disabled) return;

            const fieldName = this.getAttribute('data-name');
            const timeRange = this.getAttribute('data-time');
            const fullPrice = formatPrice(parseFloat(this.getAttribute('data-price')));
            const pitchId = this.getAttribute('data-pitch-id');
            const slotId = this.getAttribute('data-slot-id');

            if (confirm(`Xác nhận đặt sân và thanh toán full:\n\nSân: ${fieldName}\nKhung giờ: ${timeRange}\nSố tiền: ${fullPrice}`)) {
                alert(`Đang chuyển đến trang thanh toán...\n\nThông tin:\n- Sân: ${fieldName}\n- Khung giờ: ${timeRange}\n- Tổng tiền: ${fullPrice}\n- Pitch ID: ${pitchId}\n- Slot ID: ${slotId}`);
                // TODO: Chuyển đến trang thanh toán
                // window.location.href = `/Payment/Full?pitchId=${pitchId}&slotId=${slotId}`;
            }
        });
    });

    // Field card click
    document.querySelectorAll('.field-card').forEach(card => {
        card.addEventListener('click', function (e) {
            if (!e.target.closest('button')) {
                const fieldName = this.querySelector('.field-name').textContent;
                alert(`Xem chi tiết: ${fieldName}`);
                // TODO: Chuyển đến trang chi tiết sân
            }
        });
    });
}

// Hàm load dữ liệu từ API
async function loadPitches() {
    const filterData = getFilterData();
    const grid = document.querySelector('.fields-grid');

    // Hiển thị loading
    if (grid) {
        grid.innerHTML = '<div style="text-align: center; padding: 40px; color: #94a3b8;">Đang tải dữ liệu...</div>';
    }

    try {
        const response = await fetch('/Booking/GetFilteredPitches', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify(filterData)
        });

        const result = await response.json();

        if (result.success) {
            renderPitches(result.data.pitchSlots);

            // Cập nhật số lượng hiển thị
            const countDiv = document.querySelector('.fields-count');
            if (countDiv) {
                countDiv.innerHTML = `Hiển thị <strong>${result.data.displayCount}</strong> trong tổng số <strong>${result.data.totalCount}</strong> kết quả`;
            }
        } else {
            if (grid) {
                grid.innerHTML = `<div style="text-align: center; padding: 40px; color: #ef4444;">Lỗi: ${result.message}</div>`;
            }
        }
    } catch (error) {
        console.error('Error loading pitches:', error);
        if (grid) {
            grid.innerHTML = '<div style="text-align: center; padding: 40px; color: #ef4444;">Không thể tải dữ liệu. Vui lòng thử lại sau.</div>';
        }
    }
}

// Filter clear
document.querySelector('.filter-clear-btn').addEventListener('click', function () {
    document.querySelectorAll('.filter-option input').forEach(checkbox => {
        checkbox.checked = false;
    });
    document.querySelectorAll('.time-slot').forEach(slot => {
        slot.classList.remove('selected');
    });
    selectedSlotId = null;
    document.getElementById('bookingDate').value = today;
    document.querySelector('.price-input:first-child input').value = '0₫';
    document.querySelector('.price-input:last-child input').value = '2,000,000₫';

    // Load lại dữ liệu
    loadPitches();
});

// Sự kiện cho nút Lọc
document.querySelector('.filter-submit-btn').addEventListener('click', function () {
    loadPitches();
});

// Sự kiện khi thay đổi ngày
document.getElementById('bookingDate').addEventListener('change', function () {
    loadPitches();
});

// Sự kiện khi thay đổi checkbox category hoặc status
document.querySelectorAll('.filter-section:nth-child(3) .filter-option input[type="checkbox"], .filter-section:nth-child(4) .filter-option input[type="checkbox"]').forEach(checkbox => {
    checkbox.addEventListener('change', function () {
        // Có thể auto-load hoặc đợi nhấn nút Lọc
        // loadPitches(); // Bỏ comment nếu muốn tự động lọc
    });
});

// Load dữ liệu lần đầu khi trang được tải
document.addEventListener('DOMContentLoaded', function () {
    loadPitches();
});