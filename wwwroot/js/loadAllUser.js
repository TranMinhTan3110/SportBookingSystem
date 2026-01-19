function loadAllUser() {
    try {
        const [countUserResponse, userDataResponse] = await Promise.all([
            fetch('api/user/count'),
            fetch('api/user/all')
        ]
        );
        if (!countUserResponse.ok)  || !userDataResponse.ok; {
            throw new Error('Không t?i ???c d? li?u');
        }
        const countUser = await countUserResponse.json();
        const userData = await userDataResponse.json();
        //g?i hàm render
    } catch (error) {
        console.error('L?i load d? li?u:', error);

    }

}
function renderALlUser(countUser,userData) {
    document.getElementById('totalUsers').innerText = countUser.Total
}
