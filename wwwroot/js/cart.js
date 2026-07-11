// Cart functionality — badge, add/update/remove, checkout, countdown
(function () {
    'use strict';

    const cartBadge = document.getElementById('cartBadge');
    if (!cartBadge) return;

    // --- Countdown timer state ---
    let cartTimerInterval = null;
    let remainingSeconds = parseFloat(document.getElementById('cartTimerDisplay')?.dataset?.remaining ||
        document.querySelector('.cart-timer-bar')?.dataset?.remaining || 0);

    // --- QR countdown state ---
    let cartQrInterval = null;

    // ========================================================================
    //  TIMER
    // ========================================================================

    function startCartCountdown() {
        var display = document.getElementById('cartTimerDisplay');
        var bar = document.getElementById('cartTimerBar');
        var label = document.getElementById('cartTimerLabel');
        if (!display || !bar) return;

        if (cartTimerInterval) {
            clearInterval(cartTimerInterval);
            cartTimerInterval = null;
        }

        // Get remaining seconds from data attribute
        remainingSeconds = parseFloat(bar.dataset.remaining) || 0;
        if (remainingSeconds <= 0) {
            // Fallback: parse from display text like "20:30"
            var value = display.textContent.trim();
            if (value.indexOf(':') > -1) {
                var parts = value.split(':');
                remainingSeconds = parseInt(parts[0]) * 60 + parseInt(parts[1]);
            }
        }

        if (remainingSeconds <= 0) {
            showCartExpired(label);
            return;
        }

        cartTimerInterval = setInterval(function () {
            remainingSeconds = Math.max(0, remainingSeconds - 1);
            var mins = Math.floor(remainingSeconds / 60);
            var secs = Math.floor(remainingSeconds % 60);
            display.textContent = (mins < 10 ? '0' : '') + mins + ':' + (secs < 10 ? '0' : '') + secs;

            // Update bar appearance based on time left
            bar.classList.toggle('warning', remainingSeconds <= 300 && remainingSeconds > 60);
            bar.classList.toggle('danger', remainingSeconds <= 60 && remainingSeconds > 0);

            if (remainingSeconds <= 0) {
                clearInterval(cartTimerInterval);
                cartTimerInterval = null;
                bar.classList.remove('warning', 'danger');
                bar.classList.add('expired');
                showCartExpired(label);
            }
        }, 1000);
    }

    function showCartExpired(labelEl) {
        labelEl.innerHTML = '<i class="bi bi-exclamation-triangle-fill"></i> ⏰ Giỏ hàng đã hết hạn! Nhấn <a href="#" onclick="refreshCartTimer();return false;" style="color:white;font-weight:700;text-decoration:underline;">Làm mới</a> để tiếp tục.';

        // Disable checkout button
        var checkoutBtn = document.getElementById('cartCheckoutBtn');
        if (checkoutBtn) {
            checkoutBtn.disabled = true;
            checkoutBtn.className = 'cart-checkout-btn disabled';
        }
    }

    // Refresh cart timer (reset creation time)
    window.refreshCartTimer = function () {
        fetch('/Cart/RefreshTimer', { method: 'POST' })
            .then(function (r) { return r.json(); })
            .then(function (data) {
                if (data.success) {
                    var bar = document.getElementById('cartTimerBar');
                    if (bar) {
                        bar.dataset.remaining = data.remainingSeconds;
                        bar.classList.remove('warning', 'danger', 'expired');
                    }
                    var display = document.getElementById('cartTimerDisplay');
                    if (display) {
                        display.dataset.remaining = data.remainingSeconds;
                        remainingSeconds = data.remainingSeconds;
                    }
                    var label = document.getElementById('cartTimerLabel');
                    if (label) {
                        label.innerHTML = '⏱ Giỏ hàng hết hạn sau: <strong id="cartTimerDisplay" data-remaining="' + data.remainingSeconds + '">' +
                            formatTime(data.remainingSeconds) + '</strong>';
                    }
                    // Re-enable checkout
                    var checkoutBtn = document.getElementById('cartCheckoutBtn');
                    if (checkoutBtn) {
                        checkoutBtn.disabled = false;
                        checkoutBtn.className = 'cart-checkout-btn enabled';
                    }
                    startCartCountdown();
                }
            })
            .catch(function () { /* ignore */ });
    };

    function formatTime(totalSeconds) {
        var mins = Math.floor(totalSeconds / 60);
        var secs = Math.floor(totalSeconds % 60);
        return (mins < 10 ? '0' : '') + mins + ':' + (secs < 10 ? '0' : '') + secs;
    }

    // ========================================================================
    //  BADGE
    // ========================================================================

    function refreshBadge() {
        fetch('/Cart/Count')
            .then(r => r.json())
            .then(data => {
                if (data && typeof data.count === 'number') {
                    cartBadge.textContent = data.count;
                    cartBadge.style.display = data.count > 0 ? 'flex' : 'none';
                }
            })
            .catch(() => { /* ignore */ });
    }

    // ========================================================================
    //  ADD TO CART
    // ========================================================================

    window.addToCart = function (productId, productName, imageUrl, price, quantity, categoryName, brand, size) {
        quantity = quantity || 1;

        return fetch('/Cart/Add', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({
                productId: productId,
                productName: productName || '',
                imageUrl: imageUrl || '',
                price: price,
                quantity: quantity,
                categoryName: categoryName || '',
                brand: brand || '',
                size: size || ''
            })
        })
            .then(r => r.json())
            .then(data => {
                if (data.success) {
                    refreshBadge();
                    if (typeof Swal !== 'undefined') {
                        Swal.fire({
                            icon: 'success',
                            title: 'Đã thêm vào giỏ hàng!',
                            text: productName || 'Sản phẩm đã được thêm',
                            timer: 1500,
                            showConfirmButton: false
                        });
                    }
                } else {
                    if (typeof Swal !== 'undefined') {
                        Swal.fire({ icon: 'error', title: 'Lỗi', text: data.message || 'Không thể thêm vào giỏ hàng' });
                    }
                }
                return data;
            })
            .catch(err => {
                if (typeof Swal !== 'undefined') {
                    Swal.fire({ icon: 'error', title: 'Lỗi kết nối', text: 'Vui lòng thử lại sau' });
                }
                return { success: false, message: err };
            });
    };

    // ========================================================================
    //  UPDATE / REMOVE
    // ========================================================================

    window.updateCartItem = function (productId, quantity) {
        if (!productId) return Promise.reject('Missing productId');

        if (quantity <= 0) {
            return removeCartItem(productId);
        }

        return fetch('/Cart/Update', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ productId: productId, quantity: quantity })
        })
            .then(r => r.json())
            .then(data => {
                if (data.success) {
                    refreshBadge();
                    var itemTotalEl = document.querySelector('.item-total[data-product-id="' + productId + '"]');
                    if (itemTotalEl) {
                        itemTotalEl.textContent = data.itemTotal.toLocaleString() + '₫';
                    }
                    var cartTotalEl = document.getElementById('cartTotalAmount');
                    if (cartTotalEl) {
                        cartTotalEl.textContent = data.cartTotal.toLocaleString() + '₫';
                    }
                    if (data.isEmpty) {
                        location.reload();
                    }
                }
                return data;
            })
            .catch(function () {
                if (typeof Swal !== 'undefined') {
                    Swal.fire({ icon: 'error', title: 'Lỗi', text: 'Không thể cập nhật số lượng' });
                }
                return { success: false };
            });
    };

    window.removeCartItem = function (productId) {
        if (!productId) return Promise.reject('Missing productId');

        return fetch('/Cart/Remove', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ productId: productId })
        })
            .then(r => r.json())
            .then(data => {
                if (data.success) {
                    refreshBadge();
                    var row = document.querySelector('.cart-item-row[data-product-id="' + productId + '"]');
                    if (row) row.remove();
                    var cartTotalEl = document.getElementById('cartTotalAmount');
                    if (cartTotalEl) {
                        cartTotalEl.textContent = data.cartTotal.toLocaleString() + '₫';
                    }
                    if (data.isEmpty) {
                        location.reload();
                    }
                }
                return data;
            })
            .catch(function () {
                if (typeof Swal !== 'undefined') {
                    Swal.fire({ icon: 'error', title: 'Lỗi', text: 'Không thể xóa sản phẩm' });
                }
                return { success: false };
            });
    };

    // ========================================================================
    //  CHECKOUT — MODAL
    // ========================================================================

    // Show checkout confirmation modal
    window.showCartCheckoutModal = function () {
        var modalEl = document.getElementById('cartCheckoutModal');
        if (!modalEl) return;

        // Build items list from current cart rows
        var itemsHtml = '';
        var rows = document.querySelectorAll('.cart-item-row');
        var subtotal = 0;

        rows.forEach(function (row) {
            var productId = row.dataset.productId;
            var nameEl = row.querySelector('.cart-item-name');
            var priceEl = row.querySelector('.cart-item-price');
            var qtyInput = row.querySelector('.cart-qty-input');
            var totalEl = row.querySelector('.item-total');
            var imgEl = row.querySelector('.cart-item-image');

            var name = nameEl ? nameEl.textContent.trim() : '';
            var price = priceEl ? priceEl.textContent.trim() : '';
            var qty = qtyInput ? parseInt(qtyInput.value) : 1;
            var total = totalEl ? totalEl.textContent.trim() : '';
            var imgSrc = '';
            if (imgEl && imgEl.tagName === 'IMG') {
                imgSrc = imgEl.src;
            } else if (imgEl) {
                // Placeholder icon
                imgSrc = '';
            }

            // Parse total for subtotal
            if (totalEl) {
                var numStr = totalEl.textContent.replace(/[₫,.\s]/g, '').trim();
                var num = parseInt(numStr) || 0;
                subtotal += num;
            }

            itemsHtml += `
                <div class="checkout-item-row">
                    <div class="checkout-item-img">
                        ${imgSrc ? '<img src="' + imgSrc + '" alt="' + name + '" />' : '<div class="checkout-item-placeholder"><i class="bi bi-box"></i></div>'}
                    </div>
                    <div class="checkout-item-info">
                        <div class="checkout-item-name">${name}</div>
                        <div class="checkout-item-meta">${price} × ${qty}</div>
                    </div>
                    <div class="checkout-item-total">${total}</div>
                </div>
            `;
        });

        document.getElementById('checkoutItemsList').innerHTML = itemsHtml;

        // Update totals in modal
        var totalAmountEl = document.getElementById('cartTotalAmount');
        var totalText = totalAmountEl ? totalAmountEl.textContent.trim() : subtotal.toLocaleString() + '₫';
        document.getElementById('checkoutSubtotal').textContent = totalText;
        document.getElementById('checkoutTotal').textContent = totalText;

        // Get wallet balance from page
        var walletEl = document.querySelector('.cart-summary-wallet strong');
        var walletText = walletEl ? walletEl.textContent.trim() : '0₫';
        document.getElementById('checkoutWalletBalance').textContent = walletText;

        // Check for shortage
        var warningEl = document.getElementById('checkoutWarning');
        var warningTextEl = document.getElementById('checkoutWarningText');
        var shortageEl = document.querySelector('.cart-summary-wallet.shortage');
        if (shortageEl) {
            warningEl.classList.remove('d-none');
            var shortageSpan = shortageEl.querySelector('span[style*="color:var(--danger)"]');
            warningTextEl.textContent = shortageSpan ? shortageSpan.textContent.trim() : 'Số dư không đủ để thanh toán!';
        } else {
            warningEl.classList.add('d-none');
        }

        // Check expiry
        var timerBar = document.getElementById('cartTimerBar');
        if (timerBar && timerBar.classList.contains('expired')) {
            warningEl.classList.remove('d-none');
            warningTextEl.textContent = '⏰ Giỏ hàng đã hết hạn! Vui lòng nhấn "Làm mới" để tiếp tục.';
            document.getElementById('confirmCheckoutBtn').disabled = true;
        } else {
            document.getElementById('confirmCheckoutBtn').disabled = false;
        }

        // Show modal
        var modal = new bootstrap.Modal(modalEl);
        modal.show();
    };

    // Process checkout (called from modal confirm button)
    window.processCartCheckout = function () {
        var btn = document.getElementById('confirmCheckoutBtn');
        var originalHtml = btn.innerHTML;
        btn.innerHTML = '<span class="spinner-border spinner-border-sm me-2" role="status"></span> Đang xử lý...';
        btn.disabled = true;

        fetch('/Cart/Checkout', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' }
        })
            .then(function (r) { return r.json(); })
            .then(function (data) {
                btn.innerHTML = originalHtml;
                btn.disabled = false;

                if (data.success) {
                    // Close confirmation modal
                    var confirmModal = bootstrap.Modal.getInstance(document.getElementById('cartCheckoutModal'));
                    if (confirmModal) confirmModal.hide();

                    refreshBadge();

                    // Show QR modal
                    var qrImg = document.getElementById('cartQrCodeImg');
                    if (qrImg) {
                        qrImg.src = 'data:image/png;base64,' + data.qrCode;
                    }

                    // Start QR countdown (15 min = 900s)
                    var timerSpan = document.getElementById('cartQrTimer');
                    var countdownDiv = document.getElementById('cartQrCountdown');
                    if (timerSpan && countdownDiv) {
                        startCartQrCountdown(900, timerSpan, countdownDiv);
                    }

                    var qrModal = new bootstrap.Modal(document.getElementById('cartQrModal'));
                    qrModal.show();
                } else {
                    if (typeof Swal !== 'undefined') {
                        Swal.fire({ icon: 'error', title: 'Thanh toán thất bại', text: data.message || 'Vui lòng thử lại' });
                    } else {
                        alert(data.message || 'Thanh toán thất bại');
                    }
                }
            })
            .catch(function () {
                btn.innerHTML = originalHtml;
                btn.disabled = false;
                if (typeof Swal !== 'undefined') {
                    Swal.fire({ icon: 'error', title: 'Lỗi kết nối', text: 'Vui lòng thử lại sau' });
                }
            });
    };

    // QR countdown timer (15 minutes)
    function startCartQrCountdown(duration, display, container) {
        if (cartQrInterval) clearInterval(cartQrInterval);

        var timer = duration, minutes, seconds;
        var qrImg = document.getElementById('cartQrCodeImg');
        if (qrImg) qrImg.style.opacity = '1';

        function updateDisplay() {
            minutes = parseInt(timer / 60, 10);
            seconds = parseInt(timer % 60, 10);

            minutes = minutes < 10 ? '0' + minutes : minutes;
            seconds = seconds < 10 ? '0' + seconds : seconds;

            display.textContent = minutes + ':' + seconds;

            if (--timer < 0) {
                clearInterval(cartQrInterval);
                cartQrInterval = null;
                container.innerHTML = '<span class="text-danger fw-bold"><i class="bi bi-exclamation-triangle-fill"></i> Mã QR đã hết hạn!</span>';
                if (qrImg) qrImg.style.opacity = '0.2';
            }
        }

        updateDisplay();
        cartQrInterval = setInterval(updateDisplay, 1000);
    }

    // ========================================================================
    //  PAGE INIT
    // ========================================================================

    if (window.location.pathname.toLowerCase().startsWith('/cart')) {
        // Start cart countdown timer
        startCartCountdown();

        // Set up quantity input handlers
        document.querySelectorAll('.cart-qty-input').forEach(function (input) {
            input.addEventListener('change', function () {
                var pid = parseInt(this.getAttribute('data-product-id'));
                var qty = parseInt(this.value) || 0;
                if (qty > 0) {
                    updateCartItem(pid, qty);
                } else {
                    removeCartItem(pid);
                }
            });
        });

        document.querySelectorAll('.cart-qty-minus').forEach(function (btn) {
            btn.addEventListener('click', function () {
                var pid = parseInt(this.getAttribute('data-product-id'));
                var input = document.querySelector('.cart-qty-input[data-product-id="' + pid + '"]');
                var val = parseInt(input.value) || 1;
                if (val > 1) {
                    input.value = val - 1;
                    updateCartItem(pid, val - 1);
                } else {
                    removeCartItem(pid);
                }
            });
        });

        document.querySelectorAll('.cart-qty-plus').forEach(function (btn) {
            btn.addEventListener('click', function () {
                var pid = parseInt(this.getAttribute('data-product-id'));
                var input = document.querySelector('.cart-qty-input[data-product-id="' + pid + '"]');
                var val = parseInt(input.value) || 1;
                input.value = val + 1;
                updateCartItem(pid, val + 1);
            });
        });

        document.querySelectorAll('.cart-remove-btn').forEach(function (btn) {
            btn.addEventListener('click', function () {
                var pid = parseInt(this.getAttribute('data-product-id'));
                if (typeof Swal !== 'undefined') {
                    Swal.fire({
                        title: 'Xóa sản phẩm?',
                        text: 'Bạn có chắc muốn xóa sản phẩm này khỏi giỏ hàng?',
                        icon: 'warning',
                        showCancelButton: true,
                        confirmButtonColor: '#EF4444',
                        cancelButtonColor: '#64748B',
                        confirmButtonText: 'Xóa',
                        cancelButtonText: 'Giữ lại'
                    }).then(function (result) {
                        if (result.isConfirmed) removeCartItem(pid);
                    });
                } else {
                    removeCartItem(pid);
                }
            });
        });
    }

    // Refresh badge on each page load
    refreshBadge();

})();
