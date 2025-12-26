
        // Dropdown functionality
    document.addEventListener('DOMContentLoaded', function() {
            const servicesDropdown = document.getElementById('servicesDropdown');
    const userDropdown = document.getElementById('userDropdown');
    const dropdownOverlay = document.getElementById('dropdownOverlay');

    // Services Dropdown Toggle
    if (servicesDropdown) {
                const toggle = servicesDropdown.querySelector('.dropdown-toggle');
    toggle.addEventListener('click', function(e) {
        e.stopPropagation();
    const isOpen = servicesDropdown.classList.contains('open');

    closeAllDropdowns();

    if (!isOpen) {
        servicesDropdown.classList.add('open');
    dropdownOverlay.classList.add('active');
                    }
                });
            }

    // User Dropdown Toggle
    if (userDropdown) {
                const avatar = userDropdown.querySelector('.user-avatar');
    avatar.addEventListener('click', function(e) {
        e.stopPropagation();
    const isOpen = userDropdown.classList.contains('open');

    closeAllDropdowns();

    if (!isOpen) {
        userDropdown.classList.add('open');
    dropdownOverlay.classList.add('active');
                    }
                });
            }


    dropdownOverlay.addEventListener('click', closeAllDropdowns);

    document.addEventListener('click', function(e) {
                if (!e.target.closest('.dropdown') && !e.target.closest('.user-dropdown')) {
        closeAllDropdowns();
                }
            });


    function closeAllDropdowns() {
        servicesDropdown?.classList.remove('open');
    userDropdown?.classList.remove('open');
    dropdownOverlay.classList.remove('active');
            }


            document.querySelectorAll('a[href^="#"]').forEach(anchor => {
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

    const header = document.querySelector('.header');
            window.addEventListener('scroll', () => {
                if (window.pageYOffset > 50) {
        header.style.boxShadow = '0 2px 10px rgba(0, 0, 0, 0.1)';
                } else {
        header.style.boxShadow = 'none';
                }
            });
        });