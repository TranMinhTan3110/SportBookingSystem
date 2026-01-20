document.addEventListener('DOMContentLoaded', function () {
    const balanceSpan = document.getElementById('depositUserBalance');
    const phoneInput = document.getElementById('depositUser');

    // SweetAlert2 configuration
    const Toast = Swal.mixin({
        toast: true,
        position: 'top-end',
        showConfirmButton: false,
        timer: 3000,
        timerProgressBar: true
    });

    // ========================================
    // FILTERING & SEARCH FUNCTIONALITY
    // ========================================

    const searchInput = document.getElementById('searchInput');
    const typeFilter = document.getElementById('typeFilter');
    const statusFilter = document.getElementById('statusFilter');
    const dateFilter = document.getElementById('dateFilter');
    const resetButton = document.getElementById('resetFilters');
    let debounceTimer;

    // Debounced search function
    function debounceSearch() {
        clearTimeout(debounceTimer);
        debounceTimer = setTimeout(() => {
            applyFilters();
        }, 500); // Wait 500ms after user stops typing
    }

    // Apply all filters
    async function applyFilters() {
        const params = new URLSearchParams({
            search: searchInput.value.trim(),
            type: typeFilter.value,
            status: statusFilter.value,
            date: dateFilter.value,
            page: 1, // Reset to first page when filtering
            pageSize: 10
        });

        try {
            const response = await fetch(`/AdminPayment/FilterPayments?${params}`);
            if (response.ok) {
                const data = await response.json();
                updateTable(data);
                updatePagination(data);
                updateVisibleCount(data.payments.length, data.transactionCount);
            } else {
                Toast.fire({ icon: 'error', title: 'Lỗi khi lọc dữ liệu' });
            }
        } catch (error) {
            console.error('Filter error:', error);
            Toast.fire({ icon: 'error', title: 'Không thể kết nối đến máy chủ' });
        }
    }

    // Update table with filtered data
    function updateTable(data) {
        const tbody = document.getElementById('paymentTableBody');

        if (!data.payments || data.payments.length === 0) {
            tbody.innerHTML = `
                <tr>
                    <td colspan="8" style="text-align: center; padding: 40px; color: #999;">
                        <i class="bi bi-inbox" style="font-size: 48px;"></i>
                        <p style="margin-top: 10px;">Không tìm thấy giao dịch nào</p>
                    </td>
                </tr>
            `;
            return;
        }

        tbody.innerHTML = data.payments.map(item => {
            const avatar = item.user?.substring(0, 1).toUpperCase() || '?';
            const badgeClass = getBadgeClass(item.type);
            const amountClass = isPositiveTransaction(item.type) ? 'amount-positive' : 'amount-negative';
            const amountPrefix = isPositiveTransaction(item.type) ? '+' : '-';
            const statusHTML = getStatusHTML(item.status);

            return `
                <tr class="payment-row">
                    <td class="transaction-code">${item.code || '-'}</td>
                    <td>
                        <div class="user-cell">
                            <div class="avatar-circle">${avatar}</div>
                            <span class="user-name">${item.user || 'N/A'}</span>
                        </div>
                    </td>
                    <td>
                        <span class="badge ${badgeClass}">${item.type || '-'}</span>
                    </td>
                    <td class="text-right">
                        <span class="${amountClass}">${amountPrefix}${formatCurrency(item.amount)}</span>
                    </td>
                    <td>
                        <span class="payment-source">${item.source || '-'}</span>
                    </td>
                    <td>
                        <div class="date-cell">
                            <div class="date-main">${formatDate(item.date)}</div>
                            <div class="date-time">${formatTime(item.date)}</div>
                        </div>
                    </td>
                    <td>${statusHTML}</td>
                    <td class="text-right">
                        <button class="btn-icon" title="Chi tiết">
                            <i class="bi bi-eye"></i>
                        </button>
                    </td>
                </tr>
            `;
        }).join('');
    }

    // Update pagination
    function updatePagination(data) {
        const paginationNav = document.querySelector('.pagination');
        if (!paginationNav) return;

        let html = '';
        const currentPage = data.currentPage;
        const totalPages = data.totalPages;

        // Previous button
        if (currentPage > 1) {
            html += `<li class="page-item">
                <a href="#" class="page-link" data-page="${currentPage - 1}">Trước</a>
            </li>`;
        } else {
            html += `<li class="page-item disabled">
                <span class="page-link">Trước</span>
            </li>`;
        }

        // Page numbers
        for (let i = 1; i <= totalPages; i++) {
            if (i === currentPage) {
                html += `<li class="page-item active">
                    <span class="page-link">${i}</span>
                </li>`;
            } else if (i === 1 || i === totalPages || (i >= currentPage - 2 && i <= currentPage + 2)) {
                html += `<li class="page-item">
                    <a href="#" class="page-link" data-page="${i}">${i}</a>
                </li>`;
            } else if (i === currentPage - 3 || i === currentPage + 3) {
                html += `<li class="page-item disabled">
                    <span class="page-link">...</span>
                </li>`;
            }
        }

        // Next button
        if (currentPage < totalPages) {
            html += `<li class="page-item">
                <a href="#" class="page-link" data-page="${currentPage + 1}">Sau</a>
            </li>`;
        } else {
            html += `<li class="page-item disabled">
                <span class="page-link">Sau</span>
            </li>`;
        }

        paginationNav.innerHTML = html;

        // Attach click events to pagination links
        paginationNav.querySelectorAll('a.page-link').forEach(link => {
            link.addEventListener('click', async (e) => {
                e.preventDefault();
                const page = parseInt(link.dataset.page);
                await loadPage(page);
            });
        });
    }

    // Load specific page
    async function loadPage(page) {
        const params = new URLSearchParams({
            search: searchInput.value.trim(),
            type: typeFilter.value,
            status: statusFilter.value,
            date: dateFilter.value,
            page: page,
            pageSize: 10
        });

        try {
            const response = await fetch(`/AdminPayment/FilterPayments?${params}`);
            if (response.ok) {
                const data = await response.json();
                updateTable(data);
                updatePagination(data);
                updateVisibleCount(data.payments.length, data.transactionCount);
            }
        } catch (error) {
            console.error('Pagination error:', error);
        }
    }

    // Update visible count
    function updateVisibleCount(visible, total) {
        const visibleCountSpan = document.getElementById('visibleCount');
        if (visibleCountSpan) {
            visibleCountSpan.textContent = visible;
        }
    }

    // Helper: Get badge class for transaction type
    function getBadgeClass(type) {
        if (type === 'Nạp tiền') return 'badge-success';
        if (type === 'Thanh toán sân' || type === 'Thanh toán đồ') return 'badge-default';
        if (type === 'Chuyển tiền') return 'badge-info';
        return 'badge-secondary';
    }

    // Helper: Check if transaction is positive
    function isPositiveTransaction(type) {
        return type === 'Nạp tiền' || type === 'Chuyển tiền';
    }

    // Helper: Get status HTML
    function getStatusHTML(status) {
        switch (status) {
            case 'Thành công':
                return `<span class="status-indicator">
                    <span class="status-dot status-success"></span>
                    <span class="status-text status-success-text">Thành công</span>
                </span>`;
            case 'Chờ xử lý':
                return `<span class="status-indicator">
                    <span class="status-dot status-warning"></span>
                    <span class="status-text status-warning-text">Chờ xử lý</span>
                </span>`;
            case 'Đã hủy':
                return `<span class="status-indicator">
                    <span class="status-dot status-danger"></span>
                    <span class="status-text status-danger-text">Đã hủy</span>
                </span>`;
            default:
                return `<span>${status}</span>`;
        }
    }

    // Helper: Format currency
    function formatCurrency(amount) {
        return new Intl.NumberFormat('vi-VN').format(amount) + 'đ';
    }

    // Helper: Format date
    function formatDate(dateString) {
        const date = new Date(dateString);
        return date.toLocaleDateString('vi-VN');
    }

    // Helper: Format time
    function formatTime(dateString) {
        const date = new Date(dateString);
        return date.toLocaleTimeString('vi-VN', { hour: '2-digit', minute: '2-digit' });
    }

    // Event listeners for filters
    searchInput.addEventListener('input', debounceSearch);
    typeFilter.addEventListener('change', applyFilters);
    statusFilter.addEventListener('change', applyFilters);
    dateFilter.addEventListener('change', applyFilters);

    // Reset filters button
    resetButton.addEventListener('click', function () {
        searchInput.value = '';
        typeFilter.value = 'all';
        statusFilter.value = 'all';
        dateFilter.value = '';
        applyFilters();
    });

    // ========================================
    // TAB SWITCHING
    // ========================================
    document.querySelectorAll('.tab-btn').forEach(btn => {
        btn.addEventListener('click', function () {
            document.querySelectorAll('.tab-btn, .tab-content').forEach(el => el.classList.remove('active'));
            this.classList.add('active');
            document.getElementById(`${this.dataset.tab}-tab`).classList.add('active');

            if (this.dataset.tab === 'purchase') {
                loadProducts();
            }
        });
    });

    // ========================================
    // DEPOSIT FUNCTIONALITY
    // ========================================
    phoneInput.addEventListener('change', async function () {
        const phone = this.value.trim();
        if (phone.length < 10) return;

        try {
            const res = await fetch(`/AdminPayment/GetUserByPhone?phone=${phone}`);
            if (res.ok) {
                const data = await res.json();
                balanceSpan.textContent = new Intl.NumberFormat('vi-VN').format(data.walletBalance) + 'đ';
                balanceSpan.dataset.userId = data.userId;
                Toast.fire({ icon: 'success', title: 'Đã tìm thấy khách hàng' });
            } else {
                Swal.fire({ icon: 'error', title: 'Lỗi', text: 'Số điện thoại này chưa đăng ký thành viên!' });
                balanceSpan.textContent = '0đ';
                balanceSpan.dataset.userId = "";
            }
        } catch (e) {
            console.error(e);
        }
    });

    // Submit deposit
    document.getElementById('depositForm').addEventListener('submit', async function (e) {
        e.preventDefault();
        const userId = balanceSpan.dataset.userId;
        if (!userId) {
            Swal.fire({ icon: 'warning', title: 'Chú ý', text: 'Vui lòng nhập SĐT khách hàng hợp lệ trước!' });
            return;
        }

        const confirm = await Swal.fire({
            title: 'Xác nhận nạp tiền?',
            text: `Bạn có chắc chắn muốn nạp tiền cho khách hàng này không?`,
            icon: 'question',
            showCancelButton: true,
            confirmButtonColor: '#28a745',
            cancelButtonColor: '#d33',
            confirmButtonText: 'Đồng ý nạp',
            cancelButtonText: 'Hủy'
        });

        if (confirm.isConfirmed) {
            Swal.showLoading();

            const dto = {
                UserID: parseInt(userId),
                Amount: parseFloat(document.getElementById('depositAmount').value),
                PaymentMethod: document.getElementById('depositMethod').value,
                Message: document.getElementById('depositMessage').value
            };

            try {
                const res = await fetch('/AdminPayment/CreateDeposit', {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify(dto)
                });
                const result = await res.json();

                if (result.success) {
                    await Swal.fire({
                        icon: 'success',
                        title: 'Thành công',
                        text: 'Nạp tiền thành công!',
                        confirmButtonText: 'OK'
                    });
                    location.reload();
                } else {
                    Swal.fire({ icon: 'error', title: 'Thất bại', text: result.message });
                }
            } catch (e) {
                Swal.fire({ icon: 'error', title: 'Lỗi', text: 'Không thể kết nối đến máy chủ!' });
            }
        }
    });

    // Reset deposit form
    document.querySelector('.btn-reset-form').addEventListener('click', function () {
        balanceSpan.textContent = '0đ';
        balanceSpan.dataset.userId = "";
    });

    // ========================================
    // PRODUCT PURCHASE FUNCTIONALITY
    // ========================================
    async function loadProducts() {
        const productSelect = document.getElementById('purchaseProduct');
        try {
            const res = await fetch('/AdminPayment/GetProducts');
            if (res.ok) {
                const products = await res.json();
                productSelect.innerHTML = '<option value="">-- Chọn sản phẩm --</option>';
                products.forEach(p => {
                    const option = document.createElement('option');
                    option.value = p.productId;
                    option.textContent = `${p.productName} - ${p.price.toLocaleString('vi-VN')}đ`;
                    option.dataset.price = p.price;
                    option.dataset.stock = p.stock;
                    productSelect.appendChild(option);
                });
            }
        } catch (e) {
            console.error('Lỗi load sản phẩm:', e);
        }
    }

    // Find customer by phone (purchase tab)
    document.getElementById('purchaseUser').addEventListener('change', async function () {
        const phone = this.value.trim();
        if (phone.length < 10) return;

        try {
            const res = await fetch(`/AdminPayment/GetUserByPhone?phone=${phone}`);
            if (res.ok) {
                const data = await res.json();
                document.getElementById('purchaseUserBalance').textContent =
                    new Intl.NumberFormat('vi-VN').format(data.walletBalance) + 'đ';
                document.getElementById('purchaseUserBalance').dataset.userId = data.userId;
                Toast.fire({ icon: 'success', title: 'Đã nhận diện khách hàng' });
            } else {
                Swal.fire({ icon: 'error', title: 'Lỗi', text: 'Không tìm thấy khách hàng!' });
                document.getElementById('purchaseUserBalance').textContent = '0đ';
                document.getElementById('purchaseUserBalance').dataset.userId = "";
            }
        } catch (e) {
            console.error(e);
        }
    });

    // Update price and stock
    document.getElementById('purchaseProduct').addEventListener('change', function () {
        const selectedOption = this.options[this.selectedIndex];
        if (!selectedOption.value) return;

        const price = selectedOption.dataset.price || 0;
        const stock = selectedOption.dataset.stock || 0;

        document.getElementById('purchasePrice').value =
            parseFloat(price).toLocaleString('vi-VN') + 'đ';
        document.getElementById('productStock').textContent = stock;
        updatePurchaseTotal();
    });

    // Calculate total
    document.getElementById('purchaseQuantity').addEventListener('input', updatePurchaseTotal);

    function updatePurchaseTotal() {
        const selectedOption = document.getElementById('purchaseProduct').selectedOptions[0];
        const price = parseFloat(selectedOption?.dataset.price || 0);
        const quantity = parseInt(document.getElementById('purchaseQuantity').value || 0);
        const total = price * quantity;
        document.getElementById('purchaseTotal').value = total.toLocaleString('vi-VN') + 'đ';
    }

    // Submit purchase
    document.getElementById('purchaseForm').addEventListener('submit', function (e) {
        e.preventDefault();
        Swal.fire({
            icon: 'info',
            title: 'Thông báo',
            text: 'Chức năng mua hàng đang được xử lý ở phía Backend!',
            confirmButtonColor: '#3085d6'
        });
    });
});

