// ============================================
// SPORTHUB - USER LAYOUT JAVASCRIPT
// Dropdown + Mobile Menu + Smooth Scroll
// ============================================

document.addEventListener('DOMContentLoaded', function () {

    // ============================================
    // ELEMENTS
    // ============================================
    const servicesDropdown = document.getElementById('servicesDropdown');
    const userDropdown = document.getElementById('userDropdown');
    const dropdownOverlay = document.getElementById('dropdownOverlay');

    // Mobile Menu Elements
    const mobileMenuToggle = document.getElementById('mobileMenuToggle');
    const mobileNav = document.getElementById('mobileNav');
    const mobileNavOverlay = document.getElementById('mobileNavOverlay');
    const mobileNavClose = document.getElementById('mobileNavClose');
    const mobileNavLinks = document.querySelectorAll('.mobile-nav-link');

    const header = document.querySelector('.header');

    // ============================================
    // SERVICES DROPDOWN
    // ============================================
    if (servicesDropdown) {
        const toggle = servicesDropdown.querySelector('.dropdown-toggle');

        toggle.addEventListener('click', function (e) {
            e.preventDefault();
            e.stopPropagation();
            const isOpen = servicesDropdown.classList.contains('open');

            // Close all dropdowns first
            closeAllDropdowns();

            // Toggle this dropdown
            if (!isOpen) {
                servicesDropdown.classList.add('open');
                dropdownOverlay.classList.add('active');
            }
        });
    }

    // ============================================
    // USER DROPDOWN
    // ============================================
    if (userDropdown) {
        const avatar = userDropdown.querySelector('.user-avatar');

        avatar.addEventListener('click', function (e) {
            e.stopPropagation();
            const isOpen = userDropdown.classList.contains('open');

            // Close all dropdowns first
            closeAllDropdowns();

            // Toggle this dropdown
            if (!isOpen) {
                userDropdown.classList.add('open');
                dropdownOverlay.classList.add('active');
            }
        });
    }

    // Close dropdowns when clicking overlay
    if (dropdownOverlay) {
        dropdownOverlay.addEventListener('click', closeAllDropdowns);
    }

    // Close dropdowns when clicking outside
    document.addEventListener('click', function (e) {
        if (!e.target.closest('.dropdown') && !e.target.closest('.user-dropdown')) {
            closeAllDropdowns();
        }
    });

    // ============================================
    // MOBILE MENU
    // ============================================

    // Open Mobile Menu
    if (mobileMenuToggle) {
        mobileMenuToggle.addEventListener('click', function (e) {
            e.stopPropagation();
            openMobileMenu();
        });
    }

    // Close Mobile Menu
    if (mobileNavClose) {
        mobileNavClose.addEventListener('click', function (e) {
            e.stopPropagation();
            closeMobileMenu();
        });
    }

    // Close Mobile Menu when clicking overlay
    if (mobileNavOverlay) {
        mobileNavOverlay.addEventListener('click', function () {
            closeMobileMenu();
        });
    }

    // Close Mobile Menu when clicking a link
    mobileNavLinks.forEach(link => {
        link.addEventListener('click', function () {
            closeMobileMenu();
        });
    });

    // ============================================
    // SMOOTH SCROLL FOR ANCHOR LINKS
    // ============================================
    const anchorLinks = document.querySelectorAll('a[href^="#"]');

    anchorLinks.forEach(anchor => {
        anchor.addEventListener('click', function (e) {
            e.preventDefault();
            const targetId = this.getAttribute('href');

            if (targetId === '#') return;

            const target = document.querySelector(targetId);
            if (target) {
                target.scrollIntoView({
                    behavior: 'smooth',
                    block: 'start'
                });
            }
        });
    });

    // ============================================
    // HEADER SHADOW ON SCROLL
    // ============================================
    window.addEventListener('scroll', function () {
        if (window.pageYOffset > 50) {
            header.style.boxShadow = '0 2px 10px rgba(0, 0, 0, 0.1)';
        } else {
            header.style.boxShadow = 'none';
        }
    });

    // ============================================
    // CLOSE MOBILE MENU ON WINDOW RESIZE
    // ============================================
    window.addEventListener('resize', function () {
        if (window.innerWidth > 768) {
            closeMobileMenu();
        }
    });

    // ============================================
    // FUNCTIONS
    // ============================================

    /**
     * Close all dropdown menus
     */
    function closeAllDropdowns() {
        if (servicesDropdown) {
            servicesDropdown.classList.remove('open');
        }
        if (userDropdown) {
            userDropdown.classList.remove('open');
        }
        dropdownOverlay.classList.remove('active');
    }

    /**
     * Open mobile navigation menu
     */
    function openMobileMenu() {
        mobileNav.classList.add('active');
        mobileNavOverlay.classList.add('active');
        document.body.style.overflow = 'hidden';
    }

    /**
     * Close mobile navigation menu
     */
    function closeMobileMenu() {
        mobileNav.classList.remove('active');
        mobileNavOverlay.classList.remove('active');
        document.body.style.overflow = '';
    }

    // ============================================
    // KEYBOARD NAVIGATION (ESC to close)
    // ============================================
    document.addEventListener('keydown', function (e) {
        if (e.key === 'Escape') {
            closeAllDropdowns();
            closeMobileMenu();
        }
    });

    // ============================================
    // CONSOLE LOG (Development)
    // ============================================
    console.log(' SportHub Layout loaded successfully!');
    console.log(' Mobile menu: ' + (mobileMenuToggle ? 'Ready' : 'Not found'));
    console.log(' Services dropdown: ' + (servicesDropdown ? 'Ready' : 'Not found'));
    console.log(' User dropdown: ' + (userDropdown ? 'Ready' : 'Not found'));
});

// ============================================
// UTILITY FUNCTIONS (Global)
// ============================================

/**
 * Format currency to Vietnamese Dong
 * @param {number} amount - Amount to format
 * @returns {string} Formatted currency string
 */
function formatCurrency(amount) {
    return new Intl.NumberFormat('vi-VN', {
        style: 'currency',
        currency: 'VND'
    }).format(amount);
}

/**
 * 
 * @param {string} message 
 * @param {string} type 
 */
function showToast(message, type = 'info') {
    console.log(`[${type.toUpperCase()}] ${message}`);
    
    alert(message);
}

/**
 * Debounce function
 * @param {Function} func - Function to debounce
 * @param {number} wait - Wait time in milliseconds
 * @returns {Function} Debounced function
 */
function debounce(func, wait) {
    let timeout;
    return function executedFunction(...args) {
        const later = () => {
            clearTimeout(timeout);
            func(...args);
        };
        clearTimeout(timeout);
        timeout = setTimeout(later, wait);
    };
}

/**
 * Throttle function
 * @param {Function} func - Function to throttle
 * @param {number} limit - Limit time in milliseconds
 * @returns {Function} Throttled function
 */
function throttle(func, limit) {
    let inThrottle;
    return function (...args) {
        if (!inThrottle) {
            func.apply(this, args);
            inThrottle = true;
            setTimeout(() => inThrottle = false, limit);
        }
    };
}