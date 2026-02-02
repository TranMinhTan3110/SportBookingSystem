let revenueChart = null;
let categoryChart = null;

function initializeMonthFilters() {
    const now = new Date();
    const twoMonthsAgo = new Date(now.getFullYear(), now.getMonth() - 2, 1);

    document.getElementById('fromMonth').value = twoMonthsAgo.getMonth() + 1;
    document.getElementById('fromYear').value = twoMonthsAgo.getFullYear();
    document.getElementById('toMonth').value = now.getMonth() + 1;
    document.getElementById('toYear').value = now.getFullYear();
}

// Toggle hiển thị bộ lọc tháng
function toggleMonthFilter() {
    const filterSection = document.getElementById('monthFilterSection');
    if (filterSection.style.display === 'none') {
        filterSection.style.display = 'block';
    } else {
        filterSection.style.display = 'none';
    }
}

// Áp dụng bộ lọc theo tháng
async function applyMonthFilter() {
    const fromMonth = parseInt(document.getElementById('fromMonth').value);
    const fromYear = parseInt(document.getElementById('fromYear').value);
    const toMonth = parseInt(document.getElementById('toMonth').value);
    const toYear = parseInt(document.getElementById('toYear').value);

    const fromDate = new Date(fromYear, fromMonth - 1, 1);
    const toDate = new Date(toYear, toMonth - 1, 1);

    if (fromDate > toDate) {
        alert('Tháng bắt đầu không được lớn hơn tháng kết thúc');
        return;
    }

    try {
        showLoading();

        const response = await fetch(
            `/ReportAD/GetMonthlySummaries?fromMonth=${fromMonth}&fromYear=${fromYear}&toMonth=${toMonth}&toYear=${toYear}`
        );
        const result = await response.json();

        if (result.success) {
            updateMonthlySummary(result.monthlySummaries);
        } else {
            alert(result.message || 'Có lỗi xảy ra khi tải dữ liệu');
        }
    } catch (error) {
        console.error('Error:', error);
        alert('Có lỗi xảy ra khi kết nối đến server');
    } finally {
        hideLoading();
    }
}

// Áp dụng bộ lọc chính
async function applyFilter() {
    const fromDate = document.getElementById('fromDate').value;
    const toDate = document.getElementById('toDate').value;

    if (!fromDate || !toDate) {
        alert('Vui lòng chọn khoảng thời gian');
        return;
    }

    if (new Date(fromDate) > new Date(toDate)) {
        alert('Ngày bắt đầu không được lớn hơn ngày kết thúc');
        return;
    }

    try {
        showLoading();

        const response = await fetch(`/ReportAD/GetReportData?fromDate=${fromDate}&toDate=${toDate}`);
        const result = await response.json();

        if (result.success) {
            updateStatsCards(result);

            updateTopSports(result.topSports);

            updateMonthlySummary(result.monthlySummaries);

            await updateCharts(fromDate, toDate);
        } else {
            alert(result.message || 'Có lỗi xảy ra khi tải dữ liệu');
        }
    } catch (error) {
        console.error('Error:', error);
        alert('Có lỗi xảy ra khi kết nối đến server');
    } finally {
        hideLoading();
    }
}

// Cập nhật thẻ thống kê
function updateStatsCards(data) {
    document.getElementById('totalRevenue').innerText = formatCurrency(data.totalRevenue);
    document.getElementById('bookingRevenue').innerText = formatCurrency(data.bookingRevenue);
    document.getElementById('serviceRevenue').innerText = formatCurrency(data.serviceRevenue);

    const growthElement = document.querySelector('.card-total .stat-change');
    const isPositive = data.growthRate >= 0;

    growthElement.className = `stat-change ${isPositive ? 'positive' : 'negative'}`;
    growthElement.innerHTML = `
        <i class="fas fa-arrow-${isPositive ? 'up' : 'down'}"></i>
        <span>${isPositive ? '+' : ''}${data.growthRate.toFixed(1)}% so với kỳ trước</span>
    `;
}

