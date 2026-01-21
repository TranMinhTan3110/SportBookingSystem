// ============================================
// FOOD & DRINKS ADMIN MANAGEMENT JAVASCRIPT
// ============================================

// Global variables
let currentEditId = null;
let selectedImageFile = null;

// ============================================
// MODAL FUNCTIONS
// ============================================
function openModal(mode, productId = null) {
    const modal = document.getElementById('productModal');
    const modalTitle = document.getElementById('modalTitle');

    if (mode === 'add') {
        modalTitle.innerHTML = '➕ Thêm Sản Phẩm Mới';
        document.getElementById('productForm').reset();
        currentEditId = null;
        removeImage();
    } else if (mode === 'edit') {
        modalTitle.innerHTML = '✏️ Chỉnh Sửa Sản Phẩm';
        currentEditId = productId;
        loadProductData(productId);
    }

    modal.classList.add('active');
    document.body.style.overflow = 'hidden';
}

function closeModal() {
    const modal = document.getElementById('productModal');
    modal.classList.remove('active');
    document.body.style.overflow = '';
    document.getElementById('productForm').reset();
    removeImage();
    currentEditId = null;
}

// ============================================
// LOAD PRODUCT DATA FOR EDIT
// ============================================
function loadProductData(productId) {
    // Show loading
    Swal.fire({
        title: 'Đang tải...',
        text: 'Vui lòng đợi',
        allowOutsideClick: false,
        didOpen: () => {
            Swal.showLoading();
        }
    });

    fetch(`/FoodAD/GetProduct/${productId}`)
        .then(response => response.json())
        .then(result => {
            Swal.close();

            if (result.status === 'success') {
                const data = result.data;
                document.getElementById('productName').value = data.productName;
                document.getElementById('productCategory').value = data.categoryId;
                document.getElementById('productPrice').value = data.price;
                document.getElementById('productStock').value = data.stockQuantity;
                document.getElementById('productUnit').value = data.unit;

                // Load existing image
                if (data.imageUrl) {
                    document.getElementById('previewImg').src = data.imageUrl;
                    document.querySelector('.btn-remove-image').style.display = 'flex';
                    document.getElementById('productImage').value = data.imageUrl;
                } else {
                    removeImage();
                }
            } else {
                Swal.fire({
                    icon: 'error',
                    title: 'Lỗi!',
                    text: result.message,
                    confirmButtonColor: '#10B981'
                });
            }
        })
        .catch(error => {
            Swal.close();
            console.error('Error:', error);
            Swal.fire({
                icon: 'error',
                title: 'Lỗi!',
                text: 'Có lỗi xảy ra khi tải dữ liệu sản phẩm!',
                confirmButtonColor: '#10B981'
            });
        });
}

// ============================================
// SAVE PRODUCT (CREATE/UPDATE)
// ============================================
async function saveProduct() {
    const form = document.getElementById('productForm');
    if (!form.checkValidity()) {
        Swal.fire({
            icon: 'warning',
            title: 'Thiếu thông tin!',
            text: 'Vui lòng điền đầy đủ thông tin bắt buộc!',
            confirmButtonColor: '#10B981'
        });
        return;
    }

    // Show loading
    Swal.fire({
        title: selectedImageFile ? 'Đang upload hình ảnh...' : 'Đang lưu...',
        text: 'Vui lòng đợi',
        allowOutsideClick: false,
        didOpen: () => {
            Swal.showLoading();
        }
    });

    let imageUrl = document.getElementById('productImage').value;

    // Upload image if new file is selected
    if (selectedImageFile) {
        const uploadResult = await uploadImage(selectedImageFile);
        if (uploadResult.success) {
            imageUrl = uploadResult.url;
        } else {
            Swal.fire({
                icon: 'error',
                title: 'Lỗi upload!',
                text: 'Có lỗi xảy ra khi upload hình ảnh!',
                confirmButtonColor: '#10B981'
            });
            return;
        }
    }

    const productData = {
        productId: currentEditId || 0,
        productName: document.getElementById('productName').value.trim(),
        categoryId: parseInt(document.getElementById('productCategory').value),
        price: parseFloat(document.getElementById('productPrice').value),
        stockQuantity: parseInt(document.getElementById('productStock').value),
        unit: document.getElementById('productUnit').value.trim() || 'Phần',
        imageUrl: imageUrl
    };

    const url = currentEditId ? '/FoodAD/Update' : '/FoodAD/Create';

    fetch(url, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
        },
        body: JSON.stringify(productData)
    })
        .then(response => response.json())
        .then(result => {
            if (result.status === 'success') {
                Swal.fire({
                    icon: 'success',
                    title: 'Thành công!',
                    text: result.message,
                    confirmButtonColor: '#10B981',
                    timer: 1500
                }).then(() => {
                    closeModal();
                    location.reload();
                });
            } else {
                Swal.fire({
                    icon: 'error',
                    title: 'Lỗi!',
                    text: result.message,
                    confirmButtonColor: '#10B981'
                });
            }
        })
        .catch(error => {
            console.error('Error:', error);
            Swal.fire({
                icon: 'error',
                title: 'Lỗi!',
                text: 'Có lỗi xảy ra khi lưu sản phẩm!',
                confirmButtonColor: '#10B981'
            });
        });
}

