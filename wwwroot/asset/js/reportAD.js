// Revenue Line Chart
const revenueCtx = document.getElementById('revenueChart').getContext('2d');
const revenueChart = new Chart(revenueCtx, {
    type: 'line',
    data: {
        labels: ['01/01', '03/01', '05/01', '07/01', '09/01', '11/01', '13/01', '15/01', '17/01'],
        datasets: [
            {
                label: 'Đặt Sân',
                data: [12500, 15200, 13800, 16500, 14900, 17200, 15800, 18500, 19200],
                borderColor: '#10b981',
                backgroundColor: 'rgba(16, 185, 129, 0.1)',
                tension: 0.4,
                fill: true
            },
            {
                label: 'Dịch Vụ',
                data: [5200, 6100, 5800, 6900, 6400, 7100, 6800, 7500, 7900],
                borderColor: '#f59e0b',
                backgroundColor: 'rgba(245, 158, 11, 0.1)',
                tension: 0.4,
                fill: true
            },
            {
                label: 'Tổng',
                data: [17700, 21300, 19600, 23400, 21300, 24300, 22600, 26000, 27100],
                borderColor: '#3b82f6',
                backgroundColor: 'rgba(59, 130, 246, 0.1)',
                tension: 0.4,
                fill: true,
                borderWidth: 3
            }
        ]
    },
    options: {
        responsive: true,
        maintainAspectRatio: false, // Quan trọng cho responsive
        plugins: {
            legend: {
                position: window.innerWidth < 768 ? 'bottom' : 'top', // Đổi vị trí legend trên mobile
                labels: {
                    color: '#e2e8f0',
                    font: {
                        size: window.innerWidth < 576 ? 10 : 12, // Font nhỏ hơn trên mobile
                        weight: '600'
                    },
                    padding: window.innerWidth < 576 ? 10 : 15,
                    usePointStyle: true,
                    boxWidth: window.innerWidth < 576 ? 6 : 8
                }
            },
            tooltip: {
                backgroundColor: 'rgba(15, 23, 42, 0.9)',
                titleColor: '#e2e8f0',
                bodyColor: '#e2e8f0',
                borderColor: 'rgba(148, 163, 184, 0.2)',
                borderWidth: 1,
                padding: window.innerWidth < 576 ? 8 : 12,
                displayColors: true,
                titleFont: {
                    size: window.innerWidth < 576 ? 11 : 13
                },
                bodyFont: {
                    size: window.innerWidth < 576 ? 10 : 12
                },
                callbacks: {
                    label: function (context) {
                        return context.dataset.label + ': ' +
                            new Intl.NumberFormat('vi-VN').format(context.parsed.y * 1000) + '₫';
                    }
                }
            }
        },
        scales: {
            y: {
                beginAtZero: true,
                grid: {
                    color: 'rgba(148, 163, 184, 0.1)',
                    drawBorder: false
                },
                ticks: {
                    color: '#94a3b8',
                    font: { size: window.innerWidth < 576 ? 9 : 11 },
                    callback: function (value) {
                        return value + 'K';
                    },
                    maxTicksLimit: window.innerWidth < 576 ? 5 : 8 // Ít ticks hơn trên mobile
                }
            },
            x: {
                grid: {
                    display: false,
                    drawBorder: false
                },
                ticks: {
                    color: '#94a3b8',
                    font: { size: window.innerWidth < 576 ? 9 : 11 },
                    maxRotation: window.innerWidth < 576 ? 45 : 0, // Xoay label trên mobile
                    minRotation: window.innerWidth < 576 ? 45 : 0
                }
            }
        }
    }
});

// Category Pie Chart
const categoryCtx = document.getElementById('categoryChart').getContext('2d');
const categoryChart = new Chart(categoryCtx, {
    type: 'doughnut',
    data: {
        labels: ['Đặt Sân', 'Đồ Ăn & Thức Uống', 'Đồ Dùng & Quần Áo'],
        datasets: [{
            data: [198850, 52390, 34260],
            backgroundColor: [
                'rgba(16, 185, 129, 0.8)',
                'rgba(245, 158, 11, 0.8)',
                'rgba(59, 130, 246, 0.8)'
            ],
            borderColor: [
                '#10b981',
                '#f59e0b',
                '#3b82f6'
            ],
            borderWidth: 2
        }]
    },
    options: {
        responsive: true,
        maintainAspectRatio: false, // Quan trọng cho responsive
        plugins: {
            legend: {
                position: 'bottom',
                labels: {
                    color: '#e2e8f0',
                    font: {
                        size: window.innerWidth < 576 ? 10 : 12,
                        weight: '600'
                    },
                    padding: window.innerWidth < 576 ? 8 : 15,
                    usePointStyle: true,
                    boxWidth: window.innerWidth < 576 ? 6 : 8
                }
            },
            tooltip: {
                backgroundColor: 'rgba(15, 23, 42, 0.9)',
                titleColor: '#e2e8f0',
                bodyColor: '#e2e8f0',
                borderColor: 'rgba(148, 163, 184, 0.2)',
                borderWidth: 1,
                padding: window.innerWidth < 576 ? 8 : 12,
                titleFont: {
                    size: window.innerWidth < 576 ? 11 : 13
                },
                bodyFont: {
                    size: window.innerWidth < 576 ? 10 : 12
                },
                callbacks: {
                    label: function (context) {
                        const total = context.dataset.data.reduce((a, b) => a + b, 0);
                        const percentage = Math.round((context.parsed / total) * 100);
                        return context.label + ': ' + percentage + '%';
                    }
                }
            }
        },
        cutout: window.innerWidth < 576 ? '60%' : '70%' // Cutout nhỏ hơn trên mobile
    }
});

