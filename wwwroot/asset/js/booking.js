const today = new Date().toISOString().split('T')[0];
let currentPage = 1;
const pageSize = 9;

const bookingModalElement = document.getElementById('bookingModal');
const bookingModal = bookingModalElement ? new bootstrap.Modal(bookingModalElement) : null;

document.addEventListener('DOMContentLoaded', function () {
    console.log(' booking.js loaded');

    const modalDateInput = document.getElementById('modalBookingDate');
    if (modalDateInput) {
        modalDateInput.value = today;
        modalDateInput.min = today;
    }

    const isBookingPage = document.querySelector('.filter-sidebar');
    if (isBookingPage) {
        loadPitchesList();
    }

    modalDateInput?.addEventListener('change', function () {
        const pitchId = document.getElementById('modalPitchId').value;
        if (pitchId) loadSlotsForModal(pitchId, this.value);
    });

    document.querySelector('.filter-submit-btn')?.addEventListener('click', () => {
        currentPage = 1;
        loadPitchesList();
    });

    document.querySelector('.btn-reset-filter')?.addEventListener('click', function () {
        document.querySelectorAll('.filter-option input').forEach(cb => cb.checked = false);
        currentPage = 1;
        loadPitchesList();

        if (typeof Swal !== 'undefined') {
            Swal.mixin({
                toast: true, position: 'top-end', showConfirmButton: false, timer: 1500, timerProgressBar: true
            }).fire({ icon: 'success', title: 'Đã làm mới bộ lọc' });
        }
    });
});

async function loadPitchesList() {
    const grid = document.querySelector('.fields-grid');
    const countDiv = document.querySelector('.fields-count');

    const categoryIds = Array.from(document.querySelectorAll('.filter-option[data-category-id] input:checked'))
        .map(cb => parseInt(cb.closest('.filter-option').dataset.categoryId));

    const capacities = Array.from(document.querySelectorAll('.filter-option[data-capacity] input:checked'))
        .map(cb => parseInt(cb.closest('.filter-option').dataset.capacity));

    const requestData = {
        categoryIds: categoryIds.length > 0 ? categoryIds : null,
        capacities: capacities.length > 0 ? capacities : null,
        page: currentPage,
        pageSize: pageSize
    };

    if (grid) grid.innerHTML = '<div class="text-center col-span-3 py-5"><i class="fas fa-spinner fa-spin fa-2x text-muted"></i></div>';

    try {
        const response = await fetch('/Booking/GetFilteredPitches', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(requestData)
        });
        const result = await response.json();

        if (result.success) {
            renderPitchesList(result.data.pitches);
            if (countDiv) countDiv.innerHTML = `Tìm thấy <strong>${result.data.displayCount}</strong> sân`;
        }
    } catch (e) {
        console.error(e);
    }
}

function renderPitchesList(pitches) {
    const grid = document.querySelector('.fields-grid');
    if (!pitches || pitches.length === 0) {
        grid.innerHTML = '<div class="text-center w-100 py-5 text-muted">Không tìm thấy sân nào.</div>';
        return;
    }

    grid.innerHTML = pitches.map(pitch => {
        const img = pitch.imageUrl || '/asset/img/default-pitch.jpg';
        const price = new Intl.NumberFormat('vi-VN').format(pitch.pricePerHour);

        let pitchTypeBadge = '';
        if (pitch.categoryName.includes('Bóng Đá')) {
            if (pitch.capacity === 10) {
                pitchTypeBadge = `<span class="badge bg-success ms-2" style="font-size: 11px; vertical-align: middle;">Sân 5</span>`;
            } else if (pitch.capacity === 14) {
                pitchTypeBadge = `<span class="badge bg-warning text-dark ms-2" style="font-size: 11px; vertical-align: middle;">Sân 7</span>`;
            }
        }

        return `
            <div class="field-card">
                <div class="field-image">
                    <img src="${img}" alt="${pitch.pitchName}" onerror="this.src='https://via.placeholder.com/400x250'">
                    <span class="field-category-badge">${pitch.categoryName}</span>
                </div>
                <div class="field-content">
                    <div class="d-flex align-items-center mb-2">
                        <h3 class="field-name m-0">${pitch.pitchName}</h3>
                        ${pitchTypeBadge}
                    </div>
                    <div class="field-meta">
                        <span><i class="fas fa-users"></i> ${pitch.capacity} người</span>
                        <span><i class="fas fa-tag"></i> ~${price}đ/h</span>
                    </div>
                    <div class="mt-3">
                        <button class="btn-book-now" onclick="openBookingModal(${pitch.pitchId}, '${pitch.pitchName}')">
                            Đặt Sân Ngay
                        </button>
                    </div>
                </div>
            </div>
        `;
    }).join('');
}

