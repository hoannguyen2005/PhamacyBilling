<template>
  <div class="bills-page">
    <div class="page-header">
      <div class="page-title">
        <h2>Thu Viện Phí & Hóa Đơn</h2>
        <p>Quản lý các khoản phí khám lâm sàng, hóa đơn thuốc và tiến hành thanh toán xuất kho</p>
      </div>
      <button @click="openManualBillModal" class="btn btn-primary">
        💵 Lập Hóa Đơn Thủ Công
      </button>
    </div>

    <!-- Filters & Tabs -->
    <div style="display: flex; justify-content: space-between; align-items: center; margin-bottom: 1.5rem; flex-wrap: wrap; gap: 1rem;">
      <div class="nav-links" style="background-color: white; padding: 0.25rem; border-radius: 0.5rem; border: 1px solid var(--border-color)">
        <button 
          @click="setStatusFilter('')" 
          :class="['btn', !statusFilter ? 'btn-primary' : 'btn-secondary']"
          style="padding: 0.4rem 1rem; font-size: 0.85rem; border-radius: 0.35rem;"
        >
          Tất cả
        </button>
        <button 
          @click="setStatusFilter('Pending')" 
          :class="['btn', statusFilter === 'Pending' ? 'btn-primary' : 'btn-secondary']"
          style="padding: 0.4rem 1rem; font-size: 0.85rem; border-radius: 0.35rem;"
        >
          ⏳ Chờ thanh toán
        </button>
        <button 
          @click="setStatusFilter('Paid')" 
          :class="['btn', statusFilter === 'Paid' ? 'btn-primary' : 'btn-secondary']"
          style="padding: 0.4rem 1rem; font-size: 0.85rem; border-radius: 0.35rem;"
        >
          ✅ Đã thanh toán
        </button>
      </div>
      <button @click="fetchBills" class="btn btn-secondary">🔄 Tải lại</button>
    </div>

    <!-- Bills Table -->
    <div class="card" style="padding: 0; overflow: hidden;">
      <div class="table-responsive">
        <table class="app-table">
          <thead>
            <tr>
              <th>Mã HD</th>
              <th>Bệnh nhân</th>
              <th>Ngày lập</th>
              <th>Khám lâm sàng</th>
              <th>Tiền thuốc</th>
              <th>Tổng cộng</th>
              <th>Trạng thái</th>
              <th style="text-align: right;">Thao tác</th>
            </tr>
          </thead>
          <tbody>
            <tr v-if="loading">
              <td colspan="8" style="text-align: center; padding: 2rem;">Đang tải danh sách hóa đơn...</td>
            </tr>
            <tr v-else-if="bills.length === 0">
              <td colspan="8" style="text-align: center; padding: 2rem;">Chưa có hóa đơn nào trong bộ lọc này.</td>
            </tr>
            <tr v-for="bill in bills" :key="bill.id">
              <td style="font-family: monospace; font-size: 0.8rem;">{{ bill.id.substring(0, 8) }}...</td>
              <td style="font-weight: 600;">{{ bill.patientName }}</td>
              <td>{{ formatDate(bill.createdAt) }}</td>
              <td>{{ formatCurrency(bill.clinicFee) }}</td>
              <td>{{ formatCurrency(bill.medicineFee) }}</td>
              <td style="font-weight: 700; color: var(--primary); font-size: 0.95rem;">
                {{ formatCurrency(bill.totalAmount) }}
              </td>
              <td>
                <span :class="['badge', getPaymentStatusClass(bill.paymentStatus)]">
                  {{ translatePaymentStatus(bill.paymentStatus) }}
                </span>
              </td>
              <td style="text-align: right;">
                <div style="display: inline-flex; gap: 0.5rem; justify-content: flex-end;">
                  <button @click="viewBillInvoice(bill)" class="btn btn-secondary" style="padding: 0.35rem 0.75rem; font-size: 0.8rem;">
                    📄 Chi tiết
                  </button>
                  <button 
                    v-if="bill.paymentStatus === 'Pending'" 
                    @click="openPayModal(bill)" 
                    class="btn btn-success" 
                    style="padding: 0.35rem 0.75rem; font-size: 0.8rem;"
                  >
                    💵 Thanh toán
                  </button>
                </div>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>

    <!-- Pay Modal -->
    <div v-if="showPayModal" class="modal-overlay">
      <div class="modal-content" style="max-width: 450px;">
        <div class="modal-header">
          <h3>Thanh Toán Hóa Đơn</h3>
          <button @click="showPayModal = false" class="btn-close">&times;</button>
        </div>
        <form @submit.prevent="submitPayment">
          <div class="modal-body">
            <div style="background-color: #f8fafc; padding: 1rem; border-radius: 0.5rem; border: 1px solid var(--border-color); margin-bottom: 1.25rem;">
              <p style="margin-bottom: 0.25rem;">Bệnh nhân: <b>{{ selectedBill?.patientName }}</b></p>
              <p style="margin-bottom: 0.25rem;">Phí khám: <b>{{ formatCurrency(selectedBill?.clinicFee) }}</b></p>
              <p style="margin-bottom: 0.25rem;">Tiền thuốc: <b>{{ formatCurrency(selectedBill?.medicineFee) }}</b></p>
              <p style="font-size: 1.1rem; border-top: 1px dashed var(--border-color); padding-top: 0.5rem; margin-top: 0.5rem; color: var(--primary);">
                Tổng thanh toán: <b>{{ formatCurrency(selectedBill?.totalAmount) }}</b>
              </p>
            </div>

            <div class="form-group">
              <label class="form-label">Phương thức thanh toán *</label>
              <select v-model="paymentForm.paymentMethod" class="form-input" required>
                <option value="Cash">💵 Tiền mặt (Cash)</option>
                <option value="BankTransfer">🏦 Chuyển khoản ngân hàng (QR)</option>
                <option value="POS">💳 Quẹt thẻ POS</option>
              </select>
            </div>

            <div class="form-group" v-if="paymentForm.paymentMethod !== 'Cash'">
              <label class="form-label">Mã giao dịch / Ref Code</label>
              <input 
                v-model="paymentForm.transactionCode" 
                type="text" 
                class="form-input" 
                placeholder="Nhập mã chuẩn giao dịch ngân hàng/POS" 
              />
            </div>
          </div>
          <div class="modal-footer">
            <button type="button" @click="showPayModal = false" class="btn btn-secondary">Hủy</button>
            <button type="submit" class="btn btn-success" :disabled="submittingPayment">
              ✅ Xác nhận đã thu tiền
            </button>
          </div>
        </form>
      </div>
    </div>

    <!-- View Invoice / Printable Receipt Modal -->
    <div v-if="showInvoiceModal" class="modal-overlay">
      <div class="modal-content modal-lg">
        <div class="modal-header">
          <h3>Hóa Đơn Thu Viện Phí</h3>
          <button @click="showInvoiceModal = false" class="btn-close">&times;</button>
        </div>
        <div class="modal-body" id="invoice-print-area">
          <div style="border: 2px solid #0f172a; padding: 2rem; border-radius: 0.5rem; background-color: #ffffff; color: #000000; font-family: 'Courier New', Courier, monospace;">
            <div style="text-align: center; margin-bottom: 1.5rem;">
              <h2 style="font-weight: 700; text-transform: uppercase;">PHÒNG KHÁM ĐA KHOA ĐẠI HỌC</h2>
              <p>Địa chỉ: 123 Đường Sáng Tạo, Khu Công Nghệ Cao, Quận 9, TP.HCM</p>
              <p>Điện thoại: (028) 3736 1234</p>
              <hr style="margin-top: 1rem; border: 1px double #000;"/>
            </div>

            <div style="display: grid; grid-template-columns: 1fr 1fr; gap: 1rem; margin-bottom: 1.5rem; font-size: 0.9rem;">
              <div>
                <p><b>Mã Hóa đơn:</b> {{ selectedBill?.id }}</p>
                <p><b>Bệnh nhân:</b> {{ selectedBill?.patientName }}</p>
                <p><b>Mã BN:</b> {{ selectedBill?.patientId }}</p>
              </div>
              <div style="text-align: right;">
                <p><b>Ngày in:</b> {{ formatDate(selectedBill?.createdAt) }}</p>
                <p><b>Trạng thái:</b> {{ translatePaymentStatus(selectedBill?.paymentStatus).toUpperCase() }}</p>
                <p v-if="selectedBill?.paidAt"><b>Thanh toán lúc:</b> {{ formatDate(selectedBill?.paidAt) }}</p>
              </div>
            </div>

            <table style="width: 100%; border-collapse: collapse; margin-bottom: 1.5rem; font-size: 0.9rem;">
              <thead>
                <tr style="border-bottom: 1px solid #000; border-top: 1px solid #000;">
                  <th style="text-align: left; padding: 0.5rem 0;">MÔ TẢ KHOẢN THU</th>
                  <th style="text-align: right; padding: 0.5rem 0;">THÀNH TIỀN</th>
                </tr>
              </thead>
              <tbody>
                <tr v-for="item in selectedBill?.billItems" :key="item.id" style="border-bottom: 1px dashed #ccc;">
                  <td style="padding: 0.5rem 0;">{{ item.description }}</td>
                  <td style="text-align: right; padding: 0.5rem 0; font-weight: bold;">{{ formatCurrency(item.amount) }}</td>
                </tr>
              </tbody>
            </table>

            <div style="text-align: right; font-size: 1rem; font-weight: bold; border-top: 1px solid #000; padding-top: 0.5rem;">
              <p>Phí khám bệnh: {{ formatCurrency(selectedBill?.clinicFee) }}</p>
              <p>Tổng tiền thuốc: {{ formatCurrency(selectedBill?.medicineFee) }}</p>
              <p style="font-size: 1.15rem; margin-top: 0.25rem;">TỔNG TIỀN PHẢI TRẢ: {{ formatCurrency(selectedBill?.totalAmount) }}</p>
            </div>

            <div v-if="selectedBill?.payments.length > 0" style="margin-top: 1rem; border-top: 1px solid #000; padding-top: 0.5rem; font-size: 0.85rem;">
              <p><b>💳 Thông tin giao dịch:</b></p>
              <div v-for="payment in selectedBill.payments" :key="payment.id">
                <p>• Số tiền: {{ formatCurrency(payment.amountPaid) }} | Phương thức: {{ translatePaymentMethod(payment.paymentMethod) }}</p>
                <p v-if="payment.transactionCode">• Mã GD: {{ payment.transactionCode }}</p>
                <p>• Nhân viên thu ngân: {{ payment.receivedByName }}</p>
              </div>
            </div>

            <div style="text-align: center; margin-top: 2rem; font-size: 0.8rem; font-style: italic;">
              <p>Cảm ơn quý khách đã đến khám bệnh!</p>
              <p>Chúc quý khách luôn mạnh khỏe và bình an.</p>
            </div>
          </div>
        </div>
        <div class="modal-footer">
          <button @click="showInvoiceModal = false" class="btn btn-secondary">Đóng</button>
          <button @click="printInvoice" class="btn btn-primary">🖨️ In hóa đơn</button>
        </div>
      </div>
    </div>

    <!-- Manual Bill Modal -->
    <div v-if="showManualModal" class="modal-overlay">
      <div class="modal-content modal-lg">
        <div class="modal-header">
          <h3>Lập Hóa Đơn Thủ Công</h3>
          <button @click="closeManualModal" class="btn-close">&times;</button>
        </div>
        <form @submit.prevent="submitManualBill">
          <div class="modal-body">
            <div style="display: grid; grid-template-columns: 1fr 1fr; gap: 1rem;">
              <div class="form-group">
                <label class="form-label">Tên bệnh nhân *</label>
                <input v-model="manualForm.patientName" type="text" class="form-input" placeholder="Ví dụ: Nguyễn Văn C" required />
              </div>
              <div class="form-group">
                <label class="form-label">Phí khám lâm sàng (VND) *</label>
                <input v-model.number="manualForm.clinicFee" type="number" class="form-input" min="0" required />
              </div>
            </div>

            <div style="border-top: 1px solid var(--border-color); padding-top: 1rem; margin-top: 1rem;">
              <div style="display: flex; justify-content: space-between; align-items: center; margin-bottom: 0.75rem;">
                <h4 style="font-size: 0.9rem; font-weight: 700;">🛍️ Các dịch vụ / thuốc thủ công kèm theo</h4>
                <button type="button" @click="addManualItem" class="btn btn-secondary" style="padding: 0.3rem 0.6rem; font-size: 0.75rem;">
                  ➕ Thêm mục chi phí
                </button>
              </div>

              <!-- List manual items -->
              <div v-for="(item, index) in manualForm.customItems" :key="index" style="display: grid; grid-template-columns: 3fr 2fr 0.3fr; gap: 1rem; align-items: center; margin-bottom: 0.5rem;">
                <div>
                  <input v-model="item.description" type="text" class="form-input" placeholder="Tên dịch vụ/thiết bị/thuốc phát sinh" required />
                </div>
                <div>
                  <input v-model.number="item.amount" type="number" class="form-input" min="0" placeholder="Số tiền (VND)" required />
                </div>
                <div>
                  <button type="button" @click="removeManualItem(index)" style="background: none; border: none; color: var(--danger); font-size: 1.1rem; cursor: pointer;">
                    🗑️
                  </button>
                </div>
              </div>
            </div>
          </div>
          <div class="modal-footer">
            <button type="button" @click="closeManualModal" class="btn btn-secondary">Hủy</button>
            <button type="submit" class="btn btn-primary" :disabled="submittingManualBill">
              💵 Tạo hóa đơn thu tiền
            </button>
          </div>
        </form>
      </div>
    </div>
  </div>