// Hàm xử lý nút Áp Dụng Filter
function applyFilter() {
    const fromDate = document.getElementById('fromDate').value;
    const toDate = document.getElementById('toDate').value;
    const period = document.getElementById('reportPeriod').value;

    // Hiệu ứng loading giả lập khi nhấn nút
    const btn = document.querySelector('.btn-apply');
    const originalText = btn.innerHTML;
    btn.innerHTML = '<i class="fas fa-spinner fa-spin me-2"></i>Đang xử lý...';
    btn.disabled = true;

    setTimeout(() => {
        alert('Đã cập nhật dữ liệu báo cáo từ ' + fromDate + ' đến ' + toDate);
        btn.innerHTML = originalText;
        btn.disabled = false;

        // Lưu ý: Thực tế bạn sẽ gọi AJAX tại đây để lấy dữ liệu từ SQL và cập nhật lại revenueChart.data
    }, 1000);
}

// Hàm update chart khi resize
function updateChartsOnResize() {
    const isMobile = window.innerWidth < 576;
    const isTablet = window.innerWidth >= 576 && window.innerWidth < 768;

    // Update Revenue Chart
    revenueChart.options.plugins.legend.position = isMobile || isTablet ? 'bottom' : 'top';
    revenueChart.options.plugins.legend.labels.font.size = isMobile ? 10 : 12;
    revenueChart.options.plugins.legend.labels.padding = isMobile ? 10 : 15;
    revenueChart.options.plugins.legend.labels.boxWidth = isMobile ? 6 : 8;
    revenueChart.options.plugins.tooltip.padding = isMobile ? 8 : 12;
    revenueChart.options.plugins.tooltip.titleFont.size = isMobile ? 11 : 13;
    revenueChart.options.plugins.tooltip.bodyFont.size = isMobile ? 10 : 12;
    revenueChart.options.scales.y.ticks.font.size = isMobile ? 9 : 11;
    revenueChart.options.scales.y.ticks.maxTicksLimit = isMobile ? 5 : 8;
    revenueChart.options.scales.x.ticks.font.size = isMobile ? 9 : 11;
    revenueChart.options.scales.x.ticks.maxRotation = isMobile ? 45 : 0;
    revenueChart.options.scales.x.ticks.minRotation = isMobile ? 45 : 0;

    // Update Category Chart
    categoryChart.options.plugins.legend.labels.font.size = isMobile ? 10 : 12;
    categoryChart.options.plugins.legend.labels.padding = isMobile ? 8 : 15;
    categoryChart.options.plugins.legend.labels.boxWidth = isMobile ? 6 : 8;
    categoryChart.options.plugins.tooltip.padding = isMobile ? 8 : 12;
    categoryChart.options.plugins.tooltip.titleFont.size = isMobile ? 11 : 13;
    categoryChart.options.plugins.tooltip.bodyFont.size = isMobile ? 10 : 12;
    categoryChart.options.cutout = isMobile ? '60%' : '70%';

    // Update charts
    revenueChart.update();
    categoryChart.update();
}

// Lắng nghe sự kiện resize với debounce
let resizeTimeout;
window.addEventListener('resize', () => {
    clearTimeout(resizeTimeout);
    resizeTimeout = setTimeout(() => {
        updateChartsOnResize();
        revenueChart.resize();
        categoryChart.resize();
    }, 250);
});

// Tự động điều chỉnh kích thước biểu đồ khi in
window.addEventListener('beforeprint', () => {
    revenueChart.resize();
    categoryChart.resize();
});

// Khởi tạo responsive khi load trang
window.addEventListener('load', () => {
    updateChartsOnResize();
});