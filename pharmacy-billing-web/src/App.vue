<template>
  <div class="app-container">
    <!-- Navigation Header (only show when logged in) -->
    <header v-if="isLoggedIn" class="app-header">
      <div class="header-logo">
        <span class="logo-icon">➕</span>
        <div class="logo-text">
          <h1>Clinic billing</h1>
          <p>Pharmacy & Billing Portal</p>
        </div>
      </div>

      <nav class="nav-links">
        <router-link v-if="isAdmin" to="/" class="nav-item" active-class="active">
          📊 Dashboard
        </router-link>
        <router-link to="/medicines" class="nav-item" active-class="active">
          💊 Kho Thuốc
        </router-link>
        <router-link to="/prescriptions" class="nav-item" active-class="active">
          📄 Đơn Thuốc
        </router-link>
        <router-link v-if="isAdmin || isReceptionist" to="/bills" class="nav-item" active-class="active">
          💵 Viện Phí
        </router-link>
      </nav>

      <div class="header-user">
        <div class="user-badge" :class="userRoleClass">
          {{ userRoleDisplay }}
        </div>
        <div class="user-info">
          <span class="user-name">{{ userFullName }}</span>
        </div>
        <button @click="logout" class="btn-logout" title="Đăng xuất">
          🚪 Đăng xuất
        </button>
      </div>
    </header>

    <!-- Main Content Area -->
    <main class="app-content" :class="{ 'no-header': !isLoggedIn }">
      <router-view></router-view>
    </main>
  </div>
</template>

<script>
import { ref, computed, watch, onMounted } from 'vue';
import { useRouter, useRoute } from 'vue-router';

export default {
  name: 'App',
  setup() {
    const router = useRouter();
    const route = useRoute();
    const isLoggedIn = ref(false);
    const userFullName = ref('');
    const userRole = ref('');

    const checkLoginStatus = () => {
      const token = localStorage.getItem('token');
      const userJson = localStorage.getItem('user');
      
      if (token && userJson) {
        try {
          const user = JSON.parse(userJson);
          isLoggedIn.value = true;
          userFullName.value = user.fullName;
          userRole.value = user.roleName;
        } catch (e) {
          logout();
        }
      } else {
        isLoggedIn.value = false;
        userFullName.value = '';
        userRole.value = '';
      }
    };

    const logout = () => {
      localStorage.removeItem('token');
      localStorage.removeItem('user');
      isLoggedIn.value = false;
      router.push('/login');
    };

    const isAdmin = computed(() => userRole.value === 'Admin');
    const isReceptionist = computed(() => userRole.value === 'Receptionist');

    const userRoleDisplay = computed(() => {
      switch (userRole.value) {
        case 'Admin': return 'Quản trị';
        case 'Doctor': return 'Bác sĩ';
        case 'Receptionist': return 'Thu ngân';
        case 'Patient': return 'Bệnh nhân';
        default: return userRole.value;
      }
    });

    const userRoleClass = computed(() => {
      return `role-${userRole.value?.toLowerCase() || 'default'}`;
    });

    // Monitor routing path to re-verify login state
    watch(() => route.path, () => {
      checkLoginStatus();
    });

    onMounted(() => {
      checkLoginStatus();
    });

    return {
      isLoggedIn,
      userFullName,
      userRole,
      isAdmin,
      isReceptionist,
      userRoleDisplay,
      userRoleClass,
      logout
    };
  }
};
</script>