// ============================================
// DELETE PRODUCT
// ============================================
function deleteProduct(id) {
    Swal.fire({
        title: 'Xác nhận xóa?',
        text: "Bạn có chắc chắn muốn xóa sản phẩm này? Hành động này không thể hoàn tác!",
        icon: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#EF4444',
        cancelButtonColor: '#6B7280',
        confirmButtonText: 'Xóa',
        cancelButtonText: 'Hủy'
    }).then((result) => {
        if (result.isConfirmed) {
            // Show loading
            Swal.fire({
                title: 'Đang xóa...',
                text: 'Vui lòng đợi',
                allowOutsideClick: false,
                didOpen: () => {
                    Swal.showLoading();
                }
            });

            fetch(`/FoodAD/Delete/${id}`, {
                method: 'POST',
            })
                .then(response => response.json())
                .then(result => {
                    if (result.status === 'success') {
                        Swal.fire({
                            icon: 'success',
                            title: 'Đã xóa!',
                            text: result.message,
                            confirmButtonColor: '#10B981',
                            timer: 1500
                        }).then(() => {
                            location.reload();
                        });
                    } else {
                        Swal.fire({
                            icon: 'error',
                            title: 'Lỗi!',
                            text: result.message,
                            confirmButtonColor: '#10B981'
                        });
                    }
                })
                .catch(error => {
                    console.error('Error:', error);
                    Swal.fire({
                        icon: 'error',
                        title: 'Lỗi!',
                        text: 'Có lỗi xảy ra khi xóa sản phẩm!',
                        confirmButtonColor: '#10B981'
                    });
                });
        }
    });
}

// ============================================
// ADD STOCK QUICK
// ============================================
function addStockQuick(productId) {
    Swal.fire({
        title: 'Thêm số lượng vào kho',
        input: 'number',
        inputLabel: 'Nhập số lượng cần thêm',
        inputValue: 10,
        showCancelButton: true,
        confirmButtonColor: '#10B981',
        cancelButtonColor: '#6B7280',
        confirmButtonText: 'Thêm',
        cancelButtonText: 'Hủy',
        inputValidator: (value) => {
            if (!value || value <= 0) {
                return 'Vui lòng nhập số lượng hợp lệ (lớn hơn 0)!';
            }
        }
    }).then((result) => {
        if (result.isConfirmed) {
            const qty = parseInt(result.value);

            // Show loading
            Swal.fire({
                title: 'Đang cập nhật...',
                text: 'Vui lòng đợi',
                allowOutsideClick: false,
                didOpen: () => {
                    Swal.showLoading();
                }
            });

            fetch('/FoodAD/AddStock', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify({
                    productId: productId,
                    amount: qty
                })
            })
                .then(response => response.json())
                .then(result => {
                    if (result.status === 'success') {
                        Swal.fire({
                            icon: 'success',
                            title: 'Thành công!',
                            text: result.message,
                            confirmButtonColor: '#10B981',
                            timer: 1500
                        }).then(() => {
                            location.reload();
                        });
                    } else {
                        Swal.fire({
                            icon: 'error',
                            title: 'Lỗi!',
                            text: result.message,
                            confirmButtonColor: '#10B981'
                        });
                    }
                })
                .catch(error => {
                    console.error('Error:', error);
                    Swal.fire({
                        icon: 'error',
                        title: 'Lỗi!',
                        text: 'Có lỗi xảy ra khi cập nhật số lượng!',
                        confirmButtonColor: '#10B981'
                    });
                });
        }
    });
}

