let currentEditId = null;
let selectedImageFile = null;
function openModal(mode, productId = null) {
    const modal = document.getElementById('productModal');
    const modalTitle = document.getElementById('modalTitle');

    if (mode === 'add') {
        modalTitle.innerHTML = 'Thêm Sản Phẩm Mới';
        document.getElementById('productForm').reset();
        currentEditId = null;
        removeImage();
    } else if (mode === 'edit') {
        modalTitle.innerHTML = 'Chỉnh Sửa Sản Phẩm';
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

function loadProductData(productId) {
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

    Swal.fire({
        title: selectedImageFile ? 'Đang upload hình ảnh...' : 'Đang lưu...',
        text: 'Vui lòng đợi',
        allowOutsideClick: false,
        didOpen: () => {
            Swal.showLoading();
        }
    });

    let imageUrl = document.getElementById('productImage').value;

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

function previewImage(event) {
    const file = event.target.files[0];
    if (!file) return;

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

    const reader = new FileReader();
    reader.onload = function (e) {
        const previewImg = document.getElementById('previewImg');
        const btnRemove = document.querySelector('.btn-remove-image');

        previewImg.src = e.target.result;
        btnRemove.style.display = 'flex';

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

function refreshFilters() {
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
function applyFilters() {
    const searchTerm = document.getElementById('searchInput').value.toLowerCase().trim();
    const categoryFilter = document.getElementById('filterType').value;
    const stockFilter = document.getElementById('filterStock').value;
    const rows = document.querySelectorAll('#productTableBody tr');

    let visibleCount = 0;

    rows.forEach(row => {
        let showRow = true;

        if (searchTerm) {
            const productName = row.querySelector('.product-name');
            if (productName) {
                const productNameText = productName.textContent.toLowerCase();
                if (!productNameText.includes(searchTerm)) {
                    showRow = false;
                }
            }
        }

        // Lọc theo loại
        if (categoryFilter && showRow) {
            const rowCategoryId = row.getAttribute('data-category-id');
            if (rowCategoryId !== categoryFilter) {
                showRow = false;
            }
        }

        // Lọc theo số lượng
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

        row.style.display = showRow ? '' : 'none';
        if (showRow) visibleCount++;
    });

    return visibleCount;
}
function initializeSearch() {
    const searchInput = document.getElementById('searchInput');
    if (!searchInput) return;

    searchInput.addEventListener('input', function (e) {
        applyFilters();
    });
}

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

function initializeModalClick() {
    const modal = document.getElementById('productModal');
    if (!modal) return;

    modal.addEventListener('click', function (e) {
        if (e.target === this) {
            closeModal();
        }
    });
}

function initializeKeyboardShortcuts() {
    document.addEventListener('keydown', function (e) {
        if (e.key === 'Escape') {
            const modal = document.getElementById('productModal');
            if (modal && modal.classList.contains('active')) {
                closeModal();
            }
        }

        if ((e.ctrlKey || e.metaKey) && e.key === 'k') {
            e.preventDefault();
            const searchInput = document.getElementById('searchInput');
            if (searchInput) {
                searchInput.focus();
            }
        }
    });
}

document.addEventListener('DOMContentLoaded', function () {
    console.log('Food & Drinks Admin Page Loaded');

    initializeSearch();
    initializeTypeFilter();
    initializeStockFilter();
    initializeModalClick();
    initializeKeyboardShortcuts();
    initializePagination();

    console.log('All features initialized successfully');

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


function formatCurrency(amount) {
    return new Intl.NumberFormat('vi-VN', {
        style: 'currency',
        currency: 'VND'
    }).format(amount);
}

function formatNumber(num) {
    return new Intl.NumberFormat('vi-VN').format(num);
}

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

    // Kiem tra trang hien tai
    if (currentPage > totalPages && totalPages > 0) {
        currentPage = totalPages;
    }
    if (currentPage < 1) {
        currentPage = 1;
    }

    // An tat ca cac hang truoc do
    visibleRows.forEach(row => row.classList.add('pagination-hidden'));

    // Hien thi cac hang cho trang hien tai
    const startIndex = (currentPage - 1) * itemsPerPage;
    const endIndex = startIndex + itemsPerPage;

    visibleRows.slice(startIndex, endIndex).forEach(row => {
        row.classList.remove('pagination-hidden');
    });

    // Cap nhat UI
    updatePaginationUI();
}

function updatePaginationUI() {
    // Cap nhat thong tin
    const startItem = totalItems === 0 ? 0 : (currentPage - 1) * itemsPerPage + 1;
    const endItem = Math.min(currentPage * itemsPerPage, totalItems);

    document.getElementById('paginationInfo').innerHTML =
        `Hiển thị <strong>${startItem}-${endItem}</strong> của <strong>${totalItems}</strong> sản phẩm`;

    // Cap nhat trang thai nut
    document.getElementById('firstPage').disabled = currentPage === 1;
    document.getElementById('prevPage').disabled = currentPage === 1;
    document.getElementById('nextPage').disabled = currentPage === totalPages || totalPages === 0;
    document.getElementById('lastPage').disabled = currentPage === totalPages || totalPages === 0;

    generatePageNumbers();
}

function generatePageNumbers() {
    const paginationPages = document.getElementById('paginationPages');
    paginationPages.innerHTML = '';

    if (totalPages === 0) return;

    const maxVisiblePages = 5;
    let startPage = Math.max(1, currentPage - Math.floor(maxVisiblePages / 2));
    let endPage = Math.min(totalPages, startPage + maxVisiblePages - 1);

    if (endPage - startPage < maxVisiblePages - 1) {
        startPage = Math.max(1, endPage - maxVisiblePages + 1);
    }

    // Trang dau tien
    if (startPage > 1) {
        addPageButton(1);
        if (startPage > 2) {
            addEllipsis();
        }
    }

    // So trang
    for (let i = startPage; i <= endPage; i++) {
        addPageButton(i);
    }

    // Trang cuoi
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



function applyFilters() {
    const searchTerm = document.getElementById('searchInput').value.toLowerCase().trim();
    const categoryFilter = document.getElementById('filterType').value;
    const stockFilter = document.getElementById('filterStock').value;
    const rows = document.querySelectorAll('#productTableBody tr');

    let visibleCount = 0;

    rows.forEach(row => {
        let showRow = true;

        // Lọc theo tu khoa
        if (searchTerm) {
            const productName = row.querySelector('.product-name');
            if (productName) {
                const productNameText = productName.textContent.toLowerCase();
                if (!productNameText.includes(searchTerm)) {
                    showRow = false;
                }
            }
        }

        // Lọc theo loại
        if (categoryFilter && showRow) {
            const rowCategoryId = row.getAttribute('data-category-id');
            if (rowCategoryId !== categoryFilter) {
                showRow = false;
            }
        }

        // Lọc theo kho
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

        row.style.display = showRow ? '' : 'none';
        if (showRow) visibleCount++;
    });
    currentPage = 1;
    updatePagination();

    return visibleCount;
}

function toggleProductStatus(id, currentStatus) {
    const actionText = currentStatus ? 'ẩn' : 'hiện';
    const actionIcon = currentStatus ? 'eye-slash' : 'eye';

    Swal.fire({
        title: `Xác nhận ${actionText} sản phẩm?`,
        text: currentStatus
            ? "Sản phẩm sẽ không hiển thị cho khách hàng"
            : "Sản phẩm sẽ hiển thị trở lại cho khách hàng",
        icon: 'question',
        showCancelButton: true,
        confirmButtonColor: currentStatus ? '#F59E0B' : '#10B981',
        cancelButtonColor: '#6B7280',
        confirmButtonText: currentStatus ? 'Ẩn sản phẩm' : 'Hiện sản phẩm',
        cancelButtonText: 'Hủy'
    }).then((result) => {
        if (result.isConfirmed) {
            Swal.fire({
                title: 'Đang cập nhật...',
                text: 'Vui lòng đợi',
                allowOutsideClick: false,
                didOpen: () => {
                    Swal.showLoading();
                }
            });

            fetch(`/FoodAD/ToggleStatus/${id}`, {
                method: 'POST',
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
                        text: 'Có lỗi xảy ra khi thay đổi trạng thái!',
                        confirmButtonColor: '#10B981'
                    });
                });
        }
    });
}