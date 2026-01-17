// Modal Functions
function openModal(mode, productId = null) {
    const modal = document.getElementById('productModal');
    const modalTitle = document.getElementById('modalTitle');

    if (mode === 'add') {
        modalTitle.innerHTML = '➕ Thêm Sản Phẩm Mới';
        document.getElementById('productForm').reset();
    } else if (mode === 'edit') {
        modalTitle.innerHTML = '✏️ Chỉnh Sửa Sản Phẩm';
        // Load product data here (would fetch from server in real app)
    }

    modal.classList.add('active');
    document.body.style.overflow = 'hidden';
}

function closeModal() {
    const modal = document.getElementById('productModal');
    modal.classList.remove('active');
    document.body.style.overflow = '';
}

function saveProduct() {
    // Validate form
    const form = document.getElementById('productForm');
    if (!form.checkValidity()) {
        alert('Vui lòng điền đầy đủ thông tin bắt buộc!');
        return;
    }

    // Here you would send data to server
    alert('Lưu sản phẩm thành công!');
    closeModal();
}

function deleteProduct(id) {
    if (confirm('Bạn có chắc chắn muốn xóa sản phẩm này?')) {
        // Delete logic here
        alert('Đã xóa sản phẩm #' + id);
    }
}

// Close modal when clicking outside
document.getElementById('productModal').addEventListener('click', function (e) {
    if (e.target === this) {
        closeModal();
    }
});