// ============================================
// PROFILE PAGE - INTERACTIVE
// ============================================

document.addEventListener('DOMContentLoaded', function () {

    // ============================================
    // RECHARGE BUTTON
    // ============================================
    const rechargeBtn = document.querySelector('.btn-recharge');
    if (rechargeBtn) {
        rechargeBtn.addEventListener('click', function () {
            // TODO: Open recharge modal
            alert('Chức năng nạp tiền đang được phát triển');
        });
    }

    // ============================================
    // CHECK-IN BUTTON
    // ============================================
    const checkinBtn = document.querySelector('.btn-checkin');
    if (checkinBtn) {
        checkinBtn.addEventListener('click', function () {
            // TODO: Generate QR code
            alert('Đang tạo mã Check-in...');
        });
    }

    // ============================================


    // ============================================
    // PAY NOW BUTTONS
    // ============================================
    const payButtons = document.querySelectorAll('.btn-pay-now');
    payButtons.forEach(btn => {
        btn.addEventListener('click', function () {
            const card = this.closest('.booking-card');
            const fieldName = card.querySelector('.booking-field-info span').textContent;

            if (confirm(`Xác nhận thanh toán cho ${fieldName}?`)) {
                // TODO: Process payment
                alert('Đang xử lý thanh toán...');
            }
        });
    });

    // ============================================
    // VIEW ALL BUTTONS
    // ============================================
    const viewAllButtons = document.querySelectorAll('.btn-view-all');
    viewAllButtons.forEach(btn => {
        btn.addEventListener('click', function () {
            const section = this.closest('section');
            const sectionTitle = section.querySelector('.section-title').textContent.trim();
            console.log('View all:', sectionTitle);
            // TODO: Navigate to detail page
        });
    });

    // ============================================
    // BOOKING CARD ANIMATION
    // ============================================
    const bookingCards = document.querySelectorAll('.booking-card');
    const observerOptions = {
        threshold: 0.1,
        rootMargin: '0px 0px -50px 0px'
    };

    const observer = new IntersectionObserver((entries) => {
        entries.forEach((entry, index) => {
            if (entry.isIntersecting) {
                setTimeout(() => {
                    entry.target.style.opacity = '1';
                    entry.target.style.transform = 'translateY(0)';
                }, index * 100);
                observer.unobserve(entry.target);
            }
        });
    }, observerOptions);

    bookingCards.forEach(card => {
        card.style.opacity = '0';
        card.style.transform = 'translateY(20px)';
        card.style.transition = 'all 0.5s ease';
        observer.observe(card);
    });

    // ============================================
    // PROFILE AVATAR UPLOAD (Optional)
    // ============================================
    const avatar = document.querySelector('.profile-avatar');
    if (avatar) {
        avatar.addEventListener('click', function () {
            // TODO: Open file upload dialog
            console.log('Avatar clicked - open upload dialog');
        });
    }


    // ============================================
    // PERSONAL INFO BUTTONS & MODAL LOGIC
    // ============================================
    const emailModal = document.getElementById('emailModal');
    const phoneModal = document.getElementById('phoneModal');
    const passwordModal = document.getElementById('passwordModal');

    function openModal(modal) {
        if (modal) {
            modal.classList.add('active');
            document.body.style.overflow = 'hidden';
            // Reset fields
            const inputs = modal.querySelectorAll('input');
            inputs.forEach(input => input.value = '');
        }
    }

    function closeModal(modal) {
        if (modal) {
            modal.classList.remove('active');
            document.body.style.overflow = '';
        }
    }

    // Close on click close button or cancel button or outside modal
    document.querySelectorAll('.close-modal, .btn-cancel').forEach(btn => {
        btn.addEventListener('click', function () {
            const modal = this.closest('.modal');
            closeModal(modal);
        });
    });

    window.addEventListener('click', function (e) {
        if (e.target.classList.contains('modal')) {
            closeModal(e.target);
        }
    });

    document.querySelectorAll('.btn-change-email').forEach(btn => {
        btn.addEventListener('click', () => openModal(emailModal));
    });

    document.querySelectorAll('.btn-change-phone').forEach(btn => {
        btn.addEventListener('click', () => openModal(phoneModal));
    });

    document.querySelectorAll('.btn-change-password').forEach(btn => {
        btn.addEventListener('click', () => openModal(passwordModal));
    });

    // Handle AJAX submissions
    document.getElementById('saveEmail')?.addEventListener('click', function () {
        const newEmail = document.getElementById('newEmail').value;
        if (!newEmail) return showNotification('Vui lòng nhập email', 'error');

        this.disabled = true;
        this.innerHTML = '<i class="fas fa-spinner fa-spin"></i> Đang lưu...';

        fetch('/UserProfile/ChangeEmail', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/x-www-form-urlencoded',
            },
            body: `newEmail=${encodeURIComponent(newEmail)}`
        })
            .then(res => res.json())
            .then(data => {
                if (data.success) {
                    showNotification(data.message, 'success');
                    closeModal(emailModal);
                    setTimeout(() => location.reload(), 1500);
                } else {
                    showNotification(data.message, 'error');
                }
            })
            .finally(() => {
                this.disabled = false;
                this.textContent = 'Lưu thay đổi';
            });
    });

    document.getElementById('savePhone')?.addEventListener('click', function () {
        const newPhone = document.getElementById('newPhone').value;
        if (!newPhone) return showNotification('Vui lòng nhập số điện thoại', 'error');

        this.disabled = true;
        this.innerHTML = '<i class="fas fa-spinner fa-spin"></i> Đang lưu...';

        fetch('/UserProfile/ChangePhone', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/x-www-form-urlencoded',
            },
            body: `newPhone=${encodeURIComponent(newPhone)}`
        })
            .then(res => res.json())
            .then(data => {
                if (data.success) {
                    showNotification(data.message, 'success');
                    closeModal(phoneModal);
                    setTimeout(() => location.reload(), 1500);
                } else {
                    showNotification(data.message, 'error');
                }
            })
            .finally(() => {
                this.disabled = false;
                this.textContent = 'Lưu thay đổi';
            });
    });

    document.getElementById('savePassword')?.addEventListener('click', function () {
        const currentPassword = document.getElementById('currentPassword').value;
        const newPassword = document.getElementById('newPassword').value;
        const confirmPassword = document.getElementById('confirmPassword').value;

        if (!currentPassword || !newPassword || !confirmPassword) {
            return showNotification('Vui lòng nhập đầy đủ thông tin', 'error');
        }

        if (newPassword !== confirmPassword) {
            return showNotification('Mật khẩu xác nhận không khớp', 'error');
        }

        this.disabled = true;
        this.innerHTML = '<i class="fas fa-spinner fa-spin"></i> Đang lưu...';

        fetch('/UserProfile/ChangePassword', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/x-www-form-urlencoded',
            },
            body: `currentPassword=${encodeURIComponent(currentPassword)}&newPassword=${encodeURIComponent(newPassword)}&confirmPassword=${encodeURIComponent(confirmPassword)}`
        })
            .then(res => res.json())
            .then(data => {
                if (data.success) {
                    showNotification(data.message, 'success');
                    closeModal(passwordModal);
                    setTimeout(() => location.reload(), 1500);
                } else {
                    showNotification(data.message, 'error');
                }
            })
            .finally(() => {
                this.disabled = false;
                this.textContent = 'Lưu thay đổi';
            });
    });

    // ============================================
    // COPY BOOKING CODE
    // ============================================
    function copyToClipboard(text) {
        navigator.clipboard.writeText(text).then(() => {
            showNotification('Đã sao chép!', 'success');
        });
    }

    // ============================================
    // NOTIFICATION SYSTEM
    // ============================================
    function showNotification(message, type = 'info') {
        const notification = document.createElement('div');
        notification.className = `notification notification-${type}`;
        notification.textContent = message;
        notification.style.cssText = `
            position: fixed;
            top: 20px;
            right: 20px;
            background: ${type === 'success' ? '#10B981' : (type === 'error' ? '#EF4444' : '#3B82F6')};
            color: white;
            padding: 16px 24px;
            border-radius: 12px;
            box-shadow: 0 4px 12px rgba(0,0,0,0.2);
            z-index: 9999;
            animation: slideInRight 0.3s ease;
        `;

        document.body.appendChild(notification);

        setTimeout(() => {
            notification.style.animation = 'slideOutRight 0.3s ease';
            setTimeout(() => notification.remove(), 300);
        }, 3000);
    }

    // Add animations
    const style = document.createElement('style');
    style.textContent = `
        @keyframes slideInRight {
            from {
                transform: translateX(400px);
                opacity: 0;
            }
            to {
                transform: translateX(0);
                opacity: 1;
            }
        }
        @keyframes slideOutRight {
            from {
                transform: translateX(0);
                opacity: 1;
            }
            to {
                transform: translateX(400px);
                opacity: 0;
            }
        }
    `;
    document.head.appendChild(style);

    console.log('✅ Profile page loaded successfully!');
});