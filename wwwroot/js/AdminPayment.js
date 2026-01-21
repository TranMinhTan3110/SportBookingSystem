/**
 * AdminPayment.js
 * Handles client-side filtering and interactivity for the Payment Dashboard
 */

function initAdminPayment() {
    // 1. Element References
    const searchInput = document.getElementById('searchInput');
    const typeFilter = document.getElementById('typeFilter');
    const statusFilter = document.getElementById('statusFilter');
    const dateFilter = document.getElementById('dateFilter');
    const resetBtn = document.getElementById('resetFilters');

    const tableBody = document.getElementById('paymentTableBody');
    const tableRows = Array.from(tableBody.getElementsByTagName('tr'));
    const visibleCountSpan = document.getElementById('visibleCount');

    // 2. Event Listeners
    searchInput.addEventListener('input', filterTable);
    typeFilter.addEventListener('change', filterTable);
    statusFilter.addEventListener('change', filterTable);
    dateFilter.addEventListener('change', filterTable);

    resetBtn.addEventListener('click', function () {
        searchInput.value = '';
        typeFilter.value = 'all';
        statusFilter.value = 'all';
        dateFilter.value = '';
        filterTable(); // Re-run filter to show all
    });

    // 3. Filtering Logic
    function filterTable() {
        // Get current values
        const searchText = searchInput.value.toLowerCase().trim();
        const selectedType = typeFilter.value; // 'all' or specific value
        const selectedStatus = statusFilter.value; // 'all' or specific value
        const selectedDate = dateFilter.value; // YYYY-MM-DD string

        let visibleRows = 0;

        tableRows.forEach(row => {
            // Data extraction from columns
            // Col 0: Code, Col 1: User, Col 2: Type, Col 5: Date (dd/MM/yyyy), Col 6: Status

            // Add null checks to prevent errors
            if (!row.cells || row.cells.length < 7) {
                row.style.display = 'none';
                return;
            }

            const code = row.cells[0]?.textContent?.toLowerCase() || '';
            const user = row.cells[1]?.textContent?.toLowerCase() || '';
            const typeLower = row.cells[2]?.textContent?.trim() || ''; // Badge text
            const dateCell = row.cells[5]?.querySelector('div.date-main');
            const dateStr = dateCell?.textContent?.trim() || ''; // dd/MM/yyyy
            const statusText = row.cells[6]?.textContent?.trim() || ''; // "Thành công", "Chờ xử lý"...

            // -- Match Search (Code or User)
            const matchesSearch = code.includes(searchText) || user.includes(searchText);

            // -- Match Type
            // Note: The dropdown values match the row content text logic we used in View (e.g., "Nạp tiền")
            // We need to be careful with "Thanh toán" which matches "Thanh toán (Booking/Order)"
            let matchesType = true;
            if (selectedType !== 'all') {
                if (selectedType === 'Thanh toán') {
                    matchesType = typeLower.toLowerCase().includes('booking') || typeLower.toLowerCase().includes('order') || typeLower.toLowerCase().includes('thanh toán');
                } else {
                    matchesType = typeLower.includes(selectedType);
                }
            }

            // -- Match Status
            // Helper to map UI status text back to internal status keys if needed, 
            // but we can also match based on class or text content.
            // Let's deduce internal status from the text or badge class.
            let rowStatusKey = '';
            if (statusText.includes('Thành công')) rowStatusKey = 'Completed';
            else if (statusText.includes('Chờ xử lý')) rowStatusKey = 'Pending';
            else if (statusText.includes('Đã hủy')) rowStatusKey = 'Cancelled';

            const matchesStatus = (selectedStatus === 'all') || (rowStatusKey === selectedStatus);

            // -- Match Date
            // Row date is dd/MM/yyyy, Input is yyyy-MM-dd
            let matchesDate = true;
            if (selectedDate && dateStr) {
                // Convert row date to Comparable format
                const [day, month, year] = dateStr.split('/');
                // Format matches yyyy-MM-dd
                const rowDateISO = `${year}-${month}-${day}`;
                matchesDate = (rowDateISO === selectedDate);
            }

            // Final Decision
            if (matchesSearch && matchesType && matchesStatus && matchesDate) {
                row.style.display = '';
                visibleRows++;
            } else {
                row.style.display = 'none';
            }
        });

        // Update counter
        if (visibleCountSpan) {
            visibleCountSpan.textContent = visibleRows;
        }
    }

    // Initial Run
    filterTable();

    // ============================================
    // TRANSACTION MANAGEMENT
    // ============================================

    // Tab Switching
    const tabBtns = document.querySelectorAll('.tab-btn');
    const tabContents = document.querySelectorAll('.tab-content');

    console.log('Tab buttons found:', tabBtns.length);
    console.log('Tab contents found:', tabContents.length);

    tabBtns.forEach(btn => {
        btn.addEventListener('click', function () {
            const tabName = this.dataset.tab;
            console.log('Tab clicked:', tabName);

            // Remove active class from all tabs
            tabBtns.forEach(b => b.classList.remove('active'));
            tabContents.forEach(c => c.classList.remove('active'));

            // Add active class to clicked tab
            this.classList.add('active');
            const targetTab = document.getElementById(`${tabName}-tab`);
            console.log('Target tab element:', targetTab);
            if (targetTab) {
                targetTab.classList.add('active');
            }
        });
    });

    // Load Users and Products on page load
    // loadUsers(); // Disabled: Now using phone number input
    loadProducts();

    /* 
    // Load Users - Disabled
    function loadUsers() {
        fetch('/AdminPayment/GetUsers')
            .then(response => response.json())
            .then(users => {
                const depositUserSelect = document.getElementById('depositUser');
                const purchaseUserSelect = document.getElementById('purchaseUser');

                users.forEach(user => {
                    const option1 = new Option(`${user.fullName} (@${user.username})`, user.userID);
                    const option2 = new Option(`${user.fullName} (@${user.username})`, user.userID);
                    option1.dataset.balance = user.walletBalance;
                    option2.dataset.balance = user.walletBalance;
                    depositUserSelect.add(option1);
                    purchaseUserSelect.add(option2);
                });
            })
            .catch(error => console.error('Error loading users:', error));
    }
    */

    // Load Products
    function loadProducts() {
        fetch('/AdminPayment/GetProducts')
            .then(response => response.json())
            .then(products => {
                const productSelect = document.getElementById('purchaseProduct');

                products.forEach(product => {
                    const option = new Option(product.productName, product.productID);
                    option.dataset.price = product.price;
                    option.dataset.stock = product.stockQuantity;
                    productSelect.add(option);
                });
            })
            .catch(error => console.error('Error loading products:', error));
    }

    /*
    // Update balance when user is selected (Deposit)
    document.getElementById('depositUser')?.addEventListener('change', function () {
        const selectedOption = this.options[this.selectedIndex];
        const balance = selectedOption?.dataset.balance || 0;
        document.getElementById('depositUserBalance').textContent = formatCurrency(balance);
    });

    // Update balance when user is selected (Purchase)
    document.getElementById('purchaseUser')?.addEventListener('change', function () {
        const selectedOption = this.options[this.selectedIndex];
        const balance = selectedOption?.dataset.balance || 0;
        document.getElementById('purchaseUserBalance').textContent = formatCurrency(balance);
    });
    */

    // Update price and stock when product is selected
    document.getElementById('purchaseProduct')?.addEventListener('change', function () {
        const selectedOption = this.options[this.selectedIndex];
        const price = selectedOption?.dataset.price || 0;
        const stock = selectedOption?.dataset.stock || 0;

        document.getElementById('purchasePrice').value = formatCurrency(price);
        document.getElementById('productStock').textContent = stock;
        calculateTotal();
    });

    // Calculate total when quantity changes
    document.getElementById('purchaseQuantity')?.addEventListener('input', calculateTotal);

    function calculateTotal() {
        const productSelect = document.getElementById('purchaseProduct');
        const selectedOption = productSelect.options[productSelect.selectedIndex];
        const price = parseFloat(selectedOption?.dataset.price || 0);
        const quantity = parseInt(document.getElementById('purchaseQuantity').value) || 0;
        const total = price * quantity;

        document.getElementById('purchaseTotal').value = formatCurrency(total);
    }

    // Format currency
    function formatCurrency(amount) {
        return new Intl.NumberFormat('vi-VN').format(amount) + 'đ';
    }

    // Handle Deposit Form Submission
    document.getElementById('depositForm')?.addEventListener('submit', function (e) {
        e.preventDefault();

        const formData = {
            UserID: parseInt(document.getElementById('depositUser').value),
            Amount: parseFloat(document.getElementById('depositAmount').value),
            PaymentMethod: document.getElementById('depositMethod').value,
            Message: document.getElementById('depositMessage').value
        };

        if (!formData.UserID || !formData.Amount || !formData.PaymentMethod) {
            alert('Vui lòng điền đầy đủ thông tin bắt buộc!');
            return;
        }

        fetch('/AdminPayment/CreateDeposit', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(formData)
        })
            .then(response => response.json())
            .then(data => {
                if (data.success) {
                    alert(data.message + '\nMã giao dịch: ' + data.transactionCode);
                    document.getElementById('depositForm').reset();
                    location.reload(); // Reload to update table
                } else {
                    alert('Lỗi: ' + data.message);
                }
            })
            .catch(error => {
                console.error('Error:', error);
                alert('Có lỗi xảy ra khi tạo giao dịch!');
            });
    });

    // Handle Purchase Form Submission
    document.getElementById('purchaseForm')?.addEventListener('submit', function (e) {
        e.preventDefault();

        const formData = {
            UserID: parseInt(document.getElementById('purchaseUser').value),
            ProductID: parseInt(document.getElementById('purchaseProduct').value),
            Quantity: parseInt(document.getElementById('purchaseQuantity').value),
            PaymentMethod: document.getElementById('purchaseMethod').value
        };

        if (!formData.UserID || !formData.ProductID || !formData.Quantity || !formData.PaymentMethod) {
            alert('Vui lòng điền đầy đủ thông tin bắt buộc!');
            return;
        }

        fetch('/AdminPayment/CreatePurchase', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(formData)
        })
            .then(response => response.json())
            .then(data => {
                if (data.success) {
                    alert(data.message + '\nMã đơn hàng: ' + data.orderCode);
                    document.getElementById('purchaseForm').reset();
                    location.reload(); // Reload to update table
                } else {
                    alert('Lỗi: ' + data.message);
                }
            })
            .catch(error => {
                console.error('Error:', error);
                alert('Có lỗi xảy ra khi tạo đơn hàng!');
            });
        // ... (rest of the file remains same)
    });

    // ============================================
    // GLOBAL QR HANDLER
    // ============================================
    window.handleQrDetails = function (orderId, userId, productId, quantity) {
        console.log('Handling QR details:', { orderId, userId, productId, quantity });

        // 1. Switch to Purchase Tab
        const purchaseBtn = document.querySelector('.tab-btn[data-tab="purchase"]');
        if (purchaseBtn) purchaseBtn.click();

        // 2. Fetch User Info (Phone) and Fill
        fetch(`/AdminPayment/GetUserById/${userId}`)
            .then(response => response.json())
            .then(data => {
                if (data.success) {
                    const userField = document.getElementById('purchaseUser');
                    const balanceField = document.getElementById('purchaseUserBalance');

                    if (userField) userField.value = data.phone || data.username;
                    if (balanceField) balanceField.textContent = formatCurrency(data.balance);

                    // 3. Select Product
                    const productSelect = document.getElementById('purchaseProduct');
                    if (productSelect) {
                        productSelect.value = productId;
                        // Trigger change event to update price/stock
                        productSelect.dispatchEvent(new Event('change'));
                    }

                    // 4. Set Quantity
                    const qtyField = document.getElementById('purchaseQuantity');
                    if (qtyField) {
                        qtyField.value = quantity;
                        qtyField.dispatchEvent(new Event('input'));
                    }

                    Swal.fire({
                        icon: 'info',
                        title: 'Tự động điền hoàn tất',
                        text: `Đã điền thông tin cho khách hàng ${data.fullName}`,
                        timer: 2000,
                        showConfirmButton: false
                    });
                } else {
                    Swal.fire('Lỗi', 'Không tìm thấy thông tin người dùng từ mã QR.', 'error');
                }
            })
            .catch(err => {
                console.error('Error fetching user for QR:', err);
                Swal.fire('Lỗi', 'Có lỗi xảy ra khi lấy thông tin người dùng.', 'error');
            });
    };

    // 4. Handle URL Parameters (if redirected from global scanner)
    const urlParams = new URLSearchParams(window.location.search);
    const qrOrder = urlParams.get('qrOrder');
    const qrUser = urlParams.get('qrUser');
    const qrProduct = urlParams.get('qrProduct');
    const qrQty = urlParams.get('qrQty');

    if (qrOrder && qrUser && qrProduct && qrQty) {
        // Wait a bit to ensure selects are populated (loadProducts is async)
        setTimeout(() => {
            window.handleQrDetails(qrOrder, qrUser, qrProduct, qrQty);
        }, 800);
    }
}

// Check if DOM is already loaded, if so run immediately, otherwise wait for DOMContentLoaded
if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', initAdminPayment);
} else {
    // DOM already loaded, run immediately
    initAdminPayment();
}
