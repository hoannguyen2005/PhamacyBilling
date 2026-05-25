<template>
  <div class="prescriptions-page">
    <div class="page-header">
      <div class="page-title">
        <h2>Đơn Thuốc Đã Kê</h2>
        <p>Danh sách đơn thuốc của bệnh nhân đồng bộ từ dịch vụ Bệnh Án (Medical Record Service)</p>
      </div>
      <button @click="openEventModal" class="btn btn-success">
        ⚡ Giả lập Sự kiện (Event prescription.created)
      </button>
    </div>

    <!-- Prescriptions Table -->
    <div class="card" style="padding: 0; overflow: hidden;">
      <div class="table-responsive">
        <table class="app-table">
          <thead>
            <tr>
              <th>Mã đơn thuốc</th>
              <th>Bệnh nhân</th>
              <th>Bác sĩ kê toa</th>
              <th>Ngày kê đơn</th>
              <th>Số loại thuốc</th>
              <th>Trạng thái</th>
              <th style="text-align: right;">Chi tiết</th>
            </tr>
          </thead>
          <tbody>
            <tr v-if="loading">
              <td colspan="7" style="text-align: center; padding: 2rem;">Đang tải danh sách đơn thuốc...</td>
            </tr>
            <tr v-else-if="prescriptions.length === 0">
              <td colspan="7" style="text-align: center; padding: 2rem;">Chưa có đơn thuốc nào được đồng bộ. Hãy dùng nút Giả lập Sự kiện phía trên!</td>
            </tr>
            <tr v-for="pres in prescriptions" :key="pres.id">
              <td style="font-family: monospace; font-size: 0.8rem;">{{ pres.id.substring(0, 8) }}...</td>
              <td style="font-weight: 600;">{{ pres.patientName }}</td>
              <td>{{ pres.doctorName }}</td>
              <td>{{ formatDate(pres.createdDate) }}</td>
              <td>{{ pres.items.length }} loại</td>
              <td>
                <span :class="['badge', getStatusClass(pres.status)]">
                  {{ translateStatus(pres.status) }}
                </span>
              </td>
              <td style="text-align: right;">
                <button @click="viewDetails(pres)" class="btn btn-secondary" style="padding: 0.35rem 0.75rem; font-size: 0.8rem;">
                  👁️ Xem chi tiết
                </button>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>

    <!-- Details Modal -->
    <div v-if="showDetailsModal" class="modal-overlay">
      <div class="modal-content modal-lg">
        <div class="modal-header">
          <h3>Chi Tiết Đơn Thuốc #{{ selectedPrescription?.id }}</h3>
          <button @click="showDetailsModal = false" class="btn-close">&times;</button>
        </div>
        <div class="modal-body">
          <div class="prescription-meta-grid" style="display: grid; grid-template-columns: 1fr 1fr; gap: 1rem; margin-bottom: 1.5rem; border-bottom: 1px solid var(--border-color); padding-bottom: 1rem;">
            <div>
              <p>👤 <b>Bệnh nhân:</b> {{ selectedPrescription?.patientName }}</p>
              <p style="font-size: 0.8rem; color: var(--text-muted); padding-left: 1.25rem;">ID: {{ selectedPrescription?.patientId }}</p>
            </div>
            <div>
              <p>🩺 <b>Bác sĩ kê toa:</b> {{ selectedPrescription?.doctorName }}</p>
              <p>📅 <b>Ngày lập:</b> {{ formatDate(selectedPrescription?.createdDate) }}</p>
            </div>
          </div>

          <h4 style="margin-bottom: 0.75rem; font-size: 0.95rem; font-weight: 700;">💊 Danh Sách Thuốc Chỉ Định</h4>
          <table class="app-table" style="border: 1px solid var(--border-color); border-radius: 0.5rem; margin-bottom: 1rem;">
            <thead style="background-color: #f8fafc;">
              <tr>
                <th>Tên thuốc</th>
                <th>Số lượng</th>
                <th>Đơn giá</th>
                <th>Thành tiền</th>
                <th>Cách dùng</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="item in selectedPrescription?.items" :key="item.id">
                <td style="font-weight: 600;">{{ item.medicineName }}</td>
                <td>{{ item.quantity }}</td>
                <td>{{ formatCurrency(item.unitPrice) }}</td>
                <td style="font-weight: 600; color: var(--primary);">{{ formatCurrency(item.totalPrice) }}</td>
                <td style="font-size: 0.8rem; color: var(--text-muted);">{{ item.dosage }}</td>
              </tr>
            </tbody>
          </table>

          <div v-if="selectedPrescription?.notes" class="form-group" style="margin-top: 1rem;">
            <label class="form-label">Ghi chú của bác sĩ:</label>
            <div style="background-color: #f8fafc; padding: 0.75rem; border-radius: 0.5rem; border: 1px solid var(--border-color); font-size: 0.85rem;">
              {{ selectedPrescription?.notes }}
            </div>
          </div>
        </div>
        <div class="modal-footer">
          <button @click="showDetailsModal = false" class="btn btn-secondary">Đóng</button>
          <router-link v-if="selectedPrescription?.status === 'Approved'" to="/bills" class="btn btn-primary">
            💵 Đi thanh toán viện phí
          </router-link>
        </div>
      </div>
    </div>

    <!-- Simulate Event Modal -->
    <div v-if="showEventModal" class="modal-overlay">
      <div class="modal-content modal-lg">
        <div class="modal-header">
          <h3>⚡ Giả lập Sự kiện Kê đơn (prescription.created)</h3>
          <button @click="closeEventModal" class="btn-close">&times;</button>
        </div>
        <form @submit.prevent="submitSimulatedEvent">
          <div class="modal-body">
            <p style="font-size: 0.8rem; color: var(--text-muted); margin-bottom: 1.25rem; background-color: #f0fdf4; border: 1px solid #bbf7d0; padding: 0.5rem; border-radius: 0.25rem;">
              ℹ️ Đây là giả lập Event-driven communication. Khi bấm "Gửi sự kiện", hệ thống sẽ gửi một POST API tới endpoint: <code>/api/prescriptions/simulate-event</code> đóng vai trò là một Consumer nhận message queue từ Medical Record Service.
            </p>

            <div style="display: grid; grid-template-columns: 1fr 1fr; gap: 1rem;">
              <div class="form-group">
                <label class="form-label">Bệnh nhân *</label>
                <input v-model="eventForm.patientName" type="text" class="form-input" required />
              </div>
              <div class="form-group">
                <label class="form-label">Phí khám lâm sàng (VND) *</label>
                <input v-model.number="eventForm.clinicFee" type="number" class="form-input" required />
              </div>
            </div>

            <div style="display: grid; grid-template-columns: 1fr 1fr; gap: 1rem;">
              <div class="form-group">
                <label class="form-label">Bác sĩ kê đơn *</label>
                <input v-model="eventForm.doctorName" type="text" class="form-input" required />
              </div>
              <div class="form-group">
                <label class="form-label">Mã đơn thuốc (Tự động sinh)</label>
                <input v-model="eventForm.prescriptionId" type="text" class="form-input" disabled />
              </div>
            </div>

            <div class="form-group">
              <label class="form-label">Ghi chú bệnh lý</label>
              <input v-model="eventForm.notes" type="text" class="form-input" placeholder="Triệu chứng, chẩn đoán sơ bộ..." />
            </div>

            <!-- Choose Medicines in Prescription -->
            <div style="border-top: 1px solid var(--border-color); padding-top: 1rem; margin-top: 1rem;">
              <div style="display: flex; justify-content: space-between; align-items: center; margin-bottom: 0.75rem;">
                <h4 style="font-size: 0.9rem; font-weight: 700;">💊 Thuốc kê trong đơn</h4>
                <button type="button" @click="addMedicineToEvent" class="btn btn-secondary" style="padding: 0.3rem 0.6rem; font-size: 0.75rem;">
                  ➕ Thêm thuốc
                </button>
              </div>

              <!-- List added items -->
              <div v-for="(item, index) in eventForm.items" :key="index" style="display: grid; grid-template-columns: 2fr 1fr 2fr 0.2fr; gap: 0.75rem; align-items: center; margin-bottom: 0.5rem;">
                <div>
                  <select v-model="item.medicineId" class="form-input" required @change="onMedicineChange(item)">
                    <option value="" disabled>-- Chọn thuốc --</option>
                    <option v-for="med in activeMedicines" :key="med.id" :value="med.id">
                      {{ med.name }} (tồn: {{ med.stockQuantity }})
                    </option>
                  </select>
                </div>
                <div>
                  <input v-model.number="item.quantity" type="number" class="form-input" min="1" placeholder="SL" required />
                </div>
                <div>
                  <input v-model="item.dosage" type="text" class="form-input" placeholder="Cách dùng (VD: Sáng 1v)" required />
                </div>
                <div>
                  <button type="button" @click="removeMedicineFromEvent(index)" style="background: none; border: none; color: var(--danger); font-size: 1.1rem; cursor: pointer;">
                    🗑️
                  </button>
                </div>
              </div>
            </div>
          </div>
          <div class="modal-footer">
            <button type="button" @click="closeEventModal" class="btn btn-secondary">Hủy</button>
            <button type="submit" class="btn btn-success" :disabled="submittingEvent">
              🚀 {{ submittingEvent ? 'Đang gửi sự kiện...' : 'Gửi sự kiện' }}
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
  name: 'Prescriptions',
  setup() {
    const prescriptions = ref([]);
    const activeMedicines = ref([]);
    const loading = ref(false);
    const submittingEvent = ref(false);

    // Modal view details
    const showDetailsModal = ref(false);
    const selectedPrescription = ref(null);

    // Modal Event Simulation
    const showEventModal = ref(false);
    const eventForm = ref({
      prescriptionId: '',
      patientId: '',
      patientName: 'Nguyễn Văn Bình',
      doctorId: '',
      doctorName: 'BS. Lê Mạnh Hùng',
      clinicFee: 150000,
      notes: 'Đau dạ dày cấp, khó tiêu',
      items: []
    });

    const formatCurrency = (val) => {
      return new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(val);
    };

    const formatDate = (dateStr) => {
      if (!dateStr) return '';
      return new Date(dateStr).toLocaleDateString('vi-VN', { hour: '2-digit', minute: '2-digit', day: '2-digit', month: '2-digit', year: 'numeric' });
    };

    const translateStatus = (status) => {
      switch (status) {
        case 'Draft': return 'Nháp (Thiếu kho)';
        case 'Approved': return 'Chờ phát (Đã duyệt)';
        case 'Dispensed': return 'Đã phát (Hoàn thành)';
        default: return status;
      }
    };

    const getStatusClass = (status) => {
      switch (status) {
        case 'Draft': return 'badge-draft badge-warning';
        case 'Approved': return 'badge-approved';
        case 'Dispensed': return 'badge-dispensed';
        default: return 'badge-draft';
      }
    };

    const fetchPrescriptions = async () => {
      loading.value = true;
      try {
        const response = await api.get('/prescriptions');
        prescriptions.value = response.data;
      } catch (err) {
        console.error('Lỗi khi tải danh sách đơn thuốc:', err);
      } finally {
        loading.value = false;
      }
    };

    const fetchActiveMedicines = async () => {
      try {
        const response = await api.get('/medicines');
        // Only allow active medicines
        activeMedicines.value = response.data.filter(m => m.isActive);
      } catch (err) {
        console.error('Lỗi tải danh mục thuốc active:', err);
      }
    };

    const viewDetails = (pres) => {
      selectedPrescription.value = pres;
      showDetailsModal.value = true;
    };

    // Event Simulation actions
    const openEventModal = () => {
      fetchActiveMedicines();
      
      // Random Guids
      const generateGuid = () => {
        return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, function(c) {
          var r = Math.random() * 16 | 0, v = c == 'x' ? r : (r & 0x3 | 0x8);
          return v.toString(16);
        });
      };

      eventForm.value = {
        prescriptionId: generateGuid(),
        patientId: generateGuid(),
        patientName: 'Nguyễn Văn Bình',
        doctorId: generateGuid(),
        doctorName: 'BS. Lê Mạnh Hùng',
        clinicFee: 150000,
        notes: 'Đau dạ dày cấp, chướng bụng',
        items: [
          { medicineId: '', quantity: 10, dosage: 'Uống 1 viên sau ăn sáng/tối' }
        ]
      };
      showEventModal.value = true;
    };

    const closeEventModal = () => {
      showEventModal.value = false;
    };

    const addMedicineToEvent = () => {
      eventForm.value.items.push({
        medicineId: '',
        quantity: 5,
        dosage: 'Uống 1 viên hàng ngày'
      });
    };

    const removeMedicineFromEvent = (index) => {
      eventForm.value.items.splice(index, 1);
    };

    const onMedicineChange = (item) => {
      // Find medicine to autofill some sample dosage details if needed
      const med = activeMedicines.value.find(m => m.id === item.medicineId);
      if (med) {
        if (med.name.includes('Paracetamol')) {
          item.dosage = 'Uống 1 viên khi đau đầu, cách 4-6 tiếng';
        } else if (med.name.includes('Amoxicillin')) {
          item.dosage = 'Uống 1 viên sau ăn sáng/tối, ngày 2 lần';
        } else if (med.name.includes('Gaviscon')) {
          item.dosage = 'Uống 1 gói sau ăn 1 tiếng hoặc trước khi ngủ';
        } else {
          item.dosage = 'Uống 1 viên sau ăn no, ngày 2 lần';
        }
      }
    };

    const submitSimulatedEvent = async () => {
      if (eventForm.value.items.length === 0) {
        alert('Đơn thuốc phải có ít nhất 1 loại thuốc!');
        return;
      }

      // Check if any medicine is unselected
      if (eventForm.value.items.some(i => !i.medicineId)) {
        alert('Vui lòng chọn thuốc đầy đủ!');
        return;
      }

      submittingEvent.value = true;
      try {
        const response = await api.post('/prescriptions/simulate-event', eventForm.value);
        alert(`Gửi sự kiện thành công! Đơn thuốc được xử lý dưới trạng thái: ${translateStatus(response.data.status)}`);
        closeEventModal();
        fetchPrescriptions();
      } catch (err) {
        alert('Gửi sự kiện lỗi: ' + (err.response?.data?.message || err.message));
      } finally {
        submittingEvent.value = false;
      }
    };

    onMounted(() => {
      fetchPrescriptions();
    });

    return {
      prescriptions,
      activeMedicines,
      loading,
      submittingEvent,
      showDetailsModal,
      selectedPrescription,
      showEventModal,
      eventForm,
      formatCurrency,
      formatDate,
      translateStatus,
      getStatusClass,
      viewDetails,
      openEventModal,
      closeEventModal,
      addMedicineToEvent,
      removeMedicineFromEvent,
      onMedicineChange,
      submitSimulatedEvent
    };
  }
};
</script>
