document.addEventListener('DOMContentLoaded', function () {

    const servicesDropdown = document.getElementById('servicesDropdown');
    const userDropdown = document.getElementById('userDropdown');
    const dropdownOverlay = document.getElementById('dropdownOverlay');

    const mobileMenuToggle = document.getElementById('mobileMenuToggle');
    const mobileNav = document.getElementById('mobileNav');
    const mobileNavOverlay = document.getElementById('mobileNavOverlay');
    const mobileNavClose = document.getElementById('mobileNavClose');
    const mobileNavLinks = document.querySelectorAll('.mobile-nav-link');

    const header = document.querySelector('.header');

    let scrollPosition = 0;

    if (servicesDropdown) {
        const toggle = servicesDropdown.querySelector('.dropdown-toggle');

        toggle.addEventListener('click', function (e) {
            e.preventDefault();
            e.stopPropagation();

            const isOpen = servicesDropdown.classList.contains('open');
            closeAllDropdowns();

            if (!isOpen) {
                servicesDropdown.classList.add('open');
                dropdownOverlay.classList.add('active');
            }
        });
    }

    if (userDropdown) {
        const avatar = userDropdown.querySelector('.user-avatar');

        avatar.addEventListener('click', function (e) {
            e.preventDefault();
            e.stopPropagation();

            const isOpen = userDropdown.classList.contains('open');
            closeAllDropdowns();

            if (!isOpen) {
                userDropdown.classList.add('open');
                dropdownOverlay.classList.add('active');
            }
        });
    }

    if (dropdownOverlay) {
        dropdownOverlay.addEventListener('click', closeAllDropdowns);
    }

    document.addEventListener('click', function (e) {
        if (!e.target.closest('.dropdown') && !e.target.closest('.user-dropdown')) {
            closeAllDropdowns();
        }
    });

    if (mobileMenuToggle) {
        mobileMenuToggle.addEventListener('click', function (e) {
            e.stopPropagation();
            openMobileMenu();
        });
    }

    if (mobileNavClose) {
        mobileNavClose.addEventListener('click', function (e) {
            e.stopPropagation();
            closeMobileMenu();
        });
    }

    if (mobileNavOverlay) {
        mobileNavOverlay.addEventListener('click', closeMobileMenu);
    }

    mobileNavLinks.forEach(link => {
        link.addEventListener('click', closeMobileMenu);
    });

    document.querySelectorAll('a[href^="#"]').forEach(anchor => {
        anchor.addEventListener('click', function (e) {
            e.preventDefault();

            const targetId = this.getAttribute('href');
            if (targetId === '#') return;

            const target = document.querySelector(targetId);
            if (target) {
                target.scrollIntoView({ behavior: 'smooth', block: 'start' });
            }
        });
    });

    window.addEventListener('scroll', function () {
        if (header) {
            header.style.boxShadow = window.pageYOffset > 50
                ? '0 2px 10px rgba(0, 0, 0, 0.1)'
                : 'none';
        }
    });

    window.addEventListener('resize', function () {
        if (window.innerWidth > 768) {
            closeMobileMenu();
        }
    });

    document.addEventListener('keydown', function (e) {
        if (e.key === 'Escape') {
            closeAllDropdowns();
            closeMobileMenu();
        }
    });

    function closeAllDropdowns() {
        if (servicesDropdown) servicesDropdown.classList.remove('open');
        if (userDropdown) userDropdown.classList.remove('open');
        if (dropdownOverlay) dropdownOverlay.classList.remove('active');
    }

    function openMobileMenu() {
        scrollPosition = window.pageYOffset || document.documentElement.scrollTop;

        document.body.style.overflow = 'hidden';
        document.body.style.position = 'fixed';
        document.body.style.width = '100%';
        document.body.style.top = `-${scrollPosition}px`;

        mobileNav.classList.add('active');
        mobileNavOverlay.classList.add('active');
    }

    function closeMobileMenu() {
        document.body.style.overflow = '';
        document.body.style.position = '';
        document.body.style.width = '';
        document.body.style.top = '';

        window.scrollTo(0, scrollPosition);

        mobileNav.classList.remove('active');
        mobileNavOverlay.classList.remove('active');
    }
});



/**
 * @param {number} amount
 * @returns {string}
 */
function formatCurrency(amount) {
    return new Intl.NumberFormat('vi-VN', {
        style: 'currency',
        currency: 'VND'
    }).format(amount);
}

/**
 * @param {Function} func
 * @param {number} wait - milliseconds
 * @returns {Function}
 */
function debounce(func, wait) {
    let timeout;
    return function executedFunction(...args) {
        clearTimeout(timeout);
        timeout = setTimeout(() => func.apply(this, args), wait);
    };
}

/**
 * @param {Function} func
 * @param {number} limit - milliseconds
 * @returns {Function}
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