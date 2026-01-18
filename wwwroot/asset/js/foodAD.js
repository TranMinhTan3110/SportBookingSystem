// ============================================
// FOOD & DRINKS ADMIN MANAGEMENT JAVASCRIPT
// ============================================

// Modal Functions
function openModal(mode, productId = null) {
    const modal = document.getElementById('productModal');
    const modalTitle = document.getElementById('modalTitle');

    if (mode === 'add') {
        modalTitle.innerHTML = '➕ Thêm Sản Phẩm Mới';
        document.getElementById('productForm').reset();
    } else if (mode === 'edit') {
        modalTitle.innerHTML = '✏️ Chỉnh Sửa Sản Phẩm';
        // TODO: Load product data from server
        // loadProductData(productId);
    }

    modal.classList.add('active');
    document.body.style.overflow = 'hidden'; // Prevent background scroll
}

function closeModal() {
    const modal = document.getElementById('productModal');
    modal.classList.remove('active');
    document.body.style.overflow = ''; // Restore scroll
    document.getElementById('productForm').reset();
}

function saveProduct() {
    // Get form data
    const productName = document.getElementById('productName').value.trim();
    const productType = document.getElementById('productType').value;
    const productPrice = document.getElementById('productPrice').value;
    const productStock = document.getElementById('productStock').value;
    const productUnit = document.getElementById('productUnit').value.trim();
    const productImage = document.getElementById('productImage').value.trim();
    const productDescription = document.getElementById('productDescription').value.trim();

    // Validate form
    const form = document.getElementById('productForm');
    if (!form.checkValidity()) {
        alert('Vui lòng điền đầy đủ thông tin bắt buộc!');
        return;
    }

    // Additional validation
    if (!productName) {
        alert('Vui lòng nhập tên sản phẩm!');
        return;
    }

    if (!productType) {
        alert('Vui lòng chọn loại sản phẩm!');
        return;
    }

    if (parseFloat(productPrice) <= 0) {
        alert('Giá sản phẩm phải lớn hơn 0!');
        return;
    }

    if (parseInt(productStock) < 0) {
        alert('Số lượng không được âm!');
        return;
    }

    // Prepare data object
    const productData = {
        name: productName,
        type: productType,
        price: parseFloat(productPrice),
        stock: parseInt(productStock),
        unit: productUnit || 'Phần',
        imageUrl: productImage,
        description: productDescription
    };

    console.log('Product Data:', productData);

    // TODO: Send data to server via AJAX
    // Example:
    // fetch('/Admin/FoodDrinks/Create', {
    //     method: 'POST',
    //     headers: {
    //         'Content-Type': 'application/json',
    //     },
    //     body: JSON.stringify(productData)
    // })
    // .then(response => response.json())
    // .then(data => {
    //     if (data.success) {
    //         alert('Lưu sản phẩm thành công!');
    //         closeModal();
    //         location.reload(); // Reload to show new product
    //     } else {
    //         alert('Có lỗi xảy ra: ' + data.message);
    //     }
    // })
    // .catch(error => {
    //     console.error('Error:', error);
    //     alert('Có lỗi xảy ra khi lưu sản phẩm!');
    // });

    // For now, just show success message
    alert('Lưu sản phẩm thành công!');
    closeModal();
}

function deleteProduct(id) {
    if (confirm('Bạn có chắc chắn muốn xóa sản phẩm này?\nHành động này không thể hoàn tác!')) {
        // TODO: Send delete request to server
        // fetch(`/Admin/FoodDrinks/Delete/${id}`, {
        //     method: 'DELETE',
        // })
        // .then(response => response.json())
        // .then(data => {
        //     if (data.success) {
        //         alert('Đã xóa sản phẩm thành công!');
        //         location.reload();
        //     } else {
        //         alert('Có lỗi xảy ra: ' + data.message);
        //     }
        // })
        // .catch(error => {
        //     console.error('Error:', error);
        //     alert('Có lỗi xảy ra khi xóa sản phẩm!');
        // });

        console.log('Delete product #' + id);
        alert('Đã xóa sản phẩm #' + id);
    }
}

function loadProductData(productId) {
    // TODO: Fetch product data from server and populate form
    // fetch(`/Admin/FoodDrinks/GetProduct/${productId}`)
    // .then(response => response.json())
    // .then(data => {
    //     document.getElementById('productName').value = data.name;
    //     document.getElementById('productType').value = data.type;
    //     document.getElementById('productPrice').value = data.price;
    //     document.getElementById('productStock').value = data.stock;
    //     document.getElementById('productUnit').value = data.unit;
    //     document.getElementById('productImage').value = data.imageUrl;
    //     document.getElementById('productDescription').value = data.description;
    // });
}

// ============================================
// SEARCH FUNCTIONALITY
// ============================================
function initializeSearch() {
    const searchInput = document.getElementById('searchInput');
    if (!searchInput) return;

    searchInput.addEventListener('input', function (e) {
        const searchTerm = e.target.value.toLowerCase().trim();
        const rows = document.querySelectorAll('#productTableBody tr');

        rows.forEach(row => {
            const productName = row.querySelector('.product-name');
            if (!productName) return;

            const productNameText = productName.textContent.toLowerCase();

            if (productNameText.includes(searchTerm)) {
                row.style.display = '';
            } else {
                row.style.display = 'none';
            }
        });
    });
}

