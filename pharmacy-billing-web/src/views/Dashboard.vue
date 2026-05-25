<template>
  <div class="dashboard-page">
    <div class="page-header">
      <div class="page-title">
        <h2>Báo Cáo Doanh Thu & Kho</h2>
        <p>Tổng quan hoạt động tài chính phòng khám và tồn kho dược phẩm</p>
      </div>
      <button @click="fetchData" class="btn btn-secondary">
        🔄 Làm mới
      </button>
    </div>

    <!-- Stat Cards Grid -->
    <div class="dashboard-grid">
      <div class="stat-card sc-revenue">
        <div class="stat-details">
          <h4>Tổng Doanh Thu</h4>
          <div class="stat-number">{{ formatCurrency(stats.totalRevenue) }}</div>
        </div>
        <span class="stat-icon">💰</span>
      </div>

      <div class="stat-card sc-paid">
        <div class="stat-details">
          <h4>Hóa Đơn Đã Thanh Toán</h4>
          <div class="stat-number">{{ stats.paidBillsCount }}</div>
        </div>
        <span class="stat-icon">✅</span>
      </div>

      <div class="stat-card sc-pending">
        <div class="stat-details">
          <h4>Hóa Đơn Chờ</h4>
          <div class="stat-number">{{ stats.pendingBillsCount }}</div>
        </div>
        <span class="stat-icon">⏳</span>
      </div>

      <div class="stat-card sc-alerts">
        <div class="stat-details">
          <h4>Cảnh Báo Hết Thuốc</h4>
          <div class="stat-number">
            {{ stats.outOfStockCount }} <span style="font-size: 0.8rem; font-weight: normal; color: var(--danger)">hết kho</span> /
            {{ stats.lowStockAlertsCount }} <span style="font-size: 0.8rem; font-weight: normal; color: var(--warning)">sắp hết</span>
          </div>
        </div>
        <span class="stat-icon" :style="{ color: stats.outOfStockCount > 0 ? 'var(--danger)' : 'var(--warning)' }">⚠️</span>
      </div>
    </div>

    <div class="dashboard-charts">
      <!-- Custom CSS Daily Revenue Bar Chart -->
      <div class="card">
        <h3 style="font-size: 1.1rem; font-weight: 700; margin-bottom: 1rem; color: var(--text-main);">
          📈 Doanh Thu 7 Ngày Qua
        </h3>
        
        <div v-if="chartData.length === 0" style="text-align: center; line-height: 200px; color: var(--text-muted);">
          Không có dữ liệu doanh thu gần đây.
        </div>

        <div v-else class="css-chart-container">
          <div v-for="point in chartData" :key="point.dateLabel" class="chart-bar-wrapper">
            <!-- Visual Chart Combo bars -->
            <div class="chart-bar-combo">
              <div 
                class="chart-bar-clinic" 
                :style="{ height: getBarHeight(point.clinicRevenue) + '%' }" 
                :title="`Khám lâm sàng: ${formatCurrency(point.clinicRevenue)}`"
              ></div>
              <div 
                class="chart-bar-medicine" 
                :style="{ height: getBarHeight(point.medicineRevenue) + '%' }" 
                :title="`Bán thuốc: ${formatCurrency(point.medicineRevenue)}`"
              ></div>
            </div>
            <div class="chart-label">{{ point.dateLabel }}</div>
          </div>
        </div>

        <div class="chart-legend">
          <div class="legend-item">
            <span class="legend-color legend-clinic"></span>
            <span>Khám lâm sàng</span>
          </div>
          <div class="legend-item">
            <span class="legend-color legend-medicine"></span>
            <span>Thuốc & thiết bị</span>
          </div>
        </div>
      </div>

      <!-- Recent Payments list -->
      <div class="card">
        <h3 style="font-size: 1.1rem; font-weight: 700; margin-bottom: 0.5rem; color: var(--text-main);">
          📑 Giao Dịch Gần Đây
        </h3>
        <p style="font-size: 0.8rem; color: var(--text-muted); margin-bottom: 1rem;">
          5 hóa đơn thanh toán mới nhất
        </p>

        <div v-if="stats.recentPayments.length === 0" style="text-align: center; padding: 2rem 0; color: var(--text-muted);">
          Chưa có giao dịch thanh toán nào được thực hiện.
        </div>

        <div v-else class="recent-list">
          <div v-for="pay in stats.recentPayments" :key="pay.billId" class="recent-item">
            <div>
              <div class="recent-patient">{{ pay.patientName }}</div>
              <div class="recent-meta">
                {{ formatDateTime(pay.paymentDate) }} • {{ translateMethod(pay.paymentMethod) }}
              </div>
            </div>
            <div class="recent-amount">+{{ formatCurrency(pay.amount) }}</div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script>
import { ref, onMounted } from 'vue';
import api from '../services/api';

export default {
  name: 'Dashboard',
  setup() {
    const stats = ref({
      totalRevenue: 0,
      paidBillsCount: 0,
      pendingBillsCount: 0,
      activeMedicinesCount: 0,
      outOfStockCount: 0,
      lowStockAlertsCount: 0,
      recentPayments: []
    });
    
    const chartData = ref([]);
    const maxChartValue = ref(1);

    const formatCurrency = (val) => {
      return new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(val);
    };

    const formatDateTime = (dateStr) => {
      const date = new Date(dateStr);
      return date.toLocaleDateString('vi-VN', { hour: '2-digit', minute: '2-digit' });
    };

    const translateMethod = (method) => {
      switch (method) {
        case 'Cash': return 'Tiền mặt';
        case 'BankTransfer': return 'Chuyển khoản';
        case 'POS': return 'Thẻ POS';
        default: return method;
      }
    };

    const getBarHeight = (value) => {
      if (maxChartValue.value === 0) return 0;
      // Cap max at 100%
      return Math.min(100, Math.round((value / maxChartValue.value) * 100));
    };

    const fetchData = async () => {
      try {
        const statsResponse = await api.get('/dashboard/stats');
        stats.value = statsResponse.data;

        const chartResponse = await api.get('/dashboard/revenue-chart');
        chartData.value = chartResponse.data.dailyRevenue;

        // Compute maximum value in chart to scale heights appropriately
        let maxVal = 100000; // base minimum scale
        chartData.value.forEach(point => {
          if (point.clinicRevenue > maxVal) maxVal = point.clinicRevenue;
          if (point.medicineRevenue > maxVal) maxVal = point.medicineRevenue;
        });
        maxChartValue.value = maxVal;
      } catch (err) {
        console.error('Lỗi khi tải dữ liệu dashboard:', err);
      }
    };

    onMounted(() => {
      fetchData();
    });

    return {
      stats,
      chartData,
      formatCurrency,
      formatDateTime,
      translateMethod,
      getBarHeight,
      fetchData
    };
  }
};
</script>
