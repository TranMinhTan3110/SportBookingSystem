let allUsersData = [];
async function loadAllUser() {
    try {
        const [countRes, userRes] = await Promise.all([
            fetch('/api/user/getCount'),
            fetch('/api/user/allUsers')
        ]);

        if (!countRes.ok || !userRes.ok) throw new Error('Lỗi tải dữ liệu');

        const countUser = await countRes.json();
        allUsersData = await userRes.json(); 

        renderStats(countUser); 
        renderUserTable(allUsersData); 

    } catch (error) {
        console.error('Lỗi:', error);
    }
}

function renderStats(countUser) {
    document.getElementById('totalUsers').innerText = countUser.totalUsers ?? countUser.TotalUsers ?? 0;
    document.getElementById('activeUsers').innerText = countUser.activeUsers ?? countUser.ActiveUsers ?? 0;
    document.getElementById('blockedUsers').innerText = countUser.lockedUsers ?? countUser.LockedUsers ?? 0;
    const newUsersElem = document.getElementById('newUsers');
    if (newUsersElem) {
        newUsersElem.innerText = countUser.newUsers ?? countUser.NewUsers ?? 0;
    }
}

function renderUserTable(data) {
    const tbody = document.getElementById('userTableBody');
    tbody.innerHTML = '';

    if (data.length === 0) {
        document.getElementById('noResults').style.display = 'block';
        return;
    }
    document.getElementById('noResults').style.display = 'none';

    data.forEach(user => {
        const tr = document.createElement('tr');
        tr.innerHTML = `
            <td>#${user.id}</td>
            <td>
                <div class="user-info">
                    <div class="user-avatar">${(user.fullName || 'U').charAt(0)}</div>
                    <div class="user-details"><h4>${user.fullName}</h4></div>
                </div>
            </td>
            <td>${user.phoneNumber || 'N/A'}</td>
            <td>${user.email}</td>
            <td><span class="role-badge"><i class="fas fa-user"></i> ${user.role}</span></td>
            <td>${user.createdAt}</td>
            <td>
                <span class="status-badge ${user.isActive ? 'active' : 'blocked'}">
                    <i class="fas ${user.isActive ? 'fa-check-circle' : 'fa-ban'}"></i> 
                    ${user.isActive ? 'Hoạt động' : 'Bị khóa'}
                </span>
            </td>
            <td>
                <div class="action-buttons">
                   
                    <button class="btn ${user.isActive ? 'btn-block' : 'btn-unblock'}" onclick="ToggleStatus('${user.id}', ${user.isActive})">
                        <i class="fas ${user.isActive ? 'fa-lock' : 'fa-unlock'}"></i>
                    </button>
                </div>
            </td>
        `;
        tbody.appendChild(tr);
    });
}


//hàm khóa
function ToggleStatus(userId, isActive) {
    const actionText = isActive ? "khóa" : "mở khóa";
    Swal.fire({
        title: "Xác nhận thay đổi?",
        text: `Bạn có chắc muốn ${actionText} người dùng này?`,
        icon: "warning",
        showCancelButton: true,
        confirmButtonColor: isActive ? "#d33" : "#3085d6",
        confirmButtonText: "Đồng ý",
        cancelButtonText: "Hủy"
    }).then((result) => {
        if (result.isConfirmed) {
            executeToggleStatus(userId);
        }
    });
}
async function executeToggleStatus(userId) {
    try {


        const response = await fetch(`/api/user/toggleStatus/${userId}`,
            {
                method:'POST'
            });
        if (response.ok) {
            Swal.fire({
                title: "Thành công!",
                text: "Trạng thái người dùng đã được thay đổi.",
                icon: "success",
                timer: 1500
            });
            // Tải lại bảng để cập nhật giao diện mới
            loadAllUser();
        } else {
            Swal.fire("Lỗi!", "Có lỗi xảy ra khi cập nhật.", "error");
        }
    } catch (error) {
        console.error("Lỗi kết nối:", error);
    
        }
    
}


document.addEventListener("DOMContentLoaded", () => {
    loadAllUser();

    // Các nút lọc
    const filterButtons = document.querySelectorAll('.filter-btn');
    filterButtons.forEach(btn => {
        btn.addEventListener('click', function () {
            filterButtons.forEach(b => b.classList.remove('active'));
            this.classList.add('active');
            applyFilters(); // Gọi hàm lọc chung
        });
    });

    // Ô tìm kiếm
    document.getElementById('searchInput').addEventListener('input', applyFilters);
});

    // Xử lý sự kiện tìm kiếm
    document.getElementById('searchInput').addEventListener('input', function () {
        const keyword = this.value.toLowerCase();
        const filtered = allUsersData.filter(u =>
            u.fullName.toLowerCase().includes(keyword) ||
            u.email.toLowerCase().includes(keyword) ||
            u.phoneNumber.includes(keyword)
        );
        renderUserTable(filtered);
    });

//hàm để tìm kiếm theo lọc
function applyFilters() {
    const keyword = document.getElementById('searchInput').value.toLowerCase();
    const activeFilterBtn = document.querySelector('.filter-btn.active');
    const filterType = activeFilterBtn ? activeFilterBtn.getAttribute('data-filter') : 'all';

    const filtered = allUsersData.filter(u => {
        // Điều kiện tìm kiếm
        const matchesSearch = u.fullName.toLowerCase().includes(keyword) ||
            u.email.toLowerCase().includes(keyword) ||
            (u.phoneNumber && u.phoneNumber.includes(keyword));

        // Điều kiện bộ lọc trạng thái
        let matchesFilter = true;
        if (filterType === 'active') matchesFilter = u.isActive === true;
        if (filterType === 'blocked') matchesFilter = u.isActive === false;

        return matchesSearch && matchesFilter;
    });

    renderUserTable(filtered);
}