

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
        mobileNavOverlay.addEventListener('click', function () {
            closeMobileMenu();
        });
    }

    mobileNavLinks.forEach(link => {
        link.addEventListener('click', function () {
            closeMobileMenu();
        });
    });

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

    window.addEventListener('scroll', function () {
        if (window.pageYOffset > 50) {
            header.style.boxShadow = '0 2px 10px rgba(0, 0, 0, 0.1)';
        } else {
            header.style.boxShadow = 'none';
        }
    });

    window.addEventListener('resize', function () {
        if (window.innerWidth > 768) {
            closeMobileMenu();
        }
    });

    function closeAllDropdowns() {
        if (servicesDropdown) {
            servicesDropdown.classList.remove('open');
        }
        if (userDropdown) {
            userDropdown.classList.remove('open');
        }
        dropdownOverlay.classList.remove('active');
    }

    function openMobileMenu() {
        mobileNav.classList.add('active');
        mobileNavOverlay.classList.add('active');
        document.body.style.overflow = 'hidden';
    }

    function closeMobileMenu() {
        mobileNav.classList.remove('active');
        mobileNavOverlay.classList.remove('active');
        document.body.style.overflow = '';
    }

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

/**

 @param {number} amount 
  @returns {string} F
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

 * @param {Function} func 
 * @param {number} wait 
 * @returns {Function} 
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

 * @param {Function} func -
 * @param {number} limit 
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