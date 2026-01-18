$(document).ready(function () {
    $("#reset_password_form").on("submit", function (e) {
        e.preventDefault();

        const data = {
            email: $("#resetEmail").val(),
            otp: $("#otpCode").val().trim(),
            newPassword: $("#newPassword").val()
        };

        Swal.fire({ title: 'Đang xử lý...', allowOutsideClick: false, didOpen: () => { Swal.showLoading(); } });

        $.ajax({
            type: "POST",
            url: "/Login/VerifyAndResetPassword",
            data: data,
            success: function (response) {
                if (response.status === 'success') {
                    Swal.fire('Thành công', 'Mật khẩu đã được đổi. Vui lòng đăng nhập lại.', 'success')
                        .then(() => window.location.href = "/Login/SignIn");
                } else {
                    Swal.fire('Lỗi', response.message, 'error');
                }
            }
        });
    });
});