// ============================================
// FILTER BY TYPE
// ============================================
function initializeTypeFilter() {
    const filterType = document.getElementById('filterType');
    if (!filterType) return;

    filterType.addEventListener('change', function (e) {
        const filterValue = e.target.value;
        const rows = document.querySelectorAll('#productTableBody tr');

        rows.forEach(row => {
            const badge = row.querySelector('.badge');
            if (!badge) return;

            if (filterValue === '') {
                row.style.display = '';
            } else {
                const badgeType = badge.textContent.trim();

                let shouldShow = false;

                switch (filterValue) {
                    case 'Food':
                        shouldShow = badgeType === 'Đồ ăn';
                        break;
                    case 'Drink':
                        shouldShow = badgeType === 'Thức uống';
                        break;
                    case 'Snack':
                        shouldShow = badgeType === 'Đồ ăn vặt';
                        break;
                    case 'Dessert':
                        shouldShow = badgeType === 'Tráng miệng';
                        break;
                }

                row.style.display = shouldShow ? '' : 'none';
            }
        });

        // Reset other filters when type changes
        document.getElementById('filterStock').value = '';
        document.getElementById('searchInput').value = '';
    });
}

// ============================================
// FILTER BY STOCK STATUS
// ============================================
function initializeStockFilter() {
    const filterStock = document.getElementById('filterStock');
    if (!filterStock) return;

    filterStock.addEventListener('change', function (e) {
        const filterValue = e.target.value;
        const rows = document.querySelectorAll('#productTableBody tr');

        rows.forEach(row => {
            const stockStatus = row.querySelector('.stock-status');
            if (!stockStatus) return;

            if (filterValue === '') {
                row.style.display = '';
            } else {
                let shouldShow = false;

                if (filterValue === 'in' && stockStatus.classList.contains('stock-in')) {
                    shouldShow = true;
                } else if (filterValue === 'low' && stockStatus.classList.contains('stock-low')) {
                    shouldShow = true;
                } else if (filterValue === 'out' && stockStatus.classList.contains('stock-out')) {
                    shouldShow = true;
                }

                row.style.display = shouldShow ? '' : 'none';
            }
        });
    });
}

// ============================================
// MODAL CLOSE ON OUTSIDE CLICK
// ============================================
function initializeModalClick() {
    const modal = document.getElementById('productModal');
    if (!modal) return;

    modal.addEventListener('click', function (e) {
        if (e.target === this) {
            closeModal();
        }
    });
}

// ============================================
// KEYBOARD SHORTCUTS
// ============================================
function initializeKeyboardShortcuts() {
    document.addEventListener('keydown', function (e) {
        // ESC to close modal
        if (e.key === 'Escape') {
            const modal = document.getElementById('productModal');
            if (modal && modal.classList.contains('active')) {
                closeModal();
            }
        }

        // Ctrl/Cmd + K to focus search
        if ((e.ctrlKey || e.metaKey) && e.key === 'k') {
            e.preventDefault();
            const searchInput = document.getElementById('searchInput');
            if (searchInput) {
                searchInput.focus();
            }
        }
    });
}

// ============================================
// FORM VALIDATION ENHANCEMENT
// ============================================
function initializeFormValidation() {
    const form = document.getElementById('productForm');
    if (!form) return;

    // Real-time validation for price
    const priceInput = document.getElementById('productPrice');
    if (priceInput) {
        priceInput.addEventListener('input', function (e) {
            const value = parseFloat(e.target.value);
            if (value < 0) {
                e.target.value = 0;
            }
        });
    }

    // Real-time validation for stock
    const stockInput = document.getElementById('productStock');
    if (stockInput) {
        stockInput.addEventListener('input', function (e) {
            const value = parseInt(e.target.value);
            if (value < 0) {
                e.target.value = 0;
            }
        });
    }
}

// ============================================
// TABLE ROW HOVER EFFECT
// ============================================
function initializeTableHover() {
    const rows = document.querySelectorAll('#productTableBody tr');

    rows.forEach(row => {
        row.addEventListener('mouseenter', function () {
            this.style.transform = 'scale(1.01)';
        });

        row.addEventListener('mouseleave', function () {
            this.style.transform = 'scale(1)';
        });
    });
}

// ============================================
// INITIALIZE ALL FUNCTIONS ON PAGE LOAD
// ============================================
document.addEventListener('DOMContentLoaded', function () {
    console.log('Food & Drinks Admin Page Loaded');

    // Initialize all features
    initializeSearch();
    initializeTypeFilter();
    initializeStockFilter();
    initializeModalClick();
    initializeKeyboardShortcuts();
    initializeFormValidation();
    initializeTableHover();

    console.log('All features initialized successfully');
});

// ============================================
// UTILITY FUNCTIONS
// ============================================

// Format currency
function formatCurrency(amount) {
    return new Intl.NumberFormat('vi-VN', {
        style: 'currency',
        currency: 'VND'
    }).format(amount);
}

// Format number with thousand separator
function formatNumber(num) {
    return new Intl.NumberFormat('vi-VN').format(num);
}

// Show toast notification (if you want to add toast notifications)
function showToast(message, type = 'success') {
    // TODO: Implement toast notification
    console.log(`[${type.toUpperCase()}] ${message}`);
}

// Export functions for potential use in other scripts
if (typeof module !== 'undefined' && module.exports) {
    module.exports = {
        openModal,
        closeModal,
        saveProduct,
        deleteProduct,
        formatCurrency,
        formatNumber
    };
}