// ============================================
// IMAGE UPLOAD FUNCTIONS
// ============================================
function previewImage(event) {
    const file = event.target.files[0];
    if (!file) return;

    // Validate file type
    if (!file.type.startsWith('image/')) {
        Swal.fire({
            icon: 'error',
            title: 'File không hợp lệ!',
            text: 'Vui lòng chọn file hình ảnh!',
            confirmButtonColor: '#10B981'
        });
        event.target.value = '';
        return;
    }

    // Validate file size (max 5MB)
    if (file.size > 5 * 1024 * 1024) {
        Swal.fire({
            icon: 'error',
            title: 'File quá lớn!',
            text: 'Kích thước file không được vượt quá 5MB!',
            confirmButtonColor: '#10B981'
        });
        event.target.value = '';
        return;
    }

    selectedImageFile = file;

    // Preview image
    const reader = new FileReader();
    reader.onload = function (e) {
        const previewImg = document.getElementById('previewImg');
        const btnRemove = document.querySelector('.btn-remove-image');

        previewImg.src = e.target.result;
        btnRemove.style.display = 'flex';

        // Show success toast
        const Toast = Swal.mixin({
            toast: true,
            position: 'top-end',
            showConfirmButton: false,
            timer: 2000,
            timerProgressBar: true
        });

        Toast.fire({
            icon: 'success',
            title: 'Đã chọn hình ảnh'
        });
    };
    reader.readAsDataURL(file);
}

function removeImage() {
    selectedImageFile = null;
    const fileInput = document.getElementById('productImageFile');
    if (fileInput) {
        fileInput.value = '';
    }
    document.getElementById('previewImg').src = 'https://via.placeholder.com/300x300/374151/9CA3AF?text=Chon+Hinh';
    const btnRemove = document.querySelector('.btn-remove-image');
    if (btnRemove) {
        btnRemove.style.display = 'none';
    }
}

async function uploadImage(file) {
    try {
        const formData = new FormData();
        formData.append('imageFile', file);

        const response = await fetch('/FoodAD/UploadImage', {
            method: 'POST',
            body: formData
        });

        const result = await response.json();
        return result;
    } catch (error) {
        console.error('Upload error:', error);
        return { success: false };
    }
}

// ============================================
// REFRESH FILTERS
// ============================================
function refreshFilters() {
    // Show loading toast
    const Toast = Swal.mixin({
        toast: true,
        position: 'top-end',
        showConfirmButton: false,
        timer: 1000,
        timerProgressBar: true
    });

    Toast.fire({
        icon: 'info',
        title: 'Đang làm mới...'
    });

    // Reset tất cả các bộ lọc
    document.getElementById('searchInput').value = '';
    document.getElementById('filterType').value = '';
    document.getElementById('filterStock').value = '';

    // Reload trang sau 500ms
    setTimeout(() => {
        window.location.href = '/FoodAD/Index';
    }, 500);
}
// ============================================
// COMBINED FILTER FUNCTION
// ============================================
function applyFilters() {
    const searchTerm = document.getElementById('searchInput').value.toLowerCase().trim();
    const categoryFilter = document.getElementById('filterType').value;
    const stockFilter = document.getElementById('filterStock').value;
    const rows = document.querySelectorAll('#productTableBody tr');

    let visibleCount = 0;

    rows.forEach(row => {
        let showRow = true;

        // Filter by search term
        if (searchTerm) {
            const productName = row.querySelector('.product-name');
            if (productName) {
                const productNameText = productName.textContent.toLowerCase();
                if (!productNameText.includes(searchTerm)) {
                    showRow = false;
                }
            }
        }

        // Filter by category
        if (categoryFilter && showRow) {
            const rowCategoryId = row.getAttribute('data-category-id');
            if (rowCategoryId !== categoryFilter) {
                showRow = false;
            }
        }

        // Filter by stock
        if (stockFilter && showRow) {
            const stockStatus = row.querySelector('.stock-status');
            if (stockStatus) {
                let matchStock = false;
                if (stockFilter === 'in' && stockStatus.classList.contains('stock-in')) {
                    matchStock = true;
                } else if (stockFilter === 'low' && stockStatus.classList.contains('stock-low')) {
                    matchStock = true;
                } else if (stockFilter === 'out' && stockStatus.classList.contains('stock-out')) {
                    matchStock = true;
                }
                if (!matchStock) {
                    showRow = false;
                }
            }
        }

        // Apply visibility
        row.style.display = showRow ? '' : 'none';
        if (showRow) visibleCount++;
    });

    return visibleCount;
}
// ============================================
// SEARCH FUNCTIONALITY
// ============================================
function initializeSearch() {
    const searchInput = document.getElementById('searchInput');
    if (!searchInput) return;

    searchInput.addEventListener('input', function (e) {
        applyFilters();
    });
}

