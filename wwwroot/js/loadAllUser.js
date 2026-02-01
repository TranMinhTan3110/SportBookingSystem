function loadAllUser() {
    try {
        const [countUserResponse, userDataResponse] = await Promise.all([
            fetch('api/user/count'),
            fetch('api/user/all')
        ]
        );
        if (!countUserResponse.ok)  || !userDataResponse.ok; {
            throw new Error('Không tải dữ liệu');
        }
        const countUser = await countUserResponse.json();
        const userData = await userDataResponse.json();
        //gọi hàm render
    } catch (error) {
        console.error('Lỗi load dữ liệu:', error);

    }

}
function renderALlUser(countUser,userData) {
    document.getElementById('totalUsers').innerText = countUser.Total
}
