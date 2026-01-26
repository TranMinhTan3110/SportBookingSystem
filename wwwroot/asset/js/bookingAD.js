document.getElementById('fieldImageInput')?.addEventListener('change', function (e) {
    const container = document.getElementById('imagePreviewContainer');
    container.innerHTML = '';

    if (e.target.files.length > 0) {
        const file = e.target.files[0];
        const reader = new FileReader();
        reader.onload = function (event) {
            const div = document.createElement('div');
            div.className = 'image-item';
            div.innerHTML = `
                <img src="${event.target.result}" alt="Preview" style="max-width: 200px; border-radius: 8px; margin-top: 10px;">
            `;
            container.appendChild(div);
        };
        reader.readAsDataURL(file);
    }
});

// ========== THÊM SÂN MỚI ==========
document.getElementById('addFieldForm')?.addEventListener('submit', async function (e) {
    e.preventDefault();

    const formData = new FormData(this);
    const submitBtn = document.getElementById('btnAddField');

    if (submitBtn) {
        submitBtn.disabled = true;
        submitBtn.innerHTML = '<i class="fas fa-spinner fa-spin"></i> Đang lưu...';
    }

    try {
        const response = await fetch('/Pitches/Create', {
            method: 'POST',
            body: formData
        });

        const result = await response.json();

        if (result.success) {
            // ✅ SweetAlert2 - Thành công
            await Swal.fire({
                icon: 'success',
                title: 'Thành công!',
                text: result.message,
                confirmButtonText: 'OK',
                confirmButtonColor: '#10b981'
            });
            window.location.reload();
        } else {
            // ❌ SweetAlert2 - Thất bại
            await Swal.fire({
                icon: 'error',
                title: 'Oops...',
                text: result.message || 'Đã xảy ra lỗi khi thêm sân',
                confirmButtonText: 'Đóng',
                confirmButtonColor: '#ef4444'
            });
        }
    } catch (error) {
        console.error('Error:', error);
        await Swal.fire({
            icon: 'error',
            title: 'Lỗi!',
            text: 'Đã xảy ra lỗi khi thêm sân',
            confirmButtonText: 'Đóng',
            confirmButtonColor: '#ef4444'
        });
    } finally {
        if (submitBtn) {
            submitBtn.disabled = false;
            submitBtn.innerHTML = '<i class="fas fa-save"></i> Lưu Thông Tin';
        }
    }
});

// ========== LOAD DỮ LIỆU ĐỂ EDIT ==========
document.querySelectorAll('.btn-edit').forEach(btn => {
    btn.addEventListener('click', async function () {
        const pitchId = this.getAttribute('data-pitch-id');

        // Hiển thị loading
        Swal.fire({
            title: 'Đang tải...',
            allowOutsideClick: false,
            didOpen: () => {
                Swal.showLoading();
            }
        });

        try {
            const response = await fetch(`/Pitches/GetPitch/${pitchId}`);
            const result = await response.json();

            Swal.close(); // Đóng loading

            if (result.success) {
                const data = result.data;

                // Fill dữ liệu vào form
                document.getElementById('editPitchId').value = data.pitchId;
                document.getElementById('editPitchName').value = data.pitchName;
                document.getElementById('editPricePerHour').value = data.pricePerHour;
                document.getElementById('editCapacity').value = data.capacity;
                document.getElementById('editStatus').value = data.status;
                document.getElementById('editCategoryId').value = data.categoryId;
                document.getElementById('editDescription').value = data.description || '';

                // Hiển thị ảnh hiện tại
                const imageContainer = document.getElementById('currentImageContainer');
                if (data.imageUrl) {
                    imageContainer.innerHTML = `
                        <div class="image-item">
                            <img src="${data.imageUrl}" alt="Current" style="max-width: 150px; border-radius: 8px; margin-bottom: 10px;">
                        </div>
                    `;
                } else {
                    imageContainer.innerHTML = '<p class="text-muted small">Chưa có ảnh</p>';
                }
            } else {
                await Swal.fire({
                    icon: 'error',
                    title: 'Lỗi!',
                    text: 'Không thể tải thông tin sân',
                    confirmButtonColor: '#ef4444'
                });
            }
        } catch (error) {
            Swal.close();
            console.error('Error:', error);
            await Swal.fire({
                icon: 'error',
                title: 'Lỗi!',
                text: 'Đã xảy ra lỗi khi tải thông tin sân',
                confirmButtonColor: '#ef4444'
            });
        }
    });
});

