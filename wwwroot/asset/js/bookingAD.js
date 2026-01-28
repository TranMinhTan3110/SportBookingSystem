document.addEventListener('DOMContentLoaded', function () {
    // 1. Cấu hình Flatpickr (Chọn giờ 24h)
    flatpickr(".time-picker", {
        enableTime: true,
        noCalendar: true,
        dateFormat: "H:i",
        time_24hr: true,
        defaultDate: "12:00",
        minuteIncrement: 30
    });

    // 2. Preview ảnh khi chọn file
    document.getElementById('fieldImageInput')?.addEventListener('change', function (e) {
        const container = document.getElementById('imagePreviewContainer');
        container.innerHTML = '';

        if (e.target.files.length > 0) {
            const file = e.target.files[0];
            const reader = new FileReader();
            reader.onload = function (event) {
                const div = document.createElement('div');
                div.className = 'image-item';
                div.innerHTML = `<img src="${event.target.result}" alt="Preview" style="max-width: 200px; border-radius: 8px; margin-top: 10px;">`;
                container.appendChild(div);
            };
            reader.readAsDataURL(file);
        }
    });

    // 3. Xử lý Thêm Sân Mới
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
                await Swal.fire({
                    icon: 'success',
                    title: 'Thành công',
                    text: result.message,
                    confirmButtonColor: '#10b981'
                });
                window.location.reload();
            } else {
                Swal.fire({ icon: 'error', title: 'Thất bại', text: result.message, confirmButtonColor: '#ef4444' });
            }
        } catch (error) {
            Swal.fire({ icon: 'error', title: 'Lỗi', text: 'Lỗi hệ thống khi thêm sân', confirmButtonColor: '#ef4444' });
        } finally {
            if (submitBtn) {
                submitBtn.disabled = false;
                submitBtn.innerHTML = '<i class="fas fa-save"></i> Lưu Thông Tin';
            }
        }
    });

    // 4. Xử lý Load dữ liệu để Edit
    document.querySelectorAll('.btn-edit').forEach(btn => {
        btn.addEventListener('click', async function () {
            const pitchId = this.getAttribute('data-pitch-id');
            Swal.showLoading();

            try {
                const response = await fetch(`/Pitches/GetPitch/${pitchId}`);
                const result = await response.json();
                Swal.close();

                if (result.success) {
                    const data = result.data;
                    document.getElementById('editPitchId').value = data.pitchId;
                    document.getElementById('editPitchName').value = data.pitchName;
                    document.getElementById('editPricePerHour').value = data.pricePerHour;
                    document.getElementById('editCapacity').value = data.capacity;
                    document.getElementById('editStatus').value = data.status;
                    document.getElementById('editCategoryId').value = data.categoryId;
                    document.getElementById('editDescription').value = data.description || '';

                    const imageContainer = document.getElementById('currentImageContainer');
                    if (data.imageUrl) {
                        imageContainer.innerHTML = `<div class="image-item"><img src="${data.imageUrl}" alt="Current" style="max-width: 150px; border-radius: 8px; margin-bottom: 10px;"></div>`;
                    } else {
                        imageContainer.innerHTML = '<p class="text-muted small">Chưa có ảnh</p>';
                    }
                }
            } catch (error) {
                Swal.close();
                Swal.fire({ icon: 'error', title: 'Lỗi', text: 'Không thể tải thông tin sân', confirmButtonColor: '#ef4444' });
            }
        });
    });

    // 5. Xử lý Cập nhật Sân
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
                await Swal.fire({
                    icon: 'success',
                    title: 'Thành công',
                    text: result.message,
                    confirmButtonColor: '#10b981'
                });
                window.location.reload();
            } else {
                Swal.fire({ icon: 'error', title: 'Thất bại', text: result.message, confirmButtonColor: '#ef4444' });
            }
        } catch (error) {
            Swal.fire({ icon: 'error', title: 'Lỗi', text: 'Lỗi hệ thống khi cập nhật', confirmButtonColor: '#ef4444' });
        } finally {
            if (submitBtn) {
                submitBtn.disabled = false;
                submitBtn.innerHTML = '<i class="fas fa-save"></i> Cập Nhật';
            }
        }
    });

    // 6. Xử lý Xóa Sân
    document.querySelectorAll('.btn-delete').forEach(btn => {
        btn.addEventListener('click', async function () {
            const pitchId = this.getAttribute('data-pitch-id');
            const confirmResult = await Swal.fire({
                title: 'Xóa sân này?',
                text: "Hành động này không thể hoàn tác!",
                icon: 'warning',
                showCancelButton: true,
                confirmButtonColor: '#ef4444',
                cancelButtonColor: '#6b7280',
                confirmButtonText: 'Xóa',
                cancelButtonText: 'Hủy'
            });

            if (!confirmResult.isConfirmed) return;

            const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value;
            const formData = new FormData();
            if (token) formData.append('__RequestVerificationToken', token);

            try {
                const response = await fetch(`/Pitches/Delete/${pitchId}`, {
                    method: 'POST',
                    body: formData
                });
                const result = await response.json();

                if (result.success) {
                    await Swal.fire({ icon: 'success', title: 'Đã xóa!', text: result.message, confirmButtonColor: '#10b981' });
                    window.location.reload();
                } else {
                    Swal.fire({ icon: 'error', title: 'Lỗi', text: result.message, confirmButtonColor: '#ef4444' });
                }
            } catch (error) {
                Swal.fire({ icon: 'error', title: 'Lỗi', text: 'Lỗi hệ thống khi xóa', confirmButtonColor: '#ef4444' });
            }
        });
    });

    // 7. Quản lý Bảng Giá (Mở Modal & Load giá)
    document.querySelectorAll('.btn-price').forEach(btn => {
        btn.addEventListener('click', function () {
            const pitchId = this.getAttribute('data-pitch-id');
            const pitchName = this.getAttribute('data-pitch-name');

            document.getElementById('priceConfigPitchName').textContent = pitchName;
            document.getElementById('pricePitchId').value = pitchId;

            loadPitchPrices(pitchId);
        });
    });

    // 8. Xử lý Form Thêm Giá
    document.getElementById('addPriceForm')?.addEventListener('submit', async function (e) {
        e.preventDefault();
        const formData = new FormData(this);
        const pitchId = document.getElementById('pricePitchId').value;

        try {
            const response = await fetch('/Pitches/AddPrice', {
                method: 'POST',
                body: formData
            });
            const result = await response.json();

            if (result.success) {
                this.reset();
                document.getElementById('pricePitchId').value = pitchId;

                await Swal.fire({
                    icon: 'success',
                    title: 'Thành công',
                    text: result.message,
                    toast: true,
                    position: 'top-end',
                    showConfirmButton: false,
                    timer: 1500
                });

                loadPitchPrices(pitchId);
            } else {
                Swal.fire('Lỗi', result.message, 'error');
            }
        } catch (error) {
            Swal.fire('Lỗi', 'Không thể kết nối server', 'error');
        }
    });

    // --- LOGIC QUẢN LÝ KHUNG GIỜ (TIME SLOT) ---

    const timeSlotModalElement = document.getElementById('manageTimeSlotModal');
    if (timeSlotModalElement) {
        window.timeSlotModal = new bootstrap.Modal(timeSlotModalElement);
    }

    // Xử lý Thêm Mới TimeSlot
    document.getElementById('formAddTimeSlot')?.addEventListener('submit', async function (e) {
        e.preventDefault();

        const start = document.getElementById('slotStart').value;
        const end = document.getElementById('slotEnd').value;
        const formData = new FormData();
        formData.append('startTime', start);
        formData.append('endTime', end);

        try {
            const res = await fetch('/TimeSlot/Create', { method: 'POST', body: formData });
            const json = await res.json();

            if (json.success) {
                const Toast = Swal.mixin({
                    toast: true, position: 'top-end', showConfirmButton: false, timer: 1500
                });
                Toast.fire({ icon: 'success', title: 'Đã thêm khung giờ' });

                //reset
                this.reset();
                loadTimeSlots();
            } else {
                Swal.fire('Lỗi', json.message, 'error');
            }
        } catch (err) {
            Swal.fire('Lỗi', 'Lỗi kết nối server', 'error');
        }
    });
});



