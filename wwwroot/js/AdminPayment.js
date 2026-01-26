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

    const searchInput = document.getElementById('searchInput');
    const typeFilter = document.getElementById('typeFilter');
    const statusFilter = document.getElementById('statusFilter');
    const dateFilter = document.getElementById('dateFilter');
    const resetButton = document.getElementById('resetFilters');
    let debounceTimer;


    function debounceSearch() {
        clearTimeout(debounceTimer);
        debounceTimer = setTimeout(() => {
            applyFilters();
        }, 500);
    }


    async function applyFilters() {
        const params = new URLSearchParams({
            search: searchInput?.value.trim() || '',
            type: typeFilter?.value || 'all',
            status: statusFilter?.value || 'all',
            date: dateFilter?.value || '',
            page: 1,
            pageSize: 10
        });

        try {
            const response = await fetch(`/AdminPayment/FilterPayments?${params}`);
            if (response.ok) {
                const data = await response.json();
                updateTable(data);
                updatePagination(data);
                updateVisibleCount(data.payments ? data.payments.length : 0, data.transactionCount || 0);
            } else {
                Toast.fire({ icon: 'error', title: 'Lỗi khi lọc dữ liệu' });
            }
        } catch (error) {
            console.error('Filter error:', error);
            Toast.fire({ icon: 'error', title: 'Không thể kết nối đến máy chủ' });
        }
    }

    // Update table with data
    function updateTable(data) {
        const tbody = document.getElementById('paymentTableBody');
        if (!tbody) return;

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

            const type = (item.type || '').trim();
            if (type.includes('Hoàn tiền') && type !== 'Hoàn tiền') {
                console.log(`Mismatch found: '${type}' (Length: ${type.length}) vs 'Hoàn tiền'`);
            }

            const isPositive = type !== 'Hoàn tiền';
            const amountClass = isPositive ? 'amount-positive' : 'amount-negative';
            const amountPrefix = isPositive ? '+' : '-';
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

    // Update pagination UI
    function updatePagination(data) {
        const paginationWrapper = document.querySelector('.pagination');
        if (!paginationWrapper) return;

        let html = '';
        const currentPage = data.currentPage;
        const totalPages = data.totalPages;

        if (currentPage > 1) {
            html += `<li class="page-item"><a href="#" class="page-link" data-page="${currentPage - 1}">Trước</a></li>`;
        } else {
            html += `<li class="page-item disabled"><span class="page-link">Trước</span></li>`;
        }

        for (let i = 1; i <= totalPages; i++) {
            if (i === currentPage) {
                html += `<li class="page-item active"><span class="page-link">${i}</span></li>`;
            } else if (i === 1 || i === totalPages || (i >= currentPage - 2 && i <= currentPage + 2)) {
                html += `<li class="page-item"><a href="#" class="page-link" data-page="${i}">${i}</a></li>`;
            } else if (i === currentPage - 3 || i === currentPage + 3) {
                html += `<li class="page-item disabled"><span class="page-link">...</span></li>`;
            }
        }

        if (currentPage < totalPages) {
            html += `<li class="page-item"><a href="#" class="page-link" data-page="${currentPage + 1}">Sau</a></li>`;
        } else {
            html += `<li class="page-item disabled"><span class="page-link">Sau</span></li>`;
        }

        paginationWrapper.innerHTML = html;

        paginationWrapper.querySelectorAll('a.page-link').forEach(link => {
            link.addEventListener('click', (e) => {
                e.preventDefault();
                loadPage(parseInt(link.dataset.page));
            });
        });
    }

    async function loadPage(page) {
        const params = new URLSearchParams({
            search: searchInput.value.trim(),
            type: typeFilter.value,
            status: statusFilter.value,
            date: dateFilter.value,
            page: page,
            pageSize: 10
        });

        const response = await fetch(`/AdminPayment/FilterPayments?${params}`);
        if (response.ok) {
            const data = await response.json();
            updateTable(data);
            updatePagination(data);
            updateVisibleCount(data.payments.length, data.transactionCount);
        }
    }

    function updateVisibleCount(visible, total) {
        const visibleCountSpan = document.getElementById('visibleCount');
        if (visibleCountSpan) {
            visibleCountSpan.textContent = visible;
        }
    }

    function getBadgeClass(type) {
        type = (type || '').trim();
        if (type === 'Nạp tiền' || type === 'Hoàn tiền') return 'badge-success';
        if (type === 'Thanh toán sân' || type === 'Thanh toán đồ') return 'badge-default';
        if (type === 'Chuyển tiền') return 'badge-info';
        return 'badge-secondary';
    }

    function getStatusHTML(status) {
        switch (status) {
            case 'Thành công':
                return `<span class="status-indicator"><span class="status-dot status-success"></span><span class="status-text status-success-text">Thành công</span></span>`;
            case 'Chờ xử lý':
                return `<span class="status-indicator"><span class="status-dot status-warning"></span><span class="status-text status-warning-text">Chờ xử lý</span></span>`;
            case 'Đã hủy':
                return `<span class="status-indicator"><span class="status-dot status-danger"></span><span class="status-text status-danger-text">Đã hủy</span></span>`;
            default:
                return `<span>${status}</span>`;
        }
    }

    function formatCurrency(amount) {
        return new Intl.NumberFormat('vi-VN').format(amount) + 'đ';
    }

    function formatDate(dateString) {
        return new Date(dateString).toLocaleDateString('vi-VN');
    }

    function formatTime(dateString) {
        return new Date(dateString).toLocaleTimeString('vi-VN', { hour: '2-digit', minute: '2-digit' });
    }

    // Event listeners
    searchInput?.addEventListener('input', debounceSearch);
    typeFilter?.addEventListener('change', applyFilters);
    statusFilter?.addEventListener('change', applyFilters);
    dateFilter?.addEventListener('change', applyFilters);
    resetButton?.addEventListener('click', () => {
        searchInput.value = '';
        typeFilter.value = 'all';
        statusFilter.value = 'all';
        dateFilter.value = '';
        applyFilters();
    });

    document.querySelectorAll('.tab-btn').forEach(btn => {
        btn.addEventListener('click', function () {
            document.querySelectorAll('.tab-btn, .tab-content').forEach(el => el.classList.remove('active'));
            this.classList.add('active');
            const targetId = `${this.dataset.tab}-tab`;
            document.getElementById(targetId)?.classList.add('active');

            if (this.dataset.tab === 'purchase') {
                loadProducts();
            }
        });
    });


    phoneInput?.addEventListener('change', async function () {
        const phone = this.value.trim();
        if (phone.length < 10) return;

        try {
            const res = await fetch(`/AdminPayment/GetUserByPhone?phone=${phone}`);
            if (res.ok) {
                const data = await res.json();
                balanceSpan.textContent = formatCurrency(data.walletBalance);
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

    document.getElementById('depositForm')?.addEventListener('submit', async function (e) {
        e.preventDefault();
        const userId = balanceSpan.dataset.userId;
        if (!userId) {
            Swal.fire({ icon: 'warning', title: 'Chú ý', text: 'Vui lòng nhập SĐT khách hàng hợp lệ trước!' });
            return;
        }

        const confirm = await Swal.fire({
            title: 'Xác nhận nạp tiền?',
            text: `Xác nhận nạp tiền cho khách hàng?`,
            icon: 'question',
            showCancelButton: true,
            confirmButtonText: 'Đồng ý',
            cancelButtonText: 'Hủy'
        });

        if (confirm.isConfirmed) {
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
                    Swal.fire('Thành công', 'Nạp tiền thành công!', 'success').then(() => location.reload());
                } else {
                    Swal.fire('Thất bại', result.message, 'error');
                }
            } catch (e) {
                Swal.fire('Lỗi', 'Không thể kết nối đến máy chủ!', 'error');
            }
        }
    });

    async function loadProducts() {
        const productSelect = document.getElementById('purchaseProduct');
        if (!productSelect || productSelect.options.length > 1) return;

        try {
            const res = await fetch('/AdminPayment/GetProducts');
            if (res.ok) {
                const products = await res.json();
                productSelect.innerHTML = '<option value="">-- Chọn sản phẩm --</option>';
                products.forEach(p => {
                    const option = document.createElement('option');
                    option.value = p.productId;
                    option.textContent = `${p.productName} - ${formatCurrency(p.price)}`;
                    option.dataset.price = p.price;
                    option.dataset.stock = p.stock;
                    productSelect.appendChild(option);
                });
            }
        } catch (e) { console.error('Lỗi load sản phẩm:', e); }
    }

    document.getElementById('purchaseUser')?.addEventListener('change', async function () {
        const phone = this.value.trim();
        if (phone.length < 10) return;

        try {
            const res = await fetch(`/AdminPayment/GetUserByPhone?phone=${phone}`);
            if (res.ok) {
                const data = await res.json();
                document.getElementById('purchaseUserBalance').textContent = formatCurrency(data.walletBalance);
                document.getElementById('purchaseUserBalance').dataset.userId = data.userId;
            }
        } catch (e) { console.error(e); }
    });

    document.getElementById('purchaseProduct')?.addEventListener('change', function () {
        const option = this.selectedOptions[0];
        if (!option.value) return;
        document.getElementById('purchasePrice').value = formatCurrency(option.dataset.price);
        document.getElementById('productStock').textContent = option.dataset.stock;
        updatePurchaseTotal();
    });

    document.getElementById('purchaseQuantity')?.addEventListener('input', updatePurchaseTotal);

    function updatePurchaseTotal() {
        const option = document.getElementById('purchaseProduct')?.selectedOptions[0];
        const price = parseFloat(option?.dataset.price || 0);
        const qty = parseInt(document.getElementById('purchaseQuantity')?.value || 0);
        document.getElementById('purchaseTotal').value = formatCurrency(price * qty);
    }

    document.getElementById('purchaseForm')?.addEventListener('submit', function (e) {
        e.preventDefault();
        Swal.fire('Thông báo', 'Chức năng mua hàng đang được xử lý ở phía Backend!', 'info');
    });

    const fulfillmentModal = new bootstrap.Modal(document.getElementById('qrFulfillmentModal'));

    window.handleQrDetails = async function (orderId, userId, productId, quantity) {
        if (orderId && orderId !== '0') {
            const url = new URL(window.location.href);
            url.searchParams.delete('qrOrder');
            url.searchParams.delete('qrUser');
            url.searchParams.delete('qrProduct');
            url.searchParams.delete('qrQty');
            window.history.replaceState({}, '', url);

            showFulfillmentModal(orderId);
            return;
        }


        const purchaseBtn = document.querySelector('.tab-btn[data-tab="purchase"]');
        if (purchaseBtn) purchaseBtn.click();

        fetch(`/AdminPayment/GetUserById/${userId}`)
            .then(res => res.json())
            .then(data => {
                if (data.success) {
                    document.getElementById('purchaseUser').value = data.phone || data.username;
                    document.getElementById('purchaseUserBalance').textContent = formatCurrency(data.balance);
                    document.getElementById('purchaseUserBalance').dataset.userId = data.userId;

                    const productSelect = document.getElementById('purchaseProduct');
                    if (productSelect) {
                        productSelect.value = productId;
                        productSelect.dispatchEvent(new Event('change'));
                    }

                    const qtyField = document.getElementById('purchaseQuantity');
                    if (qtyField) {
                        qtyField.value = quantity;
                        qtyField.dispatchEvent(new Event('input'));
                    }
                    Toast.fire({ icon: 'success', title: 'Tự động điền hoàn tất', text: `Khách hàng: ${data.fullName}` });
                }
            });
    };

    async function showFulfillmentModal(orderId) {
        const loading = document.getElementById('fulfillmentLoading');
        const content = document.getElementById('fulfillmentContent');

        loading.classList.remove('d-none');
        content.classList.add('d-none');
        fulfillmentModal.show();

        try {
            const res = await fetch(`/AdminPayment/GetOrderForFulfillment?orderId=${orderId}`);
            if (res.ok) {
                const data = await res.json();

                if (data.error) {
                    fulfillmentModal.hide();
                    Swal.fire({
                        icon: 'error',
                        title: 'QR Hết hạn',
                        text: data.message
                    });
                    return;
                }

                document.getElementById('fOrderId').value = data.orderId;
                document.getElementById('fOrderCode').textContent = data.orderCode;
                document.getElementById('fCustomerName').textContent = data.customerName;
                document.getElementById('fProductName').textContent = data.productName;
                document.getElementById('fQuantity').textContent = `x ${data.quantity}`;
                document.getElementById('fTotalAmount').textContent = formatCurrency(data.totalAmount);

                loading.classList.add('d-none');
                content.classList.remove('d-none');
            } else {
                fulfillmentModal.hide();
                Toast.fire({ icon: 'error', title: 'Không tìm thấy thông tin đơn hàng' });
            }
        } catch (e) {
            fulfillmentModal.hide();
            Toast.fire({ icon: 'error', title: 'Lỗi tải đơn hàng' });
        }
    }

    async function processFulfillment(status) {
        const orderId = document.getElementById('fOrderId').value;
        const confirm = await Swal.fire({
            title: status === 'Thành công' ? 'Xác nhận đơn hàng?' : 'Hủy đơn hàng?',
            text: status === 'Thành công' ? 'Xác nhận khách đã nhận đồ?' : 'Hủy đơn và hoàn tiền cho khách?',
            icon: status === 'Thành công' ? 'question' : 'warning',
            showCancelButton: true,
            confirmButtonText: 'Đồng ý',
            cancelButtonText: 'Không',
            confirmButtonColor: status === 'Thành công' ? '#198754' : '#dc3545'
        });

        if (confirm.isConfirmed) {
            try {
                const res = await fetch('/AdminPayment/UpdateOrderStatus', {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify({ orderId: parseInt(orderId), newStatus: status })
                });
                const result = await res.json();
                if (result.success) {
                    fulfillmentModal.hide();
                    Swal.fire('Thành công', result.message, 'success').then(() => location.reload());
                } else {
                    Swal.fire('Lỗi', result.message, 'error');
                }
            } catch (e) {
                Swal.fire('Lỗi', 'Lỗi kết nối máy chủ', 'error');
            }
        }
    }

    document.getElementById('btnFulfillSuccess')?.addEventListener('click', () => processFulfillment('Thành công'));
    document.getElementById('btnFulfillCancel')?.addEventListener('click', () => processFulfillment('Đã hủy'));

    const urlParams = new URLSearchParams(window.location.search);
    const qrOrder = urlParams.get('qrOrder');
    const qrUser = urlParams.get('qrUser');
    const qrProduct = urlParams.get('qrProduct');
    const qrQty = urlParams.get('qrQty');

    if (qrOrder || (qrUser && qrProduct && qrQty)) {
        setTimeout(() => window.handleQrDetails(qrOrder || 0, qrUser, qrProduct, qrQty), 1000);
    }


    const modalElement = document.getElementById('rewardSettingsModal');
    modalElement?.addEventListener('show.bs.modal', async function () {
        try {
            const res = await fetch(`/AdminPayment/GetRewardSetting`);
            if (res.ok) {
                const data = await res.json();
                document.getElementById('spendingRatio').value = data.amountStep;
                document.getElementById('depositRatio').value = data.pointBonus;
                document.getElementById('isRewardActive').checked = data.isActive;
            }
        } catch (e) { console.error("Lỗi khi load dữ liệu:", e); }
    });

    document.getElementById('saveRewardSettings')?.addEventListener('click', async function () {
        const dto = {
            AmountStep: parseFloat(document.getElementById('spendingRatio').value),
            PointBonus: parseFloat(document.getElementById('depositRatio').value),
            IsActive: document.getElementById('isRewardActive').checked
        };

        try {
            const res = await fetch(`/AdminPayment/SaveRewardSettings`, {
                method: 'POST',
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify(dto)
            });
            const result = await res.json();
            if (result.success) {
                Swal.fire('Thành công', result.message, 'success');
                bootstrap.Modal.getInstance(modalElement)?.hide();
            } else {
                throw new Error(result.message);
            }
        } catch (e) { Swal.fire('Lỗi', e.message || 'Lỗi kết nối server', 'error'); }
    });
});