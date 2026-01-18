$(document).ready(function () {
    $("#forgot_password_form").on("submit", function (e) {
        e.preventDefault();
        var email = $("#email").val();

        Swal.fire({ title: 'Đang xử lý...', allowOutsideClick: false, didOpen: () => { Swal.showLoading(); } });

        $.ajax({
            type: "POST",
            url: "/Login/ForgotPassword",
            data: { email: email },
            success: function (response) {
                if (response.status === 'success') {
                    Swal.fire({
                        icon: 'success',
                        title: 'Thành công',
                        text: response.message
                    }).then(() => {
                        window.location.href = "/Login/ResetPassword?email=" + email;
                    });
                } else {
                    Swal.fire('Lỗi', response.message, 'error');
                }
            }
        });
    });
});