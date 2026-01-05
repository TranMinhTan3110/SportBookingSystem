function openTab(evt, tabName) {
    // Ẩn tất cả nội dung tab
    var i, tabcontent, tablinks;
    tabcontent = document.getElementsByClassName("tab-content");
    for (i = 0; i < tabcontent.length; i++) {
        tabcontent[i].classList.remove("active");
    }

    // Xóa class active ở tất cả các nút tab
    tablinks = document.getElementsByClassName("tab-btn");
    for (i = 0; i < tablinks.length; i++) {
        tablinks[i].classList.remove("active");
    }

    // Hiện tab hiện tại và thêm class active vào nút
    document.getElementById(tabName).classList.add("active");
    evt.currentTarget.classList.add("active");
}