window.openBookingModal = function (pitchId, pitchName) {
    if (!bookingModal) return;
    document.getElementById('modalPitchId').value = pitchId;
    document.getElementById('modalPitchName').innerText = pitchName;

    const dateInput = document.getElementById('modalBookingDate');
    if (dateInput) dateInput.value = today;

    bookingModal.show();
    loadSlotsForModal(pitchId, today);
}

async function loadSlotsForModal(pitchId, date) {
    const container = document.getElementById('modalSlotsGrid');
    container.innerHTML = '<div class="text-center py-3"><i class="fas fa-spinner fa-spin"></i> Đang tải lịch...</div>';

    const requestData = {
        date: date,
        specificPitchIds: [parseInt(pitchId)],
        page: 1, pageSize: 1
    };

    try {
        const response = await fetch('/Booking/GetFilteredPitches', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(requestData)
        });
        const result = await response.json();

        if (result.success && result.data.pitches.length > 0) {
            renderModalSlots(result.data.pitches[0].slots, pitchId, result.data.pitches[0].pitchName);
        } else {
            container.innerHTML = '<div class="text-center text-danger py-3">Không tải được dữ liệu sân.</div>';
        }
    } catch (e) {
        container.innerHTML = '<div class="text-center text-danger py-3">Lỗi kết nối.</div>';
    }
}

function renderModalSlots(slots, pitchId, pitchName) {
    const container = document.getElementById('modalSlotsGrid');
    if (!slots || slots.length === 0) {
        container.innerHTML = '<div class="text-center text-muted py-3">Không có lịch trống.</div>';
        return;
    }

    const html = slots.map(slot => {
        const isBooked = slot.status === 'booked';
        const isExpired = slot.status === 'expired';

        let btnClass = 'slot-modal available';
        if (isBooked) {
            btnClass = 'slot-modal booked';
        } else if (isExpired) {
            btnClass = 'slot-modal expired';
        }

        const isDisabled = isBooked || isExpired;

        const price = new Intl.NumberFormat('vi-VN').format(slot.fullPrice);

        return `
            <button class="${btnClass}" ${isDisabled ? 'disabled' : ''}
                    onclick="confirmBooking(${pitchId}, ${slot.slotId}, '${pitchName}', '${slot.timeRange}', ${slot.fullPrice}, ${slot.depositPrice})">
                <span class="time">${slot.timeRange}</span>
                <span class="price">${price}đ</span>
                <span class="status">${slot.statusText}</span>
            </button>
        `;
    }).join('');

    container.innerHTML = html;
}

