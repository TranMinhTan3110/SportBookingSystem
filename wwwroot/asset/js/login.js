$(document).ready(function () {
    $("#togglePassword").click(function () {
        const passwordField = $("#password");
        const eyeIcon = $("#eyeIcon");
        const type = passwordField.attr("type") === "password" ? "text" : "password";
        passwordField.attr("type", type);
        if (type === "text") {
            eyeIcon.removeClass("bi-eye").addClass("bi-eye-slash");
        } else {
            eyeIcon.removeClass("bi-eye-slash").addClass("bi-eye");
        }
    });

    //Xử lý Đăng nhập
    $("#login_form").on("submit", function (e) {
        e.preventDefault();

        Swal.fire({
            title: 'Đang xác thực...',
            text: 'Vui lòng chờ trong giây lát',
            allowOutsideClick: false,
            didOpen: () => {
                Swal.showLoading();
            }
        });

        var phoneNumber = $("#phoneNumber").val().trim();
        var password = $("#password").val();

        $.ajax({
            type: "POST",
            url: "/Login/SignIn",
            data: {
                phoneNumber: phoneNumber,
                password: password
            },
            success: function (response) {
                if (response.status === 'success') {
                    Swal.fire({
                        icon: 'success',
                        title: 'Đăng nhập thành công!',
                        text: 'Đang chuyển hướng...',
                        timer: 1500,
                        showConfirmButton: false
                    }).then(function () {
                        window.location.href = response.redirect;
                    });
                } else {
                    Swal.fire({
                        icon: 'error',
                        title: 'Thất bại',
                        text: response.message
                    });
                }
            },
            error: function () {
                Swal.fire({
                    icon: 'error',
                    title: 'Lỗi!',
                    text: 'Không thể kết nối đến máy chủ.'
                });
            }
        });
    });
});