// ============================================
// ADMIN PROFILE PAGE - INTERACTIVE
// ============================================

document.addEventListener('DOMContentLoaded', function () {

    console.log('✅ Admin Profile page loaded successfully!');

    // ============================================
    // PERSONAL INFO BUTTONS
    // ============================================
    document.querySelectorAll('.btn-change-email').forEach(btn => {
        btn.addEventListener('click', () => {
            // TODO: Open Change Email Modal for Admin
            alert('Admin: Chức năng đổi Email đang được phát triển');
        });
    });

    document.querySelectorAll('.btn-change-phone').forEach(btn => {
        btn.addEventListener('click', () => {
            // TODO: Open Change Phone Modal for Admin
            alert('Admin: Chức năng đổi Số điện thoại đang được phát triển');
        });
    });

    document.querySelectorAll('.btn-change-password').forEach(btn => {
        btn.addEventListener('click', () => {
            // TODO: Open Change Password Modal for Admin
            alert('Admin: Chức năng đổi Mật khẩu đang được phát triển');
        });
    });

    // ============================================
    // PROFILE AVATAR UPLOAD
    // ============================================
    const avatar = document.querySelector('.profile-avatar');
    if (avatar) {
        avatar.addEventListener('click', function () {
            // TODO: Open file upload dialog
            console.log('Admin Avatar clicked - open upload dialog');
        });
    }

});
