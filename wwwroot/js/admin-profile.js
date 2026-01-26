document.addEventListener('DOMContentLoaded', function () {
    console.log('✅ Admin Profile page loaded successfully!');


    const emailModal = document.getElementById('emailModal');
    const phoneModal = document.getElementById('phoneModal');
    const passwordModal = document.getElementById('passwordModal');


    document.querySelectorAll('.close-modal, .btn-cancel').forEach(btn => {
        btn.addEventListener('click', () => {
            emailModal.classList.remove('active');
            phoneModal.classList.remove('active');
            passwordModal.classList.remove('active');
        });
    });

    document.querySelector('.btn-change-email').addEventListener('click', () => {
        emailModal.classList.add('active');
    });

    document.querySelector('.btn-change-phone').addEventListener('click', () => {
        phoneModal.classList.add('active');
    });

    document.querySelector('.btn-change-password').addEventListener('click', () => {
        passwordModal.classList.add('active');
    });

    document.getElementById('saveEmail').addEventListener('click', async function () {
        const newEmail = document.getElementById('newEmail').value;
        await performUpdate(this, '/AdminProfile/ChangeEmail', { newEmail });
    });

    document.getElementById('savePhone').addEventListener('click', async function () {
        const newPhone = document.getElementById('newPhone').value;
        await performUpdate(this, '/AdminProfile/ChangePhone', { newPhone });
    });

    document.getElementById('savePassword').addEventListener('click', async function () {
        const currentPassword = document.getElementById('currentPassword').value;
        const newPassword = document.getElementById('newPassword').value;
        const confirmPassword = document.getElementById('confirmPassword').value;

        if (newPassword !== confirmPassword) {
            showNotification('Mật khẩu xác nhận không khớp.', 'error');
            return;
        }

        await performUpdate(this, '/AdminProfile/ChangePassword', { currentPassword, newPassword, confirmPassword });
    });

    async function performUpdate(btn, url, data) {
        const originalText = btn.innerHTML;
        btn.innerHTML = '<i class="fas fa-spinner fa-spin"></i> Đang lưu...';
        btn.disabled = true;

        try {
            const formData = new FormData();
            for (const key in data) {
                formData.append(key, data[key]);
            }

            const response = await fetch(url, {
                method: 'POST',
                body: formData
            });

            const result = await response.json();

            if (result.success) {
                showNotification(result.message, 'success');
                setTimeout(() => location.reload(), 1500);
            } else {
                showNotification(result.message, 'error');
            }
        } catch (error) {
            showNotification('Đã có lỗi xảy ra. Vui lòng thử lại.', 'error');
        } finally {
            btn.innerHTML = originalText;
            btn.disabled = false;
        }
    }


    function showNotification(message, type = 'success') {
        const notification = document.createElement('div');
        notification.className = `notification ${type}`;
        notification.style.cssText = `
            position: fixed;
            top: 20px;
            right: 20px;
            padding: 15px 25px;
            border-radius: 12px;
            color: white;
            font-weight: 600;
            z-index: 10001;
            box-shadow: 0 4px 12px rgba(0,0,0,0.15);
            background: ${type === 'success' ? '#10B981' : '#EF4444'};
            animation: slideIn 0.3s ease-out;
        `;
        notification.innerHTML = `<i class="fas ${type === 'success' ? 'fa-check-circle' : 'fa-exclamation-circle'}"></i> ${message}`;
        document.body.appendChild(notification);

        setTimeout(() => {
            notification.style.animation = 'slideOut 0.3s ease-in forwards';
            setTimeout(() => notification.remove(), 300);
        }, 3000);
    }
});

const style = document.createElement('style');
style.textContent = `
    @keyframes slideIn { from { transform: translateX(100%); opacity: 0; } to { transform: translateX(0); opacity: 1; } }
    @keyframes slideOut { from { transform: translateX(0); opacity: 1; } to { transform: translateX(100%); opacity: 0; } }
`;
document.head.appendChild(style);
