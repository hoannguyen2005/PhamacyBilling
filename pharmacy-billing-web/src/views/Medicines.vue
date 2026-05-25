<template>
  <div class="medicines-page">
    <div class="page-header">
      <div class="page-title">
        <h2>Quản Lý Kho Thuốc</h2>
        <p>Danh mục thuốc hoạt chất, đơn giá bán, đơn vị tính và theo dõi mức tồn kho</p>
      </div>
      <button v-if="isAdmin" @click="openCreateModal" class="btn btn-primary">
        ➕ Thêm Thuốc Mới
      </button>
    </div>

    <!-- Search Controls -->
    <div class="search-box">
      <input 
        v-model="searchQuery" 
        @input="handleSearch"
        type="text" 
        class="form-input search-input" 
        placeholder="Tìm tên thuốc hoặc hoạt chất..."
      />
      <button @click="fetchMedicines" class="btn btn-secondary">🔍 Tìm</button>
    </div>

    <!-- Medicines Table -->
    <div class="card" style="padding: 0; overflow: hidden;">
      <div class="table-responsive">
        <table class="app-table">
          <thead>
            <tr>
              <th>Tên thuốc</th>
              <th>Hoạt chất</th>
              <th>ĐVT</th>
              <th>Đơn giá bán</th>
              <th>Tồn kho</th>
              <th>Trạng thái</th>
              <th style="text-align: right;">Thao tác</th>
            </tr>
          </thead>
          <tbody>
            <tr v-if="loading">
              <td colspan="7" style="text-align: center; padding: 2rem;">Đang tải danh mục thuốc...</td>
            </tr>
            <tr v-else-if="medicines.length === 0">
              <td colspan="7" style="text-align: center; padding: 2rem;">Không tìm thấy thuốc nào phù hợp.</td>
            </tr>
            <tr v-for="med in medicines" :key="med.id">
              <td style="font-weight: 600;">{{ med.name }}</td>
              <td>{{ med.activeIngredient || '—' }}</td>
              <td>{{ med.unit }}</td>
              <td style="font-weight: 600; color: var(--primary);">{{ formatCurrency(med.price) }}</td>
              <td>
                <span v-if="med.stockQuantity === 0" class="badge badge-warning" style="background-color: #fee2e2; color: #dc2626;">
                  🚫 Hết hàng (0)
                </span>
                <span v-else-if="med.stockQuantity <= med.minAlertQuantity" class="badge badge-warning">
                  ⚠️ Thấp ({{ med.stockQuantity }})
                </span>
                <span v-else class="badge badge-paid" style="background-color: #ecfdf5; color: #065f46;">
                  📦 Đủ ({{ med.stockQuantity }})
                </span>
              </td>
              <td>
                <span :class="['badge', med.isActive ? 'badge-paid' : 'badge-cancelled']">
                  {{ med.isActive ? 'Hoạt động' : 'Vô hiệu hóa' }}
                </span>
              </td>
              <td style="text-align: right;">
                <div style="display: inline-flex; gap: 0.5rem; justify-content: flex-end;">
                  <button @click="openImportModal(med)" class="btn btn-secondary" style="padding: 0.35rem 0.75rem; font-size: 0.8rem;">
                    📥 Nhập kho
                  </button>
                  <button v-if="isAdmin" @click="openEditModal(med)" class="btn btn-secondary" style="padding: 0.35rem 0.75rem; font-size: 0.8rem; color: var(--primary);">
                    ✏️ Sửa
                  </button>
                  <button v-if="isAdmin && med.isActive" @click="deactivateMedicine(med.id)" class="btn btn-secondary" style="padding: 0.35rem 0.75rem; font-size: 0.8rem; color: var(--danger);">
                    ❌ Xóa
                  </button>
                </div>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>

    <!-- Create/Edit Modal -->
    <div v-if="showModal" class="modal-overlay">
      <div class="modal-content">
        <div class="modal-header">
          <h3>{{ isEditing ? 'Cập Nhật Thông Tin Thuốc' : 'Thêm Thuốc Mới Vào Kho' }}</h3>
          <button @click="closeModal" class="btn-close">&times;</button>
        </div>
        <form @submit.prevent="saveMedicine">
          <div class="modal-body">
            <div class="form-group">
              <label class="form-label" for="med-name">Tên thuốc *</label>
              <input v-model="medForm.name" type="text" id="med-name" class="form-input" required />
            </div>

            <div class="form-group">
              <label class="form-label" for="med-ingredient">Hoạt chất chính</label>
              <input v-model="medForm.activeIngredient" type="text" id="med-ingredient" class="form-input" placeholder="Ví dụ: Paracetamol" />
            </div>

            <div class="form-row" style="display: grid; grid-template-columns: 1fr 1fr; gap: 1rem;">
              <div class="form-group">
                <label class="form-label" for="med-unit">Đơn vị tính *</label>
                <input v-model="medForm.unit" type="text" id="med-unit" class="form-input" placeholder="Viên, Hộp, Chai..." required />
              </div>
              <div class="form-group">
                <label class="form-label" for="med-price">Đơn giá bán (VND) *</label>
                <input v-model.number="medForm.price" type="number" id="med-price" class="form-input" min="0" required />
              </div>
            </div>

            <div class="form-row" style="display: grid; grid-template-columns: 1fr 1fr; gap: 1rem;">
              <div class="form-group" v-if="!isEditing">
                <label class="form-label" for="med-stock">Số lượng tồn ban đầu</label>
                <input v-model.number="medForm.initialStock" type="number" id="med-stock" class="form-input" min="0" />
              </div>
              <div class="form-group">
                <label class="form-label" for="med-alert">Ngưỡng cảnh báo tối thiểu</label>
                <input v-model.number="medForm.minAlertQuantity" type="number" id="med-alert" class="form-input" min="0" />
              </div>
            </div>

            <div class="form-group" v-if="isEditing">
              <label class="form-label" style="display: inline-flex; align-items: center; gap: 0.5rem; cursor: pointer;">
                <input v-model="medForm.isActive" type="checkbox" style="width: 16px; height: 16px;" />
                Thuốc đang được sử dụng (Hoạt động)
              </label>
            </div>
          </div>
          <div class="modal-footer">
            <button type="button" @click="closeModal" class="btn btn-secondary">Hủy</button>
            <button type="submit" class="btn btn-primary">Lưu lại</button>
          </div>
        </form>
      </div>
    </div>

    <!-- Stock Import Modal -->
    <div v-if="showImportModal" class="modal-overlay">
      <div class="modal-content" style="max-width: 400px;">
        <div class="modal-header">
          <h3>Nhập Thêm Kho Thuốc</h3>
          <button @click="closeImportModal" class="btn-close">&times;</button>
        </div>
        <form @submit.prevent="submitStockImport">
          <div class="modal-body">
            <p style="font-size: 0.9rem; margin-bottom: 1rem; color: var(--text-muted);">
              Thuốc: <b style="color: var(--text-main)">{{ selectedMedicine?.name }}</b><br/>
              Tồn kho hiện tại: <b style="color: var(--primary)">{{ selectedMedicine?.stockQuantity }} {{ selectedMedicine?.unit }}</b>
            </p>
            <div class="form-group">
              <label class="form-label" for="import-qty">Số lượng nhập thêm</label>
              <input v-model.number="importQuantity" type="number" id="import-qty" class="form-input" min="1" required />
            </div>
          </div>
          <div class="modal-footer">
            <button type="button" @click="closeImportModal" class="btn btn-secondary">Hủy</button>
            <button type="submit" class="btn btn-success">📥 Nhập kho</button>
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
  name: 'Medicines',
  setup() {
    const medicines = ref([]);
    const loading = ref(false);
    const searchQuery = ref('');
    const isAdmin = ref(false);

    // Modal forms states
    const showModal = ref(false);
    const isEditing = ref(false);
    const currentMedId = ref(null);
    const medForm = ref({
      name: '',
      activeIngredient: '',
      unit: 'Viên',
      price: 0,
      initialStock: 0,
      minAlertQuantity: 10,
      isActive: true
    });

    const showImportModal = ref(false);
    const selectedMedicine = ref(null);
    const importQuantity = ref(50);

    const checkUserRole = () => {
      const userJson = localStorage.getItem('user');
      if (userJson) {
        const user = JSON.parse(userJson);
        isAdmin.value = user.roleName === 'Admin';
      }
    };

    const formatCurrency = (val) => {
      return new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(val);
    };

    const fetchMedicines = async () => {
      loading.value = true;
      try {
        const response = await api.get('/medicines', {
          params: { search: searchQuery.value }
        });
        medicines.value = response.data;
      } catch (err) {
        console.error('Lỗi khi tải danh sách thuốc:', err);
      } finally {
        loading.value = false;
      }
    };

    let searchTimeout = null;
    const handleSearch = () => {
      clearTimeout(searchTimeout);
      searchTimeout = setTimeout(() => {
        fetchMedicines();
      }, 300);
    };

    // CRUD Modal Actions
    const openCreateModal = () => {
      isEditing.value = false;
      currentMedId.value = null;
      medForm.value = {
        name: '',
        activeIngredient: '',
        unit: 'Viên',
        price: 1000,
        initialStock: 100,
        minAlertQuantity: 10,
        isActive: true
      };
      showModal.value = true;
    };

    const openEditModal = (med) => {
      isEditing.value = true;
      currentMedId.value = med.id;
      medForm.value = {
        name: med.name,
        activeIngredient: med.activeIngredient || '',
        unit: med.unit,
        price: med.price,
        minAlertQuantity: med.minAlertQuantity,
        isActive: med.isActive
      };
      showModal.value = true;
    };

    const closeModal = () => {
      showModal.value = false;
    };

    const saveMedicine = async () => {
      try {
        if (isEditing.value) {
          await api.put(`/medicines/${currentMedId.value}`, {
            name: medForm.value.name,
            activeIngredient: medForm.value.activeIngredient,
            unit: medForm.value.unit,
            price: medForm.value.price,
            isActive: medForm.value.isActive,
            minAlertQuantity: medForm.value.minAlertQuantity
          });
        } else {
          await api.post('/medicines', {
            name: medForm.value.name,
            activeIngredient: medForm.value.activeIngredient,
            unit: medForm.value.unit,
            price: medForm.value.price,
            initialStock: medForm.value.initialStock,
            minAlertQuantity: medForm.value.minAlertQuantity
          });
        }
        closeModal();
        fetchMedicines();
      } catch (err) {
        alert('Có lỗi xảy ra khi lưu thuốc: ' + (err.response?.data?.message || err.message));
      }
    };

    const deactivateMedicine = async (id) => {
      if (confirm('Bạn có chắc chắn muốn ngưng sử dụng thuốc này không?')) {
        try {
          await api.delete(`/medicines/${id}`);
          fetchMedicines();
        } catch (err) {
          alert('Lỗi: ' + (err.response?.data?.message || err.message));
        }
      }
    };

    // Stock Import Actions
    const openImportModal = (med) => {
      selectedMedicine.value = med;
      importQuantity.value = 50;
      showImportModal.value = true;
    };

    const closeImportModal = () => {
      showImportModal.value = false;
      selectedMedicine.value = null;
    };

    const submitStockImport = async () => {
      if (importQuantity.value <= 0) return;
      try {
        await api.post(`/medicines/${selectedMedicine.value.id}/stock`, {
          quantity: importQuantity.value
        });
        closeImportModal();
        fetchMedicines();
      } catch (err) {
        alert('Lỗi nhập kho: ' + (err.response?.data?.message || err.message));
      }
    };

    onMounted(() => {
      checkUserRole();
      fetchMedicines();
    });

    return {
      medicines,
      loading,
      searchQuery,
      isAdmin,
      showModal,
      isEditing,
      medForm,
      showImportModal,
      selectedMedicine,
      importQuantity,
      formatCurrency,
      fetchMedicines,
      handleSearch,
      openCreateModal,
      openEditModal,
      closeModal,
      saveMedicine,
      deactivateMedicine,
      openImportModal,
      closeImportModal,
      submitStockImport
    };
  }
};
</script>
