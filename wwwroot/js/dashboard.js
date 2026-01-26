document.addEventListener('DOMContentLoaded', function () {

    const ctx = document.getElementById('revenueChart');

    if (!ctx) {
        console.log('⚠️ Chart canvas not found');
        return;
    }

    const data = {
        labels: ['T2', 'T3', 'T4', 'T5', 'T6', 'T7', 'CN'],
        datasets: [{
            label: 'Doanh thu',
            data: [36000000, 45000000, 33000000, 54000000, 42000000, 51000000, 57000000],
            backgroundColor: function (context) {
                const chart = context.chart;
                const { ctx, chartArea } = chart;
                if (!chartArea) return null;

                const gradient = ctx.createLinearGradient(0, chartArea.top, 0, chartArea.bottom);
                gradient.addColorStop(0, 'rgba(16, 185, 129, 1)');
                gradient.addColorStop(1, 'rgba(16, 185, 129, 0.3)');
                return gradient;
            },
            borderColor: '#10B981',
            borderWidth: 2,
            borderRadius: 8,
            hoverBackgroundColor: function (context) {
                const chart = context.chart;
                const { ctx, chartArea } = chart;
                if (!chartArea) return null;

                const gradient = ctx.createLinearGradient(0, chartArea.top, 0, chartArea.bottom);
                gradient.addColorStop(0, 'rgba(245, 158, 11, 1)');
                gradient.addColorStop(1, 'rgba(245, 158, 11, 0.3)');
                return gradient;
            },
            hoverBorderColor: '#F59E0B',
        }]
    };

    const config = {
        type: 'bar',
        data: data,
        options: {
            responsive: true,
            maintainAspectRatio: false,
            plugins: {
                legend: {
                    display: false
                },
                tooltip: {
                    backgroundColor: 'rgba(15, 23, 42, 0.95)',
                    titleColor: '#94A3AF',
                    bodyColor: '#10B981',
                    titleFont: {
                        size: 11,
                        weight: '600'
                    },
                    bodyFont: {
                        size: 14,
                        weight: '700'
                    },
                    padding: 12,
                    borderColor: '#10B981',
                    borderWidth: 1,
                    displayColors: false,
                    callbacks: {
                        title: function (context) {
                            const days = ['Thứ 2', 'Thứ 3', 'Thứ 4', 'Thứ 5', 'Thứ 6', 'Thứ 7', 'Chủ nhật'];
                            return days[context[0].dataIndex];
                        },
                        label: function (context) {
                            return new Intl.NumberFormat('vi-VN', {
                                style: 'currency',
                                currency: 'VND',
                                minimumFractionDigits: 0
                            }).format(context.parsed.y);
                        }
                    }
                }
            },
            scales: {
                y: {
                    beginAtZero: true,
                    max: 60000000,
                    ticks: {
                        color: '#94A3AF',
                        font: {
                            size: 11,
                            weight: '600'
                        },
                        callback: function (value) {
                            return (value / 1000000) + 'M';
                        }
                    },
                    grid: {
                        color: 'rgba(148, 163, 175, 0.3)',
                        drawBorder: false
                    }
                },
                x: {
                    ticks: {
                        color: '#94A3AF',
                        font: {
                            size: 12,
                            weight: '600'
                        }
                    },
                    grid: {
                        display: false
                    }
                }
            },
            animation: {
                duration: 1000,
                easing: 'easeOutQuart',
                delay: (context) => {
                    return context.dataIndex * 100;
                }
            }
        }
    };

    const chart = new Chart(ctx, config);

    console.log('📊 Chart.js loaded successfully!');
});