</template>

<script>
import { ref, onMounted } from 'vue';
import api from '../services/api';

export default {
  name: 'Bills',
  setup() {
    const bills = ref([]);
    const loading = ref(false);
    const statusFilter = ref('');
    
    // Pay Modal
    const showPayModal = ref(false);
    const selectedBill = ref(null);
    const submittingPayment = ref(false);
    const paymentForm = ref({
      paymentMethod: 'Cash',
      transactionCode: ''
    });

    // Invoice View Modal
    const showInvoiceModal = ref(false);

    // Manual Bill Modal
    const showManualModal = ref(false);
    const submittingManualBill = ref(false);
    const manualForm = ref({
      patientId: '',
      patientName: '',
      clinicFee: 150000,
      customItems: []
    });

    const formatCurrency = (val) => {
      return new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(val);
    };

    const formatDate = (dateStr) => {
      if (!dateStr) return '';
      return new Date(dateStr).toLocaleDateString('vi-VN', { hour: '2-digit', minute: '2-digit', day: '2-digit', month: '2-digit', year: 'numeric' });
    };

    const translatePaymentStatus = (status) => {
      switch (status) {
        case 'Pending': return 'Chờ thanh toán';
        case 'Paid': return 'Đã thanh toán';
        case 'Cancelled': return 'Đã hủy';
        default: return status;
      }
    };

    const getPaymentStatusClass = (status) => {
      switch (status) {
        case 'Pending': return 'badge-pending';
        case 'Paid': return 'badge-paid';
        case 'Cancelled': return 'badge-cancelled';
        default: return '';
      }
    };

    const translatePaymentMethod = (method) => {
      switch (method) {
        case 'Cash': return 'Tiền mặt';
        case 'BankTransfer': return 'Chuyển khoản';
        case 'POS': return 'POS Quẹt thẻ';
        default: return method;
      }
    };

    const fetchBills = async () => {
      loading.value = true;
      try {
        const response = await api.get('/bills', {
          params: { status: statusFilter.value }
        });
        bills.value = response.data;
      } catch (err) {
        console.error('Lỗi khi tải danh sách hóa đơn:', err);
      } finally {
        loading.value = false;
      }
    };

    const setStatusFilter = (status) => {
      statusFilter.value = status;
      fetchBills();
    };

    // Checkout functions
    const openPayModal = (bill) => {
      selectedBill.value = bill;
      paymentForm.value = {
        paymentMethod: 'Cash',
        transactionCode: ''
      };
      showPayModal.value = true;
    };

    const submitPayment = async () => {
      submittingPayment.value = true;
      try {
        const response = await api.post(`/bills/${selectedBill.value.id}/pay`, {
          paymentMethod: paymentForm.value.paymentMethod,
          transactionCode: paymentForm.value.transactionCode
        });
        alert(response.data.message || 'Thanh toán thành công!');
        showPayModal.value = false;
        fetchBills();
      } catch (err) {
        alert('Lỗi khi thanh toán: ' + (err.response?.data?.message || err.message));
      } finally {
        submittingPayment.value = false;
      }
    };

    // View printable invoice receipt
    const viewBillInvoice = async (bill) => {
      try {
        const response = await api.get(`/bills/${bill.id}`);
        selectedBill.value = response.data;
        showInvoiceModal.value = true;
      } catch (err) {
        alert('Không lấy được chi tiết hóa đơn: ' + err.message);
      }
    };

    const printInvoice = () => {
      const printContents = document.getElementById('invoice-print-area').innerHTML;
      const originalContents = document.body.innerHTML;

      // Simple print simulation
      const printWindow = window.open('', '_blank');
      printWindow.document.write('<html><head><title>In hóa đơn phòng khám</title>');
      printWindow.document.write('<style>body { font-family: monospace; padding: 20px; }</style>');
      printWindow.document.write('</head><body>');
      printWindow.document.write(printContents);
      printWindow.document.write('</body></html>');
      printWindow.document.close();
      printWindow.focus();
      printWindow.print();
      printWindow.close();
    };

    // Manual Invoicing actions
    const openManualBillModal = () => {
      const generateGuid = () => {
        return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, function(c) {
          var r = Math.random() * 16 | 0, v = c == 'x' ? r : (r & 0x3 | 0x8);
          return v.toString(16);
        });
      };

      manualForm.value = {
        patientId: generateGuid(),
        patientName: '',
        clinicFee: 150000,
        customItems: [
          { description: 'Mua nhiệt kế y tế', amount: 50000 }
        ]
      };
      showManualModal.value = true;
    };

    const closeManualModal = () => {
      showManualModal.value = false;
    };

    const addManualItem = () => {
      manualForm.value.customItems.push({
        description: '',
        amount: 10000
      });
    };

    const removeManualItem = (index) => {
      manualForm.value.customItems.splice(index, 1);
    };

    const submitManualBill = async () => {
      if (!manualForm.value.patientName) return;
      if (manualForm.value.customItems.length === 0) {
        alert('Phải có ít nhất 1 dòng chi tiết dịch vụ!');
        return;
      }

      submittingManualBill.value = true;
      try {
        await api.post('/bills/create-manual', manualForm.value);
        alert('Lập hóa đơn thủ công thành công!');
        closeManualModal();
        fetchBills();
      } catch (err) {
        alert('Tạo hóa đơn lỗi: ' + (err.response?.data?.message || err.message));
      } finally {
        submittingManualBill.value = false;
      }
    };

    onMounted(() => {
      fetchBills();
    });

    return {
      bills,
      loading,
      statusFilter,
      showPayModal,
      selectedBill,
      submittingPayment,
      paymentForm,
      showInvoiceModal,
      showManualModal,
      submittingManualBill,
      manualForm,
      formatCurrency,
      formatDate,
      translatePaymentStatus,
      getPaymentStatusClass,
      translatePaymentMethod,
      fetchBills,
      setStatusFilter,
      openPayModal,
      submitPayment,
      viewBillInvoice,
      printInvoice,
      openManualBillModal,
      closeManualModal,
      addManualItem,
      removeManualItem,
      submitManualBill
    };
  }
};
</script>