window.confirmBooking = function (pitchId, slotId, pitchName, timeRange, fullPrice, depositPrice) {
    const dateSelected = document.getElementById('modalBookingDate').value;
    const fullPriceFmt = new Intl.NumberFormat('vi-VN').format(fullPrice);

    if (bookingModal) bookingModal.hide();

    if (typeof Swal === 'undefined') {
        alert("Lỗi: SweetAlert2 chưa được tải.");
        return;
    }

    Swal.fire({
        title: 'Xác nhận đặt sân',
        html: `
            <div style="text-align: left; font-size: 15px;">
                <p><i class="fa fa-futbol"></i> <b>${pitchName}</b></p>
                <p><i class="fa fa-calendar"></i> <b>${dateSelected}</b> | <i class="fa fa-clock"></i> <b>${timeRange}</b></p>
                <hr>
                <p><i class="fa fa-money-bill"></i> Tổng tiền: <b class="text-success">${fullPriceFmt}đ</b></p>
                <p class="text-muted small"><i>Tiền sẽ được trừ trực tiếp vào ví của bạn.</i></p>
            </div>
        `,
        icon: 'question',
        showCancelButton: true,
        confirmButtonText: 'Thanh toán & Đặt sân',
        cancelButtonText: 'Quay lại',
        confirmButtonColor: '#10b981',
        cancelButtonColor: '#6b7280'
    }).then(async (res) => {
        if (res.isConfirmed) {
            Swal.fire({
                title: 'Đang xử lý...',
                text: 'Vui lòng chờ trong giây lát',
                allowOutsideClick: false,
                didOpen: () => { Swal.showLoading() }
            });

            try {
                const formData = new FormData();
                formData.append('pitchId', pitchId);
                formData.append('slotId', slotId);
                formData.append('date', dateSelected);

                console.log(' Đang gửi request đặt sân...');

                const response = await fetch('/Booking/BookPitch', { method: 'POST', body: formData });
                const data = await response.json();

                console.log(' Response từ server:', data);

                if (data.success) {
                    console.log(' Số dư mới:', data.newBalance);

                    if (data.newBalance !== undefined) {
                        console.log(' Đang cập nhật số dư ví...');
                        updateWalletBalance(data.newBalance);
                    } else {
                        console.warn('⚠ Server không trả về newBalance!');
                    }

                    await Swal.fire({
                        title: ' Đặt sân thành công!',
                        html: `
                            <div style="text-align: center;">
                                <p style="color: #64748b; margin-bottom: 10px;">Mã đặt sân: <strong>${data.bookingCode}</strong></p>
                                
                                <div style="background: #fff; padding: 10px; display: inline-block; border: 1px dashed #ccc; border-radius: 8px;">
                                    <img src="${data.qrCode}" alt="QR Code" style="width: 200px; height: 200px; display: block;">
                                </div>

                                <div class="alert alert-info mt-3" style="font-size: 13px; text-align: left;">
                                    <i class="fas fa-info-circle"></i> <strong>Hướng dẫn:</strong><br>
                                    - Vui lòng đưa mã này cho nhân viên tại sân để nhận sân.<br>
                                    - Bạn có thể xem lại mã này trong phần <b>Lịch sử > Đặt sân</b>.
                                </div>
                            </div>
                        `,
                        icon: 'success',
                        confirmButtonText: 'Đã lưu mã',
                        confirmButtonColor: '#10b981',
                        allowOutsideClick: false
                    });


                    console.log('🔔 Dispatching bookingCreated event...');
                    window.dispatchEvent(new Event('bookingCreated'));

                    if (bookingModal) {
                        bookingModal.show();
                        loadSlotsForModal(pitchId, dateSelected);
                    }
                } else {
                    Swal.fire({
                        title: 'Thất bại',
                        text: data.message,
                        icon: 'error',
                        confirmButtonColor: '#ef4444'
                    }).then(() => {
                        if (bookingModal) bookingModal.show();
                    });
                }
            } catch (e) {
                console.error('❌ Lỗi:', e);
                Swal.fire('Lỗi', 'Lỗi hệ thống.', 'error');
                if (bookingModal) bookingModal.show();
            }
        } else {
            if (bookingModal) bookingModal.show();
        }
    });
}

function updateWalletBalance(newBalance) {
    console.log('💵 updateWalletBalance được gọi với số dư:', newBalance);

    const walletElements = document.querySelectorAll('.wallet-balance');

    if (walletElements.length > 0) {
        walletElements.forEach((el) => {
            el.textContent = new Intl.NumberFormat('vi-VN').format(newBalance) + '₫';

            el.style.transition = 'all 0.3s ease';
            el.style.color = '#10b981';
            el.style.fontWeight = 'bold';
            setTimeout(() => {
                el.style.color = '';
                el.style.fontWeight = '';
            }, 1500);
        });
    }

    const headerWallet = document.querySelector('.wallet-balances');
    if (headerWallet) {
        headerWallet.textContent = new Intl.NumberFormat('vi-VN').format(newBalance) + '₫';
    }

    const navWallet = document.querySelector('.navbar .wallet-balance');
    if (navWallet) {
        navWallet.textContent = new Intl.NumberFormat('vi-VN').format(newBalance) + '₫';
    }
}