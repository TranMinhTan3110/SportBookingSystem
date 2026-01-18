$(document).ready(function () {
    $("#signup_form").on("submit", function (e) {
        e.preventDefault();

        var formData = {
            FullName: $("#fullName").val(),
            Phone: $("#phoneNumber").val(),
            Email: $("#email").val(), 
            Password: $("#password").val()
        };

        $.ajax({
            url: "/Login/SignUp",
            type: "POST",
            data: formData, 
            success: function (response) {
                if (response.status === "success") {
                    Swal.fire("Thành công", response.message, "success")
                        .then(() => window.location.href = response.redirect);
                } else {
                    Swal.fire("Lỗi đăng ký", response.message, "error");
                }
            },
            error: function () {
                Swal.fire("Lỗi", "Không thể kết nối đến máy chủ", "error");
            }
        });
    });
});