// ============================================
// FILTER BY TYPE - USING DATA ATTRIBUTE
// ============================================
function initializeTypeFilter() {
    const filterType = document.getElementById('filterType');
    if (!filterType) return;

    filterType.addEventListener('change', function (e) {
        const visibleCount = applyFilters();

        const Toast = Swal.mixin({
            toast: true,
            position: 'top-end',
            showConfirmButton: false,
            timer: 1500,
            timerProgressBar: true
        });

        if (e.target.value === '') {
            Toast.fire({
                icon: 'info',
                title: `Hiển thị tất cả (${visibleCount} sản phẩm)`
            });
        } else {
            const selectedOption = filterType.options[filterType.selectedIndex];
            Toast.fire({
                icon: 'info',
                title: `${selectedOption.text}: ${visibleCount} sản phẩm`
            });
        }
    });
}

// ============================================
// FILTER BY STOCK STATUS
// ============================================
function initializeStockFilter() {
    const filterStock = document.getElementById('filterStock');
    if (!filterStock) return;

    filterStock.addEventListener('change', function (e) {
        const visibleCount = applyFilters();

        const Toast = Swal.mixin({
            toast: true,
            position: 'top-end',
            showConfirmButton: false,
            timer: 1500,
            timerProgressBar: true
        });

        const statusText = {
            'in': 'Còn hàng',
            'low': 'Sắp hết',
            'out': 'Hết hàng'
        };

        if (e.target.value === '') {
            Toast.fire({
                icon: 'info',
                title: `Hiển thị tất cả (${visibleCount} sản phẩm)`
            });
        } else {
            Toast.fire({
                icon: 'info',
                title: `${statusText[e.target.value]}: ${visibleCount} sản phẩm`
            });
        }
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
    initializePagination(); 

    console.log('All features initialized successfully');

    // Show welcome toast
    const Toast = Swal.mixin({
        toast: true,
        position: 'top-end',
        showConfirmButton: false,
        timer: 2000,
        timerProgressBar: true
    });

    Toast.fire({
        icon: 'success',
        title: 'Trang đã sẵn sàng!'
    });
});

// ============================================
// UTILITY FUNCTIONS
// ============================================
function formatCurrency(amount) {
    return new Intl.NumberFormat('vi-VN', {
        style: 'currency',
        currency: 'VND'
    }).format(amount);
}

function formatNumber(num) {
    return new Intl.NumberFormat('vi-VN').format(num);
}
// ============================================
// PAGINATION FUNCTIONALITY
// ============================================
let currentPage = 1;
let itemsPerPage = 10;
let totalItems = 0;
let totalPages = 0;

function initializePagination() {
    updatePagination();
}

function updatePagination() {
    const rows = document.querySelectorAll('#productTableBody tr');
    const visibleRows = Array.from(rows).filter(row => row.style.display !== 'none');

    totalItems = visibleRows.length;
    totalPages = Math.ceil(totalItems / itemsPerPage);

    // Ensure current page is valid
    if (currentPage > totalPages && totalPages > 0) {
        currentPage = totalPages;
    }
    if (currentPage < 1) {
        currentPage = 1;
    }

    // Hide all rows first
    visibleRows.forEach(row => row.classList.add('pagination-hidden'));

    // Show rows for current page
    const startIndex = (currentPage - 1) * itemsPerPage;
    const endIndex = startIndex + itemsPerPage;

    visibleRows.slice(startIndex, endIndex).forEach(row => {
        row.classList.remove('pagination-hidden');
    });

    // Update pagination UI
    updatePaginationUI();
}

function updatePaginationUI() {
    // Update info text
    const startItem = totalItems === 0 ? 0 : (currentPage - 1) * itemsPerPage + 1;
    const endItem = Math.min(currentPage * itemsPerPage, totalItems);

    document.getElementById('paginationInfo').innerHTML =
        `Hiển thị <strong>${startItem}-${endItem}</strong> của <strong>${totalItems}</strong> sản phẩm`;

    // Update buttons state
    document.getElementById('firstPage').disabled = currentPage === 1;
    document.getElementById('prevPage').disabled = currentPage === 1;
    document.getElementById('nextPage').disabled = currentPage === totalPages || totalPages === 0;
    document.getElementById('lastPage').disabled = currentPage === totalPages || totalPages === 0;

    // Generate page numbers
    generatePageNumbers();
}

function generatePageNumbers() {
    const paginationPages = document.getElementById('paginationPages');
    paginationPages.innerHTML = '';

    if (totalPages === 0) return;

    const maxVisiblePages = 5;
    let startPage = Math.max(1, currentPage - Math.floor(maxVisiblePages / 2));
    let endPage = Math.min(totalPages, startPage + maxVisiblePages - 1);

    // Adjust start if we're near the end
    if (endPage - startPage < maxVisiblePages - 1) {
        startPage = Math.max(1, endPage - maxVisiblePages + 1);
    }

    // First page
    if (startPage > 1) {
        addPageButton(1);
        if (startPage > 2) {
            addEllipsis();
        }
    }

    // Page numbers
    for (let i = startPage; i <= endPage; i++) {
        addPageButton(i);
    }

    // Last page
    if (endPage < totalPages) {
        if (endPage < totalPages - 1) {
            addEllipsis();
        }
        addPageButton(totalPages);
    }
}

function addPageButton(pageNum) {
    const button = document.createElement('button');
    button.className = 'page-number' + (pageNum === currentPage ? ' active' : '');
    button.textContent = pageNum;
    button.onclick = () => goToPage(pageNum);
    document.getElementById('paginationPages').appendChild(button);
}

function addEllipsis() {
    const ellipsis = document.createElement('span');
    ellipsis.className = 'page-ellipsis';
    ellipsis.textContent = '...';
    document.getElementById('paginationPages').appendChild(ellipsis);
}

function goToPage(page) {
    if (page < 1 || page > totalPages) return;
    currentPage = page;
    updatePagination();

    // Scroll to top of table
    document.querySelector('.table-section').scrollIntoView({ behavior: 'smooth', block: 'start' });
}

function goToFirstPage() {
    goToPage(1);
}

function goToPrevPage() {
    goToPage(currentPage - 1);
}

function goToNextPage() {
    goToPage(currentPage + 1);
}

function goToLastPage() {
    goToPage(totalPages);
}

// ============================================
// UPDATE APPLY FILTERS TO WORK WITH PAGINATION
// ============================================
function applyFilters() {
    const searchTerm = document.getElementById('searchInput').value.toLowerCase().trim();
    const categoryFilter = document.getElementById('filterType').value;
    const stockFilter = document.getElementById('filterStock').value;
    const rows = document.querySelectorAll('#productTableBody tr');

    let visibleCount = 0;

    rows.forEach(row => {
        let showRow = true;

        // Filter by search term
        if (searchTerm) {
            const productName = row.querySelector('.product-name');
            if (productName) {
                const productNameText = productName.textContent.toLowerCase();
                if (!productNameText.includes(searchTerm)) {
                    showRow = false;
                }
            }
        }

        // Filter by category
        if (categoryFilter && showRow) {
            const rowCategoryId = row.getAttribute('data-category-id');
            if (rowCategoryId !== categoryFilter) {
                showRow = false;
            }
        }

        // Filter by stock
        if (stockFilter && showRow) {
            const stockStatus = row.querySelector('.stock-status');
            if (stockStatus) {
                let matchStock = false;
                if (stockFilter === 'in' && stockStatus.classList.contains('stock-in')) {
                    matchStock = true;
                } else if (stockFilter === 'low' && stockStatus.classList.contains('stock-low')) {
                    matchStock = true;
                } else if (stockFilter === 'out' && stockStatus.classList.contains('stock-out')) {
                    matchStock = true;
                }
                if (!matchStock) {
                    showRow = false;
                }
            }
        }

        // Apply visibility for filtering
        row.style.display = showRow ? '' : 'none';
        if (showRow) visibleCount++;
    });

    // Reset to first page when filters change
    currentPage = 1;
    updatePagination();

    return visibleCount;
}