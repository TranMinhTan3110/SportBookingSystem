
document.addEventListener('DOMContentLoaded', function () {
    loadProducts();
    loadCategories();
    setupFilters();
});

let productCategories = [];

// Modal Functions
function openModal(mode, productId = null) {
    const modal = document.getElementById('productModal');
    const modalTitle = document.getElementById('modalTitle');
    const form = document.getElementById('productForm');

    form.reset();
    removeImage();
    document.getElementById('productId').value = '';

    if (mode === 'add') {
        modalTitle.innerHTML = '➕ Thêm Sản Phẩm Mới';
    } else if (mode === 'edit') {
        modalTitle.innerHTML = '✏️ Chỉnh Sửa Sản Phẩm';
        loadProductToEdit(productId);
    }

    modal.classList.add('active');
    document.body.style.overflow = 'hidden';
}

function loadProductToEdit(id) {
    fetch(`/SportProduct/GetById?id=${id}`)
        .then(response => response.json())
        .then(res => {
            if (res.success) {
                const p = res.data;
                document.getElementById('productId').value = p.productId;
                document.getElementById('productName').value = p.productName;
                document.getElementById('productType').value = p.categoryId || '';
                document.getElementById('productBrand').value = p.brand || '';
                document.getElementById('productSize').value = p.size || '';
                document.getElementById('productPrice').value = p.price;
                document.getElementById('productStock').value = p.stockQuantity;
                document.getElementById('productUnit').value = p.unit || '';

                if (p.imageUrl) {
                    const preview = document.getElementById('imagePreview');
                    const container = document.getElementById('imagePreviewContainer');
                    preview.src = p.imageUrl;
                    container.style.display = 'block';
                }
            }
        });
}

function closeModal() {
    const modal = document.getElementById('productModal');
    modal.classList.remove('active');
    document.body.style.overflow = '';
}


function handleFileSelect(input) {
    const file = input.files[0];
    if (file) {
        if (!file.type.startsWith('image/')) {
            Swal.fire('Lỗi!', 'Vui lòng chọn một định dạng hình ảnh hợp lệ.', 'error');
            input.value = '';
            return;
        }

        const reader = new FileReader();
        reader.onload = function (e) {
            const preview = document.getElementById('imagePreview');
            const container = document.getElementById('imagePreviewContainer');
            preview.src = e.target.result;
            container.style.display = 'block';
        }
        reader.readAsDataURL(file);
    }
}

function removeImage() {
    const input = document.getElementById('productImageFile');
    const preview = document.getElementById('imagePreview');
    const container = document.getElementById('imagePreviewContainer');

    input.value = '';
    preview.src = '';
    container.style.display = 'none';
}


function setupFilters() {
    const searchInput = document.getElementById('searchInput');
    const filterType = document.getElementById('filterType');
    const filterBrand = document.getElementById('filterBrand');
    const filterStock = document.getElementById('filterStock');

    let debounceTimer;
    searchInput.addEventListener('keyup', () => {
        clearTimeout(debounceTimer);
        debounceTimer = setTimeout(() => loadProducts(), 500);
    });

    [filterType, filterBrand, filterStock].forEach(el => {
        el.addEventListener('change', () => loadProducts());
    });
}

function loadCategories() {
    fetch('/SportProduct/GetCategories')
        .then(response => response.json())
        .then(res => {
            if (res.success) {
                productCategories = res.data;
                renderCategoryDropdowns(res.data);
            }
        })
        .catch(err => console.error('Error loading categories:', err));
}

function renderCategoryDropdowns(categories) {
    const filterType = document.getElementById('filterType');
    const productType = document.getElementById('productType');

    filterType.innerHTML = '<option value="">Tất cả</option>';
    productType.innerHTML = '<option value="">Chọn loại sản phẩm</option>';

    categories.forEach(cat => {
        filterType.innerHTML += `<option value="${cat.categoryName}">${cat.categoryName}</option>`;
        productType.innerHTML += `<option value="${cat.categoryId}" data-name="${cat.categoryName}">${cat.categoryName}</option>`;
    });
}

function loadProducts() {
    const search = document.getElementById('searchInput').value;
    const type = document.getElementById('filterType').value;
    const brand = document.getElementById('filterBrand').value;
    const stockStatus = document.getElementById('filterStock').value;

    const params = new URLSearchParams({
        search,
        type,
        brand,
        stockStatus
    });

    fetch(`/SportProduct/GetAll?${params.toString()}`)
        .then(response => response.json())
        .then(res => {
            if (res.success) {
                renderProductTable(res.data);
            }
        })
        .catch(err => console.error('Error loading products:', err));
}

function renderProductTable(products) {
    const tableBody = document.getElementById('productTableBody');
    tableBody.innerHTML = '';

    products.forEach(product => {
        const tr = document.createElement('tr');
        tr.innerHTML = `
            <td>#${product.productId}</td>
            <td><img src="${product.imageUrl || 'https://via.placeholder.com/200'}" alt="Product" class="product-img"></td>
            <td>
                <div class="product-name">${product.productName}</div>
                <div class="product-brand">${product.brand || 'N/A'}</div>
            </td>
            <td><span class="badge badge-type">${product.productType || 'Khác'}</span></td>
            <td>${product.size || 'N/A'}</td>
            <td>${product.price.toLocaleString('vi-VN')}₫</td>
            <td><span class="stock-status ${product.stockQuantity > 10 ? 'stock-in' : (product.stockQuantity > 0 ? 'stock-low' : 'stock-out')}">${product.stockQuantity}</span></td>
            <td>
                <div class="action-buttons">
                    <button class="btn-action btn-view" onclick="updateStockPrompt(${product.productId}, '${product.productName}')" title="Nhập thêm hàng"><i class="fa-solid fa-plus"></i></button>
                    <button class="btn-action btn-edit" onclick="openModal('edit', ${product.productId})" title="Sửa"><i class="fa-solid fa-pen-to-square"></i></button>
                    <button class="btn-action btn-delete" onclick="deleteProduct(${product.productId})" title="Xóa"><i class="fa-solid fa-trash"></i></button>
                </div>
            </td>
        `;
        tableBody.appendChild(tr);
    });
}