document.addEventListener('DOMContentLoaded', () => {
    // Khai báo biến đại diện cho Modal để dùng chung cho cả 2 sự kiện
    const modalElement = document.getElementById('rewardSettingsModal');

    // 1. Load quy định điểm thưởng khi Modal mở lên
    if (modalElement) {
        modalElement.addEventListener('show.bs.modal', async function () {
            try {
                const res = await fetch(`/AdminPayment/GetRewardSetting`);
                if (res.ok) {
                    const data = await res.json();

                    
                    document.getElementById('spendingRatio').value = data.amountStep;
                    document.getElementById('depositRatio').value = data.pointBonus;
                    document.getElementById('isRewardActive').checked = data.isActive;
                }
            } catch (e) {
                console.error("Lỗi khi load dữ liệu:", e);
            }
        });
    }

    // 2. Sự kiện lưu quy định điểm thưởng
    const saveBtn = document.getElementById('saveRewardSettings');
    if (saveBtn) {
        saveBtn.addEventListener('click', async function () {
            const dto = {
                AmountStep: parseFloat(document.getElementById('spendingRatio').value),
                PointBonus: parseFloat(document.getElementById('depositRatio').value),
                IsActive: document.getElementById('isRewardActive').checked
            };

            try {
                const res = await fetch(`/AdminPayment/SaveRewardSettings`, {
                    method: 'POST',
                    headers: {
                        "Content-Type": "application/json"
                    },
                    body: JSON.stringify(dto)
                });

                const result = await res.json();

                // Kiểm tra result.success 
                if (result.success) {
                    Swal.fire('Thành công', result.message, 'success');

                  
                    const modalInstance = bootstrap.Modal.getInstance(modalElement);
                    if (modalInstance) {
                        modalInstance.hide();
                    }
                } else {
                    throw new Error(result.message);
                }

            } catch (e) {
                console.error("Lỗi khi lưu:", e);
                Swal.fire('Lỗi', e.message || 'Không thể kết nối đến server', 'error');
            }
        });
    }
});