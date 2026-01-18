// Image upload preview
document.getElementById('fieldImageInput')?.addEventListener('change', function (e) {
    const container = document.getElementById('imagePreviewContainer');
    container.innerHTML = '';

    Array.from(e.target.files).forEach((file, index) => {
        const reader = new FileReader();
        reader.onload = function (event) {
            const div = document.createElement('div');
            div.className = 'image-item';
            div.innerHTML = `
                        <img src="${event.target.result}" alt="Preview ${index + 1}">
                        <button type="button" class="btn-remove-image" onclick="this.parentElement.remove()">
                            <i class="fas fa-times"></i>
                        </button>
                    `;
            container.appendChild(div);
        };
        reader.readAsDataURL(file);
    });
});

// Delete confirmation
document.querySelectorAll('.btn-delete').forEach(btn => {
    btn.addEventListener('click', function () {
        if (confirm('Bạn có chắc chắn muốn xóa sân này?')) {
            console.log('Delete field');
        }
    });
});

// Filter functionality
document.querySelectorAll('.filter-select, .search-input').forEach(input => {
    input.addEventListener('change', function () {
        console.log('Filter changed');
    });
});