// Cập nhật danh sách môn thể thao
function updateTopSports(sports) {
    const container = document.getElementById('topSportsContainer');
    if (!container || !sports || sports.length === 0) return;

    container.innerHTML = sports.map(sport => `
        <div class="glass-card sport-card ${sport.colorClass}">
            <div class="sport-header">
                <div class="sport-icon">
                    <i class="${sport.iconClass}"></i>
                </div>
                <div class="sport-name">${sport.sportName}</div>
            </div>
            <div class="sport-stats">
                <div class="sport-stat-row">
                    <span class="sport-stat-label">Doanh thu</span>
                    <span class="sport-stat-value">${formatCurrency(sport.revenue)}</span>
                </div>
                <div class="sport-stat-row">
                    <span class="sport-stat-label">Số lượt đặt</span>
                    <span class="sport-stat-value">${sport.bookingCount}</span>
                </div>
                <div class="sport-stat-row">
                    <span class="sport-stat-label">Tăng trưởng</span>
                    <span class="growth-badge ${sport.growth >= 0 ? 'growth-positive' : 'growth-negative'}">
                        <i class="fas fa-arrow-${sport.growth >= 0 ? 'up' : 'down'}"></i>${sport.growth.toFixed(1)}%
                    </span>
                </div>
            </div>
        </div>
    `).join('');
}

// Cập nhật bảng tổng kết tháng
function updateMonthlySummary(summaries) {
    const tbody = document.getElementById('monthlySummaryBody');
    if (!tbody || !summaries || summaries.length === 0) {
        if (tbody) {
            tbody.innerHTML = '<tr><td colspan="5" class="text-center text-muted">Không có dữ liệu</td></tr>';
        }
        return;
    }

    tbody.innerHTML = summaries.map(summary => `
        <tr>
            <td>
                <div class="fw-semibold">${summary.monthLabel}</div>
                <div class="text-muted small">${summary.dateRange}</div>
            </td>
            <td class="amount-cell">${formatCurrency(summary.totalRevenue)}</td>
            <td>${summary.transactionCount}</td>
            <td>${formatCurrency(summary.averagePerTransaction)}</td>
            <td>
                <span class="growth-badge ${summary.growthRate >= 0 ? 'growth-positive' : 'growth-negative'}">
                    <i class="fas fa-arrow-${summary.growthRate >= 0 ? 'up' : 'down'}"></i>${summary.growthRate.toFixed(1)}%
                </span>
            </td>
        </tr>
    `).join('');
}

// Cập nhật biểu đồ
async function updateCharts(fromDate, toDate) {
    try {
        const response = await fetch(`/ReportAD/GetChartData?fromDate=${fromDate}&toDate=${toDate}`);
        const result = await response.json();

        if (result.success) {
            updateRevenueChart(result.labels, result.bookingData, result.serviceData);

            const totalBooking = result.bookingData.reduce((a, b) => a + b, 0);
            const totalService = result.serviceData.reduce((a, b) => a + b, 0);
            updateCategoryChart(totalBooking, totalService);
        }
    } catch (error) {
        console.error('Error updating charts:', error);
    }
}

// Vẽ biểu đồ doanh thu theo thời gian
function updateRevenueChart(labels, bookingData, serviceData) {
    const ctx = document.getElementById('revenueChart');
    if (!ctx) return;

    // Hủy chart cũ nếu có
    if (revenueChart) {
        revenueChart.destroy();
    }

    revenueChart = new Chart(ctx.getContext('2d'), {
        type: 'line',
        data: {
            labels: labels,
            datasets: [
                {
                    label: 'Đặt Sân',
                    data: bookingData,
                    borderColor: '#10b981',
                    backgroundColor: 'rgba(16, 185, 129, 0.1)',
                    tension: 0.4,
                    fill: true
                },
                {
                    label: 'Dịch Vụ',
                    data: serviceData,
                    borderColor: '#f59e0b',
                    backgroundColor: 'rgba(245, 158, 11, 0.1)',
                    tension: 0.4,
                    fill: true
                }
            ]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            plugins: {
                legend: {
                    display: true,
                    position: 'top',
                    labels: {
                        color: '#000',
                        font: { size: 12 }
                    }
                },
                tooltip: {
                    callbacks: {
                        label: function (context) {
                            return context.dataset.label + ': ' + formatCurrency(context.parsed.y);
                        }
                    }
                }
            },
            scales: {
                y: {
                    beginAtZero: true,
                    ticks: {
                        color: '#000',
                        callback: function (value) {
                            return (value / 1000000).toFixed(1) + 'M';
                        }
                    },
                    grid: {
                        color: 'rgba(148, 163, 184, 0.1)'
                    }
                },
                x: {
                    ticks: {
                        color: '#000'
                    },
                    grid: {
                        color: 'rgba(148, 163, 184, 0.1)'
                    }
                }
            }
        }
    });
}