function saveProduct() {
    const form = document.getElementById('productForm');
    if (!form.checkValidity()) {
        Swal.fire('Thông báo', 'Vui lòng điền đầy đủ thông tin bắt buộc!', 'warning');
        return;
    }

    const id = document.getElementById('productId').value;
    const isEdit = id !== '';

    const typeSelect = document.getElementById('productType');
    const selectedOption = typeSelect.options[typeSelect.selectedIndex];

    const formData = new FormData();
    if (isEdit) formData.append('ProductId', id);
    formData.append('ProductName', document.getElementById('productName').value);
    formData.append('CategoryId', typeSelect.value); // Send ID
    formData.append('ProductType', selectedOption.getAttribute('data-name') || ''); // Send Name if needed for ProductType field
    formData.append('Brand', document.getElementById('productBrand').value);
    formData.append('Size', document.getElementById('productSize').value);
    formData.append('Price', document.getElementById('productPrice').value);
    formData.append('StockQuantity', document.getElementById('productStock').value);
    formData.append('Unit', document.getElementById('productUnit').value);

    const fileInput = document.getElementById('productImageFile');
    if (fileInput.files.length > 0) {
        formData.append('imageFile', fileInput.files[0]);
    }

    // DEBUG LOGS
    console.log("--- DEBUG: SENDING PRODUCT DATA ---");
    const debugObj = {};
    formData.forEach((value, key) => {
        if (key !== 'imageFile') debugObj[key] = value;
        else debugObj[key] = value.name + " (" + value.type + ")";
    });
    console.log("JSON Representation:", JSON.stringify(debugObj, null, 2));
    console.log("--- END DEBUG ---");

    Swal.fire({
        title: 'Đang lưu...',
        allowOutsideClick: false,
        didOpen: () => {
            Swal.showLoading();
        }
    });

    const url = isEdit ? '/SportProduct/Update' : '/SportProduct/Create';

    fetch(url, {
        method: 'POST',
        body: formData
    })
        .then(response => response.json())
        .then(res => {
            if (res.success) {
                Swal.fire({
                    icon: 'success',
                    title: 'Thành công!',
                    text: res.message,
                    timer: 2000,
                    showConfirmButton: false
                }).then(() => {
                    closeModal();
                    loadProducts();
                });
            } else {
                let errorMsg = res.message;
                if (res.errors) errorMsg += '<br>' + res.errors.join('<br>');
                Swal.fire('Lỗi!', errorMsg, 'error');
            }
        })
        .catch(err => {
            console.error('Fetch error:', err);
            Swal.fire('Lỗi!', 'Có lỗi xảy ra khi gửi yêu cầu!', 'error');
        });
}

function updateStockPrompt(id, name) {
    Swal.fire({
        title: 'Nhập thêm hàng',
        text: `Nhập số lượng bạn muốn thêm cho: ${name}`,
        input: 'number',
        inputAttributes: {
            min: 1,
            step: 1
        },
        showCancelButton: true,
        confirmButtonText: 'Cập nhật',
        cancelButtonText: 'Hủy',
        inputValidator: (value) => {
            if (!value || value <= 0) {
                return 'Vui lòng nhập số lượng hợp lệ!'
            }
        }
    }).then((result) => {
        if (result.isConfirmed) {
            const added = parseInt(result.value);
            fetch(`/SportProduct/UpdateStock?id=${id}&quantity=${added}`, { method: 'POST' })
                .then(r => r.json())
                .then(res => {
                    if (res.success) {
                        Swal.fire('Thành công', res.message, 'success');
                        loadProducts();
                    } else {
                        Swal.fire('Lỗi', res.message, 'error');
                    }
                });
        }
    });
}

function deleteProduct(id) {
    Swal.fire({
        title: 'Xác nhận xóa?',
        text: "Bạn có chắc chắn muốn xóa sản phẩm này không?",
        icon: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#d33',
        cancelButtonColor: '#3085d6',
        confirmButtonText: 'Có, xóa ngay!',
        cancelButtonText: 'Hủy'
    }).then((result) => {
        if (result.isConfirmed) {
            fetch(`/SportProduct/Delete?id=${id}`, { method: 'POST' })
                .then(r => r.json())
                .then(res => {
                    if (res.success) {
                        Swal.fire('Đã xóa', res.message, 'success');
                        loadProducts();
                    } else {
                        Swal.fire('Lỗi', res.message, 'error');
                    }
                });
        }
    });
}


document.getElementById('productModal').addEventListener('click', function (e) {
    if (e.target === this) {
        closeModal();
    }
});
document.getElementById('productModal').addEventListener('click', function (e) {
    if (e.target === this) {
        closeModal();
    }
});