// Hàm Load danh sách giá 
async function loadPitchPrices(pitchId) {
    const tbody = document.getElementById('priceTableBody');
    tbody.innerHTML = '<tr><td colspan="4" class="text-center">Đang tải...</td></tr>';

    try {
        const response = await fetch(`/Pitches/GetPrices?pitchId=${pitchId}`);
        const result = await response.json();

        if (result.success) {
            if (result.data.length === 0) {
                tbody.innerHTML = '<tr><td colspan="4" class="text-center text-muted">Chưa có cấu hình giá riêng. Đang dùng giá mặc định.</td></tr>';
                return;
            }

            tbody.innerHTML = result.data.map(item => `
                <tr>
                    <td><span class="badge bg-info text-dark">${item.startTime.substring(0, 5)} - ${item.endTime.substring(0, 5)}</span></td>
                    <td class="fw-bold text-success">${new Intl.NumberFormat('vi-VN').format(item.price)}đ</td>
                    <td>${item.note || '-'}</td>
                    <td>
                        <button class="btn btn-sm btn-danger" onclick="deletePrice(${item.id})"><i class="fas fa-trash"></i></button>
                    </td>
                </tr>
            `).join('');
        }
    } catch (error) {
        tbody.innerHTML = '<tr><td colspan="4" class="text-center text-danger">Lỗi tải dữ liệu</td></tr>';
    }
}

