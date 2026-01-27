

document.addEventListener('DOMContentLoaded', function () {

    const rechargeBtn = document.querySelector('.btn-recharge');
    if (rechargeBtn) {
        rechargeBtn.addEventListener('click', function () {

            alert('Chức năng nạp tiền đang được phát triển');
        });
    }
    const checkinBtns = document.querySelectorAll('.btn-checkin:not(.btn-order-qr)');
    const checkInQrModal = document.getElementById('checkInQrModal');

    if (checkinBtns.length > 0) {
        checkinBtns.forEach(btn => {
            btn.addEventListener('click', function () {
                const code = this.dataset.code;
                if (!code) return showNotification('Không tìm thấy mã check-in', 'error');

                this.innerHTML = '<i class="fas fa-spinner fa-spin"></i> Đang tạo...';
                this.disabled = true;

                // Reset modal content for Booking
                const modalTitle = document.getElementById('qrModalTitle');
                const modalAlert = document.getElementById('qrModalAlert');
                if (modalTitle) modalTitle.textContent = "Mã Check-in Đặt Sân";
                if (modalAlert) modalAlert.innerHTML = '<i class="fas fa-info-circle"></i> Vui lòng đưa mã này cho nhân viên tại sân để nhận sân.';

                fetch(`/UserProfile/GenerateBookingQr?checkInCode=${code}`)
                    .then(res => res.json())
                    .then(data => {
                        if (data.success) {
                            document.getElementById('checkInQrImage').src = 'data:image/png;base64,' + data.qrCode;
                            document.getElementById('checkInCodeDisplay').textContent = code;

                            // Hide countdown for bookings if not applicable (or reset if needed)
                            document.getElementById('qrCountdown').style.display = 'none';

                            openModal(checkInQrModal);
                        } else {
                            showNotification('Lỗi tạo mã QR', 'error');
                        }
                    })
                    .catch(err => showNotification('Lỗi kết nối', 'error'))
                    .finally(() => {
                        this.innerHTML = '<i class="fas fa-qrcode"></i> Lấy mã Check-in';
                        this.disabled = false;
                    });
            });
        });
    }


    const payButtons = document.querySelectorAll('.btn-pay-now');
    payButtons.forEach(btn => {
        btn.addEventListener('click', function () {
            const card = this.closest('.booking-card');
            const fieldName = card.querySelector('.booking-field-info span').textContent;

            if (confirm(`Xác nhận thanh toán cho ${fieldName}?`)) {
                alert('Đang xử lý thanh toán...');
            }
        });
    });


    const viewAllButtons = document.querySelectorAll('.btn-view-all');
    viewAllButtons.forEach(btn => {
        btn.addEventListener('click', function () {
            const section = this.closest('section');
            const sectionTitle = section.querySelector('.section-title').textContent.trim();
            console.log('View all:', sectionTitle);

        });
    });


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

    const avatar = document.querySelector('.profile-avatar');
    if (avatar) {
        avatar.addEventListener('click', function () {
            console.log('Avatar clicked - open upload dialog');
        });
    }


    const emailModal = document.getElementById('emailModal');
    const phoneModal = document.getElementById('phoneModal');
    const passwordModal = document.getElementById('passwordModal');

    function openModal(modal) {
        if (modal) {
            modal.classList.add('active');
            document.body.style.overflow = 'hidden';
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

    function copyToClipboard(text) {
        navigator.clipboard.writeText(text).then(() => {
            showNotification('Đã sao chép!', 'success');
        });
    }

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


    const orderQrBtns = document.querySelectorAll('.btn-order-qr');
    if (orderQrBtns) {
        orderQrBtns.forEach(btn => {
            btn.addEventListener('click', function () {
                const orderId = this.dataset.orderId;
                if (!orderId) return showNotification('Không tìm thấy mã đơn hàng', 'error');

                this.innerHTML = '<i class="fas fa-spinner fa-spin"></i> Đang tải...';
                this.classList.add('disabled');

                // Update modal content for Order
                const modalTitle = document.getElementById('qrModalTitle');
                const modalAlert = document.getElementById('qrModalAlert');
                if (modalTitle) modalTitle.textContent = "Mã QR Đơn Hàng";
                if (modalAlert) modalAlert.innerHTML = '<i class="fas fa-info-circle"></i> Vui lòng đưa mã này cho nhân viên tại Canteen để nhận món.';

                fetch(`/UserProfile/GenerateOrderQr?orderId=${orderId}`)
                    .then(res => res.json())
                    .then(data => {
                        if (data.success) {
                            document.getElementById('checkInQrImage').src = 'data:image/png;base64,' + data.qrCode;
                            document.getElementById('checkInCodeDisplay').textContent = 'Mã đơn hàng #' + data.orderCode;

                            const countdownDiv = document.getElementById('qrCountdown');
                            const timerSpan = document.getElementById('countdownTimer');

                            if (data.remainingSeconds > 0) {
                                countdownDiv.style.display = 'block';
                                startCountdown(data.remainingSeconds, timerSpan, countdownDiv);
                            } else {
                                countdownDiv.style.display = 'block';
                                countdownDiv.innerHTML = "Mã QR đã hết hạn!";
                                document.getElementById('checkInQrImage').style.opacity = '0.2';
                            }

                            openModal(checkInQrModal);
                        } else {
                            showNotification(data.message || 'Lỗi tạo mã QR', 'error');
                        }
                    })
                    .catch(err => showNotification('Lỗi kết nối', 'error'))
                    .finally(() => {
                        this.innerHTML = '<i class="fas fa-qrcode"></i> Lấy mã Order';
                        this.classList.remove('disabled');
                    });
            });
        });
    }

    let countdownInterval;
    function startCountdown(duration, display, container) {
        if (countdownInterval) clearInterval(countdownInterval);

        let timer = duration, minutes, seconds;

        function updateDisplay() {
            minutes = parseInt(timer / 60, 10);
            seconds = parseInt(timer % 60, 10);

            minutes = minutes < 10 ? "0" + minutes : minutes;
            seconds = seconds < 10 ? "0" + seconds : seconds;

            display.textContent = minutes + ":" + seconds;

            if (--timer < 0) {
                clearInterval(countdownInterval);
                container.innerHTML = "Mã QR đã hết hạn!";
                const img = document.getElementById('checkInQrImage');
                if (img) img.style.opacity = '0.2';
            }
        }

        updateDisplay();
        countdownInterval = setInterval(updateDisplay, 1000);
    }

    document.querySelectorAll('.close-modal, .btn-cancel').forEach(btn => {
        btn.addEventListener('click', () => {
            if (countdownInterval) clearInterval(countdownInterval);
        });
    });

    console.log(' Profile page loaded successfully!');
});