document.addEventListener('DOMContentLoaded', function () {
    initFilters();
    loadBookings();
});

function initFilters() {
    const today = new Date().toISOString().split('T')[0];
    const fromDateEl = document.getElementById('fromDate');
    if (fromDateEl) fromDateEl.value = today;

    const searchInput = document.getElementById('searchInput');
    if (searchInput) searchInput.addEventListener('input', debounce(loadBookings, 500));

    if (fromDateEl) fromDateEl.addEventListener('change', loadBookings);
    const toDateEl = document.getElementById('toDate');
    if (toDateEl) toDateEl.addEventListener('change', loadBookings);

    const categorySelect = document.getElementById('filterPitchCategory');
    if (categorySelect) categorySelect.addEventListener('change', loadBookings);

    const timeSlotSelect = document.getElementById('filterTimeSlot');
    if (timeSlotSelect) timeSlotSelect.addEventListener('change', loadBookings);

    const btnReset = document.getElementById('btnResetFilter');
    if (btnReset) {
        btnReset.addEventListener('click', () => {
            if (searchInput) searchInput.value = '';
            if (fromDateEl) fromDateEl.value = today;
            if (toDateEl) toDateEl.value = '';
            if (categorySelect) categorySelect.value = '';
            if (timeSlotSelect) timeSlotSelect.value = '';
            loadBookings();
        });
    }
}

function loadBookings() {
    const tableBody = document.getElementById('bookingTableBody');
    if (!tableBody) return;

    tableBody.innerHTML = '<tr><td colspan="6" class="text-center py-4 text-muted"><i class="fas fa-spinner fa-spin me-2"></i>Đang tải dữ liệu...</td></tr>';

    const filterText = document.getElementById('searchInput')?.value || '';
    const fromDate = document.getElementById('fromDate')?.value || '';
    const toDate = document.getElementById('toDate')?.value || '';
    const categoryId = document.getElementById('filterPitchCategory')?.value || '';
    const timeSlotId = document.getElementById('filterTimeSlot')?.value || '';


    const params = new URLSearchParams({
        search: filterText,
        fromDate: fromDate,
        toDate: toDate,
        categoryId: categoryId,
        timeSlotId: timeSlotId
    });

    fetch(`/ManageBookingAD/GetBookings?${params.toString()}`)
        .then(response => response.json())
        .then(data => {
            renderTable(data);
        })
        .catch(error => {
            console.error('Error loading bookings:', error);
            tableBody.innerHTML = '<tr><td colspan="6" class="text-center py-4 text-danger">Lỗi tải dữ liệu. Vui lòng thử lại.</td></tr>';
        });
}

function renderTable(data) {
    const tableBody = document.getElementById('bookingTableBody');
    const showingCount = document.getElementById('showingCount');

    if (!tableBody) return;

    if (!data || data.length === 0) {
        tableBody.innerHTML = '<tr><td colspan="6" class="text-center py-4 text-muted">Không tìm thấy đơn đặt sân nào.</td></tr>';
        if (showingCount) showingCount.textContent = 0;
        return;
    }

    if (showingCount) showingCount.textContent = data.length;

    tableBody.innerHTML = data.map(item => `
        <tr>
            <td><span class="booking-code">${item.code}</span></td>
            <td>
                <div class="fw-bold">${item.customerName}</div>
                <div class="small text-muted">${item.customerPhone}</div>
            </td>
            <td>${item.pitchName}</td>
            <td>${item.date}<br><small class="text-muted">${item.time}</small></td>
            <td class="fw-bold text-success">${new Intl.NumberFormat('vi-VN').format(item.totalPrice)}đ</td>
        </tr>
    `).join('');
}


function debounce(func, wait) {
    let timeout;
    return function executedFunction(...args) {
        const later = () => {
            clearTimeout(timeout);
            func(...args);
        };
        clearTimeout(timeout);
        timeout = setTimeout(later, wait);
    };
}