// ========== CẬP NHẬT SÂN ==========
document.getElementById('editFieldForm')?.addEventListener('submit', async function (e) {
    e.preventDefault();

    const formData = new FormData(this);
    const submitBtn = document.getElementById('btnEditField');

    if (submitBtn) {
        submitBtn.disabled = true;
        submitBtn.innerHTML = '<i class="fas fa-spinner fa-spin"></i> Đang cập nhật...';
    }

    try {
        const response = await fetch('/Pitches/Edit', {
            method: 'POST',
            body: formData
        });

        const result = await response.json();

        if (result.success) {
            // ✅ SweetAlert2 - Thành công
            await Swal.fire({
                icon: 'success',
                title: 'Thành công!',
                text: result.message,
                confirmButtonText: 'OK',
                confirmButtonColor: '#10b981'
            });
            window.location.reload();
        } else {
            // ❌ SweetAlert2 - Thất bại
            await Swal.fire({
                icon: 'error',
                title: 'Oops...',
                text: result.message || 'Đã xảy ra lỗi khi cập nhật sân',
                confirmButtonText: 'Đóng',
                confirmButtonColor: '#ef4444'
            });
        }
    } catch (error) {
        console.error('Error:', error);
        await Swal.fire({
            icon: 'error',
            title: 'Lỗi!',
            text: 'Đã xảy ra lỗi khi cập nhật sân',
            confirmButtonText: 'Đóng',
            confirmButtonColor: '#ef4444'
        });
    } finally {
        if (submitBtn) {
            submitBtn.disabled = false;
            submitBtn.innerHTML = '<i class="fas fa-save"></i> Cập Nhật';
        }
    }
});

// ========== XÓA SÂN ==========
document.querySelectorAll('.btn-delete').forEach(btn => {
    btn.addEventListener('click', async function () {
        const pitchId = this.getAttribute('data-pitch-id');

        // ⚠️ SweetAlert2 - Xác nhận xóa
        const confirmResult = await Swal.fire({
            title: 'Bạn có chắc chắn?',
            text: "Bạn sẽ không thể hoàn tác hành động này!",
            icon: 'warning',
            showCancelButton: true,
            confirmButtonColor: '#ef4444',
            cancelButtonColor: '#6b7280',
            confirmButtonText: 'Xóa',
            cancelButtonText: 'Hủy'
        });

        if (!confirmResult.isConfirmed) return;

        // Lấy token chống CSRF
        const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value;
        if (!token) {
            await Swal.fire({
                icon: 'error',
                title: 'Lỗi!',
                text: 'Không tìm thấy token bảo mật',
                confirmButtonColor: '#ef4444'
            });
            return;
        }

        const formData = new FormData();
        formData.append('__RequestVerificationToken', token);

        // Hiển thị loading
        Swal.fire({
            title: 'Đang xóa...',
            allowOutsideClick: false,
            didOpen: () => {
                Swal.showLoading();
            }
        });

        try {
            const response = await fetch(`/Pitches/Delete/${pitchId}`, {
                method: 'POST',
                body: formData
            });

            const result = await response.json();

            if (result.success) {
                // ✅ SweetAlert2 - Xóa thành công
                await Swal.fire({
                    icon: 'success',
                    title: 'Đã xóa!',
                    text: result.message,
                    confirmButtonText: 'OK',
                    confirmButtonColor: '#10b981'
                });
                window.location.reload();
            } else {
                // ❌ SweetAlert2 - Xóa thất bại
                await Swal.fire({
                    icon: 'error',
                    title: 'Không thể xóa!',
                    text: result.message,
                    confirmButtonText: 'Đóng',
                    confirmButtonColor: '#ef4444'
                });
            }
        } catch (error) {
            console.error('Error:', error);
            await Swal.fire({
                icon: 'error',
                title: 'Lỗi!',
                text: 'Đã xảy ra lỗi khi xóa sân',
                confirmButtonText: 'Đóng',
                confirmButtonColor: '#ef4444'
            });
        }
    });
});