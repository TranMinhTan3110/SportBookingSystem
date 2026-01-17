const searchInput = document.getElementById('searchInput');
const tableBody = document.getElementById('userTableBody');
const noResults = document.getElementById('noResults');
const rows = tableBody.getElementsByTagName('tr');

// Xử lý tìm kiếm
searchInput.addEventListener('input', function () {
    const searchTerm = this.value.toLowerCase();
    filterData();
});

// Xử lý nút lọc (Filter)
const filterBtns = document.querySelectorAll('.filter-btn');
filterBtns.forEach(btn => {
    btn.addEventListener('click', function () {
        filterBtns.forEach(b => b.classList.remove('active'));
        this.classList.add('active');
        filterData();
    });
});

// Hàm lọc dữ liệu chung
function filterData() {
    const searchTerm = searchInput.value.toLowerCase();
    const currentFilter = document.querySelector('.filter-btn.active').dataset.filter;
    let visibleCount = 0;

    for (let row of rows) {
        const text = row.textContent.toLowerCase();
        const rowStatus = row.dataset.status;

        const matchSearch = text.includes(searchTerm);
        let matchFilter = currentFilter === 'all' || rowStatus === currentFilter;

        if (matchSearch && matchFilter) {
            row.classList.remove('hidden');
            visibleCount++;
        } else {
            row.classList.add('hidden');
        }
    }

    noResults.style.display = visibleCount === 0 ? 'block' : 'none';
    updateStats();
}

// Hàm cập nhật thống kê
function updateStats() {
    let total = 0, active = 0, blocked = 0;

    for (let row of rows) {
        if (!row.classList.contains('hidden')) {
            total++;
            if (row.dataset.status === 'active') active++;
            else blocked++;
        }
    }

    document.getElementById('totalUsers').textContent = total;
    document.getElementById('activeUsers').textContent = active;
    document.getElementById('blockedUsers').textContent = blocked;
}

// Định dạng tiền tệ
const formatCurrency = (amount) => {
    return new Intl.NumberFormat('vi-VN', {
        style: 'currency',
        currency: 'VND'
    }).format(amount);
};

// Xem chi tiết User (Mở Modal)
function viewUser(button) {
    const row = button.closest('tr');

    // Đã sửa lỗi cú pháp ở đoạn này
    const userData = {
        id: row.dataset.id,
        username: row.dataset.username,
        fullname: row.dataset.fullname,
        email: row.dataset.email,
        phone: row.dataset.phone,
        role: row.dataset.role,
        roleId: row.dataset.roleid,
        status: row.dataset.status,
        joinDate: row.dataset.joindate,
        lastLogin: row.dataset.lastlogin,
        wallet: row.dataset.wallet,
        points: row.dataset.points,
        bookings: row.dataset.bookings,
        spent: row.dataset.spent
    };

    const modalBody = document.getElementById('modalBody');
    modalBody.innerHTML = `
        <div class="info-section">
            <div class="section-title">
                <i class="fas fa-id-card"></i> Thông Tin Cơ Bản
            </div>
            <div class="info-row"><div class="info-label">ID Người dùng</div><div class="info-value">#${userData.id}</div></div>
            <div class="info-row"><div class="info-label">Tên đăng nhập</div><div class="info-value">@${userData.username}</div></div>
            <div class="info-row"><div class="info-label">Họ và tên</div><div class="info-value">${userData.fullname}</div></div>
            <div class="info-row"><div class="info-label">Email</div><div class="info-value">${userData.email}</div></div>
            <div class="info-row"><div class="info-label">Số điện thoại</div><div class="info-value">${userData.phone}</div></div>
            <div class="info-row"><div class="info-label">Phân quyền</div><div class="info-value">
                <span class="role-badge ${userData.roleId == 1 ? 'admin' : ''}">
                    <i class="fas ${userData.roleId == 1 ? 'fa-user-shield' : 'fa-user'}"></i>
                    ${userData.role}
                </span>
            </div></div>
            <div class="info-row">
                <div class="info-label">Trạng thái</div>
                <div class="info-value">
                    <span class="status-badge ${userData.status === 'active' ? 'active' : 'blocked'}">
                        <i class="fas ${userData.status === 'active' ? 'fa-check-circle' : 'fa-ban'}"></i>
                        ${userData.status === 'active' ? 'Hoạt động' : 'Bị khóa'}
                    </span>
                </div>
            </div>
        </div>

        <div class="info-section">
            <div class="section-title">
                <i class="fas fa-wallet"></i> Thông Tin Tài Chính
            </div>
            <div class="financial-info">
                <div class="financial-grid">
                    <div class="financial-item">
                        <div class="financial-label">
                            <i class="fas fa-money-bill-wave"></i> Số Dư Ví
                        </div>
                        <div class="financial-value money">${formatCurrency(userData.wallet)}</div>
                    </div>
                    <div class="financial-item">
                        <div class="financial-label">
                            <i class="fas fa-gift"></i> Điểm Thưởng
                        </div>
                        <div class="financial-value points">${userData.points}</div>
                    </div>
                </div>
            </div>
            <div class="info-row" style="margin-top: 10px;">
                <div class="info-label">Tổng chi tiêu</div>
                <div class="info-value">${formatCurrency(userData.spent)}</div>
            </div>
            <div class="info-row">
                <div class="info-label">Tổng đặt sân</div>
                <div class="info-value">${userData.bookings} lần</div>
            </div>
        </div>

        <div class="info-section">
            <div class="section-title">
                <i class="fas fa-clock"></i> Hoạt Động
            </div>
            <div class="info-row">
                <div class="info-label">Ngày tham gia</div>
                <div class="info-value">${userData.joinDate}</div>
            </div>
            <div class="info-row">
                <div class="info-label">Đăng nhập lần cuối</div>
                <div class="info-value">${userData.lastLogin}</div>
            </div>
        </div>
    `;

    const modalFooter = document.getElementById('modalFooter');
    modalFooter.innerHTML = `
        <button class="btn btn-secondary" onclick="closeModal()">
            <i class="fas fa-times"></i> Đóng
        </button>
        ${userData.status === 'active'
            ? `<button class="btn btn-danger" onclick="closeModal()">
                <i class="fas fa-lock"></i> Khóa Tài Khoản
                </button>`
            : `<button class="btn btn-success" onclick="closeModal()">
                <i class="fas fa-unlock"></i> Mở Khóa
                </button>`
        }
    `;

    document.getElementById('userModal').classList.add('active');
}

// Đóng Modal
function closeModal() {
    document.getElementById('userModal').classList.remove('active');
}

// Đóng modal khi click ra ngoài vùng nội dung
document.getElementById('userModal').addEventListener('click', function (e) {
    if (e.target === this) {
        closeModal();
    }
});