// Hàm Xóa Giá
window.deletePrice = async function (id) {
    const confirmResult = await Swal.fire({
        title: 'Xóa khung giờ này?',
        text: "Không thể hoàn tác!",
        icon: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#ef4444',
        cancelButtonColor: '#6b7280',
        confirmButtonText: 'Xóa ngay',
        cancelButtonText: 'Hủy'
    });

    if (!confirmResult.isConfirmed) return;

    try {
        const formData = new FormData();
        formData.append('id', id);

        const response = await fetch('/Pitches/DeletePrice', {
            method: 'POST',
            body: formData
        });
        const result = await response.json();

        if (result.success) {
            await Swal.fire({
                icon: 'success',
                title: 'Đã xóa!',
                toast: true,
                position: 'top-end',
                showConfirmButton: false,
                timer: 1500
            });

            const pitchId = document.getElementById('pricePitchId').value;
            loadPitchPrices(pitchId);
        } else {
            Swal.fire({ icon: 'error', title: 'Lỗi', text: result.message, confirmButtonColor: '#ef4444' });
        }
    } catch (error) {
        Swal.fire({ icon: 'error', title: 'Lỗi hệ thống', confirmButtonColor: '#ef4444' });
    }
};

// 1. Mở Modal & Load dữ liệu
window.openTimeSlotModal = function () {
    if (window.timeSlotModal) {
        window.timeSlotModal.show();
        loadTimeSlots();
    } else {
      
        const el = document.getElementById('manageTimeSlotModal');
        if (el) {
            window.timeSlotModal = new bootstrap.Modal(el);
            window.timeSlotModal.show();
            loadTimeSlots();
        } else {
            console.error("Modal element not found");
        }
    }
}

// 2. Load danh sách từ API 
async function loadTimeSlots() {
    const tbody = document.getElementById('timeSlotTableBody');
    if (!tbody) return;

    tbody.innerHTML = '<tr><td colspan="3" class="text-center">Đang tải...</td></tr>';

    try {
        const res = await fetch('/TimeSlot/GetAll');
        const json = await res.json();

        if (json.success) {
            if (json.data.length === 0) {
                tbody.innerHTML = '<tr><td colspan="3" class="text-center text-muted">Chưa có khung giờ nào.</td></tr>';
                return;
            }

            tbody.innerHTML = json.data.map(slot => `
                <tr>
                    <td class="fw-bold">${slot.slotName}</td>
                    <td>${slot.startTime.substring(0, 5)} - ${slot.endTime.substring(0, 5)}</td>
                    <td class="text-center">
                        <button class="btn btn-sm btn-outline-danger border-0" onclick="deleteTimeSlot(${slot.slotId})">
                            <i class="fas fa-trash-alt"></i>
                        </button>
                    </td>
                </tr>
            `).join('');
        }
    } catch (err) {
        console.error(err);
        tbody.innerHTML = '<tr><td colspan="3" class="text-center text-danger">Lỗi tải dữ liệu.</td></tr>';
    }
}

// 4. Xử lý Xóa Time Slot 
window.deleteTimeSlot = async function (id) {
    if (!confirm('Bạn có chắc muốn xóa khung giờ này không?')) return;

    const formData = new FormData();
    formData.append('id', id);

    try {
        const res = await fetch('/TimeSlot/Delete', { method: 'POST', body: formData });
        const json = await res.json();

        if (json.success) {
            loadTimeSlots(); // Reload lại bảng
        } else {
            alert(json.message);
        }
    } catch (err) {
        alert('Lỗi xóa dữ liệu');
    }
}