// Vẽ biểu đồ doanh thu theo nguồn
function updateCategoryChart(booking, service) {
    const ctx = document.getElementById('categoryChart');
    if (!ctx) return;

    // Hủy chart cũ nếu có
    if (categoryChart) {
        categoryChart.destroy();
    }

    categoryChart = new Chart(ctx.getContext('2d'), {
        type: 'doughnut',
        data: {
            labels: ['Đặt Sân', 'Dịch Vụ'],
            datasets: [{
                data: [booking, service],
                backgroundColor: ['#10b981', '#f59e0b'],
                borderColor: ['#0f172a', '#0f172a'],
                borderWidth: 2
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            plugins: {
                legend: {
                    display: true,
                    position: 'bottom',
                    labels: {
                        color: '#000',
                        font: { size: 12 },
                        padding: 15
                    }
                },
                tooltip: {
                    callbacks: {
                        label: function (context) {
                            const total = context.dataset.data.reduce((a, b) => a + b, 0);
                            const percentage = total > 0 ? ((context.parsed / total) * 100).toFixed(1) : 0;
                            return context.label + ': ' + formatCurrency(context.parsed) + ' (' + percentage + '%)';
                        }
                    }
                }
            }
        }
    });
}

// Khởi tạo biểu đồ ban đầu
async function initializeCharts() {
    const fromDate = document.getElementById('fromDate').value;
    const toDate = document.getElementById('toDate').value;

    if (fromDate && toDate) {
        await updateCharts(fromDate, toDate);
    }
}

// Xử lý thay đổi kỳ báo cáo
async function handlePeriodChange() {
    const period = document.getElementById('reportPeriod').value;

    if (period === 'custom') {
        return;
    }

    try {
        showLoading();

        const response = await fetch(`/ReportAD/GetReportByPeriod?period=${period}`);
        const result = await response.json();

        if (result.success) {
            document.getElementById('fromDate').value = result.fromDate;
            document.getElementById('toDate').value = result.toDate;

            updateStatsCards(result.data);
            updateTopSports(result.data.topSports);
            updateMonthlySummary(result.data.monthlySummaries);
            await updateCharts(result.fromDate, result.toDate);
        }
    } catch (error) {
        console.error('Error:', error);
        alert('Có lỗi xảy ra khi tải dữ liệu');
    } finally {
        hideLoading();
    }
}

// Format tiền tệ
function formatCurrency(value) {
    return new Intl.NumberFormat('vi-VN').format(Math.round(value)) + '₫';
}

// Hiển thị loading
function showLoading() {
    let loadingOverlay = document.getElementById('loadingOverlay');

    if (!loadingOverlay) {
        loadingOverlay = document.createElement('div');
        loadingOverlay.id = 'loadingOverlay';
        loadingOverlay.innerHTML = `
            <div style="position: fixed; top: 0; left: 0; width: 100%; height: 100%; 
                        background: rgba(0,0,0,0.5); display: flex; justify-content: center; 
                        align-items: center; z-index: 9999;">
                <div style="background: white; padding: 20px; border-radius: 8px; text-align: center;">
                    <i class="fas fa-spinner fa-spin fa-2x text-primary mb-2"></i>
                    <div>Đang tải dữ liệu...</div>
                </div>
            </div>
        `;
        document.body.appendChild(loadingOverlay);
    }

    loadingOverlay.style.display = 'block';
}

// Ẩn loading
function hideLoading() {
    const loadingOverlay = document.getElementById('loadingOverlay');
    if (loadingOverlay) {
        loadingOverlay.style.display = 'none';
    }
}