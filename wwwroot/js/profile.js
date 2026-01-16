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
    // QUICK ACTION CARDS
    // ============================================
    const actionCards = document.querySelectorAll('.action-card');
    actionCards.forEach(card => {
        card.addEventListener('click', function (e) {
            e.preventDefault();
            const label = this.querySelector('.action-label').textContent;
            console.log('Action clicked:', label);
            // TODO: Navigate to appropriate page
        });
    });

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
    // WALLET BALANCE ANIMATION
    // ============================================
    function animateBalance() {
        const balanceEl = document.querySelector('.wallet-balance');
        if (!balanceEl) return;

        const finalBalance = parseInt(balanceEl.textContent.replace(/[^\d]/g, ''));
        let currentBalance = 0;
        const duration = 1500;
        const increment = finalBalance / (duration / 16);

        const timer = setInterval(() => {
            currentBalance += increment;
            if (currentBalance >= finalBalance) {
                currentBalance = finalBalance;
                clearInterval(timer);
            }
            balanceEl.textContent = Math.floor(currentBalance).toLocaleString('vi-VN') + '₫';
        }, 16);
    }

    // Run animation on page load
    setTimeout(animateBalance, 500);

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
            background: ${type === 'success' ? '#10B981' : '#3B82F6'};
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